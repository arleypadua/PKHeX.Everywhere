import type { PokemonSummary } from '@pkhex-everywhere/engine'

const iconsUrl = 'https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/versions/generation-viii/icons'

const formSuffixes: Record<string, string> = {
  alola: '-alola',
  galar: '-galar',
}

export function PokemonSprite({ pokemon }: { pokemon: Pick<PokemonSummary, 'speciesId' | 'species' | 'form'> }) {
  const suffix = formSuffixes[pokemon.form.name.toLowerCase()] ?? ''
  return <img alt={pokemon.species} src={`${iconsUrl}/${pokemon.speciesId}${suffix}.png`} />
}
