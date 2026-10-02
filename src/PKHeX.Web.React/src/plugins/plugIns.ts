import type { Base64, Engine } from '@pkhex-everywhere/engine'
import { toBase64 } from '../base64'
import {
  defaultSourceUrl,
  downloadUrl,
  readSource,
  sourceManifestUrl,
  type PlugInSource,
  type PlugInStore,
  type SourcePlugIn,
  type StoredPlugIn,
} from './store'

type PlugInsEngine = Pick<Engine, 'plugins'>

export interface AvailablePlugIn {
  sourceUrl: string
  sourceName: string
  id: string
  name: string
  description: string | null
  projectUrl: string | null
  version: string
}

export type PlugIns = ReturnType<typeof createPlugIns>

export function createPlugIns(engine: PlugInsEngine, store: PlugInStore, fetchUrl: typeof fetch = fetch) {
  async function get(url: string) {
    const response = await fetchUrl(url)
    if (!response.ok) throw new Error(`${url} answered ${response.status}.`)
    return response
  }

  const download = async (url: string): Promise<Base64> => toBase64(new Uint8Array(await (await get(url)).arrayBuffer()))

  async function fetchSource(sourceUrl: string) {
    const source = { ...readSource(await (await get(sourceManifestUrl(sourceUrl))).json()), sourceUrl }
    store.writeSource(source)
    return source
  }

  async function refreshSources(): Promise<PlugInSource[]> {
    const stored = store.readSources()
    const urls = stored.length ? stored.map((s) => s.sourceUrl) : [defaultSourceUrl]
    const sources = await Promise.all(
      urls.map((url) =>
        fetchSource(url).catch((error) => {
          console.warn(`Couldn't refresh the plug-in source ${url}.`, error)
          return stored.find((s) => s.sourceUrl === url)
        }),
      ),
    )
    return sources.filter((s): s is PlugInSource => !!s)
  }

  async function replace(stored: StoredPlugIn, plugIn: SourcePlugIn, state: StoredPlugIn['state'] | null) {
    const version = await engine.plugins.newestCompatible(plugIn.publishedVersions, null)
    if (!version) return false

    const fileUrl = downloadUrl(stored.sourceUrl, plugIn, version)
    const assembly = await download(fileUrl)
    if (!(await engine.plugins.isSupported(assembly))) return false

    await engine.plugins.register(assembly, state && { ...state, hasNewerVersion: false })
    await store.writePlugIn({ ...stored, fileUrl, assembly, state: await engine.plugins.state(stored.id) })
    return true
  }

  async function storedWithSource(id: string, sources: PlugInSource[]) {
    const stored = await store.readPlugIn(id)
    const source = stored && sources.find((s) => s.sourceUrl === stored.sourceUrl)
    const plugIn = source?.plugIns.find((p) => p.id === id)
    return stored && plugIn ? { stored, plugIn } : undefined
  }

  async function updateIncompatible(sources: PlugInSource[]) {
    for (const installed of await engine.plugins.installed()) {
      if (!installed.needsReinstall) continue
      try {
        const found = await storedWithSource(installed.id, sources)
        if (found && !(await replace(found.stored, found.plugIn, found.stored.state)))
          console.warn(`Plug-in ${installed.id} has no version this app can run and needs reinstall.`)
      } catch (error) {
        console.warn(`Couldn't update plug-in ${installed.id}.`, error)
      }
    }
  }

  async function flagNewerVersions(sources: PlugInSource[]) {
    for (const installed of await engine.plugins.installed()) {
      if (installed.needsReinstall) continue
      try {
        const found = await storedWithSource(installed.id, sources)
        if (!found) continue

        await engine.plugins.newestCompatible(found.plugIn.publishedVersions, installed.id)
        const state = await engine.plugins.state(installed.id)
        if (state.hasNewerVersion !== found.stored.state.hasNewerVersion) await store.writePlugIn({ ...found.stored, state })
      } catch (error) {
        console.warn(`Couldn't check plug-in ${installed.id} for a newer version.`, error)
      }
    }
  }

  return {
    async registerStored() {
      for (const plugIn of await store.readPlugIns()) {
        try {
          await engine.plugins.register(plugIn.assembly, plugIn.state)
        } catch (error) {
          console.error(`Couldn't load plug-in ${plugIn.id}.`, error)
        }
      }
    },

    async refresh() {
      const sources = await refreshSources()
      await updateIncompatible(sources)
      await flagNewerVersions(sources)
    },

    async available(): Promise<AvailablePlugIn[]> {
      const stored = store.readSources()
      const sources = stored.length ? stored : await refreshSources()
      const installed = new Set((await engine.plugins.installed()).map((p) => p.id))
      const available: AvailablePlugIn[] = []
      for (const source of sources)
        for (const plugIn of source.plugIns) {
          if (installed.has(plugIn.id)) continue
          const version = await engine.plugins.newestCompatible(plugIn.publishedVersions, null)
          if (!version) continue
          available.push({
            sourceUrl: source.sourceUrl,
            sourceName: source.name,
            id: plugIn.id,
            name: plugIn.name,
            description: plugIn.description,
            projectUrl: plugIn.projectUrl,
            version: version.version,
          })
        }
      return available
    },

    async install(sourceUrl: string, id: string) {
      const plugIn = store.readSources().find((s) => s.sourceUrl === sourceUrl)?.plugIns.find((p) => p.id === id)
      if (!plugIn) throw new Error(`No plug-in ${id} at ${sourceUrl}.`)

      const version = await engine.plugins.newestCompatible(plugIn.publishedVersions, null)
      if (!version) throw new Error(`${plugIn.name} has no version this app can run.`)

      const fileUrl = downloadUrl(sourceUrl, plugIn, version)
      const assembly = await download(fileUrl)
      if (!(await engine.plugins.isSupported(assembly))) throw new Error(`${plugIn.name} ${version.version} can't run in this app.`)

      const installed = await engine.plugins.register(assembly, null)
      await store.writePlugIn({ id: installed.id, sourceUrl, fileUrl, assembly, state: await engine.plugins.state(installed.id) })
      return installed.id
    },

    async update(id: string) {
      const stored = await store.readPlugIn(id)
      if (!stored) return false

      const plugIn = (await fetchSource(stored.sourceUrl)).plugIns.find((p) => p.id === id)
      return !!plugIn && replace(stored, plugIn, null)
    },

    async uninstall(id: string) {
      await engine.plugins.unregister(id)
      await store.removePlugIn(id)
    },

    async persist(id: string) {
      const stored = await store.readPlugIn(id)
      if (stored) await store.writePlugIn({ ...stored, state: await engine.plugins.state(id) })
    },
  }
}
