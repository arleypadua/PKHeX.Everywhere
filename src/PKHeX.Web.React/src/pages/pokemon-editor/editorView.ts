import type { PokemonSummary } from '@pkhex-everywhere/engine'

export type EditorView = 'not-found' | 'read-only' | 'editor'

export function editorView(saved: Pick<PokemonSummary, 'editable'> | undefined): EditorView {
  if (!saved) return 'not-found'
  return saved.editable ? 'editor' : 'read-only'
}
