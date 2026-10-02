import type { SaveCapability } from '@pkhex-everywhere/engine'
import { useLoadedGame } from '@pkhex-everywhere/react'
import { supports } from '../capabilities'

export function useSupports(capability: SaveCapability): boolean {
  const { game } = useLoadedGame()
  return supports(game, capability)
}
