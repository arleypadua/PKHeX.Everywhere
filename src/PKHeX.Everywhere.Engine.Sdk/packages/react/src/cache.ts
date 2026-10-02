import { affects } from '@pkhex-everywhere/engine/internal'

type Entry =
  | { status: 'pending'; promise: Promise<void> }
  | { status: 'success'; value: unknown }
  | { status: 'error'; error: unknown }

interface Query {
  entry: Entry
  topics: readonly string[]
  fetch: () => Promise<unknown>
  generation: number
}

export class QueryCache {
  private readonly queries = new Map<string, Query>()
  private readonly listeners = new Set<() => void>()

  get(key: string, topics: readonly string[], fetch: () => Promise<unknown>): Entry {
    const existing = this.queries.get(key)
    if (existing) return existing.entry

    const query = { topics, fetch, generation: 0 } as Query
    this.queries.set(key, query)
    this.load(key, query)
    return query.entry
  }

  peek(key: string): Entry | undefined {
    return this.queries.get(key)?.entry
  }

  subscribe = (listener: () => void) => {
    this.listeners.add(listener)
    return () => void this.listeners.delete(listener)
  }

  invalidate = (changed: readonly string[]) => {
    for (const [key, query] of this.queries) {
      if (!affects(changed, query.topics)) continue
      if (query.entry.status === 'error') this.queries.delete(key)
      else this.load(key, query)
    }
  }

  private load(key: string, query: Query) {
    const generation = ++query.generation
    const promise = query.fetch().then(
      (value) => this.settle(key, query, generation, { status: 'success', value }),
      (error: unknown) => this.settle(key, query, generation, { status: 'error', error }),
    )
    // A success entry stays in place so pages keep showing it during the refetch.
    if (query.entry?.status !== 'success') query.entry = { status: 'pending', promise }
  }

  private settle(key: string, query: Query, generation: number, entry: Entry) {
    if (query.generation !== generation || this.queries.get(key) !== query) return
    query.entry = entry
    this.listeners.forEach((listener) => listener())
  }
}
