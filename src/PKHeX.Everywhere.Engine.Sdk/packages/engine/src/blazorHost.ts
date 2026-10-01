import type { AssemblyExports, EngineHost } from './host'

interface DotnetRuntime {
  getAssemblyExports(assemblyName: string): Promise<AssemblyExports>
}

declare global {
  var pkhexEngineReady: boolean | undefined
  var pkhexEngineOnReady: (() => void) | undefined
  var pkhexEngineOnChange: ((topics: string[]) => void) | undefined
  var Blazor: { runtime?: DotnetRuntime } | undefined
  var getDotnetRuntime: ((id: number) => DotnetRuntime | undefined) | undefined
}

// Blazor.start() resolves before the renderer attaches, so readiness comes from the .NET side (EngineExports.SignalReady).
export function blazorHost(): EngineHost {
  return {
    ready: () =>
      globalThis.pkhexEngineReady
        ? Promise.resolve()
        : new Promise((resolve) => {
            const previous = globalThis.pkhexEngineOnReady
            globalThis.pkhexEngineOnReady = () => {
              previous?.()
              resolve()
            }
          }),
    onChange: (listener) => {
      globalThis.pkhexEngineOnChange = listener
    },
    getAssemblyExports: (assemblyName) => {
      const runtime = globalThis.Blazor?.runtime ?? globalThis.getDotnetRuntime?.(0)
      if (!runtime) return Promise.reject(new Error('The .NET runtime is not available.'))
      return runtime.getAssemblyExports(assemblyName)
    },
  }
}
