import type { SaveSummary } from '@pkhex-everywhere/engine'

export const pkhemuStores = {
  play: {
    url: 'https://play.google.com/store/apps/details?id=fyi.openpkx.pkhemu&referrer=utm_source%3Dpkhex-web%26utm_medium%3Dexport-card',
    badge: '/google-play-badge.png',
    badgeWidth: 141,
    alt: 'Get it on Google Play',
    devices: 'Android',
  },
  appStore: {
    url: 'https://apps.apple.com/app/id6821904696',
    badge: '/app-store-badge.svg',
    badgeWidth: 126,
    alt: 'Download on the App Store',
    devices: 'iPhone and iPad',
  },
}

export type PkhemuStore = keyof typeof pkhemuStores

export interface PkhemuVisitor {
  generation: number
  userAgent: string
  maxTouchPoints: number
  dismissed: boolean
}

export function pkhemuStore({ generation, userAgent, maxTouchPoints, dismissed }: PkhemuVisitor): PkhemuStore | null {
  if (dismissed || generation < 1 || generation > 5) return null
  if (/Android/i.test(userAgent)) return 'play'
  if (/iPhone|iPad|iPod/i.test(userAgent)) return 'appStore'
  // iPadOS Safari sends a Mac user agent, so touch support is what tells an iPad from a Mac.
  if (/Macintosh/i.test(userAgent) && maxTouchPoints > 1) return 'appStore'
  return null
}

export function cardGeneration(game: SaveSummary) {
  return game.format?.baseGeneration ?? game.generation
}
