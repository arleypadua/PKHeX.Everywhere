import { beforeEach, describe, expect, it } from 'vitest'
import { readEnabledFormats } from './enabledFormats'

describe('enabled save formats', () => {
  beforeEach(() => localStorage.clear())

  it('are none by default', () => {
    expect(readEnabledFormats('', () => localStorage)).toEqual([])
  })

  it('add a format and keep it', () => {
    expect(readEnabledFormats('?enableFormat=emerald-rogue', () => localStorage)).toEqual(['emerald-rogue'])
    expect(readEnabledFormats('?enableFormat=inclement-emerald', () => localStorage)).toEqual([
      'emerald-rogue',
      'inclement-emerald',
    ])
    expect(readEnabledFormats('', () => localStorage)).toEqual(['emerald-rogue', 'inclement-emerald'])
  })

  it('add a format once', () => {
    readEnabledFormats('?enableFormat=emerald-rogue', () => localStorage)

    expect(readEnabledFormats('?enableFormat=emerald-rogue', () => localStorage)).toEqual(['emerald-rogue'])
  })

  it('remove a format', () => {
    readEnabledFormats('?enableFormat=emerald-rogue&enableFormat=inclement-emerald', () => localStorage)

    expect(readEnabledFormats('?disableFormat=emerald-rogue', () => localStorage)).toEqual(['inclement-emerald'])
    expect(readEnabledFormats('', () => localStorage)).toEqual(['inclement-emerald'])
  })

  it('clear the stored list when the last one is removed', () => {
    readEnabledFormats('?enableFormat=emerald-rogue', () => localStorage)

    expect(readEnabledFormats('?disableFormat=emerald-rogue', () => localStorage)).toEqual([])
    expect(localStorage.length).toBe(0)
  })

  it('ignore a stored list that is not a list of ids', () => {
    localStorage.setItem('enabledFormats', '{"emerald-rogue":true}')

    expect(readEnabledFormats('?enableFormat=inclement-emerald', () => localStorage)).toEqual(['inclement-emerald'])
  })

  it('are none when storage is blocked', () => {
    const blocked = () => {
      throw new DOMException('The operation is insecure.', 'SecurityError')
    }

    expect(readEnabledFormats('?enableFormat=emerald-rogue', blocked)).toEqual([])
  })

  it('are none when storage throws on write', () => {
    const full = {
      getItem: () => null,
      setItem: () => {
        throw new DOMException('Quota exceeded.', 'QuotaExceededError')
      },
      removeItem: () => {},
    } as unknown as Storage

    expect(readEnabledFormats('?enableFormat=emerald-rogue', () => full)).toEqual([])
  })
})
