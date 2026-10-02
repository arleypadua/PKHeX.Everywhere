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

/** The arguments of the call `Name`, such as `[at: PokemonHandle]` for `pokemon.details`. */
export type CallArgs<Name extends CallName> = Parameters<CallFunction<Name>>
/** What the call `Name` resolves to. */
export type CallResult<Name extends CallName> = Awaited<ReturnType<CallFunction<Name>>>

const fixes: Partial<Record<ErrorCode, (name: CallName) => string>> = {
  'no-save': (name) => `${name} needs a loaded save. Load one with useLoadedGame().load(file) and wrap the component in <RequireGame>.`,
  'no-draft': (name) => `${name} needs an open draft. Open one with usePokemon(at).edit() or .clone() first.`,
}

function explain(name: CallName, error: unknown) {
  if (!(error instanceof EngineError)) return error
  const fix = fixes[error.code]
  return fix ? new EngineError(error.code, fix(name)) : error
}

/**
 * Runs a query by name and suspends until it resolves. Refetches when a command changes a topic the query reads,
 * and keeps showing the previous value while it does.
 */
export function useQuery<Name extends QueryName>(name: Name, ...args: CallArgs<Name>): CallResult<Name> {
  const { engine, cache } = useEngineContext()
  const key = JSON.stringify([name, args])
  const load = () =>
    call(engine, name, args).catch((error: unknown) => {
      throw explain(name, error)
    })
  // Starting the fetch during render is safe: a missing entry always suspends, so nothing renders from this pass.
  const entry = useSyncExternalStore(cache.subscribe, () => cache.peek(key)) ?? cache.get(key, queryTopics[name], load)

  if (entry.status === 'pending') throw entry.promise
  if (entry.status === 'error') throw entry.error
  return entry.value as CallResult<Name>
}
