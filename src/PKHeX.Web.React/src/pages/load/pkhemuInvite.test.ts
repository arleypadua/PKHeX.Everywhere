import { describe, expect, it } from 'vitest'
import type { SaveSummary } from '@pkhex-everywhere/engine'
import { cardGeneration, pkhemuStore } from './pkhemuInvite'

const android = 'Mozilla/5.0 (Linux; Android 14; Pixel 8) AppleWebKit/537.36 Chrome/129.0 Mobile Safari/537.36'
const desktop = 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/129.0 Safari/537.36'
const iPhone =
  'Mozilla/5.0 (iPhone; CPU iPhone OS 18_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/18.1 Mobile/15E148 Safari/604.1'
const iPadOS =
  'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/18.1 Safari/605.1.15'

describe('pkhemuStore', () => {
  const visitor = { generation: 3, userAgent: android, maxTouchPoints: 5, dismissed: false }

  it('sends an Android visitor with a Gen 3 save to Google Play', () => {
    expect(pkhemuStore(visitor)).toBe('play')
  })

  it('sends iPhone and iPad visitors to the App Store', () => {
    expect(pkhemuStore({ ...visitor, userAgent: iPhone })).toBe('appStore')
    expect(pkhemuStore({ ...visitor, userAgent: iPadOS })).toBe('appStore')
  })

  it('shows for Gen 1-5 saves', () => {
    for (const generation of [1, 2, 3, 4, 5]) expect(pkhemuStore({ ...visitor, generation })).toBe('play')
  })

  it('hides from desktop visitors, including Macs', () => {
    expect(pkhemuStore({ ...visitor, userAgent: desktop, maxTouchPoints: 0 })).toBeNull()
    expect(pkhemuStore({ ...visitor, userAgent: iPadOS, maxTouchPoints: 0 })).toBeNull()
  })

  it('hides from visitors who dismissed it or tapped the badge', () => {
    expect(pkhemuStore({ ...visitor, dismissed: true })).toBeNull()
  })

  it('hides for Gen 6+ saves', () => {
    for (const generation of [6, 7, 8, 9]) expect(pkhemuStore({ ...visitor, generation })).toBeNull()
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
