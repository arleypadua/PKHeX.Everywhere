import { EngineError, type FormatEntry } from '@pkhex-everywhere/engine'

export type LoadFailure = { kind: 'unsupported' } | { kind: 'formatChoice'; candidates: FormatEntry[] }

export function loadFailure(error: unknown): LoadFailure | undefined {
  if (!(error instanceof EngineError)) return undefined
  if (error.code === 'format-choice-required') {
    return error.candidates?.length
      ? { kind: 'formatChoice', candidates: error.candidates }
      : { kind: 'unsupported' }
  }
  return error.code === 'invalid-save' ? { kind: 'unsupported' } : undefined
}
