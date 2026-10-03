import { useSyncExternalStore } from 'react'

export function createStore<T>(initial: T) {
  let value = initial
  const listeners = new Set<() => void>()
  const subscribe = (listener: () => void) => {
    listeners.add(listener)
    return () => void listeners.delete(listener)
  }
  return {
    get: () => value,
    set(next: T) {
      if (next === value) return
      value = next
      listeners.forEach((listener) => listener())
    },
    subscribe,
    use: () => useSyncExternalStore(subscribe, () => value),
  }
}
