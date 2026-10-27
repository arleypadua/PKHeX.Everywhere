import { act, cleanup, render, screen } from '@testing-library/react'
import { Suspense, type ReactNode } from 'react'
import { afterEach, describe, expect, it } from 'vitest'
import { EngineError, type Engine, type PokemonSummary } from '@pkhex-everywhere/engine'
import { EngineProvider, useQuery } from '../src'
import { ErrorBoundary } from './ErrorBoundary'
import { fakeEngine } from './fakeEngine'

const pikachu = { id: 'pikachu:1', species: 'Pikachu', level: 12 } as PokemonSummary
const raichu = { id: 'pikachu:1', species: 'Raichu', level: 30 } as PokemonSummary

function deferredEngine() {
  const pending: { resolve: (value: unknown) => void; reject: (error: unknown) => void }[] = []
  const { engine, calls, emitChange } = fakeEngine(() => new Promise((resolve, reject) => pending.push({ resolve, reject })))
  const settle = (settle: () => void) =>
    act(async () => {
      await new Promise((resolve) => setTimeout(resolve))
      settle()
    })
  const resolveAll = (value: unknown) => settle(() => pending.splice(0).forEach((p) => p.resolve(value)))
  const resolveNext = (value: unknown) => settle(() => pending.shift()!.resolve(value))
  const rejectAll = (error: unknown) => settle(() => pending.splice(0).forEach((p) => p.reject(error)))
  return { engine, calls, resolveAll, resolveNext, rejectAll, emitChange: (changed: string[]) => act(async () => emitChange(changed)) }
}

function PartyNames() {
  const party = useQuery('party.get')
  return <p>{party.map((p) => `${p.species} ${p.level}`).join(', ')}</p>
}

function Encounters({ version }: { version: number }) {
  const rows = useQuery('encounters.search', version, 25)
  return <p>encounters: {rows.length}</p>
}

function renderParty(engine: Engine) {
  return render(
    <EngineProvider engine={engine}>
      <ErrorBoundary>
        <Suspense fallback={<p>loading</p>}>
          <PartyNames />
        </Suspense>
      </ErrorBoundary>
    </EngineProvider>,
  )
}

afterEach(cleanup)

describe('useQuery', () => {
  it('suspends until the first result arrives', async () => {
    const { engine, resolveAll } = deferredEngine()

    renderParty(engine)

    expect(screen.getByText('loading')).toBeDefined()

    await resolveAll([pikachu])

    expect(screen.getByText('Pikachu 12')).toBeDefined()
    expect(screen.queryByText('loading')).toBeNull()
  })

  it('sends one request for components reading the same call and arguments', async () => {
    const { engine, calls, resolveAll } = deferredEngine()

    render(
      <EngineProvider engine={engine}>
        <Suspense fallback={<p>loading</p>}>
          <PartyNames />
          <PartyNames />
        </Suspense>
      </EngineProvider>,
    )
    await resolveAll([pikachu])

    expect(screen.getAllByText('Pikachu 12')).toHaveLength(2)
    expect(calls).toEqual([{ name: 'party.get', args: [] }])
  })

  it('throws query errors to the nearest error boundary', async () => {
    const { engine } = fakeEngine(() => {
      throw new Error('boom')
    })

    renderParty(engine)
    await act(async () => {})

    expect(screen.getByText('error: boom')).toBeDefined()
  })

  it('refetches a query when a topic it reads changes', async () => {
    const { engine, calls, resolveAll, emitChange } = deferredEngine()
    renderParty(engine)
    await resolveAll([pikachu])

    await emitChange(['party'])
    await resolveAll([raichu])

    expect(calls).toHaveLength(2)
    expect(screen.getByText('Raichu 30')).toBeDefined()
  })

  it('keeps showing the old data while the refetch runs', async () => {
    const { engine, resolveAll, emitChange } = deferredEngine()
    renderParty(engine)
    await resolveAll([pikachu])

    await emitChange(['party'])

    expect(screen.getByText('Pikachu 12')).toBeDefined()
    expect(screen.queryByText('loading')).toBeNull()
  })

  it('suspends and shows fresh data after a save change', async () => {
    const { engine, resolveAll, emitChange } = deferredEngine()
    renderParty(engine)
    await resolveAll([pikachu])

    await emitChange(['*'])
    expect(screen.getByText('loading')).toBeDefined()

    await resolveAll([raichu])
    expect(screen.getByText('Raichu 30')).toBeDefined()
  })

  it('does not fetch a query the new save rejects after a save change', async () => {
    let loaded = false
    const { engine, calls, emitChange } = fakeEngine((name, args) => {
      if (name === 'encounters.search' && loaded && args[0] === 20)
        throw new EngineError('bad-arguments', 'SoulSilver has no encounters for version 20.')
      return []
    })
    const page = (child: ReactNode) => (
      <EngineProvider engine={engine}>
        <ErrorBoundary>
          <Suspense fallback={<p>loading</p>}>{child}</Suspense>
        </ErrorBoundary>
      </EngineProvider>
    )
    const { rerender } = render(page(<Encounters version={20} />))
    await act(async () => {})
    expect(screen.getByText('encounters: 0')).toBeDefined()

    rerender(page(null))
    loaded = true
    await act(async () => emitChange(['*']))

    expect(calls).toEqual([{ name: 'encounters.search', args: [20, 25] }])
  })

  it('ignores changes to topics the query does not read', async () => {
    const { engine, calls, resolveAll, emitChange } = deferredEngine()
    renderParty(engine)
    await resolveAll([pikachu])

    await emitChange(['box/3'])

    expect(calls).toHaveLength(1)
  })

  it('shows the newest data when a change arrives during the first fetch', async () => {
    const { engine, resolveNext, emitChange } = deferredEngine()
    renderParty(engine)

    await emitChange(['party'])
    await resolveNext([pikachu])
    expect(screen.getByText('loading')).toBeDefined()

    await resolveNext([raichu])
    expect(screen.getByText('Raichu 30')).toBeDefined()
  })

  it('fetches a failed query again after a change', async () => {
    const { engine, calls, rejectAll, resolveAll, emitChange } = deferredEngine()
    const { rerender } = renderParty(engine)
    await rejectAll(new Error('boom'))

    await emitChange(['party'])
    rerender(
      <EngineProvider engine={engine}>
        <ErrorBoundary key="retry">
          <Suspense fallback={<p>loading</p>}>
            <PartyNames />
          </Suspense>
        </ErrorBoundary>
      </EngineProvider>,
    )
    await resolveAll([pikachu])

    expect(calls).toHaveLength(2)
    expect(screen.getByText('Pikachu 12')).toBeDefined()
  })
})
