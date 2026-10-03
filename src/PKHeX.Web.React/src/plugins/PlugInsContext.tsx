import { createContext, useContext, useMemo, useSyncExternalStore } from 'react'
import { useQuery } from '@pkhex-everywhere/react'
import { withFailedToLoad, type PlugIns } from './plugIns'

const PlugInsContext = createContext<PlugIns | null>(null)

export const PlugInsProvider = PlugInsContext

export function usePlugIns(): PlugIns {
  const plugIns = useContext(PlugInsContext)
  if (!plugIns) throw new Error('usePlugIns must be used inside a PlugInsProvider.')
  return plugIns
}

export function useInstalledPlugIns() {
  const plugIns = usePlugIns()
  const installed = useQuery('plugins.installed')
  const failed = useSyncExternalStore(plugIns.subscribe, plugIns.failedToLoad)
  return useMemo(() => withFailedToLoad(installed, failed), [installed, failed])
}
