import { useCallback, useSyncExternalStore } from 'react'

export type Theme = 'light' | 'dark'

export interface DotNetNavigator {
  invokeMethodAsync(method: 'NavigateTo', url: string): Promise<void>
}

export interface HostBridge {
  navigator: DotNetNavigator
  theme: Theme
  calculatorUrl: string
}

let dotNetNavigator: DotNetNavigator | undefined
let theme: Theme = 'light'
let calculatorUrl = ''
const listeners = new Set<() => void>()

export function connectHost(host: HostBridge) {
  dotNetNavigator = host.navigator
  calculatorUrl = host.calculatorUrl
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
  return useCallback((url: string) => void dotNetNavigator?.invokeMethodAsync('NavigateTo', url).catch(console.error), [])
}

export function openCalculator(showdown: string) {
  window.open(calculatorImportUrl(calculatorUrl, showdown), '_blank')
}

function calculatorImportUrl(baseUrl: string, showdown: string) {
  let binary = ''
  for (const byte of new TextEncoder().encode(showdown)) binary += String.fromCharCode(byte)
  return `${baseUrl}/?import=${btoa(binary)}`
}
