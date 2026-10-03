import type { SaveSummary } from '@pkhex-everywhere/engine'

export function romHackWarning(game: SaveSummary | null) {
  if (!game?.format) return undefined
  return {
    title: `${game.format.name} support is experimental`,
    content:
      "We're still working on it, and it changes often. We test every change, but editing this save could still corrupt it. Keep a backup of the original file.",
  }
}
