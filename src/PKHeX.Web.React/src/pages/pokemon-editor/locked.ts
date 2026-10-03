import type { PokemonField, PokemonOptions } from '@pkhex-everywhere/engine'

export function isLocked(options: PokemonOptions, field: PokemonField): boolean {
  return options.locked.includes(field)
}
