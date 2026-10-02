import { version } from '../package.json'
import { crypto } from './crypto'
import { dotnetUrlMeta } from './dotnetUrlMeta'
import type { AssemblyExports, EngineHost } from './host'

export interface DotnetRuntime {
  setModuleImports(moduleName: string, moduleImports: Record<string, unknown>): void
  getAssemblyExports(assemblyName: string): Promise<AssemblyExports>
  runMain(): Promise<number>
}

interface DotnetModule {
  dotnet: { create(): Promise<DotnetRuntime> }
}

declare global {
  var pkhexEngineOnChange: ((topics: string[]) => void) | undefined
  var pkhexEngineOnEvent: ((event: string) => void) | undefined
}

export interface WasmHostOptions {
  dotnetUrl?: string
  load?: (url: string) => Promise<DotnetModule>
}

const cdnDotnetUrl = `https://cdn.jsdelivr.net/npm/@pkhex-everywhere/engine@${version}/_framework/dotnet.js`

function pageDotnetUrl() {
  const content = globalThis.document?.querySelector<HTMLMetaElement>(`meta[name="${dotnetUrlMeta}"]`)?.content
  return content ? new URL(content, document.baseURI).href : undefined
}

const importModule = (url: string) => import(/* @vite-ignore */ url) as Promise<DotnetModule>

export function wasmHost({
  dotnetUrl = pageDotnetUrl() ?? cdnDotnetUrl,
  load = importModule,
}: WasmHostOptions = {}): EngineHost {
  let booted: Promise<DotnetRuntime> | undefined
  const boot = () =>
    (booted ??= load(dotnetUrl).then(async ({ dotnet }) => {
      const runtime = await dotnet.create()
      runtime.setModuleImports('pkhex-crypto', crypto)
      await runtime.runMain()
      return runtime
    }))

  return {
    ready: () => boot().then(() => undefined),
    onChange: (listener) => {
      globalThis.pkhexEngineOnChange = listener
    },
    onEvent: (listener) => {
      globalThis.pkhexEngineOnEvent = listener
    },
    getAssemblyExports: (assemblyName) => boot().then((runtime) => runtime.getAssemblyExports(assemblyName)),
  }
}
