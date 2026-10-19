import { describe, expect, it } from 'vitest'
import { expectPlaceholder, failToLoad, render } from '../testing/render'
import { PokemonSprite, pokemonIconUrl } from './PokemonSprite'

const iconsUrl = 'https://raw.githubusercontent.com/PokeAPI/sprites/ba403973fdf9d0ac59175813861e6a1f8ed5020c/sprites/pokemon/versions/generation-viii/icons'

describe('pokemonIconUrl', () => {
  it('returns the species icon', () => {
    expect(pokemonIconUrl({ speciesId: 25 })).toBe(`${iconsUrl}/25.png`)
  })

  it('no longer points at the deleted master folder', () => {
    const url = pokemonIconUrl({ speciesId: 25 })
    expect(url).not.toContain('/master/')
    expect(url).not.toContain('-alola')
  })

  it('builds no URL for an unknown species', () => {
    expect(pokemonIconUrl({ speciesId: null })).toBeNull()
  })
})

describe('PokemonSprite', () => {
  it('shows the species icon', () => {
    const { container } = render(<PokemonSprite pokemon={{ speciesId: 25, species: 'Pikachu' }} />)
    expect(container.querySelector('img')?.getAttribute('src')).toBe(`${iconsUrl}/25.png`)
  })

  it('shows the placeholder for an unknown species', () => {
    const { container } = render(<PokemonSprite pokemon={{ speciesId: null, species: '(Unknown)' }} />)
    expectPlaceholder(container, '(Unknown)')
  })

  it('shows the placeholder when the icon fails to load', () => {
    const { container } = render(<PokemonSprite pokemon={{ speciesId: 62146, species: 'Missing' }} />)
    failToLoad(container.querySelector('img')!)
    expectPlaceholder(container, 'Missing')
  })

  it('tries again when the species changes after a failed load', () => {
    const { container, rerender } = render(<PokemonSprite pokemon={{ speciesId: 62146, species: 'Missing' }} />)
    failToLoad(container.querySelector('img')!)
    rerender(<PokemonSprite pokemon={{ speciesId: 25, species: 'Pikachu' }} />)
    expect(container.querySelector('img')?.getAttribute('src')).toBe(`${iconsUrl}/25.png`)
  })
})
