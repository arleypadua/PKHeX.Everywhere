import type { ReactNode } from 'react'
import { useLoadedGame } from './generated/hooks'

/** Renders `fallback` until a save is loaded, then `children`. Wrap hooks that need a save in it. */
export function RequireGame({ fallback = null, children }: { fallback?: ReactNode; children?: ReactNode }) {
  const { game } = useLoadedGame()
  return game ? children : fallback
}
