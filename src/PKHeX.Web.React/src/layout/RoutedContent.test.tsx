import { act } from 'react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import type { Engine } from '@pkhex-everywhere/engine'
import { EngineProvider } from '@pkhex-everywhere/react'
import { engine } from '../app'
import { render } from '../testing/render'
import { RoutedContent } from './RoutedContent'

vi.mock('../app', () => {
  const error = new TypeError('Importing a module script failed.')
  const plugInsLoaded = Promise.reject(error)
  plugInsLoaded.catch(() => {})
  const engine = {
    status: { state: 'failed', loaded: 0, total: 0, error },
    onStatusChange: () => () => {},
    subscribe: () => () => {},
    game: { version: () => Promise.reject(error) },
  } as unknown as Engine
  return { engine, plugInsLoaded }
})

async function renderFailedBoot() {
  const { container } = render(
    <EngineProvider engine={engine}>
      <RoutedContent />
    </EngineProvider>,
  )
  await act(() => Promise.resolve())
  return container
}

function reloadButton(container: HTMLElement) {
  return [...container.querySelectorAll('button')].find((button) => button.textContent === 'Reload')
}

describe('RoutedContent', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('offers a reload instead of an issue link when the engine fails to boot', async () => {
    const container = await renderFailedBoot()

    expect(reloadButton(container)).toBeDefined()
    expect(container.textContent).not.toContain('File an issue on GitHub')
  })

  it('reloads the page from the boot failure alert', async () => {
    const reload = vi.fn()
    vi.stubGlobal('location', { ...location, reload })
    const container = await renderFailedBoot()

    act(() => reloadButton(container)!.click())

    expect(reload).toHaveBeenCalledOnce()
  })
})
