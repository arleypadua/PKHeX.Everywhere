import { describe, expect, it } from 'vitest'
import { showRomHacksBadge } from './romHacksBadge'
import { romHacksNewsDate } from '../../news'

const [year, month, day] = romHacksNewsDate.split('-').map(Number)

describe('showRomHacksBadge', () => {
  it('shows the badge on the release date', () => {
    expect(showRomHacksBadge(new Date(year, month - 1, day))).toBe(true)
  })

  it('shows the badge on the last day of the 4-month window', () => {
    expect(showRomHacksBadge(new Date(year, month + 3, day - 1, 23, 59))).toBe(true)
  })

  it('hides the badge from 4 months after the release date', () => {
    expect(showRomHacksBadge(new Date(year, month + 3, day))).toBe(false)
  })
})
