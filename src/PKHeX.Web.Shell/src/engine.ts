export type EngineError = { code: string; message: string }
export type Result<T> = { ok: true; value: T } | { ok: false; error: EngineError }

export type SaveSummaryDto = { fileName: string; version: string; trainer: string }
export type PartySlotDto = {
  slot: number
  speciesId: number
  species: string
  nickname: string
  level: number
  isShiny: boolean
  heldItem: string
}

type Exports = {
  LoadSave(bytes: Uint8Array, fileName: string): string
  GetSave(): string
  GetParty(): string
}

declare global {
  interface Window {
    Blazor: {
      start(options?: object): Promise<void>
      navigateTo(uri: string, options: { replaceHistoryEntry: boolean }): void
      runtime?: { getAssemblyExports(assembly: string): Promise<any> }
      rootComponents: {
        add(element: HTMLElement, identifier: string, parameters: object): Promise<{
          setParameters(parameters: object): Promise<void>
          dispose(): Promise<void>
        }>
      }
    }
    pkhexEngine: { onChanged(topic: string): void; onNavigate(path: string): void; onReady(): void }
    getDotnetRuntime?: (id: number) => { getAssemblyExports(assembly: string): Promise<any> }
  }
}

const listeners = new Set<(topic: string) => void>()
let navigateListener: (path: string) => void = () => {}

export function onBlazorNavigate(listener: (path: string) => void) {
  navigateListener = listener
}
let exportsPromise: Promise<Exports> | undefined

export function subscribe(listener: (topic: string) => void) {
  listeners.add(listener)
  return () => {
    listeners.delete(listener)
  }
}

export function boot(): Promise<Exports> {
  exportsPromise ??= (async () => {
    let ready!: () => void
    const hostReady = new Promise<void>((r) => (ready = r))
    window.pkhexEngine = {
      onReady: () => ready(),
      onChanged: (topic) => listeners.forEach((l) => l(topic)),
      onNavigate: (path) => navigateListener(path),
    }
    await window.Blazor.start({ environment: 'Shell' })
    const runtime = window.Blazor.runtime ?? window.getDotnetRuntime!(0)
    const assembly = await runtime.getAssemblyExports('PKHeX.Engine.dll')
    await hostReady
    performance.mark('engine-ready')
    return assembly.PKHeX.Engine.EngineExports as Exports
  })()
  return exportsPromise
}

async function call<T>(invoke: (e: Exports) => string): Promise<Result<T>> {
  return JSON.parse(invoke(await boot())) as Result<T>
}

export const engine = {
  loadSave: (bytes: Uint8Array, fileName: string) => call<SaveSummaryDto>((e) => e.LoadSave(bytes, fileName)),
  getSave: () => call<SaveSummaryDto>((e) => e.GetSave()),
  getParty: () => call<PartySlotDto[]>((e) => e.GetParty()),
}
