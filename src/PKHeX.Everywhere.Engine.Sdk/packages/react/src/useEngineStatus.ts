import { useSyncExternalStore } from 'react'
import type { EngineStatus } from '@pkhex-everywhere/engine'
import { useEngine } from './EngineProvider'

export function useEngineStatus(): EngineStatus {
  const engine = useEngine()
  return useSyncExternalStore(engine.onStatusChange, () => engine.status)
}
