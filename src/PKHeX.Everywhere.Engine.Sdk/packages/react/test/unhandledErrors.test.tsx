import { cleanup, render } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { EngineError, type Engine } from '@pkhex-everywhere/engine'
import { EngineProvider } from '../src'

const engine = { subscribe: () => () => {} } as unknown as Engine

function rejectUnhandled(reason: unknown) {
  const event = Object.assign(new Event('unhandledrejection', { cancelable: true }), { reason })
  window.dispatchEvent(event)
  return event
}

afterEach(cleanup)

describe('unhandled command errors', () => {
  it('reports engine errors that nobody handled', () => {
    const onUnhandledError = vi.fn()
    render(<EngineProvider engine={engine} onUnhandledError={onUnhandledError} />)
    const error = new EngineError('out-of-range', 'Level must be between 1 and 100, got 101.')

    const event = rejectUnhandled(error)

    expect(onUnhandledError).toHaveBeenCalledExactlyOnceWith(error)
    expect(event.defaultPrevented).toBe(true)
  })

  it('leaves other rejections to the browser', () => {
    const onUnhandledError = vi.fn()
    render(<EngineProvider engine={engine} onUnhandledError={onUnhandledError} />)

    const event = rejectUnhandled(new Error('boom'))

    expect(onUnhandledError).not.toHaveBeenCalled()
    expect(event.defaultPrevented).toBe(false)
  })

  it('reports an error once when several providers are mounted', () => {
    const first = vi.fn()
    const second = vi.fn()
    render(
      <>
        <EngineProvider engine={engine} onUnhandledError={first} />
        <EngineProvider engine={engine} onUnhandledError={second} />
      </>,
    )

    rejectUnhandled(new EngineError('no-save', 'No save is loaded.'))

    expect(first.mock.calls.length + second.mock.calls.length).toBe(1)
  })

  it('stops reporting after the provider unmounts', () => {
    const onUnhandledError = vi.fn()
    const { unmount } = render(<EngineProvider engine={engine} onUnhandledError={onUnhandledError} />)

    unmount()
    rejectUnhandled(new EngineError('no-save', 'No save is loaded.'))

    expect(onUnhandledError).not.toHaveBeenCalled()
  })
})
