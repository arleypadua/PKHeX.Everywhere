import { createEngine, EngineError, type EngineHost } from '@pkhex-everywhere/engine'

type Dispatch = (name: string, args: unknown[]) => unknown

export function fakeEngine(dispatch: Dispatch, { booted = true } = {}) {
  const calls: { name: string; args: unknown[] }[] = []
  const changeListeners = new Set<(topics: string[]) => void>()
  const progressListeners = new Set<(loaded: number, total: number) => void>()
  let boot!: { resolve: () => void; reject: (error: unknown) => void }
  const ready = booted ? Promise.resolve() : new Promise<void>((resolve, reject) => (boot = { resolve, reject }))

  const host: EngineHost = {
    ready: () => ready,
    onChange: (listener) => void changeListeners.add(listener),
    onEvent: () => {},
    onProgress: (listener) => void progressListeners.add(listener),
    getAssemblyExports: async () => ({
      PKHeX: {
        Everywhere: {
          Engine: {
            EngineExports: {
              Call: async (name, json) => {
                const args = JSON.parse(json) as unknown[]
                calls.push({ name, args })
                try {
                  return JSON.stringify({ ok: true, value: (await dispatch(name, args)) ?? null })
                } catch (error) {
                  if (!(error instanceof EngineError)) throw error
                  return JSON.stringify({ ok: false, error: { code: error.code, message: error.message } })
                }
              },
            },
          },
        },
      },
    }),
  }

  return {
    engine: createEngine({ host }),
    calls,
    emitChange: (changed: string[]) => changeListeners.forEach((listener) => listener(changed)),
    emitProgress: (loaded: number, total: number) => progressListeners.forEach((listener) => listener(loaded, total)),
    finishBoot: () => boot.resolve(),
    failBoot: (error: unknown) => boot.reject(error),
  }
}
