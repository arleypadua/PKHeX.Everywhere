import type { Settings } from './settings'

export interface NewsEntry {
  date: string
  headline?: string
  items: string[]
}

export const romHacksNewsDate = '2026-10-04'

export const news: NewsEntry[] = [
  {
    date: romHacksNewsDate,
    headline: 'PKHeX.Web now opens Unbound, Radical Red and Emerald Imperium saves.',
    items: ['Added support for Pokémon Unbound, Radical Red and Emerald Imperium saves.'],
  },
  {
    date: '2026-10-01',
    items: [
      'Added an Alpha toggle to the Pokémon editor.',
      'Added a Show all moves checkbox to the Moves tab.',
      'The Stats tab shows Hidden Power type and power.',
      "Imported Pokémon files convert to the loaded save's format.",
      'Action menus open on click instead of hover.',
      'Encounter Search preselects the game version on your first visit.',
      'Trainer name is editable on the Home page.',
      'Pre-Gen7 Pokémon show their 16-bit OT TID.',
      'Gen1/2 Pokémon of the same species no longer open the wrong one.',
      'Plug-in updates no longer crash on a 404.',
      'Search works in dropdowns again.',
      'Empty Legends: Arceus saves without item pockets load.',
      'The analytics page shows an alert when results fail to load.',
      'Money edits are disabled on Legends: Z-A saves without a money block.',
      'TM item icons load, and unnamed Z-A items are skipped.',
      'The Moves tab no longer crashes on Battle Revolution saves or unknown moves.',
      'The Pokémon editor no longer crashes on species missing from the loaded game.',
      'Demo loads a bundled Emerald save.',
      'Encounter Search sends you to the load page when no save is loaded.',
      "Let's Go party edits no longer crash on saves with empty slots.",
      'Malformed save files show an error instead of crashing.',
      'Fixed a unique id error in box rows.',
    ],
  },
  {
    date: '2026-03-20',
    items: [
      'Upgraded PKHeX.Core and ALM plugins to latest version',
      'Upgraded from .NET 9.x to 10.x',
      'Small bug fixes',
    ],
  },
  {
    date: '2025-05-01',
    items: [
      "Fixed exporting of Let's Go Eevee save files.",
      'Update to privacy policy and cookie consent.',
      'Upgraded PKHeX.Core and ALM plugins to latest version',
    ],
  },
  {
    date: '2025-03-25',
    items: [
      'Introduced plugin error pages showing a list of errors whenever executing a plugin',
      'Upgraded PKHeX.Core and ALM plugins to latest version',
    ],
  },
  {
    date: '2025-02-14',
    items: [
      'Minor bug fixes',
      'News banner',
    ],
  },
  {
    date: '2024-11-30',
    items: [
      'Migrated to .NET 9',
      'Upgraded PKHeX.Core and ALM plugins to latest',
      'Added support for Battle Points.',
    ],
  },
  {
    date: '2024-11-29',
    items: [
      'Introduced the PKHeX.Web Cloud (alpha) with up to 6 Pokémon stored in the cloud.',
    ],
  },
]

export const latestNewsDate = news[0].date

export function newsHeadline(entry: NewsEntry) {
  return entry.headline ?? 'PKHeX.Web just got updated'
}

export function checkUnseenNews(settings: Settings, today: Date) {
  const since = settings.readLastDateNewsSeen()
  const lastSeen = since ?? formatDate(daysBefore(today, 30))
  if (since === null) settings.writeLastDateNewsSeen(lastSeen)
  return { since, unseen: lastSeen < latestNewsDate }
}

export function markNewsSeen(settings: Settings) {
  settings.writeLastDateNewsSeen(latestNewsDate)
}

function daysBefore(date: Date, days: number) {
  return new Date(date.getFullYear(), date.getMonth(), date.getDate() - days)
}

export function formatDate(date: Date) {
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`
}
