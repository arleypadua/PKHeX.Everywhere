import { describe, expect, it } from 'vitest'
import type { MoveSlot } from '@pkhex-everywhere/engine'
import { moveChoices, noMove } from './moveChoices'

const slot = (id: number, name: string, isUnknown = false): MoveSlot => ({ id, name, pp: 0, maxPp: 0, isUnknown })

describe('moveChoices', () => {
  const thunderbolt = { id: 85, name: 'Thunderbolt' }
  const surf = { id: 57, name: 'Surf' }
  const unknown = slot(9001, 'Unknown move #90', true)
  const slots = [slot(thunderbolt.id, thunderbolt.name), unknown, slot(0, '(None)'), slot(0, '(None)')]

  it('shows an unknown move in its slot', () => {
    expect(moveChoices(slots, [thunderbolt, surf])(unknown)).toEqual([{ id: unknown.id, name: unknown.name }, surf, noMove])
  })

  it('offers an unknown move only to the slot holding it', () => {
    expect(moveChoices(slots, [thunderbolt, surf])(slots[2])).toEqual([surf, noMove])
  })
})
