import type { SaveCapability, SaveSummary } from '@pkhex-everywhere/engine'

export function supports(game: SaveSummary | null, capability: SaveCapability): boolean {
  return !game || game.capabilities.includes(capability)
}

export function gameName(game: SaveSummary): string {
  return game.format?.name ?? game.version
}

export function statsAreApproximate(game: SaveSummary | null): boolean {
  return game?.statsApproximate ?? false
}
