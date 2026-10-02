import type { PokemonId, PokemonSummary } from '@pkhex-everywhere/engine'

export const routes = {
  pokemon: ({ at, id }: Pick<PokemonSummary, 'at' | 'id'>) => `/pokemon/${at.source}/${id}`,
  pokemonEditorPreview: ({ at, id }: Pick<PokemonSummary, 'at' | 'id'>) => `/pokemon-next/${at.source}/${id}`,
  box: '/pokemon-box',
  searchEncounter: '/pokemon/search-encounter',
  clonePokemon: (id: PokemonId) => `/pokemon/${id}/clone`,
  encounters: ({ version, species }: { version: number; species?: number }) =>
    `/pokemon/search-encounter?${new URLSearchParams({ version: String(version), ...(species && { species: String(species) }) })}`,
}
