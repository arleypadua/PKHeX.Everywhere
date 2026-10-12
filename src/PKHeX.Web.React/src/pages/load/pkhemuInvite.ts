import type { SaveSummary } from '@pkhex-everywhere/engine'
import type { Settings } from '../../settings'

export const pkhemuJoinUrl = 'https://pkhemu.openpkx.fyi/#join'

const openToEveryone = false

export interface PkhemuVisitor {
  generation: number
  userAgent: string
  preview: boolean
  dismissed: boolean
}

export function showPkhemuCard({ generation, userAgent, preview, dismissed }: PkhemuVisitor, open = openToEveryone) {
  if (generation < 1 || generation > 5) return false
  if (preview) return true
  return open && /Android/i.test(userAgent) && !dismissed
}

export function cardGeneration(game: SaveSummary) {
  return game.format?.baseGeneration ?? game.generation
}

export function applyPkhemuPreviewParam(search: string, settings: Settings) {
  const value = new URLSearchParams(search).get('pkhemuClosedTest')
  if (value === '1') settings.writePkhemuPreview(true)
  else if (value === '0') settings.writePkhemuPreview(false)
}
