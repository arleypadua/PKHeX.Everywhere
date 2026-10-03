import { describe, expect, it } from 'vitest'
import { pokemonIconUrl } from './PokemonSprite'

const iconsUrl = 'https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/versions/generation-viii/icons'

describe('pokemonIconUrl', () => {
  it('returns the species icon', () => {
    expect(pokemonIconUrl({ speciesId: 25 })).toBe(`${iconsUrl}/25.png`)
  })

  it('returns the regional form icon', () => {
    expect(pokemonIconUrl({ speciesId: 26, form: { id: 1, name: 'Alola' } })).toBe(`${iconsUrl}/26-alola.png`)
  })

  it('builds no URL for an unknown species', () => {
    expect(pokemonIconUrl({ speciesId: null })).toBeNull()
  })
})
