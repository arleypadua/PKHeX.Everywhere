const key = 'enabledFormats'

export function readEnabledFormats(search: string, storage: () => Storage): string[] {
  try {
    const store = storage()
    const params = new URLSearchParams(search)
    const disabled = params.getAll('disableFormat')
    const stored = parse(store.getItem(key))
    const enabled = [...new Set([...stored, ...params.getAll('enableFormat')])].filter((id) => !disabled.includes(id))
    if (enabled.length === 0) store.removeItem(key)
    else store.setItem(key, JSON.stringify(enabled))
    return enabled
  } catch {
    return []
  }
}

function parse(stored: string | null): string[] {
  try {
    const ids: unknown = JSON.parse(stored ?? '[]')
    return Array.isArray(ids) ? ids.filter((id) => typeof id === 'string') : []
  } catch {
    return []
  }
}
