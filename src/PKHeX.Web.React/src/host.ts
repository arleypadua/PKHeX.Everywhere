import { useCallback, useSyncExternalStore } from 'react'
import { toBase64 } from './base64'

export type Theme = 'light' | 'dark'

declare global {
  var showGoogleCmpRevocationMessage: (() => void) | undefined
}

export interface DotNetNavigator {
  invokeMethodAsync(method: 'NavigateTo', url: string, replace: boolean): Promise<void>
  invokeMethodAsync(method: 'NotifySuccess', title: string): Promise<void>
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

export function useNavigate(): (url: string, options?: { replace?: boolean }) => void {
  return useCallback(
    (url: string, { replace = false } = {}) =>
      void dotNetNavigator?.invokeMethodAsync('NavigateTo', url, replace).catch(console.error),
    [],
  )
}

// The notification outlives the React root, which unmounts when the host navigates to a Blazor page.
export async function notifySuccessInHost(title: string) {
  await dotNetNavigator?.invokeMethodAsync('NotifySuccess', title)
}

export function openCalculator(showdown: string) {
  window.open(calculatorImportUrl(calculatorUrl, showdown), '_blank')
}

function calculatorImportUrl(baseUrl: string, showdown: string) {
  return `${baseUrl}/?import=${toBase64(new TextEncoder().encode(showdown))}`
}

export function showCookiePreferences() {
  globalThis.showGoogleCmpRevocationMessage?.()
}
