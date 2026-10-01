import { useCallback, useSyncExternalStore } from 'react'

export type Theme = 'light' | 'dark'

export interface DotNetNavigator {
  invokeMethodAsync(method: 'NavigateTo', url: string): Promise<void>
}

export interface HostBridge {
  navigator: DotNetNavigator
  theme: Theme
}

let navigator: DotNetNavigator | undefined
let theme: Theme = 'light'
const listeners = new Set<() => void>()

export function connectHost(host: HostBridge) {
  navigator = host.navigator
  setTheme(host.theme)
}

export function setTheme(next: Theme) {
  if (next === theme) return
  theme = next
  listeners.forEach((listener) => listener())
}

function subscribe(listener: () => void) {
  listeners.add(listener)
  return () => void listeners.delete(listener)
}

export function useTheme(): Theme {
  return useSyncExternalStore(subscribe, () => theme)
}

export function useNavigate(): (url: string) => void {
  return useCallback((url: string) => void navigator?.invokeMethodAsync('NavigateTo', url), [])
}
