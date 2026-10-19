import { act } from 'react'
import { beforeAll, describe, expect, it, vi } from 'vitest'
import { App } from 'antd'
import {
  EngineError,
  type Engine,
  type PokemonHandle,
  type PokemonId,
  type PokemonSummary,
  type SaveSummary,
  type TransferOffer,
  type TransferPreview,
  type TransferResult,
  type TransferSummary,
} from '@pkhex-everywhere/engine'
import { EngineProvider } from '@pkhex-everywhere/react'
import { downloadFile } from '../../host'
import { render } from '../../testing/render'
import { TransferModal } from './TransferModal'

vi.mock('../../host', async (original) => ({ ...(await original<typeof import('../../host')>()), downloadFile: vi.fn() }))

const emerald: SaveSummary = {
  fileName: 'mine.sav',
  version: 'Emerald',
  generation: 3,
  hasEvents: true,
  hasMysteryGift: false,
  format: null,
  capabilities: [],
  statsApproximate: false,
}

const partnerFile = new File([new Uint8Array(8)], 'partner.sav')

const opened: TransferSummary = {
  partner: { ...emerald, fileName: 'partner.sav' },
  routes: { send: ['link'], receive: ['link'] },
  room: { mine: 30, partner: 30 },
  trainer: { id: '12345/54321', name: 'MAY', maxNameLength: 7, gender: 'female', money: 3000, battlePoints: null, rival: null },
}

function pokemon(species: string, at: PokemonHandle): PokemonSummary {
  return {
    id: species as unknown as PokemonId,
    at,
    speciesId: 1,
    speciesIndex: 1,
    species,
    isUnknown: false,
    editable: true,
    form: { id: 0, name: '' },
    nickname: species,
    level: 30,
    isShiny: false,
    gender: 'male',
    isEgg: false,
    types: [],
  }
}

const kadabra = pokemon('Kadabra', { source: 'party', slot: 0 })
const swampert = pokemon('Swampert', { source: 'party', slot: 1 })
const zigzagoon = pokemon('Zigzagoon', { source: 'box', box: 0, slot: 3 })

const movable = async (offer: TransferOffer): Promise<TransferPreview> => ({
  offers: offer.receive.map((at) => ({
    direction: 'receive',
    route: ['link'],
    from: pokemon('', at),
    arrives: pokemon('', { source: 'box', box: 0, slot: 0 }),
    changes: [],
    saveChanges: [],
    legality: null,
  })),
  refused: [],
})

const moved: TransferResult = {
  save: { bytes: new Uint8Array([1]), fileName: 'mine.sav' },
  partner: { bytes: new Uint8Array([2]), fileName: 'partner.sav' },
  arrived: [],
}

beforeAll(() => {
  vi.stubGlobal('matchMedia', () => ({ matches: false, addEventListener: () => {}, removeEventListener: () => {} }))
  vi.stubGlobal(
    'ResizeObserver',
    class {
      observe() {}
      unobserve() {}
      disconnect() {}
    },
  )
})

const flush = () => act(async () => {})

function renderTransfer(
  open: Engine['transfer']['open'],
  { preview = movable }: { preview?: Engine['transfer']['preview'] } = {},
) {
  const transfer = {
    open: vi.fn(open),
    close: vi.fn(async () => {}),
    partnerBoxes: vi.fn(async () => [kadabra, swampert, zigzagoon]),
    preview: vi.fn(preview),
    commit: vi.fn(async (offer: TransferOffer) => ({
      ...moved,
      arrived: offer.receive.map((at) => ({ direction: 'receive' as const, id: '' as unknown as PokemonId, at })),
    })),
  }
  const engine = { subscribe: () => () => {}, transfer } as unknown as Engine
  const onClose = vi.fn()
  render(
    <App>
      <EngineProvider engine={engine}>
        <TransferModal game={emerald} onClose={onClose} />
      </EngineProvider>
    </App>,
  )
  const dialogs = () => [...document.querySelectorAll('[role="dialog"]')]
  return {
    transfer,
    onClose,
    text: () => dialogs()[0].textContent,
    openSave: async (file: File) => {
      const input = dialogs()[0].querySelector('input[type="file"]')!
      Object.defineProperty(input, 'files', { value: [file], configurable: true })
      act(() => {
        input.dispatchEvent(new Event('change', { bubbles: true }))
      })
      await flush()
    },
    click: async (label: string) => {
      const button = [...document.querySelectorAll('button')].find(
        (b) => b.textContent === label || b.getAttribute('aria-label') === label,
      )!
      act(() => button.click())
      await flush()
    },
    button: (label: string) => [...document.querySelectorAll('button')].find((b) => b.textContent === label),
    rows: () => [...dialogs()[0].querySelectorAll('tbody tr[data-row-key]')].map((row) => row.textContent),
    pick: async (species: string) => {
      const row = [...dialogs()[0].querySelectorAll('tbody tr[data-row-key]')].find((r) => r.textContent?.includes(species))!
      act(() => row.querySelector<HTMLInputElement>('input[type="checkbox"]')!.click())
      await flush()
    },
    choose: async (label: string) => {
      const option = [...document.querySelectorAll('label')].find((l) => l.textContent === label)!
      act(() => option.click())
      await flush()
    },
    notifications: () => document.querySelector('.ant-notification')?.textContent ?? '',
  }
}

describe('TransferModal', () => {
  it('asks for another save of the loaded game', () => {
    const modal = renderTransfer(async () => opened)

    expect(modal.text()).toContain('Emerald')
  })

  it('shows the partner once it opens', async () => {
    const modal = renderTransfer(async () => opened)

    await modal.openSave(partnerFile)

    expect(modal.transfer.open).toHaveBeenCalledWith(partnerFile, undefined, undefined)
    expect(modal.text()).toContain('Transfer from partner.sav')
    expect(modal.text()).toContain('MAY')
  })

  it('opens an ambiguous save with the format the player picks', async () => {
    const candidates = [{ id: 'radicalred', name: 'Pokémon Radical Red' }]
    const modal = renderTransfer(async (_data, _name, formatId) => {
      if (!formatId) throw new EngineError('format-choice-required', 'Choose a format.', candidates)
      return opened
    })

    await modal.openSave(partnerFile)
    await modal.choose('Pokémon Radical Red')
    await modal.click('Open')

    expect(modal.transfer.open).toHaveBeenLastCalledWith(partnerFile, undefined, 'radicalred')
    expect(modal.text()).toContain('partner.sav')
  })

  it('closes the transfer when the save has no route into this one', async () => {
    const ruby = { ...opened.partner, version: 'Ruby' }
    const modal = renderTransfer(async () => ({ ...opened, partner: ruby, routes: { send: null, receive: null } }))

    await modal.openSave(partnerFile)

    expect(modal.transfer.close).toHaveBeenCalled()
    expect(modal.notifications()).toContain('Ruby')
    expect(modal.text()).not.toContain('partner.sav')
  })

  it('reports a file that is not a save', async () => {
    const modal = renderTransfer(async () => {
      throw new EngineError('invalid-save', 'Not a save.')
    })

    await modal.openSave(partnerFile)

    expect(modal.notifications()).toContain('partner.sav')
  })

  it('closes the transfer on Cancel', async () => {
    const modal = renderTransfer(async () => opened)

    await modal.click('Cancel')

    expect(modal.transfer.close).toHaveBeenCalled()
    expect(modal.onClose).toHaveBeenCalled()
  })

  it('closes the transfer from the close button once a partner is open', async () => {
    const modal = renderTransfer(async () => opened)

    await modal.openSave(partnerFile)
    await modal.click('Close')

    expect(modal.transfer.close).toHaveBeenCalled()
    expect(modal.onClose).toHaveBeenCalled()
  })

  it("lists the partner's party, and its boxes on the Box switch", async () => {
    const modal = renderTransfer(async () => opened)
    await modal.openSave(partnerFile)

    expect(modal.rows()).toEqual([expect.stringContaining('Kadabra'), expect.stringContaining('Swampert')])

    await modal.choose('Box')

    expect(modal.rows()).toEqual([expect.stringContaining('Zigzagoon')])
  })

  it('moves the Pokémon picked from the party and the boxes into this save', async () => {
    const modal = renderTransfer(async () => opened)
    await modal.openSave(partnerFile)

    await modal.pick('Kadabra')
    await modal.choose('Box')
    await modal.pick('Zigzagoon')

    expect(modal.text()).toContain('2 selected · 30 free slots')
    expect(modal.transfer.preview).toHaveBeenLastCalledWith({ send: [], receive: [kadabra.at, zigzagoon.at] })

    await modal.click('Move 2 here')

    expect(modal.transfer.commit).toHaveBeenCalledWith({ send: [], receive: [kadabra.at, zigzagoon.at] })
    expect(modal.transfer.close).toHaveBeenCalled()
    expect(modal.onClose).toHaveBeenCalled()
    expect(modal.notifications()).toContain('2 Pokémon moved to your box')
  })

  it('downloads the partner save without the moved Pokémon from the notification', async () => {
    const modal = renderTransfer(async () => opened)
    await modal.openSave(partnerFile)
    await modal.pick('Kadabra')
    await modal.click('Move 1 here')

    await modal.click('Download partner.sav')

    expect(downloadFile).toHaveBeenCalledWith(moved.partner.bytes, 'partner.sav')
  })

  it("can't move with nothing picked", async () => {
    const modal = renderTransfer(async () => opened)
    await modal.openSave(partnerFile)

    expect(modal.button('Move 0 here')?.disabled).toBe(true)
  })

  it("can't move more Pokémon than this save has room for", async () => {
    const modal = renderTransfer(async () => ({ ...opened, room: { mine: 1, partner: 30 } }))
    await modal.openSave(partnerFile)

    await modal.pick('Kadabra')
    await modal.pick('Swampert')

    expect(modal.button('Move 2 here')?.disabled).toBe(true)
  })

  it('shows why a Pokémon is refused', async () => {
    const modal = renderTransfer(async () => opened, {
      preview: async (offer) => ({
        offers: [],
        refused: offer.receive.map((at) => ({ direction: 'receive', at, reason: 'lastPartyMember' })),
      }),
    })
    await modal.openSave(partnerFile)

    await modal.pick('Kadabra')

    expect(modal.rows()[0]).toContain('The party must keep a Pokémon')
    expect(modal.button('Move 1 here')?.disabled).toBe(true)
  })

  it('shows what moving a Pokémon changes', async () => {
    const modal = renderTransfer(async () => opened, {
      preview: async (offer) => {
        const preview = await movable(offer)
        preview.offers[0].changes = [{ field: 'species', before: 'Kadabra', after: 'Alakazam', reason: 'tradeEvolution' }]
        return preview
      },
    })
    await modal.openSave(partnerFile)

    await modal.pick('Kadabra')

    expect(modal.rows()[0]).toContain('Kadabra → Alakazam')
  })
})
