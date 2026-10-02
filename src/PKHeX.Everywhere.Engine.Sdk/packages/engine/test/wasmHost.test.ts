import { afterEach, describe, expect, it } from 'vitest'
import { wasmHost, type DotnetRuntime } from '../src'

function fakeDotnet({ failMain = false } = {}) {
  const loaded: string[] = []
  const moduleImports = new Map<string, Record<string, unknown>>()
  let created = 0
  let mainRuns = 0
  const runtime: DotnetRuntime = {
    setModuleImports: (name, imports) => void moduleImports.set(name, imports),
    getAssemblyExports: async () => ({ PKHeX: { Everywhere: { Engine: { EngineExports: { Call: async () => '' } } } } }),
    runMain: async () => {
      mainRuns++
      if (failMain) throw new Error('Main failed')
      return 0
    },
  }
  const load = async (url: string) => {
    loaded.push(url)
    return {
      dotnet: {
        create: async () => {
          created++
          return runtime
        },
      },
    }
  }
  return { load, loaded, moduleImports, runtime, created: () => created, mainRuns: () => mainRuns }
}

const hex = (bytes: Uint8Array) => Array.from(bytes, (b) => b.toString(16).padStart(2, '0')).join('')
const bytes = (hexString: string) => Uint8Array.from(hexString.match(/../g)!, (b) => parseInt(b, 16))

describe('wasmHost', () => {
  afterEach(() => {
    globalThis.pkhexEngineOnChange = undefined
    globalThis.pkhexEngineOnEvent = undefined
  })

  it('loads dotnet.js from the given URL and is ready once Main ran', async () => {
    const dotnet = fakeDotnet()
    const host = wasmHost({ dotnetUrl: '/_framework/dotnet.js', load: dotnet.load })

    await host.ready()

    expect(dotnet.loaded).toEqual(['/_framework/dotnet.js'])
    expect(dotnet.mainRuns()).toBe(1)
  })

  it('boots the runtime once', async () => {
    const dotnet = fakeDotnet()
    const host = wasmHost({ load: dotnet.load })

    await host.ready()
    await host.ready()
    await host.getAssemblyExports('PKHeX.Everywhere.Engine.dll')

    expect(dotnet.created()).toBe(1)
  })

  it('rejects ready when Main fails', async () => {
    const host = wasmHost({ load: fakeDotnet({ failMain: true }).load })

    await expect(host.ready()).rejects.toThrow('Main failed')
  })

  it('passes changes and events from .NET to the listeners', () => {
    const host = wasmHost({ load: fakeDotnet().load })
    const changes: string[][] = []
    const events: string[] = []
    host.onChange((topics) => changes.push(topics))
    host.onEvent((event) => events.push(event))

    globalThis.pkhexEngineOnChange?.(['party'])
    globalThis.pkhexEngineOnEvent?.('{"type":"gameExported"}')

    expect(changes).toEqual([['party']])
    expect(events).toEqual(['{"type":"gameExported"}'])
  })

  describe('crypto for .NET', () => {
    async function crypto() {
      const dotnet = fakeDotnet()
      await wasmHost({ load: dotnet.load }).ready()
      return dotnet.moduleImports.get('pkhex-crypto') as {
        encryptAes(key: Uint8Array, data: Uint8Array, mode: string, iv: Uint8Array | null): Uint8Array
        decryptAes(key: Uint8Array, data: Uint8Array, mode: string, iv: Uint8Array | null): Uint8Array
        md5(data: Uint8Array): Uint8Array
      }
    }

    // FIPS-197 appendix C.1 and SP 800-38A F.2.1.
    const key = bytes('000102030405060708090a0b0c0d0e0f')

    it('encrypts and decrypts AES-ECB', async () => {
      const { encryptAes, decryptAes } = await crypto()
      const plaintext = bytes('00112233445566778899aabbccddeeff')

      expect(hex(encryptAes(key, plaintext, 'ecb', null))).toBe('69c4e0d86a7b0430d8cdb78070b4c55a')
      expect(hex(decryptAes(key, bytes('69c4e0d86a7b0430d8cdb78070b4c55a'), 'ecb', null))).toBe(hex(plaintext))
    })

    it('encrypts and decrypts AES-CBC with the IV', async () => {
      const { encryptAes, decryptAes } = await crypto()
      const cbcKey = bytes('2b7e151628aed2a6abf7158809cf4f3c')
      const iv = bytes('000102030405060708090a0b0c0d0e0f')
      const plaintext = bytes('6bc1bee22e409f96e93d7e117393172aae2d8a571e03ac9c9eb76fac45af8e51')
      const ciphertext = '7649abac8119b246cee98e9b12e9197d5086cb9b507219ee95db113a917678b2'

      expect(hex(encryptAes(cbcKey, plaintext, 'cbc', iv))).toBe(ciphertext)
      expect(hex(decryptAes(cbcKey, bytes(ciphertext), 'cbc', iv))).toBe(hex(plaintext))
    })

    it('hashes MD5', async () => {
      const { md5 } = await crypto()

      expect(hex(md5(new TextEncoder().encode('abc')))).toBe('900150983cd24fb0d6963f7d28e17f72')
    })
  })
})
