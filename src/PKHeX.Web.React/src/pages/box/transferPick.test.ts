import { describe, expect, it } from 'vitest'
import type { PokemonHandle, TransferPreview } from '@pkhex-everywhere/engine'
import { transferPick } from './transferPick'

const room = { mine: 2, partner: 30 }
const party0: PokemonHandle = { source: 'party', slot: 0 }
const box3: PokemonHandle = { source: 'box', box: 0, slot: 3 }
const box4: PokemonHandle = { source: 'box', box: 0, slot: 4 }
const movable: TransferPreview = { offers: [], refused: [] }

describe('transferPick', () => {
  it('counts the picked Pokémon against the free slots', () => {
    expect(transferPick([party0, box3], room, movable)).toEqual({ selected: 2, freeSlots: 2, refused: 0, canMove: true })
  })

  it("can't move nothing", () => {
    expect(transferPick([], room, undefined).canMove).toBe(false)
  })

  it("can't move more Pokémon than fit", () => {
    expect(transferPick([party0, box3, box4], room, movable).canMove).toBe(false)
  })

  it('waits for the preview of the selection', () => {
    expect(transferPick([party0], room, undefined).canMove).toBe(false)
  })

  it("can't move while the preview refuses a Pokémon", () => {
    const refused: TransferPreview = { offers: [], refused: [{ direction: 'receive', at: party0, reason: 'lastPartyMember' }] }

    expect(transferPick([party0], room, refused)).toMatchObject({ refused: 1, canMove: false })
  })
})
