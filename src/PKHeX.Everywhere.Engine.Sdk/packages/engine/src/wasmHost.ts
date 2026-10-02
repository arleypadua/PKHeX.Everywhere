import { version } from '../package.json'
import { crypto } from './crypto'
import { dotnetUrlMeta } from './dotnetUrlMeta'
import type { AssemblyExports, EngineHost } from './host'

/** The parts of the .NET WebAssembly runtime the engine uses. */
export interface DotnetRuntime {
  setModuleImports(moduleName: string, moduleImports: Record<string, unknown>): void
  getAssemblyExports(assemblyName: string): Promise<AssemblyExports>
  runMain(): Promise<number>
}

interface DotnetHostBuilder {
  withModuleConfig(config: { onDownloadResourceProgress?: (loaded: number, total: number) => void }): DotnetHostBuilder
  create(): Promise<DotnetRuntime>
}

interface DotnetModule {
  dotnet: DotnetHostBuilder
}

declare global {
  var pkhexEngineOnChange: ((topics: string[]) => void) | undefined
  var pkhexEngineOnEvent: ((event: string) => void) | undefined
}

/** Options for `wasmHost()`. */
export interface WasmHostOptions {
  /** URL of `_framework/dotnet.js`. Defaults to the URL the Vite plugin injects, or else jsDelivr pinned to this package's version. */
  dotnetUrl?: string
  /** Imports `dotnet.js` from a URL. Override it only to load the runtime some other way, for example in tests. */
  load?: (url: string) => Promise<DotnetModule>
}

const cdnDotnetUrl = `https://cdn.jsdelivr.net/npm/@pkhex-everywhere/engine@${version}/_framework/dotnet.js`

function pageDotnetUrl() {
  const content = globalThis.document?.querySelector<HTMLMetaElement>(`meta[name="${dotnetUrlMeta}"]`)?.content
  return content ? new URL(content, document.baseURI).href : undefined
}

const importModule = (url: string) => import(/* @vite-ignore */ url) as Promise<DotnetModule>

const resolveUrl = (url: string) => (globalThis.document ? new URL(url, document.baseURI).href : url)

let shared: { dotnetUrl: string; host: EngineHost } | undefined

/** The host that runs the engine on .NET WebAssembly in the page. A page has one runtime, so every call returns the same host. */
export function wasmHost({ dotnetUrl, load = importModule }: WasmHostOptions = {}): EngineHost {
  if (shared) {
    if (dotnetUrl !== undefined && resolveUrl(dotnetUrl) !== resolveUrl(shared.dotnetUrl))
      console.warn(`wasmHost() already loads the runtime from ${shared.dotnetUrl}, so ${dotnetUrl} is ignored.`)
    return shared.host
  }
  const url = dotnetUrl ?? pageDotnetUrl() ?? cdnDotnetUrl
  shared = { dotnetUrl: url, host: createWasmHost(url, load) }
  return shared.host
}

function createWasmHost(dotnetUrl: string, load: (url: string) => Promise<DotnetModule>): EngineHost {
  const changeListeners = new Set<(topics: string[]) => void>()
  const eventListeners = new Set<(event: string) => void>()
  const progressListeners = new Set<(loaded: number, total: number) => void>()
  let progress: [number, number] | undefined

  globalThis.pkhexEngineOnChange = (topics) => {
    for (const listener of [...changeListeners]) listener(topics)
  }
  globalThis.pkhexEngineOnEvent = (event) => {
    for (const listener of [...eventListeners]) listener(event)
  }

  const reportProgress = (loaded: number, total: number) => {
    progress = [loaded, total]
    for (const listener of [...progressListeners]) listener(loaded, total)
  }

  let booted: Promise<DotnetRuntime> | undefined
  const boot = () =>
    (booted ??= load(dotnetUrl).then(async ({ dotnet }) => {
      const runtime = await dotnet.withModuleConfig({ onDownloadResourceProgress: reportProgress }).create()
      runtime.setModuleImports('pkhex-crypto', crypto)
      await runtime.runMain()
      return runtime
    }))

  return {
    ready: () => boot().then(() => undefined),
    onChange: (listener) => {
      changeListeners.add(listener)
      return () => void changeListeners.delete(listener)
    },
    onEvent: (listener) => {
      eventListeners.add(listener)
      return () => void eventListeners.delete(listener)
    },
    onProgress: (listener) => {
      progressListeners.add(listener)
      if (progress) listener(...progress)
      return () => void progressListeners.delete(listener)
    },
    getAssemblyExports: (assemblyName) => boot().then((runtime) => runtime.getAssemblyExports(assemblyName)),
  }
}
