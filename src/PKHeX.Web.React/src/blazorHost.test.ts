import { afterEach, describe, expect, it } from 'vitest'
import { blazorHost } from './blazorHost'

const flush = () => new Promise((resolve) => setTimeout(resolve, 0))

describe('blazorHost', () => {
  afterEach(() => {
    globalThis.pkhexEngineReady = undefined
    globalThis.pkhexEngineOnReady = undefined
    globalThis.pkhexEngineOnChange = undefined
    globalThis.pkhexEngineOnEvent = undefined
  })

  it('is ready when .NET signalled before the host was created', async () => {
    globalThis.pkhexEngineReady = true

    await expect(blazorHost().ready()).resolves.toBeUndefined()
  })

  it('waits for the .NET signal', async () => {
    let isReady = false
    void blazorHost()
      .ready()
      .then(() => (isReady = true))

    await flush()
    expect(isReady).toBe(false)

    globalThis.pkhexEngineReady = true
    globalThis.pkhexEngineOnReady?.()
    await flush()
    expect(isReady).toBe(true)
  })

  it('passes changes from .NET to the listener', () => {
    const changes: string[][] = []
    blazorHost().onChange((topics) => changes.push(topics))

    globalThis.pkhexEngineOnChange?.(['party'])

    expect(changes).toEqual([['party']])
  })

  it('passes events from .NET to the listener', () => {
    const events: string[] = []
    blazorHost().onEvent((event) => events.push(event))

    globalThis.pkhexEngineOnEvent?.('{"type":"gameExported"}')

    expect(events).toEqual(['{"type":"gameExported"}'])
  })
})
