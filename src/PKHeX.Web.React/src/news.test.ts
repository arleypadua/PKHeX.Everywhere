import { beforeEach, describe, expect, it } from 'vitest'
import { checkUnseenNews, latestNewsDate, markNewsSeen, news, newsHeadline } from './news'
import { createSettings } from './settings'

describe('news', () => {
  beforeEach(() => localStorage.clear())

  it('lists the latest entry first', () => {
    expect(news[0].date).toBe(latestNewsDate)
    expect([...news].sort((a, b) => b.date.localeCompare(a.date))).toEqual(news)
  })

  it('falls back to the default headline for an entry without one', () => {
    expect(newsHeadline({ date: '2099-01-01', items: [] })).toBe('PKHeX.Web just got updated')
  })

  it('shows news newer than the date Blazor stored', () => {
    localStorage.setItem('lastDateNewsSeen', '2025-05-01')

    expect(checkUnseenNews(createSettings(localStorage), new Date(2026, 9, 2))).toEqual({
      since: '2025-05-01',
      unseen: true,
    })
  })

  it('hides news already seen', () => {
    localStorage.setItem('lastDateNewsSeen', latestNewsDate)

    expect(checkUnseenNews(createSettings(localStorage), new Date(2026, 9, 2)).unseen).toBe(false)
  })

  it('assumes a first visit has seen the news up to 30 days ago', () => {
    const settings = createSettings(localStorage)

    expect(checkUnseenNews(settings, new Date(2099, 0, 15))).toEqual({ since: null, unseen: false })
    expect(localStorage.getItem('lastDateNewsSeen')).toBe('2098-12-16')
  })

  it('marks the latest news as seen', () => {
    markNewsSeen(createSettings(localStorage))
    expect(localStorage.getItem('lastDateNewsSeen')).toBe(latestNewsDate)
  })
})
