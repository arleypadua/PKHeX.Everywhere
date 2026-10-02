import { beforeEach, describe, expect, it } from 'vitest'
import { readRomHacksFlag } from './romHacks'

describe('ROM hacks flag', () => {
  beforeEach(() => localStorage.clear())

  it('is off by default', () => {
    expect(readRomHacksFlag('', () => localStorage)).toBe(false)
  })

  it('turns on and stays on', () => {
    expect(readRomHacksFlag('?enableRomHacks=true', () => localStorage)).toBe(true)
    expect(readRomHacksFlag('', () => localStorage)).toBe(true)
  })

  it('turns off and clears the stored flag', () => {
    readRomHacksFlag('?enableRomHacks=true', () => localStorage)

    expect(readRomHacksFlag('?enableRomHacks=false', () => localStorage)).toBe(false)
    expect(localStorage.length).toBe(0)
  })

  it('is off when storage is blocked', () => {
    const blocked = () => {
      throw new DOMException('The operation is insecure.', 'SecurityError')
    }

    expect(readRomHacksFlag('?enableRomHacks=true', blocked)).toBe(false)
  })

  it('is off when storage throws on write', () => {
    const full = {
      getItem: () => null,
      setItem: () => {
        throw new DOMException('Quota exceeded.', 'QuotaExceededError')
      },
      removeItem: () => {},
    } as unknown as Storage

    expect(readRomHacksFlag('?enableRomHacks=true', () => full)).toBe(false)
  })
})
