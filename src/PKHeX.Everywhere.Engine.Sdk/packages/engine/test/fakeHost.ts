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
  const changeListeners = new Set<(topics: string[]) => void>()
  const eventListeners = new Set<(event: string) => void>()
  const progressListeners = new Set<(loaded: number, total: number) => void>()

  const listen = <A extends unknown[]>(listeners: Set<(...args: A) => void>, listener: (...args: A) => void) => {
    const entry = (...args: A) => listener(...args)
    listeners.add(entry)
    return () => void listeners.delete(entry)
  }

  const host: EngineHost = {
    ready: () => {
      readyCalls++
      return ready
    },
    onChange: (listener) => listen(changeListeners, listener),
    onEvent: (listener) => listen(eventListeners, listener),
    onProgress: (listener) => listen(progressListeners, listener),
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
    listeners: () => ({ change: changeListeners.size, event: eventListeners.size, progress: progressListeners.size }),
  }
}
