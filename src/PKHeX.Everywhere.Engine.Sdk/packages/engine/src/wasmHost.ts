import { crypto } from './crypto'
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

const importModule = (url: string) => import(/* @vite-ignore */ url) as Promise<DotnetModule>

export function wasmHost({ dotnetUrl = '/_framework/dotnet.js', load = importModule }: WasmHostOptions = {}): EngineHost {
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
