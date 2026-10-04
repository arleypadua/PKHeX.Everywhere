import { Badge } from 'antd'
import type { PokemonSummary } from '@pkhex-everywhere/engine'
import { SpeciesImage } from './PokemonSprite'

const showdownUrl = 'https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/showdown'

export function pokemonImageUrl(speciesId: number | null, isShiny: boolean): string | null {
  if (speciesId === null) return null
  return isShiny ? `${showdownUrl}/shiny/${speciesId}.gif` : `${showdownUrl}/${speciesId}.gif`
}

interface PokemonImageProps {
  pokemon: Pick<PokemonSummary, 'speciesId' | 'species' | 'isShiny'>
}

export function PokemonImage({ pokemon }: PokemonImageProps) {
  const image = <SpeciesImage name={pokemon.species} src={pokemonImageUrl(pokemon.speciesId, pokemon.isShiny)} size={64} />
  if (!pokemon.isShiny) return image
  return (
    <Badge.Ribbon text="shiny" color="orange">
      {image}
    </Badge.Ribbon>
  )
}
