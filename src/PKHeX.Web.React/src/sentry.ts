import * as Sentry from '@sentry/browser'
import { EngineError, type Engine, type ErrorCode, type SaveVersion } from '@pkhex-everywhere/engine'

const dsn = 'https://48a86c94313f2f1c2066dee9be6add57@o4507742210949120.ingest.de.sentry.io/4507742217175120'

const enabledInBuild = import.meta.env.VITE_SENTRY === 'true'

const internalCodes: ErrorCode[] = ['unexpected', 'unknown-call', 'bad-arguments']

let game: SaveVersion | null = null

export function startSentry(enabled = enabledInBuild, options: Sentry.BrowserOptions = {}) {
  if (!enabled) return

  Sentry.init({
    dsn,
    tracesSampleRate: 0.1,
    integrations: [Sentry.browserTracingIntegration()],
    ...options,
  })

  Sentry.getGlobalScope().addEventProcessor((event) => ({
    ...event,
    contexts: { ...event.contexts, game_context: { ...gameContext(), ...event.contexts?.game_context } },
  }))
}

type WatchedEngine = Pick<Engine, 'ready' | 'subscribe' | 'onCallFailed'> & { game: Pick<Engine['game'], 'version'> }

export function watchEngine(engine: WatchedEngine, enabled = enabledInBuild) {
  if (!enabled) return

  const refresh = () =>
    engine.game.version().then(
      (version) => (game = version),
      () => {},
    )
  engine.ready.then(refresh, (error: unknown) => Sentry.captureException(error, { tags: { boot: 'failed' } }))
  engine.subscribe(['game'], () => void refresh())

  engine.onCallFailed((call, error) => {
    if (error instanceof EngineError && !internalCodes.includes(error.code)) return
    Sentry.captureException(error, { tags: { engine_call: call } })
  })
}

export function captureError(error: unknown, tags?: Record<string, string>) {
  Sentry.captureException(error, { tags })
}

export function currentRoute() {
  return location.href.replace(document.baseURI, '')
}

function gameContext() {
  return {
    current_route: currentRoute(),
    version_name: game?.version ?? null,
    version_id: game?.versionId ?? null,
    generation_name: game?.generation ?? null,
    generation_id: game?.generationId ?? null,
  }
}
