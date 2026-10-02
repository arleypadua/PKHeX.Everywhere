export interface EngineExports {
  Call(name: string, args: string): Promise<string>
}

export interface AssemblyExports {
  PKHeX: { Everywhere: { Engine: { EngineExports: EngineExports } } }
}

/** Runs the .NET engine and relays its notifications. `wasmHost()` is the one the SDK ships; pass your own to `createEngine` to run it elsewhere. */
export interface EngineHost {
  ready(): Promise<void>
  onChange(listener: (topics: string[]) => void): void | (() => void)
  onEvent(listener: (event: string) => void): void | (() => void)
  onProgress?(listener: (loaded: number, total: number) => void): void | (() => void)
  getAssemblyExports(assemblyName: string): Promise<AssemblyExports>
}
