import type { PokemonSummary } from '@pkhex-everywhere/engine'

export const routes = {
  pokemon: ({ at, id }: Pick<PokemonSummary, 'at' | 'id'>) => `/pokemon/${at.source}/${id}`,
  box: '/pokemon-box',
  plugIns: '/plugins',
  plugIn: (id: string) => `/plugins/${id}`,
  searchEncounter: '/pokemon/search-encounter',
  clonePokemon: ({ at, id }: Pick<PokemonSummary, 'at' | 'id'>) => `/pokemon/${at.source}/${id}/clone`,
  encounters: ({ version, species }: { version: number; species?: number }) =>
    `/pokemon/search-encounter?${new URLSearchParams({ version: String(version), ...(species && { species: String(species) }) })}`,
}
