import { afterEach, describe, expect, it, vi } from 'vitest'
import type { EngineEvent } from '@pkhex-everywhere/engine'
import { measurementId, startGoogleAnalytics, track } from './googleAnalytics'

function fakeEngine() {
  const listeners: ((event: EngineEvent) => void)[] = []
  return {
    onEvent: (listener: (event: EngineEvent) => void) => {
      listeners.push(listener)
      return () => {}
    },
    emit: (event: EngineEvent) => listeners.forEach((listener) => listener(event)),
  }
}

function started() {
  const gtag = vi.fn()
  globalThis.gtag = gtag
  const engine = fakeEngine()
  startGoogleAnalytics(engine, true)
  gtag.mockClear()
  return { gtag, engine }
}

const party = [
  { speciesId: 25, species: 'Pikachu', level: 12 },
  { speciesId: 1, species: 'Bulbasaur', level: 5 },
]

describe('Google Analytics', () => {
  afterEach(() => {
    globalThis.gtag = undefined
    document.head.querySelectorAll('script').forEach((script) => script.remove())
  })

  it('loads gtag and configures the property, which tracks page views from history changes', () => {
    const gtag = vi.fn()
    globalThis.gtag = gtag

    startGoogleAnalytics(fakeEngine(), true)

    expect(document.head.querySelector('script')?.src).toBe(`https://www.googletagmanager.com/gtag/js?id=${measurementId}`)
    expect(gtag).toHaveBeenCalledWith('config', measurementId)
  })

  it('does nothing when disabled', () => {
    const gtag = vi.fn()
    globalThis.gtag = gtag
    const engine = fakeEngine()

    startGoogleAnalytics(engine, false)
    engine.emit({ type: 'itemChanged', itemId: 4, count: 2 })

    expect(gtag).not.toHaveBeenCalled()
    expect(document.head.querySelector('script')).toBeNull()
  })

  it('sends events raised in .NET', () => {
    const { gtag } = started()

    track('pokemon_saved', { species_id: 25 })

    expect(gtag).toHaveBeenCalledWith('event', 'pokemon_saved', { species_id: 25 })
  })

  it('sends item changes as item_modified', () => {
    const { gtag, engine } = started()

    engine.emit({ type: 'itemChanged', itemId: 4, count: 2 })

    expect(gtag).toHaveBeenCalledWith('event', 'item_modified', { item_id: 4, quantity: 2 })
  })

  it('sends exports as game_exported with up to six party members', () => {
    const { gtag, engine } = started()

    engine.emit({
      type: 'gameExported',
      game: { version: 'Emerald', versionId: 3, generation: 'Gen3', generationId: 3, trainerGender: 'Female', boxCount: 420, party },
    })

    expect(gtag).toHaveBeenCalledWith('event', 'game_exported', {
      version_name: 'Emerald',
      version_id: 3,
      generation_name: 'Gen3',
      generation_id: 3,
      gender: 'Female',
      box_size: 420,
      party_species_id_01: 25,
      party_species_name_01: 'Pikachu',
      party_level_01: 12,
      party_species_id_02: 1,
      party_species_name_02: 'Bulbasaur',
      party_level_02: 5,
      party_species_id_03: null,
      party_species_name_03: null,
      party_level_03: null,
      party_species_id_04: null,
      party_species_name_04: null,
      party_level_04: null,
      party_species_id_05: null,
      party_species_name_05: null,
      party_level_05: null,
      party_species_id_06: null,
      party_species_name_06: null,
      party_level_06: null,
    })
  })

  it('sends loads as game_loaded', () => {
    const { gtag, engine } = started()

    engine.emit({
      type: 'gameLoaded',
      game: { version: 'Emerald', versionId: 3, generation: 'Gen3', generationId: 3, trainerGender: 'Female', boxCount: 420, party },
    })

    expect(gtag).toHaveBeenCalledWith(
      'event',
      'game_loaded',
      expect.objectContaining({ version_name: 'Emerald', box_size: 420, party_species_id_01: 25, party_level_02: 5 }),
    )
  })

  it.each([
    ['file', 'pokemon_loaded_from_file'],
    ['encounter', 'pokemon_loaded_from_encounter'],
  ] as const)('sends Pokémon added from a %s as %s', (source, name) => {
    const { gtag, engine } = started()

    engine.emit({
      type: 'pokemonAdded',
      at: { source: 'box', slot: 3, box: 1 },
      source,
      pokemon: { speciesId: 63, species: 'Abra', gender: 'Male', ball: 'Poké Ball', level: 7 },
    })

    expect(gtag).toHaveBeenCalledWith('event', name, {
      species_id: 63,
      species_name: 'Abra',
      gender: 'Male',
      ball_name: 'Poké Ball',
      level: 7,
      source: null,
    })
  })

  it('sends saved Pokémon as pokemon_saved with the slot source', () => {
    const { gtag, engine } = started()

    engine.emit({
      type: 'pokemonSaved',
      at: { source: 'party', slot: 0, box: null },
      pokemon: { speciesId: 25, species: 'Pikachu', gender: 'Female', ball: 'Great Ball', level: 42 },
    })

    expect(gtag).toHaveBeenCalledWith('event', 'pokemon_saved', {
      species_id: 25,
      species_name: 'Pikachu',
      gender: 'Female',
      ball_name: 'Great Ball',
      level: 42,
      source: 'Party',
    })
  })

  it('sends hook runs as plugin_hook_executed with the hook name and any failure', () => {
    const { gtag, engine } = started()

    engine.emit({ type: 'plugInRan', plugInId: 'Example', hookId: 'Example.Hooks+Greet', outcome: { kind: 'void' }, failure: null })
    engine.emit({
      type: 'plugInRan',
      plugInId: 'Example',
      hookId: 'Example.Fail',
      outcome: null,
      failure: { type: 'InvalidOperationException', message: 'Failed on purpose' },
    })

    expect(gtag.mock.calls).toEqual([
      ['event', 'plugin_hook_executed', { hook_name: 'Greet', exception_type: null, exception_message: null }],
      [
        'event',
        'plugin_hook_executed',
        { hook_name: 'Fail', exception_type: 'InvalidOperationException', exception_message: 'Failed on purpose' },
      ],
    ])
  })

  it('sends plug-in installs and updates', () => {
    const { gtag, engine } = started()

    engine.emit({ type: 'plugInInstalled', plugInId: 'Example', version: '1.0.0' })
    engine.emit({ type: 'plugInUpdated', plugInId: 'Example', version: '2.0.0' })

    expect(gtag.mock.calls).toEqual([
      ['event', 'plug_in_installed', { id: 'Example', version: '1.0.0' }],
      ['event', 'plug_in_updated', { id: 'Example', version: '2.0.0' }],
    ])
  })

  it('ignores events it does not report', () => {
    const { gtag, engine } = started()

    engine.emit({ type: 'pokemonChanged', at: { source: 'party', slot: 0, box: null } })

    expect(gtag).not.toHaveBeenCalled()
  })
})
