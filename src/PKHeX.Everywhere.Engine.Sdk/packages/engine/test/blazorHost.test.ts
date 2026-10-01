import { afterEach, describe, expect, it } from 'vitest'
import { blazorHost } from '../src'

const flush = () => new Promise((resolve) => setTimeout(resolve, 0))

describe('blazorHost', () => {
  afterEach(() => {
    globalThis.pkhexEngineReady = undefined
    globalThis.pkhexEngineOnReady = undefined
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
})
