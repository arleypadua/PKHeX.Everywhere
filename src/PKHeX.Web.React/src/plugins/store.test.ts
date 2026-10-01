import { beforeEach, describe, expect, it } from 'vitest'
import { IDBFactory } from 'fake-indexeddb'
import { createPlugInStore, defaultSourceUrl, type StoredPlugIn } from './store'

const source = 'https://plugins.example/main'

// What Blazored.LocalStorage wrote for a plug-in before the store moved to JS.
const blazoredPlugIn = {
  id: 'PKHeX.Web.Plugins.LiveRun',
  hasNewerVersion: true,
  plugInSourceId: source,
  fileUrl: `${source}/PKHeX.Web.Plugins.LiveRun/3.0.0/PKHeX.Web.Plugins.LiveRun.dll`,
  assemblyBytes: 'TVqQAAMAAAAEAAAA',
  enabled: false,
  featureToggles: { 'PKHeX.Web.Plugins.LiveRun.Run': true, 'PKHeX.Web.Plugins.LiveRun.Other': false },
  plugInSettings: [
    { key: 'Greeting', readOnly: false, stringValue: 'Hi' },
    { key: 'Fast', readOnly: true, booleanValue: false },
    { key: 'Speed', readOnly: false, integerValue: 3 },
    { key: 'Rom', readOnly: false, fileName: 'game.gba', filePlugInId: 'PKHeX.Web.Plugins.LiveRun' },
  ],
}

const blazoredSource = {
  sourceUrl: source,
  name: 'Default',
  sourceDescription: 'Default set of plug-ins',
  plugIns: [
    {
      id: 'PKHeX.Web.Plugins.LiveRun',
      fileName: 'PKHeX.Web.Plugins.LiveRun.dll',
      name: 'Live Run',
      description: 'Runs the save',
      projectUrl: 'https://example.com',
      publishedVersions: ['1.0.0', { Version: '3.0.0', Sdk: 2 }],
    },
  ],
}

// TG.Blazor.IndexedDB created this database and wrote file settings to it.
function blazorFilesDatabase(indexedDb: IDBFactory, data: Uint8Array | string) {
  return new Promise<void>((resolve, reject) => {
    const open = indexedDb.open('pkhex-web-db', 1)
    open.onupgradeneeded = () => {
      const store = open.result.createObjectStore('PlugInFiles', { keyPath: 'key' })
      store.createIndex('plugInId', 'plugInId', { unique: false })
    }
    open.onsuccess = () => {
      const tx = open.result.transaction('PlugInFiles', 'readwrite')
      tx.objectStore('PlugInFiles').add({
        key: 'PKHeX.Web.Plugins.LiveRun#game.gba',
        data,
        fileName: 'game.gba',
        plugInId: 'PKHeX.Web.Plugins.LiveRun',
      })
      tx.oncomplete = () => {
        open.result.close()
        resolve()
      }
      tx.onerror = () => reject(tx.error)
    }
    open.onerror = () => reject(open.error)
  })
}

describe('plug-in store', () => {
  let indexedDb: IDBFactory

  beforeEach(() => {
    localStorage.clear()
    indexedDb = new IDBFactory()
  })

  it('reads a plug-in stored in the Blazored format', async () => {
    localStorage.setItem('__plug_in__#PKHeX.Web.Plugins.LiveRun', JSON.stringify(blazoredPlugIn))
    await blazorFilesDatabase(indexedDb, new Uint8Array([1, 2, 3]))

    const plugIns = await createPlugInStore(localStorage, indexedDb).readPlugIns()

    expect(plugIns).toEqual<StoredPlugIn[]>([
      {
        id: 'PKHeX.Web.Plugins.LiveRun',
        sourceUrl: source,
        fileUrl: blazoredPlugIn.fileUrl,
        assembly: 'TVqQAAMAAAAEAAAA',
        state: {
          enabled: false,
          hasNewerVersion: true,
          toggles: [
            { hookId: 'PKHeX.Web.Plugins.LiveRun.Run', enabled: true },
            { hookId: 'PKHeX.Web.Plugins.LiveRun.Other', enabled: false },
          ],
          settings: [
            setting({ key: 'Greeting', stringValue: 'Hi' }),
            setting({ key: 'Fast', readOnly: true, booleanValue: false }),
            setting({ key: 'Speed', integerValue: 3 }),
            setting({ key: 'Rom', fileName: 'game.gba', file: 'AQID' }),
          ],
        },
      },
    ])
  })

  it('reads a file setting IndexedDB holds as a base64 string', async () => {
    localStorage.setItem('__plug_in__#PKHeX.Web.Plugins.LiveRun', JSON.stringify(blazoredPlugIn))
    await blazorFilesDatabase(indexedDb, 'AQID')

    const [plugIn] = await createPlugInStore(localStorage, indexedDb).readPlugIns()

    expect(plugIn.state.settings.find((s) => s.key === 'Rom')?.file).toBe('AQID')
  })

  it('reads a file setting whose file is missing as empty', async () => {
    localStorage.setItem('__plug_in__#PKHeX.Web.Plugins.LiveRun', JSON.stringify(blazoredPlugIn))

    const [plugIn] = await createPlugInStore(localStorage, indexedDb).readPlugIns()

    expect(plugIn.state.settings.find((s) => s.key === 'Rom')).toEqual(setting({ key: 'Rom', fileName: 'game.gba' }))
  })

  it('reads plug-ins stored before sources and file urls were recorded', async () => {
    const { plugInSourceId: _, fileUrl: __, ...old } = blazoredPlugIn
    localStorage.setItem('__plug_in__#Old', JSON.stringify({ ...old, id: 'Old', sourceUrl: 'https://old/file.dll' }))

    const [plugIn] = await createPlugInStore(localStorage, indexedDb).readPlugIns()

    expect(plugIn.sourceUrl).toBe(defaultSourceUrl)
    expect(plugIn.fileUrl).toBe('https://old/file.dll')
  })

  it('skips a stored plug-in it cannot parse', async () => {
    localStorage.setItem('__plug_in__#Broken', '{')

    expect(await createPlugInStore(localStorage, indexedDb).readPlugIns()).toEqual([])
  })

  it('reads a source stored in the Blazored format', () => {
    localStorage.setItem(`__plug_in__source__#${source}`, JSON.stringify(blazoredSource))
    localStorage.setItem('unrelated', '{}')

    expect(createPlugInStore(localStorage, indexedDb).readSources()).toEqual([
      {
        sourceUrl: source,
        name: 'Default',
        sourceDescription: 'Default set of plug-ins',
        plugIns: [
          {
            id: 'PKHeX.Web.Plugins.LiveRun',
            fileName: 'PKHeX.Web.Plugins.LiveRun.dll',
            name: 'Live Run',
            description: 'Runs the save',
            projectUrl: 'https://example.com',
            summary: null,
            publishedVersions: [
              { version: '1.0.0', sdk: 1 },
              { version: '3.0.0', sdk: 2 },
            ],
          },
        ],
      },
    ])
  })

  it('skips a source stored under the old relative url', () => {
    localStorage.setItem('__plug_in__source__#/plugins', JSON.stringify({ ...blazoredSource, sourceUrl: '/plugins' }))

    expect(createPlugInStore(localStorage, indexedDb).readSources()).toEqual([])
  })

  it('writes a source back in the format it reads', () => {
    localStorage.setItem(`__plug_in__source__#${source}`, JSON.stringify(blazoredSource))
    const store = createPlugInStore(localStorage, indexedDb)
    const sources = store.readSources()

    localStorage.clear()
    store.writeSource(sources[0])

    expect(JSON.parse(localStorage.getItem(`__plug_in__source__#${source}`)!).plugIns[0].publishedVersions).toEqual([
      { Version: '1.0.0', Sdk: 1 },
      { Version: '3.0.0', Sdk: 2 },
    ])
    expect(store.readSources()).toEqual(sources)
  })

  it('writes a plug-in in the Blazored format with its files in IndexedDB', async () => {
    localStorage.setItem('__plug_in__#PKHeX.Web.Plugins.LiveRun', JSON.stringify(blazoredPlugIn))
    await blazorFilesDatabase(indexedDb, new Uint8Array([1, 2, 3]))
    const store = createPlugInStore(localStorage, indexedDb)
    const [plugIn] = await store.readPlugIns()

    localStorage.clear()
    await store.writePlugIn(plugIn)

    expect(JSON.parse(localStorage.getItem('__plug_in__#PKHeX.Web.Plugins.LiveRun')!)).toEqual(blazoredPlugIn)
    expect(await createPlugInStore(localStorage, indexedDb).readPlugIns()).toEqual([plugIn])
  })

  it('deletes the files of settings a plug-in no longer has', async () => {
    localStorage.setItem('__plug_in__#PKHeX.Web.Plugins.LiveRun', JSON.stringify(blazoredPlugIn))
    await blazorFilesDatabase(indexedDb, new Uint8Array([1, 2, 3]))
    const store = createPlugInStore(localStorage, indexedDb)
    const [plugIn] = await store.readPlugIns()
    const settings = plugIn.state.settings.map((s) => (s.key === 'Rom' ? setting({ key: 'Rom', fileName: '' }) : s))

    await store.writePlugIn({ ...plugIn, state: { ...plugIn.state, settings } })
    localStorage.setItem('__plug_in__#PKHeX.Web.Plugins.LiveRun', JSON.stringify(blazoredPlugIn))

    expect((await store.readPlugIns())[0].state.settings.find((s) => s.key === 'Rom')?.file).toBeNull()
  })

  it('removes a plug-in and its files', async () => {
    localStorage.setItem('__plug_in__#PKHeX.Web.Plugins.LiveRun', JSON.stringify(blazoredPlugIn))
    await blazorFilesDatabase(indexedDb, new Uint8Array([1, 2, 3]))
    const store = createPlugInStore(localStorage, indexedDb)

    await store.removePlugIn('PKHeX.Web.Plugins.LiveRun')

    expect(localStorage.length).toBe(0)
    localStorage.setItem('__plug_in__#PKHeX.Web.Plugins.LiveRun', JSON.stringify(blazoredPlugIn))
    expect((await store.readPlugIns())[0].state.settings.find((s) => s.key === 'Rom')?.file).toBeNull()
  })

  it('creates the files database when Blazor never did', async () => {
    const store = createPlugInStore(localStorage, indexedDb)
    const [plugIn] = await createPlugInStore(withPlugIn(), new IDBFactory()).readPlugIns()

    await store.writePlugIn({ ...plugIn, state: { ...plugIn.state, settings: [setting({ key: 'Rom', fileName: 'a.gba', file: 'AQID' })] } })

    expect((await store.readPlugIn(plugIn.id))?.state.settings).toEqual([setting({ key: 'Rom', fileName: 'a.gba', file: 'AQID' })])
  })
})

function withPlugIn() {
  localStorage.setItem('__plug_in__#PKHeX.Web.Plugins.LiveRun', JSON.stringify(blazoredPlugIn))
  return localStorage
}

function setting(value: Partial<StoredPlugIn['state']['settings'][number]> & { key: string }) {
  return {
    readOnly: false,
    stringValue: null,
    booleanValue: null,
    integerValue: null,
    fileName: null,
    file: null,
    ...value,
  }
}
