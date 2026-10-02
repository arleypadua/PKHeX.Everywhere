import type { ReactNode } from 'react'
import { useLoadedGame } from './generated/hooks'

export function RequireGame({ fallback = null, children }: { fallback?: ReactNode; children?: ReactNode }) {
  const { game } = useLoadedGame()
  return game ? children : fallback
}
