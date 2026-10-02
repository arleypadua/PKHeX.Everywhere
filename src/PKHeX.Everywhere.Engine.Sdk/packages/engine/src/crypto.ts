import CryptoJS from 'crypto-js'

type WordArray = CryptoJS.lib.WordArray

const modes: Record<string, typeof CryptoJS.mode.ECB> = { ecb: CryptoJS.mode.ECB, cbc: CryptoJS.mode.CBC }

const words = (bytes: Uint8Array) => CryptoJS.lib.WordArray.create(bytes as unknown as number[])

function toBytes({ words, sigBytes }: WordArray) {
  const bytes = new Uint8Array(sigBytes)
  for (let i = 0; i < sigBytes; i++) bytes[i] = (words[i >>> 2] >>> (24 - (i % 4) * 8)) & 0xff
  return bytes
}

function options(mode: string, iv: Uint8Array | null) {
  const cipherMode = modes[mode]
  if (!cipherMode) throw new Error(`AES mode ${mode} is not supported.`)
  return { mode: cipherMode, padding: CryptoJS.pad.NoPadding, iv: iv ? words(iv) : undefined }
}

// .NET calls these synchronously, so they can't use SubtleCrypto.
export const crypto = {
  encryptAes: (key: Uint8Array, data: Uint8Array, mode: string, iv: Uint8Array | null) =>
    toBytes(CryptoJS.AES.encrypt(words(data), words(key), options(mode, iv)).ciphertext),
  decryptAes: (key: Uint8Array, data: Uint8Array, mode: string, iv: Uint8Array | null) =>
    toBytes(CryptoJS.AES.decrypt(CryptoJS.lib.CipherParams.create({ ciphertext: words(data) }), words(key), options(mode, iv))),
  md5: (data: Uint8Array) => toBytes(CryptoJS.MD5(words(data))),
}
