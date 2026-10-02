import * as Sentry from '@sentry/browser'
import { EngineError, type Engine, type ErrorCode, type SaveVersion } from '@pkhex-everywhere/engine'

declare global {
  var pkhexBlazorStarted: Promise<unknown> | undefined
}

const dsn = 'https://48a86c94313f2f1c2066dee9be6add57@o4507742210949120.ingest.de.sentry.io/4507742217175120'

const enabledInBuild = import.meta.env.VITE_SENTRY === 'true'

const internalCodes: ErrorCode[] = ['unexpected', 'unknown-call', 'bad-arguments']

let game: SaveVersion | null = null

const reportedBlazorErrors = new Set<string>()

export interface BlazorException {
  type: string
  message: string
  details: string
  id: string
}

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

  globalThis.pkhexBlazorStarted?.catch((error: unknown) => Sentry.captureException(error, { tags: { boot: 'failed' } }))
}

type WatchedEngine = Pick<Engine, 'ready' | 'subscribe' | 'onCallFailed'> & { game: Pick<Engine['game'], 'version'> }

export function watchEngine(engine: WatchedEngine, enabled = enabledInBuild) {
  if (!enabled) return

  const refresh = () =>
    engine.game.version().then(
      (version) => (game = version),
      () => {},
    )
  void engine.ready.then(refresh)
  engine.subscribe(['game'], () => void refresh())

  engine.onCallFailed((call, error) => {
    if (error instanceof EngineError && !internalCodes.includes(error.code)) return
    Sentry.captureException(error, { tags: { engine_call: call } })
  })
}

export function captureError(error: unknown, tags?: Record<string, string>) {
  Sentry.captureException(error, { tags })
}

// An error boundary both tracks and logs its exception, so .NET reports it twice.
export function captureBlazorError(exception: BlazorException) {
  if (reportedBlazorErrors.has(exception.id)) return
  reportedBlazorErrors.add(exception.id)
  const error = new Error(exception.message)
  error.name = exception.type
  error.stack = `${exception.type}: ${exception.message}`
  Sentry.captureException(error, {
    tags: { exception_id: exception.id },
    contexts: { game_context: { exception_id: exception.id } },
    extra: { details: exception.details },
  })
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
