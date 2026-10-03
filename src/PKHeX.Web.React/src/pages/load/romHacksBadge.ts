import { formatDate, romHacksNewsDate } from '../../news'

export function showRomHacksBadge(today: Date): boolean {
  const [year, month, day] = romHacksNewsDate.split('-').map(Number)
  const end = formatDate(new Date(year, month + 3, day))
  const date = formatDate(today)
  return date >= romHacksNewsDate && date < end
}
