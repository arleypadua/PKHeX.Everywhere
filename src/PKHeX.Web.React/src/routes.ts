import type { PokemonId, PokemonSummary } from '@pkhex-everywhere/engine'

export const routes = {
  pokemon: ({ at, id }: Pick<PokemonSummary, 'at' | 'id'>) => `/pokemon/${at.source}/${id}`,
  clonePokemon: (id: PokemonId) => `/pokemon/${id}/clone`,
}
