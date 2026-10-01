import { createContext, useContext, useState, type ReactNode } from 'react'
import type { Engine } from '@pkhex-everywhere/engine'
import { QueryCache } from './cache'

interface EngineContextValue {
  engine: Engine
  cache: QueryCache
}

const EngineContext = createContext<EngineContextValue | null>(null)

export function EngineProvider({ engine, children }: { engine: Engine; children: ReactNode }) {
  const [cache] = useState(() => new QueryCache())
  return <EngineContext value={{ engine, cache }}>{children}</EngineContext>
}

export function useEngineContext(): EngineContextValue {
  const context = useContext(EngineContext)
  if (!context) throw new Error('Engine hooks must be used inside an EngineProvider.')
  return context
}

export function useEngine(): Engine {
  return useEngineContext().engine
}
