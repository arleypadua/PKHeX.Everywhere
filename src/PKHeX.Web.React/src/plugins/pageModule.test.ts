import { describe, expect, it, vi } from 'vitest'
import type { PlugInSetting } from '@pkhex-everywhere/engine'
import type { Mount, PageContext } from '@pkhex-everywhere/plugin-sdk'
import { toBase64 } from '../base64'
import { mountPageModule, settingValue } from './pageModule'

const setting = (value: Partial<PlugInSetting>): PlugInSetting => ({
  key: 'Key',
  readOnly: false,
  stringValue: null,
  booleanValue: null,
  integerValue: null,
  fileName: null,
  file: null,
  ...value,
})

describe('settingValue', () => {
  it('returns each setting type, with files as bytes', () => {
    expect(settingValue(setting({ stringValue: 'Hi' }))).toBe('Hi')
    expect(settingValue(setting({ booleanValue: false }))).toBe(false)
    expect(settingValue(setting({ integerValue: 0 }))).toBe(0)
    expect(settingValue(setting({ fileName: 'rom.gba', file: toBase64(new Uint8Array([1, 2, 3])) }))).toEqual(
      new Uint8Array([1, 2, 3]),
    )
    expect(settingValue(setting({ fileName: '' }))).toEqual(new Uint8Array())
    expect(settingValue(null)).toBeNull()
  })
})

describe('mountPageModule', () => {
  const ctx = {} as PageContext
  const source = Promise.resolve('source')

  it('mounts the module into the element with the context', async () => {
    const mount = vi.fn<Mount>(() => () => {})
    const element = document.createElement('div')

    mountPageModule(element, source, ctx, async () => ({ mount }))

    await vi.waitFor(() => expect(mount).toHaveBeenCalledWith(element, ctx))
  })

  it('unmounts the page once it has mounted', async () => {
    const unmount = vi.fn()
    const unmountPage = mountPageModule(document.createElement('div'), source, ctx, async () => ({
      mount: async () => unmount,
    }))
    await new Promise((r) => setTimeout(r))

    unmountPage()

    await vi.waitFor(() => expect(unmount).toHaveBeenCalledOnce())
  })

  it('unmounts a page that finishes mounting after the island went away', async () => {
    const unmount = vi.fn()
    let mounted!: () => void
    const mount = vi.fn<Mount>(() => new Promise((r) => (mounted = () => r(unmount))))
    const unmountPage = mountPageModule(document.createElement('div'), source, ctx, async () => ({ mount }))
    await vi.waitFor(() => expect(mount).toHaveBeenCalled())

    unmountPage()
    mounted()

    await vi.waitFor(() => expect(unmount).toHaveBeenCalledOnce())
  })

  it('does not mount once the island went away before the module loaded', async () => {
    const mount = vi.fn<Mount>(() => () => {})
    let loaded!: () => void
    const load = vi.fn(() => new Promise<{ mount: Mount }>((r) => (loaded = () => r({ mount }))))
    const unmountPage = mountPageModule(document.createElement('div'), source, ctx, load)
    await vi.waitFor(() => expect(load).toHaveBeenCalled())

    unmountPage()
    loaded()
    await new Promise((r) => setTimeout(r))

    expect(mount).not.toHaveBeenCalled()
  })
})
