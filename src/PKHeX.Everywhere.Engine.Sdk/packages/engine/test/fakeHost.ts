import type { EngineHost } from '../src'

export function fakeHost(dispatch: (name: string, args: unknown[]) => unknown) {
  let signalReady!: () => void
  const ready = new Promise<void>((resolve) => (signalReady = resolve))
  const calls: { name: string; args: string }[] = []
  const requestedAssemblies: string[] = []

  const host: EngineHost = {
    ready: () => ready,
    getAssemblyExports: async (assemblyName) => {
      requestedAssemblies.push(assemblyName)
      return {
        PKHeX: {
          Everywhere: {
            Engine: {
              EngineExports: {
                Call: (name, args) => {
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

  return { host, signalReady, calls, requestedAssemblies }
}
