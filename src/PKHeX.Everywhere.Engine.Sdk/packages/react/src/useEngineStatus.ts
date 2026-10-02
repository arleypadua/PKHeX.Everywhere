import { useSyncExternalStore } from 'react'
import type { EngineStatus } from '@pkhex-everywhere/engine'
import { useEngine } from './EngineProvider'

/** The engine's boot status. Re-renders as the runtime downloads. Works without a save. */
export function useEngineStatus(): EngineStatus {
  const engine = useEngine()
  const read = () => engine.status
  return useSyncExternalStore(engine.onStatusChange, read, read)
}
