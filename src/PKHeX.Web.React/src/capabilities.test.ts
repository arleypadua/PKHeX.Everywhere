import { describe, expect, it } from 'vitest'
import type { SaveSummary } from '@pkhex-everywhere/engine'
import { gameName, statsAreApproximate, supports } from './capabilities'

const emerald: SaveSummary = {
  fileName: 'emerald.sav',
  version: 'Emerald',
  generation: 3,
  hasEvents: true,
  format: null,
  capabilities: ['legality', 'autoLegality', 'encounters', 'showdown', 'events', 'plugIns'],
  statsApproximate: false,
}

const unbound: SaveSummary = {
  fileName: 'unbound.sav',
  version: 'FireRed',
  generation: 9,
  hasEvents: false,
  format: { id: 'unbound', name: 'Pokémon Unbound', baseGame: 'FireRed', baseGameId: 5 },
  capabilities: [],
  statsApproximate: true,
}

describe('supports', () => {
  it('follows the capabilities of the loaded save', () => {
    expect(supports(emerald, 'showdown')).toBe(true)
    expect(supports(unbound, 'showdown')).toBe(false)
  })

  it('allows everything without a save', () => {
    expect(supports(null, 'plugIns')).toBe(true)
  })
})

describe('gameName', () => {
  it('names the save format when there is one', () => {
    expect(gameName(unbound)).toBe('Pokémon Unbound')
  })

  it('names the version otherwise', () => {
    expect(gameName(emerald)).toBe('Emerald')
  })
})

describe('statsAreApproximate', () => {
  it('follows the save', () => {
    expect(statsAreApproximate(unbound)).toBe(true)
    expect(statsAreApproximate(emerald)).toBe(false)
  })

  it('is false without a save', () => {
    expect(statsAreApproximate(null)).toBe(false)
  })

  it('does not follow the save format', () => {
    expect(statsAreApproximate({ ...unbound, statsApproximate: false })).toBe(false)
  })
})
