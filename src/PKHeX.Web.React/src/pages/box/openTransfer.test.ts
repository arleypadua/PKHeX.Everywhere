import { describe, expect, it, vi } from 'vitest'
import { EngineError, type SaveSummary, type TransferRoute, type TransferSummary } from '@pkhex-everywhere/engine'
import { openTransfer } from './openTransfer'

const file = new File([new Uint8Array(8)], 'Pokemon Emerald.sav')

const partner: SaveSummary = {
  fileName: 'Pokemon Emerald.sav',
  version: 'Emerald',
  generation: 3,
  hasEvents: true,
  hasMysteryGift: false,
  format: null,
  capabilities: [],
  statsApproximate: false,
}

const trainer = { id: '12345/54321', name: 'MAY', maxNameLength: 7, gender: 'female' as const, money: 3000, battlePoints: null, rival: null }

function summary(receive: TransferRoute[] | null): TransferSummary {
  return { partner, routes: { send: receive, receive }, room: { mine: 30, partner: 30 }, trainer }
}

function fakeTransfer(open: () => Promise<TransferSummary>) {
  return { open: vi.fn(open), close: vi.fn(async () => {}) }
}

describe('openTransfer', () => {
  it('opens a save with a route into this one', async () => {
    const opened = summary(['link'])
    const transfer = fakeTransfer(async () => opened)

    expect(await openTransfer(transfer, file)).toEqual({ kind: 'opened', summary: opened })
    expect(transfer.close).not.toHaveBeenCalled()
  })

  it('opens the save with the chosen format', async () => {
    const transfer = fakeTransfer(async () => summary(['link']))

    await openTransfer(transfer, file, 'radicalred')

    expect(transfer.open).toHaveBeenCalledWith(file, undefined, 'radicalred')
  })

  it('asks for the format of a save that might be a ROM hack', async () => {
    const candidates = [{ id: 'radicalred', name: 'Pokémon Radical Red' }]
    const transfer = fakeTransfer(async () => {
      throw new EngineError('format-choice-required', 'Choose a format.', candidates, 'FireRed')
    })

    expect(await openTransfer(transfer, file)).toEqual({ kind: 'formatChoice', candidates, pkhexGame: 'FireRed' })
  })

  it('reports an invalid save as unsupported', async () => {
    const transfer = fakeTransfer(async () => {
      throw new EngineError('invalid-save', 'Not a save.')
    })

    expect(await openTransfer(transfer, file)).toEqual({ kind: 'unsupported' })
  })

  it('closes the transfer when Pokémon cannot move into this save', async () => {
    const transfer = fakeTransfer(async () => summary(null))

    expect(await openTransfer(transfer, file)).toEqual({ kind: 'noRoute', partner })
    expect(transfer.close).toHaveBeenCalled()
  })

  it('leaves other errors alone', async () => {
    const transfer = fakeTransfer(async () => {
      throw new EngineError('no-save', 'No save.')
    })

    await expect(openTransfer(transfer, file)).rejects.toThrow('No save.')
  })
})
