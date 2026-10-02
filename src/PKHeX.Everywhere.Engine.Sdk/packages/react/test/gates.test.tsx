import { act, cleanup, render, renderHook, screen } from '@testing-library/react'
import { Suspense, type ReactNode } from 'react'
import { afterEach, describe, expect, it } from 'vitest'
import { draftHandle, EngineError, type Engine, type PokemonSummary, type SaveSummary } from '@pkhex-everywhere/engine'
import { EngineProvider, RequireGame, useEngineStatus, usePokemon, useQuery } from '../src'
import { ErrorBoundary } from './ErrorBoundary'
import { fakeEngine } from './fakeEngine'

const emerald = { version: 'Emerald' } as SaveSummary
const pikachu = { id: 'pikachu:1', species: 'Pikachu', level: 12 } as PokemonSummary

function PartyNames() {
  const party = useQuery('party.get')
  return <p>{party.map((p) => p.species).join(', ')}</p>
}

function renderApp(engine: Engine, children: ReactNode, onError?: (error: Error) => void) {
  return render(
    <EngineProvider engine={engine}>
      <ErrorBoundary onError={onError}>
        <Suspense fallback={<p>loading</p>}>{children}</Suspense>
      </ErrorBoundary>
    </EngineProvider>,
  )
}

afterEach(cleanup)

describe('useEngineStatus', () => {
  it('re-renders with the boot progress until the engine is ready', async () => {
    const { engine, emitProgress, finishBoot } = fakeEngine(() => null, { booted: false })
    const wrapper = ({ children }: { children: ReactNode }) => <EngineProvider engine={engine}>{children}</EngineProvider>
    const { result } = renderHook(() => useEngineStatus(), { wrapper })

    expect(result.current).toEqual({ state: 'booting', loaded: 0, total: 0 })

    act(() => emitProgress(12, 48))
    expect(result.current).toEqual({ state: 'booting', loaded: 12, total: 48 })

    await act(async () => finishBoot())
    expect(result.current).toEqual({ state: 'ready', loaded: 12, total: 48 })
  })

  it('reports a failed boot', async () => {
    const { engine, failBoot } = fakeEngine(() => null, { booted: false })
    const wrapper = ({ children }: { children: ReactNode }) => <EngineProvider engine={engine}>{children}</EngineProvider>
    const { result } = renderHook(() => useEngineStatus(), { wrapper })
    const error = new Error('dotnet.js failed to load')

    await act(async () => failBoot(error))

    expect(result.current).toMatchObject({ state: 'failed', error })
  })
})

describe('a failed boot', () => {
  it('reaches the nearest error boundary', async () => {
    const { engine, failBoot } = fakeEngine(() => null, { booted: false })
    renderApp(engine, <PartyNames />)

    await act(async () => failBoot(new Error('dotnet.js failed to load')))

    expect(screen.getByText('error: dotnet.js failed to load')).toBeDefined()
  })
})

describe('RequireGame', () => {
  it('renders the fallback while no save is loaded', async () => {
    const { engine, calls } = fakeEngine(() => null)

    renderApp(
      engine,
      <RequireGame fallback={<p>pick a save</p>}>
        <PartyNames />
      </RequireGame>,
    )
    await act(async () => {})

    expect(screen.getByText('pick a save')).toBeDefined()
    expect(calls.map((c) => c.name)).toEqual(['game.get'])
  })

  it('renders its children once a save is loaded', async () => {
    let game: SaveSummary | null = null
    const { engine, emitChange } = fakeEngine((name) => {
      if (name === 'game.get') return game
      if (name === 'party.get') return [pikachu]
      game = emerald
      emitChange(['*'])
    })
    renderApp(
      engine,
      <RequireGame fallback={<p>pick a save</p>}>
        <PartyNames />
      </RequireGame>,
    )
    await act(async () => {})

    await act(() => engine.game.load(new Uint8Array([1]), 'emerald.sav'))

    expect(screen.getByText('Pikachu')).toBeDefined()
  })
})

describe('error messages', () => {
  it('names the call and the fix when a hook runs without a save', async () => {
    const { engine } = fakeEngine(() => {
      throw new EngineError('no-save', 'No save is loaded.')
    })
    let caught: unknown
    renderApp(engine, <PartyNames />, (error) => (caught = error))
    await act(async () => {})

    expect(caught).toBeInstanceOf(EngineError)
    expect((caught as EngineError).code).toBe('no-save')
    expect((caught as EngineError).message).toBe(
      'party.get needs a loaded save. Load one with useLoadedGame().load(file) and wrap the component in <RequireGame>.',
    )
  })

  it('names the call and the fix when a hook reads a draft that is not open', async () => {
    const { engine } = fakeEngine(() => {
      throw new EngineError('no-draft', 'No draft is open.')
    })
    function Draft() {
      const { pokemon } = usePokemon(draftHandle)
      return <p>{pokemon?.species}</p>
    }
    renderApp(engine, <Draft />)
    await act(async () => {})

    expect(
      screen.getByText('error: pokemon.get needs an open draft. Open one with usePokemon(at).edit() or .clone() first.'),
    ).toBeDefined()
  })

  it('leaves other messages alone', async () => {
    const { engine } = fakeEngine(() => {
      throw new EngineError('not-found', 'No Pokémon in party slot 3.')
    })
    renderApp(engine, <PartyNames />)
    await act(async () => {})

    expect(screen.getByText('error: No Pokémon in party slot 3.')).toBeDefined()
  })
})
