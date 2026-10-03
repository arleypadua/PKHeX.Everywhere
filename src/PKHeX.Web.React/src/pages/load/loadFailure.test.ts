import { describe, expect, it } from 'vitest'
import { EngineError } from '@pkhex-everywhere/engine'
import { loadFailure } from './loadFailure'

const candidates = [{ id: 'radicalred', name: 'Pokémon Radical Red' }]

describe('loadFailure', () => {
  it('offers the candidate formats for a save that might be a ROM hack', () => {
    const error = new EngineError('format-choice-required', 'Choose a format.', candidates)
    expect(loadFailure(error)).toEqual({ kind: 'formatChoice', candidates })
  })

  it('reports a format choice with no candidates as unsupported', () => {
    expect(loadFailure(new EngineError('format-choice-required', 'Choose a format.'))).toEqual({ kind: 'unsupported' })
  })

  it('reports an invalid save as unsupported', () => {
    expect(loadFailure(new EngineError('invalid-save', 'Not a save.'))).toEqual({ kind: 'unsupported' })
  })

  it('leaves other errors alone', () => {
    expect(loadFailure(new EngineError('unexpected', 'Boom'))).toBeUndefined()
    expect(loadFailure(new Error('Boom'))).toBeUndefined()
  })
})
