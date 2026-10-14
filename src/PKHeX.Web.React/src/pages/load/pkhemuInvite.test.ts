import { beforeEach, describe, expect, it } from 'vitest'
import { createSettings } from '../../settings'
import type { SaveSummary } from '@pkhex-everywhere/engine'
import { applyPkhemuPreviewParam, cardGeneration, showPkhemuCard } from './pkhemuInvite'

const android = 'Mozilla/5.0 (Linux; Android 14; Pixel 8) AppleWebKit/537.36 Chrome/129.0 Mobile Safari/537.36'
const desktop = 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/129.0 Safari/537.36'

describe('showPkhemuCard', () => {
  const visitor = { generation: 3, userAgent: android, preview: false, dismissed: false }

  it('stays hidden without the preview flag until the card opens to everyone', () => {
    expect(showPkhemuCard(visitor)).toBe(false)
  })

  it('shows to Android visitors with a Gen 1-5 save once open to everyone', () => {
    for (const generation of [1, 2, 3, 4, 5]) expect(showPkhemuCard({ ...visitor, generation }, true)).toBe(true)
  })

  it('hides from visitors off Android', () => {
    expect(showPkhemuCard({ ...visitor, userAgent: desktop }, true)).toBe(false)
  })

  it('hides from visitors who dismissed it or tapped Join', () => {
    expect(showPkhemuCard({ ...visitor, dismissed: true }, true)).toBe(false)
  })

  it('hides for Gen 6+ saves', () => {
    for (const generation of [6, 7, 8, 9]) {
      expect(showPkhemuCard({ ...visitor, generation }, true)).toBe(false)
      expect(showPkhemuCard({ ...visitor, generation, preview: true })).toBe(false)
    }
  })

  it('shows in preview off Android and after dismissal', () => {
    expect(showPkhemuCard({ generation: 2, userAgent: desktop, preview: true, dismissed: true })).toBe(true)
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

describe('applyPkhemuPreviewParam', () => {
  beforeEach(() => localStorage.clear())

  it('turns the preview on with 1 and keeps it on without the param', () => {
    const settings = createSettings(localStorage)
    applyPkhemuPreviewParam('?pkhemuClosedTest=1', settings)
    applyPkhemuPreviewParam('', settings)
    expect(settings.readPkhemuPreview()).toBe(true)
  })

  it('turns the preview off with 0', () => {
    const settings = createSettings(localStorage)
    applyPkhemuPreviewParam('?pkhemuClosedTest=1', settings)
    applyPkhemuPreviewParam('?pkhemuClosedTest=0', settings)
    expect(settings.readPkhemuPreview()).toBe(false)
  })

  it('leaves the preview off by default', () => {
    expect(createSettings(localStorage).readPkhemuPreview()).toBe(false)
  })
})
