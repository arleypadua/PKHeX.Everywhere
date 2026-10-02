import { createContext, useContext } from 'react'
import type { PlugIns } from './plugIns'

const PlugInsContext = createContext<PlugIns | null>(null)

export const PlugInsProvider = PlugInsContext

export function usePlugIns(): PlugIns {
  const plugIns = useContext(PlugInsContext)
  if (!plugIns) throw new Error('usePlugIns must be used inside a PlugInsProvider.')
  return plugIns
}
