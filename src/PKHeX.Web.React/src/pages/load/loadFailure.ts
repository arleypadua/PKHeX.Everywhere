import { EngineError, type FormatEntry } from '@pkhex-everywhere/engine'

export type LoadFailure = { kind: 'unsupported' } | { kind: 'formatChoice'; candidates: FormatEntry[]; pkhexGame?: string }

export function loadFailure(error: unknown): LoadFailure | undefined {
  if (!(error instanceof EngineError)) return undefined
  if (error.code === 'format-choice-required') {
    return error.candidates?.length || error.pkhexGame
      ? { kind: 'formatChoice', candidates: error.candidates ?? [], pkhexGame: error.pkhexGame }
      : { kind: 'unsupported' }
  }
  return error.code === 'invalid-save' ? { kind: 'unsupported' } : undefined
}
