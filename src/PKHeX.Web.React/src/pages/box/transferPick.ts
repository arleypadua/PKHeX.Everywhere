import type { PokemonHandle, TransferPreview, TransferRoom } from '@pkhex-everywhere/engine'

export interface TransferPick {
  selected: number
  freeSlots: number
  refused: number
  canMove: boolean
}

export function transferPick(picked: PokemonHandle[], room: TransferRoom, preview: TransferPreview | undefined): TransferPick {
  const refused = preview?.refused.length ?? 0
  return {
    selected: picked.length,
    freeSlots: room.mine,
    refused,
    canMove: picked.length > 0 && picked.length <= room.mine && preview !== undefined && refused === 0,
  }
}
