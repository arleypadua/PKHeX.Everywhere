import { useSyncExternalStore } from 'react'
import type { EngineStatus } from '@pkhex-everywhere/engine'
import { useEngine } from './EngineProvider'

export function useEngineStatus(): EngineStatus {
  const engine = useEngine()
  const read = () => engine.status
  return useSyncExternalStore(engine.onStatusChange, read, read)
}
