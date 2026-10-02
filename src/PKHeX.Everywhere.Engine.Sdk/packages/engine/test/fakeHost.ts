import type { EngineHost } from '../src'

export function fakeHost(dispatch: (name: string, args: unknown[]) => unknown) {
  let signalReady!: () => void
  let failReady!: (error: unknown) => void
  const ready = new Promise<void>((resolve, reject) => {
    signalReady = resolve
    failReady = reject
  })
  let readyCalls = 0
  const calls: { name: string; args: string }[] = []
  const requestedAssemblies: string[] = []
  const changeListeners: ((topics: string[]) => void)[] = []
  const eventListeners: ((event: string) => void)[] = []
  const progressListeners: ((loaded: number, total: number) => void)[] = []

  const host: EngineHost = {
    ready: () => {
      readyCalls++
      return ready
    },
    onChange: (listener) => void changeListeners.push(listener),
    onEvent: (listener) => void eventListeners.push(listener),
    onProgress: (listener) => void progressListeners.push(listener),
    getAssemblyExports: async (assemblyName) => {
      requestedAssemblies.push(assemblyName)
      return {
        PKHeX: {
          Everywhere: {
            Engine: {
              EngineExports: {
                Call: async (name, args) => {
                  calls.push({ name, args })
                  return JSON.stringify(dispatch(name, JSON.parse(args)))
                },
              },
            },
          },
        },
      }
    },
  }

  const emitChange = (topics: string[]) => changeListeners.forEach((listener) => listener(topics))

  const emitEvent = (event: string) => eventListeners.forEach((listener) => listener(event))

  const emitProgress = (loaded: number, total: number) => progressListeners.forEach((listener) => listener(loaded, total))

  return {
    host,
    signalReady,
    failReady,
    emitChange,
    emitEvent,
    emitProgress,
    calls,
    requestedAssemblies,
    readyCalls: () => readyCalls,
  }
}
