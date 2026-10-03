import { describe, expect, it } from 'vitest'
import { editorView } from './editorView'

describe('editorView', () => {
  it('opens an editable Pokémon in the editor', () => {
    expect(editorView({ editable: true })).toBe('editor')
  })

  it('shows a Pokémon that isn\'t editable read-only, without opening a draft', () => {
    expect(editorView({ editable: false })).toBe('read-only')
  })
})
