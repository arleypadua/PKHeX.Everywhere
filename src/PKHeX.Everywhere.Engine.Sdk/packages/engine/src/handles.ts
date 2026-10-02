import type { PokemonHandle } from './generated/types'

/** The handle of the open draft. Pass it to `pokemon.*` calls to read or change the draft. */
export const draftHandle: PokemonHandle = { source: 'draft', slot: 0 }
