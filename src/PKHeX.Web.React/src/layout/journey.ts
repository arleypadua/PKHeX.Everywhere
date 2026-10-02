import type { EngineEvent } from '@pkhex-everywhere/engine'
import { routes } from '../routes'

export function createJourney() {
  let reached = false
  return {
    redirectsToLoad: (hasSave: boolean) => !reached && !hasSave,
    reachedHome() {
      reached = true
    },
  }
}

export const journey = createJourney()

export function routeAfter(event: EngineEvent): string | null {
  if (event.type === 'gameLoaded') return routes.home
  if (event.type === 'gameClosed') return routes.load
  return null
}
