import { describe, expect, it } from 'vitest'
import { createJourney, routeAfter } from './journey'

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
    expect(routeAfter({ type: 'gameLoaded' })).toBe('/')
    expect(routeAfter({ type: 'gameClosed' })).toBe('/load')
    expect(routeAfter({ type: 'itemChanged', itemId: 1, count: 1 })).toBeNull()
  })
})
