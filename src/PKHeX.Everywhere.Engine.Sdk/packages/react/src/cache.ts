type Entry =
  | { status: 'pending'; promise: Promise<void> }
  | { status: 'success'; value: unknown }
  | { status: 'error'; error: unknown }

export class QueryCache {
  private readonly entries = new Map<string, Entry>()
  private readonly listeners = new Set<() => void>()

  get(key: string, fetch: () => Promise<unknown>): Entry {
    const existing = this.entries.get(key)
    if (existing) return existing

    const promise = fetch().then(
      (value) => this.settle(key, { status: 'success', value }),
      (error: unknown) => this.settle(key, { status: 'error', error }),
    )
    const entry: Entry = { status: 'pending', promise }
    this.entries.set(key, entry)
    return entry
  }

  peek(key: string): Entry | undefined {
    return this.entries.get(key)
  }

  subscribe = (listener: () => void) => {
    this.listeners.add(listener)
    return () => void this.listeners.delete(listener)
  }

  private settle(key: string, entry: Entry) {
    this.entries.set(key, entry)
    this.listeners.forEach((listener) => listener())
  }
}
