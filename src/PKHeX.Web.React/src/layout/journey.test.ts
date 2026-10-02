import { describe, expect, it } from 'vitest'
import type { GameOverview } from '@pkhex-everywhere/engine'
import { createJourney, routeAfter } from './journey'

const emerald: GameOverview = { version: 'Emerald', versionId: 3, generation: 'Gen3', generationId: 3, trainerGender: 'Male', boxCount: 420, party: [], formatId: null }

describe('journey', () => {
  it('sends Home to the load page when no save is loaded', () => {
    expect(createJourney().redirectsToLoad(false)).toBe(true)
  })

  it('asks the same until Home is reached', () => {
    const journey = createJourney()
    journey.redirectsToLoad(false)

    expect(journey.redirectsToLoad(false)).toBe(true)
  })

  it('sends Home to the load page only once per session', () => {
    const journey = createJourney()
    journey.reachedHome()

    expect(journey.redirectsToLoad(false)).toBe(false)
  })

  it('keeps Home when a save is loaded', () => {
    expect(createJourney().redirectsToLoad(true)).toBe(false)
  })

  it('keeps Home after Home without a save', () => {
    const journey = createJourney()
    journey.reachedHome()

    expect(journey.redirectsToLoad(false)).toBe(false)
  })

  it('goes Home after a save loads and to the load page after it closes', () => {
    expect(routeAfter({ type: 'gameLoaded', game: emerald })).toBe('/')
    expect(routeAfter({ type: 'gameClosed' })).toBe('/load')
    expect(routeAfter({ type: 'itemChanged', itemId: 1, count: 1 })).toBeNull()
  })
})
