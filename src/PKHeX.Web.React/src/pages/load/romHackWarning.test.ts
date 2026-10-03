import { describe, expect, it } from 'vitest'
import type { SaveSummary } from '@pkhex-everywhere/engine'
import { romHackWarning } from './romHackWarning'

const fireRed: SaveSummary = {
  fileName: 'save.sav',
  version: 'FireRed',
  generation: 3,
  hasEvents: true,
  format: null,
  capabilities: [],
}

describe('romHackWarning', () => {
  it('warns that a ROM hack format is experimental', () => {
    const unbound: SaveSummary = {
      ...fireRed,
      format: { id: 'unbound', name: 'Pokémon Unbound', baseGame: 'FireRed', baseGameId: 5 },
    }

    expect(romHackWarning(unbound)).toEqual({
      title: 'Pokémon Unbound support is experimental',
      content:
        "We're still working on it, and it changes often. We test every change, but editing this save could still corrupt it. Keep a backup of the original file.",
    })
  })

  it('does not warn for a save PKHeX reads on its own', () => {
    expect(romHackWarning(fireRed)).toBeUndefined()
  })

  it('does not warn without a save', () => {
    expect(romHackWarning(null)).toBeUndefined()
  })
})
