export function toBase64(bytes: Uint8Array) {
  let binary = ''
  for (const byte of bytes) binary += String.fromCharCode(byte)
  return btoa(binary)
}

export function fromBase64(base64: string) {
  return Uint8Array.from(atob(base64), (char) => char.charCodeAt(0))
}
