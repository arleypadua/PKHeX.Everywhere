import { describe, expect, it, vi } from 'vitest'
import { boxActions } from './boxActions'

const handlers = {
  add: vi.fn(),
  load: vi.fn(),
  calculator: vi.fn(),
  showdown: vi.fn(),
  transfer: vi.fn(),
}

const labels = (supports: { encounters: boolean; showdown: boolean }) =>
  boxActions(supports, handlers).map((action) => action.label)

describe('boxActions', () => {
  it('ends with Transfer', () => {
    expect(labels({ encounters: true, showdown: true })).toEqual(['Add', 'Load *.pk', 'Calculator', 'Showdown', 'Transfer'])
  })

  it('keeps Load *.pk first on a save without encounters or Showdown', () => {
    expect(labels({ encounters: false, showdown: false })).toEqual(['Load *.pk', 'Transfer'])
  })

  it('opens the transfer from Transfer', () => {
    boxActions({ encounters: true, showdown: true }, handlers).at(-1)!.onClick()

    expect(handlers.transfer).toHaveBeenCalled()
  })
})
