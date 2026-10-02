import { createClient, type CallName, type EngineClient } from './generated/client'
import type { ErrorCode } from './generated/errors'
import type { Topic } from './generated/topics'
import type { EngineEvent, FormatEntry } from './generated/types'
import type { EngineExports, EngineHost } from './host'
import { affects } from './topics'
import { wasmHost } from './wasmHost'

export const engineAssembly = 'PKHeX.Everywhere.Engine.dll'

type Envelope<T> =
  | { ok: true; value: T }
  | { ok: false; error: { code: ErrorCode; message: string; candidates?: FormatEntry[] } }

/** The error a call rejects with when the engine refuses it, such as `no-save` when no save is loaded. */
export class EngineError extends Error {
  /** Why the call failed. One of `errorCodes`. */
  readonly code: ErrorCode
  /**
   * The save formats the save might be in, when `code` is `format-choice-required`.
   * Load the save again with one of their ids as `formatId`, or with `pkhex` to load it with PKHeX's own detection.
   */
  readonly candidates?: FormatEntry[]

  constructor(code: ErrorCode, message: string, candidates?: FormatEntry[]) {
    super(message)
    this.name = 'EngineError'
    this.code = code
    if (candidates) this.candidates = candidates
  }
}

/** Where the engine is in downloading and starting the .NET runtime. */
export type EngineStatus = {
  /** `idle` until the first call or, in a browser, until `createEngine()` starts the download. */
  state: 'idle' | 'booting' | 'ready' | 'failed'
  /** Runtime files downloaded so far. */
  loaded: number
  /** Runtime files to download, or 0 before the count is known. */
  total: number
  /** Why the runtime failed to load, when `state` is `failed`. */
  error?: unknown
}

/** The engine `createEngine()` returns: the client's namespaces plus boot status and change notifications. */
export type Engine = EngineClient & {
  /** Resolves once the runtime is up, and rejects if it fails to load. */
  readonly ready: Promise<void>
  /** The current boot status. */
  readonly status: EngineStatus
  /** Calls `listener` on every status change. Returns a function that stops listening. */
  onStatusChange(listener: (status: EngineStatus) => void): () => void
  /** Calls `callback` when a command changes something under `topics`. Returns a function that unsubscribes. */
  subscribe(topics: readonly Topic[], callback: (changed: Topic[]) => void): () => void
  /** Calls `listener` with each typed event the engine raises, such as `gameLoaded`. Returns a function that stops listening. */
  onEvent(listener: (event: EngineEvent) => void): () => void
  /** Calls `listener` when any call rejects, before the caller sees the error. Returns a function that stops listening. */
  onCallFailed(listener: (call: CallName, error: unknown) => void): () => void
}

type Call = <T>(name: CallName, args: unknown[]) => Promise<T>

const calls = new WeakMap<Engine, Call>()

export function call<T>(engine: Engine, name: CallName, args: unknown[]): Promise<T> {
  const invoke = calls.get(engine)
  if (!invoke) throw new Error('This engine was not made by createEngine().')
  return invoke<T>(name, args)
}

/**
 * Creates an engine. In a browser it starts downloading the runtime right away unless `lazy` is set.
 *
 * @param options.host Where the runtime runs. Defaults to `wasmHost()`.
 * @param options.lazy Wait for the first call before downloading the runtime.
 */
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
    const stopProgress = host.onProgress?.((loaded, total) => setStatus({ loaded, total }))
    exports = host
      .ready()
      .then(() => host.getAssemblyExports(engineAssembly))
      .then((assembly) => assembly.PKHeX.Everywhere.Engine.EngineExports)
    exports.then(
      () => {
        stopProgress?.()
        setStatus({ state: 'ready' })
        settle.resolve()
      },
      (error: unknown) => {
        stopProgress?.()
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
    throw new EngineError(envelope.error.code, envelope.error.message, envelope.error.candidates)
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

  const onChange = whileListened<string[]>((notify) => host.onChange(notify))

  function subscribe(topics: readonly Topic[], callback: (changed: Topic[]) => void) {
    return onChange((changed) => {
      if (affects(changed, topics)) callback(changed as Topic[])
    })
  }

  const onEvent = whileListened<EngineEvent>((notify) => host.onEvent((json) => notify(JSON.parse(json) as EngineEvent)))

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

function whileListened<T>(attach: (notify: (value: T) => void) => void | (() => void)) {
  const listeners = new Set<(value: T) => void>()
  let attached = false
  let detach: void | (() => void)

  return (listener: (value: T) => void) => {
    listeners.add(listener)
    if (!attached) {
      detach = attach((value) => {
        for (const notify of [...listeners]) notify(value)
      })
      attached = true
    }
    return () => {
      if (!listeners.delete(listener) || listeners.size > 0 || !detach) return
      detach()
      attached = false
      detach = undefined
    }
  }
}
