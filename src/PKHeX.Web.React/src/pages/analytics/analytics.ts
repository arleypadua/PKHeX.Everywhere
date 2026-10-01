const baseUrl = 'https://raw.githubusercontent.com/pkhex-web/analytics/main/data'

interface GameLoaded {
  country: string
  versionName: string
  count: string
}

interface PokemonSaved {
  speciesId: string
  count: string
}

interface ItemChanged {
  itemId: string
  count: string
}

export interface Slice {
  type: string
  value: number
}

export interface Counted {
  id: number
  count: number
}

export interface AnalyticsResults {
  byCountry: Slice[]
  byVersion: Slice[]
  species: Counted[]
  items: Counted[]
}

async function fetchResult<T>(file: string): Promise<T[]> {
  const response = await fetch(`${baseUrl}/${file}`)
  if (!response.ok) throw new Error(`${file} returned ${response.status}`)
  return response.json()
}

function sumBy<T, K>(rows: T[], key: (row: T) => K, count: (row: T) => string): Map<K, number> {
  const sums = new Map<K, number>()
  for (const row of rows) sums.set(key(row), (sums.get(key(row)) ?? 0) + Number.parseInt(count(row), 10))
  return sums
}

function slices(sums: Map<string, number>): Slice[] {
  return [...sums].map(([type, value]) => ({ type, value })).filter((slice) => slice.value > 1)
}

function counted(sums: Map<number, number>): Counted[] {
  return [...sums].map(([id, count]) => ({ id, count })).filter((entry) => entry.count > 1)
}

export async function fetchAnalyticsResults(): Promise<AnalyticsResults> {
  const [gameLoads, pokemonSaved, itemsChanged] = await Promise.all([
    fetchResult<GameLoaded>('game_loaded_by_country.json'),
    fetchResult<PokemonSaved>('pokemon_saved_by_country.json'),
    fetchResult<ItemChanged>('items_changed_by_country.json'),
  ])

  return {
    byCountry: slices(sumBy(gameLoads, (load) => load.country, (load) => load.count)),
    byVersion: slices(sumBy(gameLoads, (load) => load.versionName, (load) => load.count)),
    species: counted(sumBy(pokemonSaved, (saved) => Number(saved.speciesId), (saved) => saved.count)),
    items: counted(sumBy(itemsChanged, (changed) => Number(changed.itemId), (changed) => changed.count)),
  }
}
