import { describe, expect, it } from 'vitest'
import { EngineError } from '@pkhex-everywhere/engine'
import { loadFailure } from './loadFailure'

const candidates = [{ id: 'radicalred', name: 'Pokémon Radical Red' }]
const choiceRequired = new EngineError('format-choice-required', 'Choose a format.', candidates)

describe('loadFailure', () => {
  it('offers the candidate formats when ROM hacks are enabled', () => {
    expect(loadFailure(choiceRequired, true)).toEqual({ kind: 'formatChoice', candidates })
  })

  it('reports a save that might be a ROM hack as unsupported when ROM hacks are disabled', () => {
    expect(loadFailure(choiceRequired, false)).toEqual({ kind: 'unsupported' })
  })

  it('reports an invalid save as unsupported', () => {
    expect(loadFailure(new EngineError('invalid-save', 'Not a save.'), true)).toEqual({ kind: 'unsupported' })
  })

  it('leaves other errors alone', () => {
    expect(loadFailure(new EngineError('unexpected', 'Boom'), true)).toBeUndefined()
    expect(loadFailure(new Error('Boom'), true)).toBeUndefined()
  })
})
