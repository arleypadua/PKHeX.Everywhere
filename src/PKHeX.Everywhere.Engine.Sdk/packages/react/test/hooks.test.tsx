import { act, cleanup, render, renderHook, screen } from '@testing-library/react'
import { Suspense, type ReactNode } from 'react'
import { afterEach, describe, expect, it } from 'vitest'
import { createClient, EngineError, type Engine, type ErrorCode, type Invoke, type PokemonHandle, type PokemonSummary } from '@pkhex-everywhere/engine'
import { EngineProvider, useLoadedGame, useParty, usePokemon } from '../src'

const at: PokemonHandle = { source: 'party', slot: 0, box: null }
const pikachu = { id: 'pikachu:1', at, species: 'Pikachu', level: 12 } as PokemonSummary

type Dispatch = (name: string, args: unknown[]) => unknown

function fakeEngine(dispatch: Dispatch) {
  const calls: { name: string; args: unknown[] }[] = []
  const call = (async (name: string, args: unknown[]) => {
    calls.push({ name, args })
    return dispatch(name, args)
  }) as Invoke
  const engine = { ...createClient(call), call, subscribe: () => () => {} } as unknown as Engine
  return { engine, calls }
}

const fail = (code: ErrorCode, message: string) => {
  throw new EngineError(code, message)
}

async function renderEngineHook<T>(engine: Engine, hook: () => T) {
  const wrapper = ({ children }: { children: ReactNode }) => (
    <EngineProvider engine={engine}>
      <Suspense fallback={null}>{children}</Suspense>
    </EngineProvider>
  )
  const rendered = renderHook(hook, { wrapper })
  await act(async () => {})
  return rendered.result
}

afterEach(cleanup)

describe('entity hooks', () => {
  it('useParty returns the party', async () => {
    const { engine } = fakeEngine(() => [pikachu])

    const result = await renderEngineHook(engine, () => useParty())

    expect(result.current.party).toEqual([pikachu])
  })

  it('usePokemon returns the Pokémon with its commands bound to the handle', async () => {
    const { engine, calls } = fakeEngine((name) => (name === 'pokemon.get' ? pikachu : null))

    const result = await renderEngineHook(engine, () => usePokemon(at))
    await act(() => result.current.setLevel(50))

    expect(result.current.pokemon).toEqual(pikachu)
    expect(calls).toEqual([
      { name: 'pokemon.get', args: [at] },
      { name: 'pokemon.setLevel', args: [at, 50] },
    ])
  })

  it('usePokemon keeps its commands while the handle is unchanged', async () => {
    const { engine } = fakeEngine(() => pikachu)
    const wrapper = ({ children }: { children: ReactNode }) => (
      <EngineProvider engine={engine}>
        <Suspense fallback={null}>{children}</Suspense>
      </EngineProvider>
    )
    const { result, rerender } = renderHook(({ handle }) => usePokemon(handle), { wrapper, initialProps: { handle: at } })
    await act(async () => {})
    const { setLevel } = result.current

    rerender({ handle: { ...at } })

    expect(result.current.setLevel).toBe(setLevel)
  })

  it('useLoadedGame returns no game and the load and close commands', async () => {
    const { engine, calls } = fakeEngine(() => null)

    const result = await renderEngineHook(engine, () => useLoadedGame())
    await act(() => result.current.load('AAAA', 'save.sav'))
    await act(() => result.current.close())

    expect(result.current.game).toBeNull()
    expect(calls.slice(1)).toEqual([
      { name: 'game.load', args: ['AAAA', 'save.sav'] },
      { name: 'game.close', args: [] },
    ])
  })

  it('rejects a failed command with a typed EngineError', async () => {
    const { engine } = fakeEngine((name) => (name === 'pokemon.get' ? pikachu : fail('out-of-range', 'Level must be between 1 and 100, got 101.')))

    const result = await renderEngineHook(engine, () => usePokemon(at))
    const error = await result.current.setLevel(101).catch((e: unknown) => e)

    expect(error).toBeInstanceOf(EngineError)
    expect((error as EngineError).code satisfies ErrorCode).toBe('out-of-range')
  })

  it('renders the data without loading checks once it arrives', async () => {
    const { engine } = fakeEngine(() => [pikachu])
    function Party() {
      const { party } = useParty()
      return <p>{party.map((p) => p.species).join(', ')}</p>
    }

    render(
      <EngineProvider engine={engine}>
        <Suspense fallback={<p>loading</p>}>
          <Party />
        </Suspense>
      </EngineProvider>,
    )
    await act(async () => {})

    expect(screen.getByText('Pikachu')).toBeDefined()
  })
})
