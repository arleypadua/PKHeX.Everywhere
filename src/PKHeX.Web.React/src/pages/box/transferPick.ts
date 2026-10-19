import type { PokemonHandle, TransferPreview, TransferRoom } from '@pkhex-everywhere/engine'

export interface TransferPick {
  summary: string
  moveLabel: string
  canMove: boolean
}

export function transferPick(picked: PokemonHandle[], room: TransferRoom, preview: TransferPreview | undefined): TransferPick {
  const count = picked.length
  const refused = preview?.refused.length ?? 0
  return {
    summary: `${count} selected · ${room.mine} free slots${refused ? ` · ${refused} can't move` : ''}`,
    moveLabel: `Move ${count} here`,
    canMove: count > 0 && count <= room.mine && preview?.refused.length === 0,
  }
}
