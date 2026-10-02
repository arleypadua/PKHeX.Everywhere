import { describe, expect, it } from 'vitest'
import { pokemonImageUrl } from './PokemonImage'

const showdownUrl = 'https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/showdown'

describe('pokemonImageUrl', () => {
  it('returns the animated showdown sprite for a non-shiny Pokémon', () => {
    expect(pokemonImageUrl(25, false)).toBe(`${showdownUrl}/25.gif`)
  })

  it('returns the shiny animated showdown sprite for a shiny Pokémon', () => {
    expect(pokemonImageUrl(25, true)).toBe(`${showdownUrl}/shiny/25.gif`)
  })

  it('does not use the small generation VIII icon', () => {
    expect(pokemonImageUrl(25, false)).not.toContain('generation-viii/icons')
    expect(pokemonImageUrl(25, true)).not.toContain('generation-viii/icons')
  })
})
