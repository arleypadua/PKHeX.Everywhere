import { describe, expect, it } from 'vitest'
import { expectPlaceholder, failToLoad, render } from '../testing/render'
import { PokemonImage, pokemonImageUrl } from './PokemonImage'

const showdownUrl = 'https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/showdown'

describe('pokemonImageUrl', () => {
  it('returns the animated showdown sprite for a non-shiny Pokémon', () => {
    expect(pokemonImageUrl(25, false)).toBe(`${showdownUrl}/25.gif`)
  })

  it('returns the shiny animated showdown sprite for a shiny Pokémon', () => {
    expect(pokemonImageUrl(25, true)).toBe(`${showdownUrl}/shiny/25.gif`)
  })

  it('builds no URL for an unknown species', () => {
    expect(pokemonImageUrl(null, false)).toBeNull()
    expect(pokemonImageUrl(null, true)).toBeNull()
  })

  it('does not use the small generation VIII icon', () => {
    expect(pokemonImageUrl(25, false)).not.toContain('generation-viii/icons')
    expect(pokemonImageUrl(25, true)).not.toContain('generation-viii/icons')
  })
})

describe('PokemonImage', () => {
  it('shows the showdown sprite', () => {
    const { container } = render(<PokemonImage pokemon={{ speciesId: 25, species: 'Pikachu', isShiny: false }} />)
    expect(container.querySelector('img')?.getAttribute('src')).toBe(`${showdownUrl}/25.gif`)
  })

  it('shows the placeholder for an unknown species', () => {
    const { container } = render(<PokemonImage pokemon={{ speciesId: null, species: '(Unknown)', isShiny: false }} />)
    expectPlaceholder(container, '(Unknown)')
  })

  it('shows the placeholder when the sprite fails to load', () => {
    const { container } = render(<PokemonImage pokemon={{ speciesId: 62146, species: 'Missing', isShiny: true }} />)
    failToLoad(container.querySelector('img')!)
    expectPlaceholder(container, 'Missing')
  })
})
