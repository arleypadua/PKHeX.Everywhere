import { beforeEach, describe, expect, it } from 'vitest'
import { createSettings } from './settings'

const smogon = 'https://calc.pokemonshowdown.com'

describe('settings', () => {
  beforeEach(() => localStorage.clear())

  describe('theme', () => {
    it('reads the raw theme Blazor wrote', () => {
      localStorage.setItem('theme', 'dark')
      expect(createSettings(localStorage, () => false).readTheme()).toBe('dark')

      localStorage.setItem('theme', 'light')
      expect(createSettings(localStorage, () => true).readTheme()).toBe('light')
    })

    it('falls back to the color scheme preference when no theme is stored', () => {
      expect(createSettings(localStorage, () => true).readTheme()).toBe('dark')
      expect(createSettings(localStorage, () => false).readTheme()).toBe('light')
    })

    it('reads the themes Blazor no longer offers as light', () => {
      localStorage.setItem('theme', 'compact')
      expect(createSettings(localStorage, () => true).readTheme()).toBe('light')
    })

    it('writes the raw theme', () => {
      createSettings(localStorage).writeTheme('dark')
      expect(localStorage.getItem('theme')).toBe('dark')
    })
  })

  describe('calculator URL', () => {
    it('reads the URL Blazor wrote', () => {
      localStorage.setItem('__settings__#calculatorUrl', 'https://kinglerchamp.github.io/VanillaNuzlockeCalc')
      expect(createSettings(localStorage).readCalculatorUrl()).toBe('https://kinglerchamp.github.io/VanillaNuzlockeCalc')
    })

    it('defaults to Smogon', () => {
      expect(createSettings(localStorage).readCalculatorUrl()).toBe(smogon)
    })

    it('strips the trailing slash', () => {
      createSettings(localStorage).writeCalculatorUrl('https://calc.example.com/')
      expect(localStorage.getItem('__settings__#calculatorUrl')).toBe('https://calc.example.com')
    })

    it('removes the stored URL when cleared', () => {
      localStorage.setItem('__settings__#calculatorUrl', 'https://calc.example.com')
      const settings = createSettings(localStorage)

      settings.writeCalculatorUrl('  ')

      expect(localStorage.getItem('__settings__#calculatorUrl')).toBeNull()
      expect(settings.readCalculatorUrl()).toBe(smogon)
    })
  })

  describe('last date news seen', () => {
    it('reads the date Blazor wrote', () => {
      localStorage.setItem('lastDateNewsSeen', '2025-05-01')
      expect(createSettings(localStorage).readLastDateNewsSeen()).toBe('2025-05-01')
    })

    it('is null when never stored', () => {
      expect(createSettings(localStorage).readLastDateNewsSeen()).toBeNull()
    })

    it('writes the date as yyyy-MM-dd', () => {
      createSettings(localStorage).writeLastDateNewsSeen('2026-10-01')
      expect(localStorage.getItem('lastDateNewsSeen')).toBe('2026-10-01')
    })

    it('removes the stored date when cleared', () => {
      localStorage.setItem('lastDateNewsSeen', '2025-05-01')
      createSettings(localStorage).writeLastDateNewsSeen(null)
      expect(localStorage.getItem('lastDateNewsSeen')).toBeNull()
    })
  })
})
