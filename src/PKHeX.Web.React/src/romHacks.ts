const key = 'enableRomHacks'

export function readRomHacksFlag(search: string, storage: () => Storage): boolean {
  try {
    const store = storage()
    const requested = new URLSearchParams(search).get(key)
    if (requested === 'true') store.setItem(key, 'true')
    if (requested === 'false') store.removeItem(key)
    return store.getItem(key) === 'true'
  } catch {
    return false
  }
}
