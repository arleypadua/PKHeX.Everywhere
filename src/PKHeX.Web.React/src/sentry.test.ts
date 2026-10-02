import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import * as Sentry from '@sentry/browser'
import { EngineError, type CallName, type SaveVersion, type Topic } from '@pkhex-everywhere/engine'
import { captureError, startSentry, watchEngine } from './sentry'

const flush = () => new Promise((resolve) => setTimeout(resolve, 0))

const emerald: SaveVersion = { version: 'Emerald', versionId: 3, generation: 'Gen3', generationId: 3, formatId: null }

const unbound: SaveVersion = { version: 'FireRed', versionId: 5, generation: 'Gen9', generationId: 9, formatId: 'unbound' }

function fakeEngine(version: SaveVersion | null = emerald) {
  const failureListeners: ((call: CallName, error: unknown) => void)[] = []
  const subscribers: ((topics: Topic[]) => void)[] = []
  const engine = {
    ready: Promise.resolve(),
    game: { version: async () => engine.loaded },
    loaded: version,
    subscribe: (_topics: readonly Topic[], callback: (topics: Topic[]) => void) => {
      subscribers.push(callback)
      return () => {}
    },
    onCallFailed: (listener: (call: CallName, error: unknown) => void) => {
      failureListeners.push(listener)
      return () => {}
    },
    fail: (call: CallName, error: unknown) => failureListeners.forEach((listener) => listener(call, error)),
    change: (topics: Topic[]) => subscribers.forEach((callback) => callback(topics)),
  }
  return engine
}

let events: Sentry.ErrorEvent[] = []

function start() {
  startSentry(true, {
    beforeSend: (event) => {
      events.push(event)
      return null
    },
  })
}

describe('Sentry', () => {
  beforeEach(() => {
    events = []
    document.head.innerHTML = '<base href="/" />'
    history.replaceState(null, '', '/party?box=2')
  })

  afterEach(async () => {
    await Sentry.close()
  })

  it('does nothing when disabled', async () => {
    startSentry(false)

    expect(Sentry.getClient()).toBeUndefined()
  })

  it('uses the production DSN and samples 10% of traces', () => {
    startSentry(true)

    expect(Sentry.getClient()?.getOptions()).toMatchObject({
      dsn: 'https://48a86c94313f2f1c2066dee9be6add57@o4507742210949120.ingest.de.sentry.io/4507742217175120',
      tracesSampleRate: 0.1,
    })
  })

  it('captures uncaught errors and unhandled rejections', async () => {
    start()

    window.onerror?.('Uncaught Error: Boom', 'app.js', 1, 1, new Error('Boom'))
    window.onunhandledrejection?.(Object.assign(new Event('unhandledrejection'), { reason: new Error('Rejected') }) as PromiseRejectionEvent)
    await flush()

    expect(events.map((event) => event.exception?.values?.[0]?.value)).toEqual(['Boom', 'Rejected'])
  })

  it('captures an Engine host that fails to boot', async () => {
    start()
    const engine = { ...fakeEngine(), ready: Promise.reject(new Error('Main failed')) }

    watchEngine(engine, true)
    await flush()

    expect(events).toHaveLength(1)
    expect(events[0].exception?.values?.[0]?.value).toBe('Main failed')
    expect(events[0].tags).toMatchObject({ boot: 'failed' })
  })

  it('captures Engine calls that fail with an unexpected or internal code', async () => {
    start()
    const engine = fakeEngine()
    watchEngine(engine, true)

    engine.fail('party.get', new EngineError('unexpected', 'Boom'))
    engine.fail('party.get', new EngineError('bad-arguments', 'Bad'))
    engine.fail('nope' as CallName, new EngineError('unknown-call', 'Unknown'))
    engine.fail('party.get', new Error('The .NET runtime is not available.'))
    engine.fail('party.get', new EngineError('no-save', 'No save is loaded.'))
    engine.fail('box.addFromFile', new EngineError('unparseable', 'Not a Pokémon.'))
    await flush()

    expect(events.map((event) => [event.tags?.engine_call, event.exception?.values?.[0]?.value])).toEqual([
      ['party.get', 'Boom'],
      ['party.get', 'Bad'],
      ['nope', 'Unknown'],
      ['party.get', 'The .NET runtime is not available.'],
    ])
  })

  it('adds the current route and loaded game to every event', async () => {
    start()
    const engine = fakeEngine()
    watchEngine(engine, true)
    await flush()

    captureError(new Error('Boom'))
    await flush()

    expect(events[0].contexts?.game_context).toEqual({
      current_route: 'party?box=2',
      version_name: 'Emerald',
      version_id: 3,
      generation_name: 'Gen3',
      generation_id: 3,
      format_id: null,
    })
  })

  it('adds the save format of the loaded game', async () => {
    start()
    watchEngine(fakeEngine(unbound), true)
    await flush()

    captureError(new Error('Boom'))
    await flush()

    expect(events[0].contexts?.game_context?.format_id).toBe('unbound')
  })

  it('follows the loaded game as it changes', async () => {
    start()
    const engine = fakeEngine(null)
    watchEngine(engine, true)
    await flush()

    captureError(new Error('Before'))
    engine.loaded = emerald
    engine.change(['*'])
    await flush()
    captureError(new Error('After'))
    await flush()

    expect(events.map((event) => event.contexts?.game_context?.version_name)).toEqual([null, 'Emerald'])
  })

  it('does not watch the Engine when disabled', async () => {
    const engine = fakeEngine()
    let subscribed = false
    engine.subscribe = () => {
      subscribed = true
      return () => {}
    }
    engine.onCallFailed = () => {
      subscribed = true
      return () => {}
    }

    watchEngine(engine, false)

    expect(subscribed).toBe(false)
  })

  it('tags captured errors', async () => {
    start()

    captureError(new Error('Boom'), { exception_id: 'b7a9f2c4' })
    await flush()

    expect(events[0].tags).toMatchObject({ exception_id: 'b7a9f2c4' })
  })
})
