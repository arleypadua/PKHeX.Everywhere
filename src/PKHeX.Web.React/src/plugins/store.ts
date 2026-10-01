import type { Base64, PlugInSetting, PlugInState, PublishedVersion } from '@pkhex-everywhere/engine'
import { fromBase64, toBase64 } from '../base64'

export const defaultSourceUrl = 'https://raw.githubusercontent.com/pkhex-web/plugins-source-assets/main'

const plugInPrefix = '__plug_in__#'
const sourcePrefix = '__plug_in__source__#'
const databaseName = 'pkhex-web-db'
const filesStore = 'PlugInFiles'
const plugInIdIndex = 'plugInId'

export interface StoredPlugIn {
  id: string
  sourceUrl: string
  fileUrl: string
  assembly: Base64
  state: PlugInState
}

export interface SourcePlugIn {
  id: string
  fileName: string
  name: string
  description: string | null
  projectUrl: string | null
  summary: string | null
  publishedVersions: PublishedVersion[]
}

export interface PlugInSource {
  sourceUrl: string
  name: string
  sourceDescription: string | null
  plugIns: SourcePlugIn[]
}

export interface PlugInStore {
  readPlugIns(): Promise<StoredPlugIn[]>
  readPlugIn(id: string): Promise<StoredPlugIn | undefined>
  writePlugIn(plugIn: StoredPlugIn): Promise<void>
  removePlugIn(id: string): Promise<void>
  readSources(): PlugInSource[]
  writeSource(source: PlugInSource): void
}

// Blazored.LocalStorage wrote these records, camel-cased, and TG.Blazor.IndexedDB wrote the files.
interface StoredRecord {
  id: string
  hasNewerVersion?: boolean
  plugInSourceId?: string
  sourceUrl?: string
  fileUrl?: string
  assemblyBytes: Base64
  enabled: boolean
  featureToggles?: Record<string, boolean>
  plugInSettings?: StoredSetting[]
}

interface StoredSetting {
  key: string
  readOnly?: boolean
  stringValue?: string
  booleanValue?: boolean
  integerValue?: number
  fileName?: string
  filePlugInId?: string
}

interface FileRecord {
  key: string
  data: Uint8Array | Base64
  fileName: string
  plugInId: string
}

type Json = Record<string, unknown>

export function sourceManifestUrl(sourceUrl: string) {
  return `${trimSlash(sourceUrl)}/pkhexwebplugins.json`
}

export function downloadUrl(sourceUrl: string, plugIn: SourcePlugIn, version: PublishedVersion) {
  return `${trimSlash(sourceUrl)}/${plugIn.id}/${version.version}/${plugIn.fileName}`
}

// Manifests are Pascal-cased and stored sources camel-cased, so names are read either way.
export function readSource(json: Json, fallbackUrl?: string): PlugInSource {
  return {
    sourceUrl: text(json, 'sourceUrl') ?? fallbackUrl ?? '',
    name: text(json, 'name') ?? '',
    sourceDescription: text(json, 'sourceDescription') ?? null,
    plugIns: list(json, 'plugIns').map((plugIn) => ({
      id: text(plugIn, 'id') ?? '',
      fileName: text(plugIn, 'fileName') ?? '',
      name: text(plugIn, 'name') ?? '',
      description: text(plugIn, 'description') ?? null,
      projectUrl: text(plugIn, 'projectUrl') ?? null,
      summary: text(plugIn, 'summary') ?? null,
      publishedVersions: (field(plugIn, 'publishedVersions') as unknown[] | undefined ?? []).map(readVersion),
    })),
  }
}

export function createPlugInStore(storage: Storage = localStorage, indexedDb: IDBFactory = indexedDB): PlugInStore {
  let database: Promise<IDBDatabase> | undefined
  const files = () => (database ??= openDatabase(indexedDb))

  async function toStored(record: StoredRecord): Promise<StoredPlugIn> {
    const sourceUrl = record.plugInSourceId ?? defaultSourceUrl
    return {
      id: record.id,
      sourceUrl,
      fileUrl: record.fileUrl ?? record.sourceUrl ?? '',
      assembly: record.assemblyBytes,
      state: {
        enabled: record.enabled,
        hasNewerVersion: record.hasNewerVersion ?? false,
        toggles: Object.entries(record.featureToggles ?? {}).map(([hookId, enabled]) => ({ hookId, enabled })),
        settings: await Promise.all((record.plugInSettings ?? []).map(async (setting) => toSetting(setting, await readFile(setting)))),
      },
    }
  }

  async function readFile(setting: StoredSetting) {
    if (setting.fileName == null || setting.filePlugInId == null) return null
    try {
      const record = await request<FileRecord | undefined>(await files(), 'readonly', (store) =>
        store.get(fileKey(setting.filePlugInId!, setting.fileName!)),
      )
      if (!record) return null
      return typeof record.data === 'string' ? record.data : toBase64(record.data)
    } catch (error) {
      console.error(`Couldn't read the file ${setting.fileName} of plug-in ${setting.filePlugInId}.`, error)
      return null
    }
  }

  function readRecord(key: string): StoredRecord | undefined {
    try {
      const value = storage.getItem(key)
      return value ? (JSON.parse(value) as StoredRecord) : undefined
    } catch (error) {
      console.error(`Couldn't read the plug-in stored at ${key}.`, error)
      return undefined
    }
  }

  async function writeFiles(plugIn: StoredPlugIn) {
    const db = await files()
    const kept = plugIn.state.settings.filter((s) => s.fileName && s.file)
    const existing = await request<IDBValidKey[]>(db, 'readonly', (store) =>
      store.index(plugInIdIndex).getAllKeys(plugIn.id),
    )
    const keys = new Set(kept.map((s) => fileKey(plugIn.id, s.fileName!)))
    await transaction(db, (store) => {
      for (const key of existing) if (!keys.has(String(key))) store.delete(key)
      for (const setting of kept)
        store.put({
          key: fileKey(plugIn.id, setting.fileName!),
          data: fromBase64(setting.file!),
          fileName: setting.fileName!,
          plugInId: plugIn.id,
        } satisfies FileRecord)
    })
  }

  return {
    async readPlugIns() {
      const records = keys(storage, plugInPrefix).map(readRecord).filter((r): r is StoredRecord => !!r)
      const stored = await Promise.all(
        records.map((record) =>
          toStored(record).catch((error) => {
            console.error(`Couldn't read plug-in ${record.id}.`, error)
            return undefined
          }),
        ),
      )
      return stored.filter((p): p is StoredPlugIn => !!p)
    },

    async readPlugIn(id) {
      const record = readRecord(plugInPrefix + id)
      return record && toStored(record)
    },

    async writePlugIn(plugIn) {
      const record: StoredRecord = {
        id: plugIn.id,
        hasNewerVersion: plugIn.state.hasNewerVersion,
        plugInSourceId: plugIn.sourceUrl,
        fileUrl: plugIn.fileUrl,
        assemblyBytes: plugIn.assembly,
        enabled: plugIn.state.enabled,
        featureToggles: Object.fromEntries(plugIn.state.toggles.map((t) => [t.hookId, t.enabled])),
        plugInSettings: plugIn.state.settings.map((s) => toStoredSetting(plugIn.id, s)),
      }
      storage.setItem(plugInPrefix + plugIn.id, JSON.stringify(record))
      await writeFiles(plugIn)
    },

    async removePlugIn(id) {
      storage.removeItem(plugInPrefix + id)
      const db = await files()
      const existing = await request<IDBValidKey[]>(db, 'readonly', (store) => store.index(plugInIdIndex).getAllKeys(id))
      await transaction(db, (store) => existing.forEach((key) => store.delete(key)))
    },

    readSources() {
      // Sources stored before the default source moved to GitHub sit under a relative url nothing serves.
      return keys(storage, sourcePrefix).filter((key) => /^https?:/.test(key.slice(sourcePrefix.length))).flatMap((key) => {
        try {
          const value = storage.getItem(key)
          return value ? [readSource(JSON.parse(value) as Json, key.slice(sourcePrefix.length))] : []
        } catch (error) {
          console.error(`Couldn't read the plug-in source stored at ${key}.`, error)
          return []
        }
      })
    },

    writeSource(source) {
      storage.setItem(
        sourcePrefix + source.sourceUrl,
        JSON.stringify({
          ...source,
          plugIns: source.plugIns.map((p) => ({
            ...p,
            publishedVersions: p.publishedVersions.map((v) => ({ Version: v.version, Sdk: v.sdk })),
          })),
        }),
      )
    },
  }
}

function toSetting(setting: StoredSetting, file: Base64 | null): PlugInSetting {
  const value: PlugInSetting = {
    key: setting.key,
    readOnly: setting.readOnly ?? false,
    stringValue: null,
    booleanValue: null,
    integerValue: null,
    fileName: null,
    file: null,
  }
  if (setting.fileName != null) return { ...value, fileName: setting.fileName, file }
  if (setting.integerValue != null) return { ...value, integerValue: setting.integerValue }
  if (setting.booleanValue != null) return { ...value, booleanValue: setting.booleanValue }
  return { ...value, stringValue: setting.stringValue ?? null }
}

function toStoredSetting(plugInId: string, setting: PlugInSetting): StoredSetting {
  const stored: StoredSetting = { key: setting.key, readOnly: setting.readOnly }
  if (setting.fileName != null) return { ...stored, fileName: setting.fileName, filePlugInId: plugInId }
  if (setting.integerValue != null) return { ...stored, integerValue: setting.integerValue }
  if (setting.booleanValue != null) return { ...stored, booleanValue: setting.booleanValue }
  return { ...stored, stringValue: setting.stringValue ?? undefined }
}

// Older manifests list a version as a plain string, which targets SDK 1.
function readVersion(entry: unknown): PublishedVersion {
  if (typeof entry === 'string') return { version: entry, sdk: 1 }
  const json = entry as Json
  const sdk = field(json, 'sdk')
  return { version: text(json, 'version') ?? '', sdk: typeof sdk === 'number' ? sdk : 1 }
}

function field(json: Json, camelName: string): unknown {
  return json[camelName] ?? json[camelName[0].toUpperCase() + camelName.slice(1)]
}

function text(json: Json, camelName: string) {
  const value = field(json, camelName)
  return typeof value === 'string' ? value : undefined
}

function list(json: Json, camelName: string) {
  const value = field(json, camelName)
  return Array.isArray(value) ? (value as Json[]) : []
}

function keys(storage: Storage, prefix: string) {
  return Array.from({ length: storage.length }, (_, i) => storage.key(i)!).filter((key) => key.startsWith(prefix))
}

function trimSlash(url: string) {
  return url.replace(/\/+$/, '')
}

function fileKey(plugInId: string, fileName: string) {
  return `${plugInId}#${fileName}`
}

function openDatabase(indexedDb: IDBFactory) {
  return new Promise<IDBDatabase>((resolve, reject) => {
    const open = indexedDb.open(databaseName)
    open.onupgradeneeded = () => {
      if (open.result.objectStoreNames.contains(filesStore)) return
      const store = open.result.createObjectStore(filesStore, { keyPath: 'key' })
      store.createIndex(plugInIdIndex, plugInIdIndex, { unique: false })
    }
    open.onsuccess = () => resolve(open.result)
    open.onerror = () => reject(open.error)
  })
}

function request<T>(db: IDBDatabase, mode: IDBTransactionMode, run: (store: IDBObjectStore) => IDBRequest) {
  return new Promise<T>((resolve, reject) => {
    const call = run(db.transaction(filesStore, mode).objectStore(filesStore))
    call.onsuccess = () => resolve(call.result as T)
    call.onerror = () => reject(call.error)
  })
}

function transaction(db: IDBDatabase, run: (store: IDBObjectStore) => void) {
  return new Promise<void>((resolve, reject) => {
    const tx = db.transaction(filesStore, 'readwrite')
    run(tx.objectStore(filesStore))
    tx.oncomplete = () => resolve()
    tx.onerror = () => reject(tx.error)
  })
}
