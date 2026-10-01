import { useSyncExternalStore } from 'react'
import type { CallName, EngineClient } from '@pkhex-everywhere/engine'
import { useEngineContext } from './EngineProvider'

type Lookup<T, Path extends string> = Path extends `${infer Head}.${infer Rest}`
  ? Head extends keyof T
    ? Lookup<T[Head], Rest>
    : never
  : Path extends keyof T
    ? T[Path]
    : never

type CallFunction<Name extends CallName> = Extract<Lookup<EngineClient, Name>, (...args: never[]) => Promise<unknown>>

export type CallArgs<Name extends CallName> = Parameters<CallFunction<Name>>
export type CallResult<Name extends CallName> = Awaited<ReturnType<CallFunction<Name>>>

export function useQuery<Name extends CallName>(call: Name, ...args: CallArgs<Name>): CallResult<Name> {
  const { engine, cache } = useEngineContext()
  const key = JSON.stringify([call, args])
  // Starting the fetch during render is safe: a missing entry always suspends, so nothing renders from this pass.
  const entry = useSyncExternalStore(cache.subscribe, () => cache.peek(key)) ?? cache.get(key, () => engine.call(call, args))

  if (entry.status === 'pending') throw entry.promise
  if (entry.status === 'error') throw entry.error
  return entry.value as CallResult<Name>
}
