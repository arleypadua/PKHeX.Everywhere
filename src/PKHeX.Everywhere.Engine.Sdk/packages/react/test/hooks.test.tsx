import { act, cleanup, render, renderHook, screen } from '@testing-library/react'
import { Suspense, type ReactNode } from 'react'
import { afterEach, describe, expect, it } from 'vitest'
import {
  draftHandle,
  EngineError,
  type EditablePokemon,
  type Engine,
  type ErrorCode,
  type PokemonHandle,
  type PokemonPatch,
  type PokemonSummary,
} from '@pkhex-everywhere/engine'
import { EngineProvider, useLoadedGame, useParty, usePokemon, usePokemonDetails } from '../src'
import { fakeEngine } from './fakeEngine'

const at: PokemonHandle = { source: 'party', slot: 0, box: null }
const pikachu = { id: 'pikachu:1', at, species: 'Pikachu', level: 12 } as PokemonSummary

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
    await act(() => result.current.load(new Uint8Array([0, 0, 0]), 'save.sav'))
    await act(() => result.current.close())

    expect(result.current.game).toBeNull()
    expect(calls.slice(1)).toEqual([
      { name: 'game.load', args: ['AAAA', 'save.sav', null] },
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

  it('usePokemon binds update to the handle', async () => {
    const { engine, calls } = fakeEngine((name) => (name === 'pokemon.get' ? pikachu : null))

    const result = await renderEngineHook(engine, () => usePokemon(draftHandle))
    await act(() => result.current.update({ nickname: 'Sparky' }))

    expect(calls.at(-1)).toEqual({ name: 'pokemon.update', args: [draftHandle, { nickname: 'Sparky' }] })
  })

  it('usePokemonDetails refreshes after update changes the Pokémon', async () => {
    let details: EditablePokemon = {
      species: 25,
      form: 0,
      gender: 'male',
      nature: 3,
      ability: 9,
      heldItem: 0,
      unknownHeldItem: null,
      ball: 4,
      friendship: 70,
      language: 2,
      isShiny: false,
      isAlpha: null,
      isEgg: false,
      nickname: 'Pikachu',
      level: 12,
      pid: 1,
      types: [12],
      isInfected: false,
      isCured: false,
      trainerId: 12345,
      secretId: 54321,
      originalTrainerName: 'Red',
      originalTrainerGender: 'male',
      handlingTrainerName: '',
      handlingTrainerGender: 'male',
      currentHandler: 'originalTrainer',
      version: 7,
      metLocation: 126,
      metLevel: 5,
      metDate: '2021-03-04',
      fatefulEncounter: false,
      ivs: { health: 31, attack: 31, defense: 31, specialAttack: 31, specialDefense: 31, speed: 31 },
      evs: { health: 0, attack: 0, defense: 0, specialAttack: 0, specialDefense: 0, speed: 0 },
      avs: null,
      stats: { health: 40, attack: 25, defense: 20, specialAttack: 22, specialDefense: 22, speed: 30 },
      hiddenPower: { type: 'Dark', power: 70 },
      combatPower: null,
      calculatedCombatPower: null,
      moves: [{ id: 85, name: 'Thunderbolt', pp: 15, maxPp: 15, isUnknown: false }],
      legality: { valid: true, messages: [] },
    }
    const { engine, emitChange } = fakeEngine((name, args) => {
      if (name === 'pokemon.details') return details
      details = { ...details, ...(args[1] as PokemonPatch) } as EditablePokemon
      emitChange(['draft'])
      return null
    })

    const result = await renderEngineHook(engine, () => usePokemonDetails(draftHandle))
    await act(() => result.current.update({ level: 50 }))

    expect(result.current.details.level).toBe(50)
    expect(result.current.details.nickname).toBe('Pikachu')
  })
})
