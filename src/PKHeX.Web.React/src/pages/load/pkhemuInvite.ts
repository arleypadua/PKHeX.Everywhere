import type { SaveSummary } from '@pkhex-everywhere/engine'

export const pkhemuPlayUrl =
  'https://play.google.com/store/apps/details?id=fyi.openpkx.pkhemu&referrer=utm_source%3Dpkhex-web%26utm_medium%3Dexport-card'

export interface PkhemuVisitor {
  generation: number
  userAgent: string
  dismissed: boolean
}

export function showPkhemuCard({ generation, userAgent, dismissed }: PkhemuVisitor) {
  if (generation < 1 || generation > 5) return false
  return /Android/i.test(userAgent) && !dismissed
}

export function cardGeneration(game: SaveSummary) {
  return game.format?.baseGeneration ?? game.generation
}
