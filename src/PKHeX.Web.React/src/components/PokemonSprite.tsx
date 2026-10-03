import { QuestionCircleOutlined } from '@ant-design/icons'
import type { PokemonSummary } from '@pkhex-everywhere/engine'

const iconsUrl = 'https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/versions/generation-viii/icons'

const formSuffixes: Record<string, string> = {
  alola: '-alola',
  galar: '-galar',
}

type SpriteSpecies = Pick<PokemonSummary, 'speciesId'> & Partial<Pick<PokemonSummary, 'form'>>

export function pokemonIconUrl(pokemon: SpriteSpecies): string | null {
  if (pokemon.speciesId === null) return null
  const suffix = formSuffixes[pokemon.form?.name.toLowerCase() ?? ''] ?? ''
  return `${iconsUrl}/${pokemon.speciesId}${suffix}.png`
}

interface PlaceholderSpriteProps {
  name: string
  size: number
}

export function PlaceholderSprite({ name, size }: PlaceholderSpriteProps) {
  return <QuestionCircleOutlined role="img" aria-label={name} title={name} style={{ fontSize: size }} />
}

interface PokemonSpriteProps {
  pokemon: SpriteSpecies & Pick<PokemonSummary, 'species'>
}

export function PokemonSprite({ pokemon }: PokemonSpriteProps) {
  const url = pokemonIconUrl(pokemon)
  if (url === null) return <PlaceholderSprite name={pokemon.species} size={32} />
  return <img alt={pokemon.species} src={url} />
}
