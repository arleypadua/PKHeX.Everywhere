import type { EngineHost } from '../src'

export function fakeHost(dispatch: (name: string, args: unknown[]) => unknown) {
  let signalReady!: () => void
  const ready = new Promise<void>((resolve) => (signalReady = resolve))
  const calls: { name: string; args: string }[] = []
  const requestedAssemblies: string[] = []
  const changeListeners: ((topics: string[]) => void)[] = []

  const host: EngineHost = {
    ready: () => ready,
    onChange: (listener) => void changeListeners.push(listener),
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

  return { host, signalReady, emitChange, calls, requestedAssemblies }
}
