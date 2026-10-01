import { act, cleanup, render, screen } from '@testing-library/react'
import { Suspense } from 'react'
import { afterEach, describe, expect, it } from 'vitest'
import type { Engine, PokemonSummary } from '@pkhex-everywhere/engine'
import { EngineProvider, useQuery } from '../src'

const pikachu = { id: 'pikachu:1', species: 'Pikachu', level: 12 } as PokemonSummary

function deferredEngine() {
  const calls: { name: string; args: unknown[] }[] = []
  const pending: ((value: unknown) => void)[] = []
  const engine = {
    call: (name: string, args: unknown[]) => {
      calls.push({ name, args })
      return new Promise((resolve) => pending.push(resolve))
    },
  } as unknown as Engine
  const resolveAll = (value: unknown) => act(async () => pending.splice(0).forEach((resolve) => resolve(value)))
  return { engine, calls, resolveAll }
}

function PartyNames() {
  const party = useQuery('party.get')
  return <p>{party.map((p) => `${p.species} ${p.level}`).join(', ')}</p>
}

afterEach(cleanup)

describe('useQuery', () => {
  it('suspends until the first result arrives', async () => {
    const { engine, resolveAll } = deferredEngine()

    render(
      <EngineProvider engine={engine}>
        <Suspense fallback={<p>loading</p>}>
          <PartyNames />
        </Suspense>
      </EngineProvider>,
    )

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
    const engine = { call: () => Promise.reject(new Error('boom')) } as unknown as Engine
    const { ErrorBoundary } = await import('./ErrorBoundary')

    render(
      <EngineProvider engine={engine}>
        <ErrorBoundary>
          <Suspense fallback={<p>loading</p>}>
            <PartyNames />
          </Suspense>
        </ErrorBoundary>
      </EngineProvider>,
    )
    await act(async () => {})

    expect(screen.getByText('error: boom')).toBeDefined()
  })
})
