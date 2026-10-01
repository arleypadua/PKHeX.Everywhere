export interface EngineExports {
  Call(name: string, args: string): string
}

export interface AssemblyExports {
  PKHeX: { Everywhere: { Engine: { EngineExports: EngineExports } } }
}

export interface EngineHost {
  ready(): Promise<void>
  getAssemblyExports(assemblyName: string): Promise<AssemblyExports>
}
