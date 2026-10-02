import { createClient, type CallName, type EngineClient } from './generated/client'
import type { ErrorCode } from './generated/errors'
import type { Topic } from './generated/topics'
import type { EngineEvent } from './generated/types'
import type { EngineExports, EngineHost } from './host'
import { affects } from './topics'
import { wasmHost } from './wasmHost'

export const engineAssembly = 'PKHeX.Everywhere.Engine.dll'

type Envelope<T> = { ok: true; value: T } | { ok: false; error: { code: ErrorCode; message: string } }

export class EngineError extends Error {
  readonly code: ErrorCode

  constructor(code: ErrorCode, message: string) {
    super(message)
    this.name = 'EngineError'
    this.code = code
  }
}

export type EngineStatus = {
  state: 'idle' | 'booting' | 'ready' | 'failed'
  loaded: number
  total: number
  error?: unknown
}

export type Engine = EngineClient & {
  readonly ready: Promise<void>
  readonly status: EngineStatus
  onStatusChange(listener: (status: EngineStatus) => void): () => void
  subscribe(topics: readonly Topic[], callback: (changed: Topic[]) => void): () => void
  onEvent(listener: (event: EngineEvent) => void): () => void
  onCallFailed(listener: (call: CallName, error: unknown) => void): () => void
}

type Call = <T>(name: CallName, args: unknown[]) => Promise<T>

const calls = new WeakMap<Engine, Call>()

export function call<T>(engine: Engine, name: CallName, args: unknown[]): Promise<T> {
  const invoke = calls.get(engine)
  if (!invoke) throw new Error('This engine was not made by createEngine().')
  return invoke<T>(name, args)
}

export function createEngine({ host = wasmHost(), lazy = false }: { host?: EngineHost; lazy?: boolean } = {}): Engine {
  let status: EngineStatus = { state: 'idle', loaded: 0, total: 0 }
  const statusListeners = new Set<(status: EngineStatus) => void>()

  function setStatus(next: Partial<EngineStatus>) {
    status = { ...status, ...next }
    for (const listener of [...statusListeners]) listener(status)
  }

  function onStatusChange(listener: (status: EngineStatus) => void) {
    statusListeners.add(listener)
    return () => void statusListeners.delete(listener)
  }

  let settle!: { resolve: () => void; reject: (error: unknown) => void }
  const ready = new Promise<void>((resolve, reject) => (settle = { resolve, reject }))
  ready.catch(() => {})

  let exports: Promise<EngineExports> | undefined

  function boot() {
    if (exports) return exports
    setStatus({ state: 'booting' })
    host.onProgress?.((loaded, total) => setStatus({ loaded, total }))
    exports = host
      .ready()
      .then(() => host.getAssemblyExports(engineAssembly))
      .then((assembly) => assembly.PKHeX.Everywhere.Engine.EngineExports)
    exports.then(
      () => {
        setStatus({ state: 'ready' })
        settle.resolve()
      },
      (error: unknown) => {
        setStatus({ state: 'failed', error })
        settle.reject(error)
      },
    )
    return exports
  }

  if (!lazy && typeof window !== 'undefined') boot()

  async function dispatch<T>(name: CallName, args: unknown[]): Promise<T> {
    const engine = await boot()
    const envelope = JSON.parse(await engine.Call(name, JSON.stringify(args))) as Envelope<T>
    if (envelope.ok) return envelope.value
    throw new EngineError(envelope.error.code, envelope.error.message)
  }

  const failureListeners = new Set<(call: CallName, error: unknown) => void>()

  async function call<T>(name: CallName, args: unknown[]): Promise<T> {
    try {
      return await dispatch<T>(name, args)
    } catch (error) {
      for (const listener of [...failureListeners]) listener(name, error)
      throw error
    }
  }

  function onCallFailed(listener: (call: CallName, error: unknown) => void) {
    failureListeners.add(listener)
    return () => void failureListeners.delete(listener)
  }

  const subscribers = new Set<{ topics: readonly Topic[]; callback: (changed: Topic[]) => void }>()
  host.onChange((changed) => {
    for (const subscriber of [...subscribers])
      if (affects(changed, subscriber.topics)) subscriber.callback(changed as Topic[])
  })

  function subscribe(topics: readonly Topic[], callback: (changed: Topic[]) => void) {
    const subscriber = { topics, callback }
    subscribers.add(subscriber)
    return () => void subscribers.delete(subscriber)
  }

  const eventListeners = new Set<(event: EngineEvent) => void>()
  host.onEvent((json) => {
    const event = JSON.parse(json) as EngineEvent
    for (const listener of [...eventListeners]) listener(event)
  })

  function onEvent(listener: (event: EngineEvent) => void) {
    eventListeners.add(listener)
    return () => void eventListeners.delete(listener)
  }

  const engine: Engine = {
    ...createClient(call),
    ready,
    get status() {
      return status
    },
    onStatusChange,
    subscribe,
    onEvent,
    onCallFailed,
  }
  calls.set(engine, call)
  return engine
}
