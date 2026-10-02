import { useSyncExternalStore } from 'react'
import { EngineError, type CallName, type EngineClient, type ErrorCode, type QueryName } from '@pkhex-everywhere/engine'
import { call, queryTopics } from '@pkhex-everywhere/engine/internal'
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

const fixes: Partial<Record<ErrorCode, (call: CallName) => string>> = {
  'no-save': (call) => `${call} needs a loaded save. Load one with useLoadedGame().load(file) and wrap the component in <RequireGame>.`,
  'no-draft': (call) => `${call} needs an open draft. Open one with usePokemon(at).edit() or .clone() first.`,
}

function explain(call: CallName, error: unknown) {
  const fix = error instanceof EngineError ? fixes[error.code] : undefined
  return fix ? new EngineError((error as EngineError).code, fix(call)) : error
}

export function useQuery<Name extends QueryName>(name: Name, ...args: CallArgs<Name>): CallResult<Name> {
  const { engine, cache } = useEngineContext()
  const key = JSON.stringify([name, args])
  const fetch = () =>
    call(engine, name, args).catch((error: unknown) => {
      throw explain(name, error)
    })
  // Starting the fetch during render is safe: a missing entry always suspends, so nothing renders from this pass.
  const entry = useSyncExternalStore(cache.subscribe, () => cache.peek(key)) ?? cache.get(key, queryTopics[name], fetch)

  if (entry.status === 'pending') throw entry.promise
  if (entry.status === 'error') throw entry.error
  return entry.value as CallResult<Name>
}
