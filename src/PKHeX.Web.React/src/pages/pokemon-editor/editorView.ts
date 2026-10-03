import type { PokemonSummary } from '@pkhex-everywhere/engine'

export type EditorView = 'read-only' | 'editor'

export function editorView(saved: Pick<PokemonSummary, 'editable'>): EditorView {
  return saved.editable ? 'editor' : 'read-only'
}
