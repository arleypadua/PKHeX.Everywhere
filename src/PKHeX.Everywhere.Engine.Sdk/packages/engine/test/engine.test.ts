import { describe, expect, it } from 'vitest'
import { createEngine, EngineError, type PokemonSummary } from '../src'
import { fakeHost } from './fakeHost'

const pikachu = {
  id: 'pikachu:1',
  at: { source: 'party', slot: 0, box: null },
  speciesId: 25,
  species: 'Pikachu',
  form: { id: 0, name: '' },
  nickname: 'Sparky',
  level: 12,
  isShiny: false,
} as PokemonSummary

const flush = () => new Promise((resolve) => setTimeout(resolve, 0))

describe('createEngine', () => {
  it('waits for the host to be ready before calling .NET', async () => {
    const { host, signalReady, calls, requestedAssemblies } = fakeHost(() => ({ ok: true, value: [pikachu] }))
    const engine = createEngine({ host })

    const party = engine.party.get()
    await flush()
    expect(calls).toEqual([])
    expect(requestedAssemblies).toEqual([])

    signalReady()

    await expect(party).resolves.toEqual([pikachu])
    expect(calls).toEqual([{ name: 'party.get', args: '[]' }])
    expect(requestedAssemblies).toEqual(['PKHeX.Everywhere.Engine.dll'])
  })

  it('resolves ready once the host is ready', async () => {
    const { host, signalReady } = fakeHost(() => ({ ok: true, value: null }))
    const engine = createEngine({ host })
    let isReady = false
    void engine.ready.then(() => (isReady = true))

    await flush()
    expect(isReady).toBe(false)

    signalReady()
    await engine.ready
    expect(isReady).toBe(true)
  })

  it('passes arguments as a JSON array', async () => {
    const { host, signalReady, calls } = fakeHost(() => ({ ok: true, value: null }))
    signalReady()

    await createEngine({ host }).call('party.get', [pikachu.at, 3])

    expect(calls).toEqual([{ name: 'party.get', args: JSON.stringify([pikachu.at, 3]) }])
  })

  it('rejects with an EngineError carrying the error code', async () => {
    const { host, signalReady } = fakeHost(() => ({ ok: false, error: { code: 'no-save', message: 'No save is loaded.' } }))
    signalReady()

    const error = await createEngine({ host }).party.get().catch((e: unknown) => e)

    expect(error).toBeInstanceOf(EngineError)
    expect(error).toMatchObject({ code: 'no-save', message: 'No save is loaded.' })
  })

  it('notifies subscribers of changes to the topics they read', async () => {
    const { host, emitChange } = fakeHost(() => ({ ok: true, value: null }))
    const engine = createEngine({ host })
    const party: string[][] = []
    const box: string[][] = []
    engine.subscribe(['party'], (topics) => party.push(topics))
    engine.subscribe(['box/3'], (topics) => box.push(topics))

    emitChange(['party'])
    emitChange(['box'])
    emitChange(['*'])

    expect(party).toEqual([['party'], ['*']])
    expect(box).toEqual([['box'], ['*']])
  })

  it('stops notifying after unsubscribing', () => {
    const { host, emitChange } = fakeHost(() => ({ ok: true, value: null }))
    const engine = createEngine({ host })
    const changes: string[][] = []
    const unsubscribe = engine.subscribe(['party'], (topics) => changes.push(topics))

    unsubscribe()
    emitChange(['party'])

    expect(changes).toEqual([])
  })
})
