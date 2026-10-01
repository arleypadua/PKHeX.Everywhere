import { useCallback, useSyncExternalStore } from 'react'
import { toBase64 } from './base64'

export type Theme = 'light' | 'dark'

declare global {
  var showGoogleCmpRevocationMessage: (() => void) | undefined
}

export interface DotNetNavigator {
  invokeMethodAsync(method: 'NavigateTo', url: string, replace: boolean): Promise<void>
  invokeMethodAsync(method: 'NotifySuccess', title: string): Promise<void>
  invokeMethodAsync(method: 'SetTheme', theme: Theme): Promise<void>
  invokeMethodAsync(method: 'SetCalculatorUrl', url: string): Promise<string>
}

export interface Calculator {
  name: string
  description: string
  url: string
}

export interface HostBridge {
  navigator: DotNetNavigator
  theme: Theme
  calculatorUrl: string
  calculators: Calculator[]
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
    use: () => useSyncExternalStore(subscribe, () => value),
  }
}

let dotNetNavigator: DotNetNavigator | undefined
let calculators: Calculator[] = []
const themeStore = createStore<Theme>('light')
const calculatorUrlStore = createStore('')

export function connectHost(host: HostBridge) {
  dotNetNavigator = host.navigator
  calculators = host.calculators
  calculatorUrlStore.set(host.calculatorUrl)
  setTheme(host.theme)
}

export function setTheme(next: Theme) {
  themeStore.set(next)
}

export function useTheme(): Theme {
  return themeStore.use()
}

export async function changeTheme(next: Theme) {
  await dotNetNavigator?.invokeMethodAsync('SetTheme', next)
}

export function useCalculatorUrl(): string {
  return calculatorUrlStore.use()
}

export function getCalculators(): Calculator[] {
  return calculators
}

export async function changeCalculatorUrl(url: string) {
  if (!dotNetNavigator) return
  calculatorUrlStore.set(await dotNetNavigator.invokeMethodAsync('SetCalculatorUrl', url))
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
  window.open(calculatorImportUrl(calculatorUrlStore.get(), showdown), '_blank')
}

function calculatorImportUrl(baseUrl: string, showdown: string) {
  return `${baseUrl}/?import=${toBase64(new TextEncoder().encode(showdown))}`
}

export function showCookiePreferences() {
  globalThis.showGoogleCmpRevocationMessage?.()
}
