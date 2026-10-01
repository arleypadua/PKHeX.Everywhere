import { createContext, useContext, useEffect, useRef, useState, type ReactNode } from 'react'
import { EngineError, type Engine } from '@pkhex-everywhere/engine'
import { QueryCache } from './cache'

interface EngineContextValue {
  engine: Engine
  cache: QueryCache
}

interface EngineProviderProps {
  engine: Engine
  onUnhandledError?: (error: EngineError) => void
  children?: ReactNode
}

const EngineContext = createContext<EngineContextValue | null>(null)

// Every mounted page has its own provider, so the first one to see an error reports it for all of them.
const reported = new WeakSet<EngineError>()

export function EngineProvider({ engine, onUnhandledError, children }: EngineProviderProps) {
  const [cache] = useState(() => new QueryCache())
  useEffect(() => engine.subscribe(['*'], cache.invalidate), [engine, cache])

  const report = useRef(onUnhandledError)
  report.current = onUnhandledError
  const reports = onUnhandledError !== undefined
  useEffect(() => {
    if (!reports) return
    const handle = (event: Event) => {
      const { reason } = event as PromiseRejectionEvent
      if (!(reason instanceof EngineError) || reported.has(reason)) return
      reported.add(reason)
      event.preventDefault()
      report.current?.(reason)
    }
    window.addEventListener('unhandledrejection', handle)
    return () => window.removeEventListener('unhandledrejection', handle)
  }, [reports])

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
