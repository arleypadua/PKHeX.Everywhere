import { describe, expect, it } from 'vitest'
import { affects } from '../src/internal'

describe('affects', () => {
  it.each([
    ['party', 'party'],
    ['box', 'box/3'],
    ['box/3', 'box'],
    ['box/3', 'box/3/12'],
    ['*', 'party'],
    ['party', '*'],
  ])('a change to %s affects a reader of %s', (changed, read) => {
    expect(affects([changed], [read])).toBe(true)
  })

  it.each([
    ['party', 'box'],
    ['box/3', 'box/4'],
    ['box/1', 'box/12'],
    ['items', 'items-extra'],
  ])('a change to %s leaves a reader of %s alone', (changed, read) => {
    expect(affects([changed], [read])).toBe(false)
  })

  it('matches when any changed topic overlaps any read topic', () => {
    expect(affects(['trainer', 'box/2'], ['party', 'box'])).toBe(true)
    expect(affects(['trainer', 'box/2'], ['party', 'items'])).toBe(false)
  })
})
