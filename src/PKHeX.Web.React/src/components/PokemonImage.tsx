import { Badge } from 'antd'
import type { PokemonSummary } from '@pkhex-everywhere/engine'

const showdownUrl = 'https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/showdown'

export function pokemonImageUrl(speciesId: number, isShiny: boolean): string {
  return isShiny ? `${showdownUrl}/shiny/${speciesId}.gif` : `${showdownUrl}/${speciesId}.gif`
}

interface PokemonImageProps {
  pokemon: Pick<PokemonSummary, 'speciesId' | 'species' | 'isShiny'>
}

export function PokemonImage({ pokemon }: PokemonImageProps) {
  const image = <img alt={pokemon.species} src={pokemonImageUrl(pokemon.speciesId, pokemon.isShiny)} />
  if (!pokemon.isShiny) return image
  return (
    <Badge.Ribbon text="shiny" color="orange">
      {image}
    </Badge.Ribbon>
  )
}
