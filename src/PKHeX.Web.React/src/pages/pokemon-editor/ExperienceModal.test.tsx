import { act } from 'react'
import { beforeAll, describe, expect, it, vi } from 'vitest'
import { App } from 'antd'
import { draftHandle, type EditablePokemon, type Engine } from '@pkhex-everywhere/engine'
import { EngineProvider } from '@pkhex-everywhere/react'
import { render } from '../../testing/render'
import { ExperienceModal } from './ExperienceModal'

const draft = vi.hoisted(() => ({ details: undefined as unknown as EditablePokemon }))

vi.mock('@pkhex-everywhere/react', async (original) => {
  const actual = await original<typeof import('@pkhex-everywhere/react')>()
  return {
    ...actual,
    usePokemonDetails: (at: unknown) => {
      const engine = actual.useEngine()
      return { details: draft.details, update: (patch: unknown) => (engine as unknown as Engine).pokemon.update(at as never, patch as never) }
    },
  }
})

function detailsWith({ experience, min, max, level }: { experience: number; min: number; max: number; level: number }) {
  return { experience, experienceRange: { min, max }, level } as unknown as EditablePokemon
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

function renderModal(details: EditablePokemon) {
  draft.details = details
  const update = vi.fn(async () => {})
  const engine = { subscribe: () => () => {}, pokemon: { update } } as unknown as Engine
  const onClose = vi.fn()
  render(
    <App>
      <EngineProvider engine={engine}>
        <ExperienceModal onClose={onClose} />
      </EngineProvider>
    </App>,
  )
  const dialog = () => document.querySelector('[role="dialog"]')!
  const handle = () => dialog().querySelector('.ant-slider-handle')!
  const field = () => dialog().querySelector<HTMLInputElement>('input[aria-label="EXP"]')!
  return {
    update,
    onClose,
    text: () => dialog().textContent,
    handle,
    valueNow: () => handle().getAttribute('aria-valuenow'),
    sliderDisabled: () => dialog().querySelector('.ant-slider')!.classList.contains('ant-slider-disabled'),
    fieldDisabled: () => field().disabled,
    type: (value: string) => {
      const input = field()
      const setValue = Object.getOwnPropertyDescriptor(HTMLInputElement.prototype, 'value')!.set!
      act(() => {
        setValue.call(input, value)
        input.dispatchEvent(new Event('input', { bubbles: true }))
      })
    },
    save: async () => {
      act(() => document.querySelector<HTMLButtonElement>('.ant-modal-footer .ant-btn-primary')!.click())
      await flush()
    },
  }
}

describe('ExperienceModal', () => {
  it('shows no tooltip on the slider handle', () => {
    const modal = renderModal(detailsWith({ experience: 1200, min: 1000, max: 1500, level: 50 }))

    act(() => {
      ;(modal.handle() as HTMLElement).focus()
      modal.handle().dispatchEvent(new MouseEvent('mousedown', { bubbles: true }))
    })

    expect(document.querySelector('.ant-tooltip')).toBeNull()
    expect(document.querySelector('.ant-slider-tooltip')).toBeNull()
  })

  it('moves the slider when a value is typed and saves it', async () => {
    const modal = renderModal(detailsWith({ experience: 1200, min: 1000, max: 1500, level: 50 }))

    modal.type('1300')

    expect(modal.valueNow()).toBe('1300')

    await modal.save()

    expect(modal.update).toHaveBeenCalledWith(draftHandle, { experience: 1300 })
    expect(modal.onClose).toHaveBeenCalled()
  })

  it('disables the slider and the field at the max level', () => {
    const modal = renderModal(detailsWith({ experience: 1000, min: 1000, max: 1000, level: 100 }))

    expect(modal.sliderDisabled()).toBe(true)
    expect(modal.fieldDisabled()).toBe(true)
    expect(modal.text()).toContain('Level 100 is the most EXP this Pokémon can have.')
  })
})
