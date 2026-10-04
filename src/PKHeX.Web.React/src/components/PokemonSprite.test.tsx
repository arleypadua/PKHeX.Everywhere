import { describe, expect, it } from 'vitest'
import { failToLoad, render } from '../testing/render'
import { PokemonSprite, pokemonIconUrl } from './PokemonSprite'

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

describe('PokemonSprite', () => {
  it('shows the species icon', () => {
    const { container } = render(<PokemonSprite pokemon={{ speciesId: 25, species: 'Pikachu' }} />)
    expect(container.querySelector('img')?.getAttribute('src')).toBe(`${iconsUrl}/25.png`)
  })

  it('shows the placeholder for an unknown species', () => {
    const { container } = render(<PokemonSprite pokemon={{ speciesId: null, species: '(Unknown)' }} />)
    expect(container.querySelector('img')).toBeNull()
    expect(container.querySelector('[role="img"]')?.getAttribute('aria-label')).toBe('(Unknown)')
  })

  it('shows the placeholder when the icon fails to load', () => {
    const { container } = render(<PokemonSprite pokemon={{ speciesId: 62146, species: 'Missing' }} />)
    failToLoad(container.querySelector('img')!)
    expect(container.querySelector('img')).toBeNull()
    expect(container.querySelector('[role="img"]')?.getAttribute('aria-label')).toBe('Missing')
  })

  it('tries again when the species changes after a failed load', () => {
    const { container, rerender } = render(<PokemonSprite pokemon={{ speciesId: 62146, species: 'Missing' }} />)
    failToLoad(container.querySelector('img')!)
    rerender(<PokemonSprite pokemon={{ speciesId: 25, species: 'Pikachu' }} />)
    expect(container.querySelector('img')?.getAttribute('src')).toBe(`${iconsUrl}/25.png`)
  })
})
