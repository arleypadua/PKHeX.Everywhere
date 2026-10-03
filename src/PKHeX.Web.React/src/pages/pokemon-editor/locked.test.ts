import { describe, expect, it } from 'vitest'
import type { PokemonOptions } from '@pkhex-everywhere/engine'
import { isLocked } from './locked'

const options = (locked: PokemonOptions['locked']): PokemonOptions => ({
  species: [],
  abilities: [],
  forms: [],
  metLocations: [],
  moves: [],
  locked,
})

describe('isLocked', () => {
  it('disables the nature when the save locks it', () => {
    expect(isLocked(options(['nature']), 'nature')).toBe(true)
  })

  it('leaves the nature editable otherwise', () => {
    expect(isLocked(options([]), 'nature')).toBe(false)
    expect(isLocked(options(['ability']), 'nature')).toBe(false)
  })
})
