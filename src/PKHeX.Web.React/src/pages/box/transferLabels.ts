import type { TransferChange, TransferField, TransferRefusal } from '@pkhex-everywhere/engine'

export const refusalReasons: Record<TransferRefusal, string> = {
  noRoute: "Pokémon can't move between these games.",
  slotLocked: 'Its box slot is locked.',
  eggAcrossGenerations: 'Eggs only move by link trade.',
  languageMismatch: "Its language can't move to this game.",
  speciesNotInGame: "This game doesn't have its species or form.",
  lastPartyMember: "The party must keep a Pokémon that isn't an egg.",
  noRoom: 'No empty box slot is left for it.',
  bagFull: "The bag has no room for the Griseous Orb it holds.",
  conversionFailed: "It can't be converted to this game.",
  notInGame: "This game doesn't have one of its moves, its held item or its ability.",
  fused: "Pokémon Bank won't take a fused Kyurem.",
  holdingItem: "Poké Transporter won't take it while it holds an item.",
  shadow: "A shadow Pokémon can't be traded until it's purified.",
}

const fieldNames: Record<TransferField, string> = {
  species: 'Species',
  form: 'Form',
  heldItem: 'Held item',
  moves: 'Move',
  metLocation: 'Met location',
  metLevel: 'Met level',
  ball: 'Ball',
  friendship: 'Friendship',
  nickname: 'Nickname',
  ability: 'Ability',
  level: 'Level',
  nature: 'Nature',
  gender: 'Gender',
  shiny: 'Shiny',
  language: 'Language',
  originalTrainer: 'Original trainer',
  trainerId: 'Trainer ID',
  originGame: 'Origin game',
  metDate: 'Met date',
}

export const describeChange = (change: TransferChange) =>
  `${fieldNames[change.field]}: ${change.before ?? '—'} → ${change.after ?? '—'}`
