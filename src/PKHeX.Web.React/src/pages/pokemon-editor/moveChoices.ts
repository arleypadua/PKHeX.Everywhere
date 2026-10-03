import type { Choice, MoveSlot } from '@pkhex-everywhere/engine'

export const noMove: Choice = { id: 0, name: '(None)' }

export function moveChoices(slots: MoveSlot[], offered: Choice[]): (slot: MoveSlot) => Choice[] {
  const assigned = new Set(slots.map((slot) => slot.id).filter((id) => id !== noMove.id))
  const available = offered.filter((move) => !assigned.has(move.id))
  return (slot) => [...(slot.id === noMove.id ? [] : [{ id: slot.id, name: slot.name }]), ...available, noMove]
}
