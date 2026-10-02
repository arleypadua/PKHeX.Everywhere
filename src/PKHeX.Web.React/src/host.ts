import { useCallback, useSyncExternalStore } from 'react'
import { toBase64 } from './base64'
import { createSettings, type Theme } from './settings'

export type { Theme } from './settings'

declare global {
  var showGoogleCmpRevocationMessage: (() => void) | undefined
}

export interface DotNetHost {
  invokeMethodAsync(method: 'NavigateTo', url: string, replace: boolean): Promise<void>
  invokeMethodAsync(method: 'NotifySuccess', title: string): Promise<void>
  invokeMethodAsync(method: 'GoHome'): Promise<void>
}

export interface HostBridge {
  navigator: DotNetHost
}

function createStore<T>(initial: T) {
  let value = initial
  const listeners = new Set<() => void>()
  const subscribe = (listener: () => void) => {
    listeners.add(listener)
    return () => void listeners.delete(listener)
  }
  return {
    get: () => value,
    set(next: T) {
      if (next === value) return
      value = next
      listeners.forEach((listener) => listener())
    },
    subscribe,
    use: () => useSyncExternalStore(subscribe, () => value),
  }
}

let dotNetHost: DotNetHost | undefined
export const settings = createSettings()
const themeStore = createStore(settings.readTheme())
const calculatorUrlStore = createStore(settings.readCalculatorUrl())

export function connectHost(host: HostBridge) {
  dotNetHost = host.navigator
}

export function getTheme(): Theme {
  return themeStore.get()
}

export function useTheme(): Theme {
  return themeStore.use()
}

export function onThemeChanged(listener: (theme: Theme) => void) {
  return themeStore.subscribe(() => listener(themeStore.get()))
}

export function changeTheme(next: Theme) {
  settings.writeTheme(next)
  themeStore.set(next)
}

export function useCalculatorUrl(): string {
  return calculatorUrlStore.use()
}

export function changeCalculatorUrl(url: string) {
  settings.writeCalculatorUrl(url)
  calculatorUrlStore.set(settings.readCalculatorUrl())
}

export function useNavigate(): (url: string, options?: { replace?: boolean }) => Promise<void> {
  return useCallback(
    async (url: string, { replace = false } = {}) =>
      await dotNetHost?.invokeMethodAsync('NavigateTo', url, replace).catch(console.error),
    [],
  )
}

export function goHome() {
  void dotNetHost?.invokeMethodAsync('GoHome').catch(console.error)
}

// The notification outlives the React root, which unmounts when the host navigates to a Blazor page.
export async function notifySuccessInHost(title: string) {
  await dotNetHost?.invokeMethodAsync('NotifySuccess', title)
}

export function openCalculator(showdown: string) {
  window.open(calculatorImportUrl(calculatorUrlStore.get(), showdown), '_blank')
}

function calculatorImportUrl(baseUrl: string, showdown: string) {
  return `${baseUrl}/?import=${toBase64(new TextEncoder().encode(showdown))}`
}

export function downloadFile(bytes: Uint8Array<ArrayBuffer>, fileName: string) {
  const url = URL.createObjectURL(new Blob([bytes], { type: 'application/octet-stream' }))
  const link = document.createElement('a')
  link.href = url
  link.download = fileName
  link.click()
  setTimeout(() => URL.revokeObjectURL(url))
}

export function showCookiePreferences() {
  globalThis.showGoogleCmpRevocationMessage?.()
}
