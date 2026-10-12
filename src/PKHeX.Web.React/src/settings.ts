import { smogon } from './calculators'

export type Theme = 'light' | 'dark'

// Blazored.LocalStorage wrote these keys as raw strings before the settings moved to JS.
const themeKey = 'theme'
const calculatorUrlKey = '__settings__#calculatorUrl'
const lastDateNewsSeenKey = 'lastDateNewsSeen'
const pkhemuPreviewKey = 'pkhemuClosedTest'
const pkhemuCardDismissedKey = 'pkhemuCardDismissed'

export type Settings = ReturnType<typeof createSettings>

const prefersDarkColorScheme = () => globalThis.matchMedia?.('(prefers-color-scheme: dark)').matches ?? false

export function createSettings(storage: Storage = localStorage, prefersDark = prefersDarkColorScheme) {
  const write = (key: string, value: string | null) =>
    value === null ? storage.removeItem(key) : storage.setItem(key, value)

  return {
    readTheme(): Theme {
      const theme = storage.getItem(themeKey)
      if (theme === null) return prefersDark() ? 'dark' : 'light'
      return theme === 'dark' ? 'dark' : 'light'
    },
    writeTheme(theme: Theme) {
      write(themeKey, theme)
    },
    readCalculatorUrl(): string {
      return storage.getItem(calculatorUrlKey) ?? smogon.url
    },
    writeCalculatorUrl(url: string | null) {
      write(calculatorUrlKey, url?.trim() ? url.replace(/\/$/, '') : null)
    },
    readLastDateNewsSeen(): string | null {
      return storage.getItem(lastDateNewsSeenKey)
    },
    writeLastDateNewsSeen(date: string | null) {
      write(lastDateNewsSeenKey, date)
    },
    readPkhemuPreview(): boolean {
      return storage.getItem(pkhemuPreviewKey) === 'true'
    },
    writePkhemuPreview(on: boolean) {
      write(pkhemuPreviewKey, on ? 'true' : null)
    },
    readPkhemuCardDismissed(): boolean {
      return storage.getItem(pkhemuCardDismissedKey) === 'true'
    },
    writePkhemuCardDismissed() {
      write(pkhemuCardDismissedKey, 'true')
    },
  }
}
