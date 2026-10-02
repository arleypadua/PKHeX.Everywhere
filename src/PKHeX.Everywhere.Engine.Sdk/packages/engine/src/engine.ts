import { createClient, type CallName, type EngineClient } from './generated/client'
import type { ErrorCode } from './generated/errors'
import type { Topic } from './generated/topics'
import type { EngineEvent } from './generated/types'
import type { EngineHost } from './host'
import { affects } from './topics'

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

export type Engine = EngineClient & {
  readonly ready: Promise<void>
  call<T>(name: CallName, args: unknown[]): Promise<T>
  subscribe(topics: readonly Topic[], callback: (changed: Topic[]) => void): () => void
  onEvent(listener: (event: EngineEvent) => void): () => void
  onCallFailed(listener: (call: CallName, error: unknown) => void): () => void
}

export function createEngine({ host }: { host: EngineHost }): Engine {
  const exports = host
    .ready()
    .then(() => host.getAssemblyExports(engineAssembly))
    .then((assembly) => assembly.PKHeX.Everywhere.Engine.EngineExports)

  const ready = exports.then(() => undefined)
  ready.catch(() => {})

  async function dispatch<T>(name: CallName, args: unknown[]): Promise<T> {
    const engine = await exports
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

  return { ...createClient(call), ready, call, subscribe, onEvent, onCallFailed }
}
