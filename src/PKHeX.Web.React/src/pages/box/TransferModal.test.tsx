import { act } from 'react'
import { beforeAll, describe, expect, it, vi } from 'vitest'
import { App } from 'antd'
import { EngineError, type Engine, type SaveSummary, type TransferSummary } from '@pkhex-everywhere/engine'
import { EngineProvider } from '@pkhex-everywhere/react'
import { render } from '../../testing/render'
import { TransferModal } from './TransferModal'

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
}

beforeAll(() => {
  vi.stubGlobal('matchMedia', () => ({ matches: false, addEventListener: () => {}, removeEventListener: () => {} }))
})

const flush = () => act(async () => {})

function renderTransfer(open: Engine['transfer']['open']) {
  const transfer = { open: vi.fn(open), close: vi.fn(async () => {}) }
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
    choose: (label: string) => {
      const option = [...document.querySelectorAll('label')].find((l) => l.textContent === label)!
      act(() => option.click())
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
    expect(modal.text()).toContain('partner.sav')
  })

  it('opens an ambiguous save with the format the player picks', async () => {
    const candidates = [{ id: 'radicalred', name: 'Pokémon Radical Red' }]
    const modal = renderTransfer(async (_data, _name, formatId) => {
      if (!formatId) throw new EngineError('format-choice-required', 'Choose a format.', candidates)
      return opened
    })

    await modal.openSave(partnerFile)
    modal.choose('Pokémon Radical Red')
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
})
