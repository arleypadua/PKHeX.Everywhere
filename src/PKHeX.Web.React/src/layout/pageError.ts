import { EngineError, type SaveVersion } from '@pkhex-everywhere/engine'
import { applicationName, gitHubRepositoryIssues } from '../constants'
import { track } from '../googleAnalytics'
import { captureError, currentRoute } from '../sentry'

export interface PageError {
  id: string
  type: string
  message: string
  stack: string
  unsupportedFormat: boolean
}

export function toPageError(error: unknown, id: string = crypto.randomUUID()): PageError {
  const message = error instanceof Error ? error.message : String(error)
  return {
    id,
    type: error instanceof Error ? error.name : typeof error,
    message,
    stack: error instanceof Error ? (error.stack ?? '') : '',
    unsupportedFormat: message.includes('PKM Format needs to be'),
  }
}

export function issueLink(error: PageError, game: SaveVersion | null): string | null {
  if (error.unsupportedFormat) return null
  const body = [
    `# Error on ${applicationName}`,
    '',
    `* **Id**: ${error.id}`,
    `* **Game version**: ${game?.version ?? ''}`,
    `* **Generation**: ${game?.generation ?? ''}`,
    '## Type',
    '```',
    error.type,
    '```',
    '',
    '## Message',
    '```',
    error.message,
    '```',
    '',
    '## Stack trace',
    '```',
    error.stack,
    '```',
  ].join('\n')
  const query = new URLSearchParams({ title: `Error on ${applicationName}: ${error.message}`, body, labels: 'bug' })
  return `${gitHubRepositoryIssues}/new?${query}`
}

export function reportPageError(error: unknown, pageError: PageError, game: SaveVersion | null) {
  track('unexpected_error', {
    current_route: currentRoute(),
    exception_message: pageError.message,
    exception_stack_trace: pageError.stack,
    exception_type: pageError.type,
    exception_id: pageError.id,
    version_name: game?.version ?? null,
    version_id: game?.versionId ?? null,
    generation_name: game?.generation ?? null,
    generation_id: game?.generationId ?? null,
    format_id: game?.formatId ?? null,
  })
  if (!(error instanceof EngineError)) captureError(error, { exception_id: pageError.id })
}
