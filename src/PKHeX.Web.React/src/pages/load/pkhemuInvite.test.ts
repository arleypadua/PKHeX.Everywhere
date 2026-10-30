import { describe, expect, it } from 'vitest'
import type { SaveSummary } from '@pkhex-everywhere/engine'
import { cardGeneration, showPkhemuCard } from './pkhemuInvite'

const android = 'Mozilla/5.0 (Linux; Android 14; Pixel 8) AppleWebKit/537.36 Chrome/129.0 Mobile Safari/537.36'
const desktop = 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/129.0 Safari/537.36'

describe('showPkhemuCard', () => {
  const visitor = { generation: 3, userAgent: android, dismissed: false }

  it('shows to an Android visitor with a Gen 3 save with no flag or param', () => {
    expect(showPkhemuCard(visitor)).toBe(true)
  })

  it('shows to Android visitors with a Gen 1-5 save', () => {
    for (const generation of [1, 2, 3, 4, 5]) expect(showPkhemuCard({ ...visitor, generation })).toBe(true)
  })

  it('hides from visitors off Android', () => {
    expect(showPkhemuCard({ ...visitor, userAgent: desktop })).toBe(false)
  })

  it('hides from visitors who dismissed it or tapped the badge', () => {
    expect(showPkhemuCard({ ...visitor, dismissed: true })).toBe(false)
  })

  it('hides for Gen 6+ saves', () => {
    for (const generation of [6, 7, 8, 9]) expect(showPkhemuCard({ ...visitor, generation })).toBe(false)
  })
})

describe('cardGeneration', () => {
  const save: SaveSummary = {
    fileName: 'save.sav',
    version: 'Emerald',
    generation: 3,
    hasEvents: true,
    hasMysteryGift: false,
    format: null,
    capabilities: [],
    statsApproximate: false,
  }

  it('counts a save by its own generation', () => {
    expect(cardGeneration(save)).toBe(3)
  })

  it("counts a ROM hack by its base game's generation", () => {
    const unbound = { id: 'unbound', name: 'Pokémon Unbound', baseGame: 'FireRed', baseGameId: 4, baseGeneration: 3 }
    expect(cardGeneration({ ...save, generation: 9, format: unbound })).toBe(3)
  })
})
