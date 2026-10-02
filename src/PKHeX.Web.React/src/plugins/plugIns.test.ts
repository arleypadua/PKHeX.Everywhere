import { beforeEach, describe, expect, it, vi } from 'vitest'
import { IDBFactory } from 'fake-indexeddb'
import type { InstalledPlugIn, PlugInState, PublishedVersion } from '@pkhex-everywhere/engine'
import { toBase64 } from '../base64'
import { createPlugIns } from './plugIns'
import { createPlugInStore, defaultSourceUrl, readSource, type PlugInStore } from './store'

const source = 'https://plugins.example/main'
const v1 = toBase64(new TextEncoder().encode('v1'))
const v2 = toBase64(new TextEncoder().encode('v2'))
const v3 = toBase64(new TextEncoder().encode('v3'))

const storedState: PlugInState = {
  enabled: false,
  hasNewerVersion: false,
  toggles: [{ hookId: 'Hook', enabled: false }],
  settings: [{ key: 'Greeting', readOnly: false, stringValue: 'Hi', booleanValue: null, integerValue: null, fileName: null, file: null }],
}

const manifest = {
  SourceUrl: source,
  Name: 'Example',
  PlugIns: [
    {
      Id: 'Example',
      FileName: 'Example.dll',
      Name: 'Example plug-in',
      PublishedVersions: ['1.0.0', { Version: '2.0.0', Sdk: 2 }],
    },
  ],
}

// Stands in for the Engine: assemblies are their version, and only v2 and v3 are supported.
function fakeEngine() {
  const plugIns = new Map<string, { version: string; state: PlugInState; needsReinstall: boolean }>()
  const versionOf = (assembly: string) => new TextDecoder().decode(Uint8Array.from(atob(assembly), (c) => c.charCodeAt(0)))
  const supported = (assembly: string) => versionOf(assembly) !== 'v1'
  const engine = {
    plugins: {
      register: vi.fn(async (assembly: string, stored: PlugInState | null) => {
        plugIns.set('Example', {
          version: versionOf(assembly),
          state: stored ?? plugIns.get('Example')?.state ?? { enabled: true, hasNewerVersion: false, toggles: [], settings: [] },
          needsReinstall: !supported(assembly),
        })
        return installed('Example')
      }),
      unregister: vi.fn(async (id: string) => void plugIns.delete(id)),
      installed: async () => [...plugIns.keys()].map(installed),
      isSupported: async (assembly: string) => supported(assembly),
      state: async (id: string) => plugIns.get(id)!.state,
      newestCompatible: vi.fn(async (versions: PublishedVersion[], id: string | null) => {
        const newest = versions.filter((v) => v.sdk === 2).at(-1) ?? null
        const plugIn = id ? plugIns.get(id) : undefined
        if (plugIn && newest) plugIn.state = { ...plugIn.state, hasNewerVersion: newest.version !== '2.0.0' }
        return newest
      }),
    },
  }
  function installed(id: string): InstalledPlugIn {
    const p = plugIns.get(id)!
    return { id, name: id, version: p.version, enabled: p.state.enabled, hasNewerVersion: p.state.hasNewerVersion, needsReinstall: p.needsReinstall }
  }
  return { engine: engine as unknown as Parameters<typeof createPlugIns>[0] & typeof engine, plugIns }
}

function fakeFetch(files: Record<string, unknown>) {
  return vi.fn(async (url: string) => {
    if (!(url in files)) return new Response(null, { status: 404 })
    const body = files[url]
    return typeof body === 'string' ? new Response(Uint8Array.from(atob(body), (c) => c.charCodeAt(0))) : Response.json(body)
  }) as unknown as typeof fetch & ReturnType<typeof vi.fn>
}

describe('plug-ins', () => {
  let store: PlugInStore

  beforeEach(() => {
    localStorage.clear()
    store = createPlugInStore(localStorage, new IDBFactory())
  })

  it('registers every stored plug-in with its stored state', async () => {
    const { engine } = fakeEngine()
    await store.writePlugIn({ id: 'Example', sourceUrl: source, fileUrl: 'f', assembly: v2, state: storedState })

    await createPlugIns(engine, store, fakeFetch({})).registerStored()

    expect(engine.plugins.register).toHaveBeenCalledWith(v2, storedState)
  })

  it('seeds the default source when none is stored', async () => {
    const { engine } = fakeEngine()
    const fetch = fakeFetch({ [`${defaultSourceUrl}/pkhexwebplugins.json`]: { ...manifest, SourceUrl: defaultSourceUrl } })

    await createPlugIns(engine, store, fetch).refresh()

    expect(store.readSources().map((s) => s.sourceUrl)).toEqual([defaultSourceUrl])
  })

  it('updates a plug-in that needs reinstall and keeps its stored state', async () => {
    const { engine, plugIns } = fakeEngine()
    store.writeSource({ sourceUrl: source, name: 'Example', sourceDescription: null, plugIns: [] })
    await store.writePlugIn({ id: 'Example', sourceUrl: source, fileUrl: 'old', assembly: v1, state: storedState })
    const fetch = fakeFetch({ [`${source}/pkhexwebplugins.json`]: manifest, [`${source}/Example/2.0.0/Example.dll`]: v2 })
    const loader = createPlugIns(engine, store, fetch)
    await loader.registerStored()

    await loader.refresh()

    expect(plugIns.get('Example')).toEqual({ version: 'v2', state: storedState, needsReinstall: false })
    expect(await store.readPlugIn('Example')).toEqual({
      id: 'Example',
      sourceUrl: source,
      fileUrl: `${source}/Example/2.0.0/Example.dll`,
      assembly: v2,
      state: storedState,
    })
  })

  it('leaves a plug-in needing reinstall when its source has no version the app can run', async () => {
    const { engine, plugIns } = fakeEngine()
    store.writeSource({ sourceUrl: source, name: 'Example', sourceDescription: null, plugIns: [] })
    await store.writePlugIn({ id: 'Example', sourceUrl: source, fileUrl: 'old', assembly: v1, state: storedState })
    const old = { ...manifest, PlugIns: [{ ...manifest.PlugIns[0], PublishedVersions: ['1.0.0'] }] }
    const loader = createPlugIns(engine, store, fakeFetch({ [`${source}/pkhexwebplugins.json`]: old }))
    await loader.registerStored()

    await loader.refresh()

    expect(plugIns.get('Example')?.needsReinstall).toBe(true)
    expect((await store.readPlugIn('Example'))?.assembly).toBe(v1)
  })

  it('stores the flag of a plug-in with a newer compatible version', async () => {
    const { engine } = fakeEngine()
    store.writeSource({ sourceUrl: source, name: 'Example', sourceDescription: null, plugIns: [] })
    await store.writePlugIn({ id: 'Example', sourceUrl: source, fileUrl: 'f', assembly: v2, state: storedState })
    const newer = { ...manifest, PlugIns: [{ ...manifest.PlugIns[0], PublishedVersions: [{ Version: '3.0.0', Sdk: 2 }] }] }
    const loader = createPlugIns(engine, store, fakeFetch({ [`${source}/pkhexwebplugins.json`]: newer }))
    await loader.registerStored()

    await loader.refresh()

    expect(engine.plugins.newestCompatible).toHaveBeenCalledWith([{ version: '3.0.0', sdk: 2 }], 'Example')
    expect((await store.readPlugIn('Example'))?.state.hasNewerVersion).toBe(true)
  })

  it('installs the newest compatible version from a source and stores it', async () => {
    const { engine } = fakeEngine()
    store.writeSource(readSource(manifest))
    const plugIns = createPlugIns(engine, store, fakeFetch({ [`${source}/Example/2.0.0/Example.dll`]: v2 }))

    expect(await plugIns.available()).toEqual([
      expect.objectContaining({ sourceUrl: source, id: 'Example', name: 'Example plug-in', version: '2.0.0' }),
    ])
    expect(await plugIns.install(source, 'Example')).toBe('Example')

    expect(engine.plugins.register).toHaveBeenCalledWith(v2, null)
    expect(await store.readPlugIn('Example')).toMatchObject({ sourceUrl: source, fileUrl: `${source}/Example/2.0.0/Example.dll`, assembly: v2 })
    expect(await plugIns.available()).toEqual([])
  })

  it('updates to the newest version keeping the current state', async () => {
    const { engine, plugIns } = fakeEngine()
    await store.writePlugIn({ id: 'Example', sourceUrl: source, fileUrl: 'f', assembly: v2, state: storedState })
    const newer = { ...manifest, PlugIns: [{ ...manifest.PlugIns[0], PublishedVersions: [{ Version: '3.0.0', Sdk: 2 }] }] }
    const fetch = fakeFetch({ [`${source}/pkhexwebplugins.json`]: newer, [`${source}/Example/3.0.0/Example.dll`]: v3 })
    const loader = createPlugIns(engine, store, fetch)
    await loader.registerStored()

    expect(await loader.update('Example')).toBe(true)

    expect(engine.plugins.register).toHaveBeenLastCalledWith(v3, null)
    expect(plugIns.get('Example')).toEqual({ version: 'v3', state: storedState, needsReinstall: false })
    expect((await store.readPlugIn('Example'))?.assembly).toBe(v3)
  })

  it('uninstalls a plug-in from the engine and the store', async () => {
    const { engine } = fakeEngine()
    await store.writePlugIn({ id: 'Example', sourceUrl: source, fileUrl: 'f', assembly: v2, state: storedState })

    await createPlugIns(engine, store, fakeFetch({})).uninstall('Example')

    expect(engine.plugins.unregister).toHaveBeenCalledWith('Example')
    expect(await store.readPlugIns()).toEqual([])
  })

  it('registers an installed or updated plug-in again after a reload', async () => {
    const { engine } = fakeEngine()
    store.writeSource(readSource(manifest))
    const newer = { ...manifest, PlugIns: [{ ...manifest.PlugIns[0], PublishedVersions: [{ Version: '3.0.0', Sdk: 2 }] }] }
    const fetch = fakeFetch({
      [`${source}/Example/2.0.0/Example.dll`]: v2,
      [`${source}/pkhexwebplugins.json`]: newer,
      [`${source}/Example/3.0.0/Example.dll`]: v3,
    })
    const plugIns = createPlugIns(engine, store, fetch)
    await plugIns.install(source, 'Example')
    await plugIns.update('Example')

    const reloaded = fakeEngine()
    await createPlugIns(reloaded.engine, store, fakeFetch({})).registerStored()

    expect(reloaded.engine.plugins.register).toHaveBeenCalledWith(v3, expect.objectContaining({ enabled: true }))
  })

  it('registers nothing after a reload once a plug-in is uninstalled', async () => {
    const { engine } = fakeEngine()
    store.writeSource(readSource(manifest))
    const plugIns = createPlugIns(engine, store, fakeFetch({ [`${source}/Example/2.0.0/Example.dll`]: v2 }))
    await plugIns.install(source, 'Example')
    await plugIns.uninstall('Example')

    const reloaded = fakeEngine()
    await createPlugIns(reloaded.engine, store, fakeFetch({})).registerStored()

    expect(reloaded.engine.plugins.register).not.toHaveBeenCalled()
  })
})
