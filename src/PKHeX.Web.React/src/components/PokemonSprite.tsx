import { useState } from 'react'
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

function PlaceholderSprite({ name, size }: PlaceholderSpriteProps) {
  return <QuestionCircleOutlined role="img" aria-label={name} title={name} style={{ fontSize: size }} />
}

interface SpeciesImageProps {
  name: string
  src: string | null
  placeholderSize: number
}

export function SpeciesImage({ name, src, placeholderSize }: SpeciesImageProps) {
  const [failedSrc, setFailedSrc] = useState<string | null>(null)
  if (src === null || src === failedSrc) return <PlaceholderSprite name={name} size={placeholderSize} />
  return <img alt={name} src={src} onError={() => setFailedSrc(src)} />
}

interface PokemonSpriteProps {
  pokemon: SpriteSpecies & Pick<PokemonSummary, 'species'>
}

export function PokemonSprite({ pokemon }: PokemonSpriteProps) {
  return <SpeciesImage name={pokemon.species} src={pokemonIconUrl(pokemon)} placeholderSize={32} />
}
