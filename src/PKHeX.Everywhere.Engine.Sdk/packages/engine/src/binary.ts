import type { Binary } from './generated/types'

const chunk = 0x8000

export async function toBase64(data: Binary): Promise<string> {
  const bytes = data instanceof Uint8Array ? data : new Uint8Array(data instanceof Blob ? await data.arrayBuffer() : data)
  let binary = ''
  for (let i = 0; i < bytes.length; i += chunk) binary += String.fromCharCode(...bytes.subarray(i, i + chunk))
  return btoa(binary)
}

export function fileNameOf(data: Binary): string | null {
  return typeof File !== 'undefined' && data instanceof File ? data.name : null
}

// A path such as `save.bytes` reaches bytes in a record the value holds.
export function withBytes<T>(value: T, paths: readonly string[]): T {
  if (value == null) return value
  const converted: Record<string, unknown> = { ...value }
  for (const path of paths) {
    const [key, ...rest] = path.split('.')
    const encoded = converted[key]
    if (rest.length > 0) converted[key] = withBytes(encoded, [rest.join('.')])
    else if (typeof encoded === 'string') converted[key] = Uint8Array.from(atob(encoded), (char) => char.charCodeAt(0))
  }
  return converted as T
}
