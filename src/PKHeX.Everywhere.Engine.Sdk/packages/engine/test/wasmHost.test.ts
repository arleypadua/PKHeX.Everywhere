import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import type { DotnetRuntime, WasmHostOptions } from '../src'
import type { EngineHost } from '../src'
import { version } from '../package.json'

let wasmHost: (options?: WasmHostOptions) => EngineHost

function fakeDotnet({ failMain = false, progress = [] as [number, number][] } = {}) {
  const loaded: string[] = []
  const moduleImports = new Map<string, Record<string, unknown>>()
  let created = 0
  let onProgress: ((loaded: number, total: number) => void) | undefined
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
        withModuleConfig(config: { onDownloadResourceProgress?: (loaded: number, total: number) => void }) {
          onProgress = config.onDownloadResourceProgress
          return this
        },
        create: async () => {
          created++
          for (const [done, total] of progress) onProgress?.(done, total)
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
  beforeEach(async () => {
    vi.resetModules()
    ;({ wasmHost } = await import('../src/wasmHost'))
  })

  afterEach(() => {
    vi.restoreAllMocks()
    globalThis.pkhexEngineOnChange = undefined
    globalThis.pkhexEngineOnEvent = undefined
    Reflect.deleteProperty(globalThis, 'document')
  })

  it('loads dotnet.js from jsDelivr for the exact engine version by default', async () => {
    const dotnet = fakeDotnet()
    const host = wasmHost({ load: dotnet.load })

    await host.ready()

    expect(dotnet.loaded).toEqual([`https://cdn.jsdelivr.net/npm/@pkhex-everywhere/engine@${version}/_framework/dotnet.js`])
    expect(dotnet.mainRuns()).toBe(1)
  })

  it('loads dotnet.js from the given URL', async () => {
    const dotnet = fakeDotnet()
    const host = wasmHost({ dotnetUrl: '/_framework/dotnet.js', load: dotnet.load })

    await host.ready()

    expect(dotnet.loaded).toEqual(['/_framework/dotnet.js'])
  })

  it('loads dotnet.js from the URL the Vite plugin puts on the page', async () => {
    const meta = { content: './_framework/dotnet.js' }
    globalThis.document = {
      baseURI: 'https://example.com/app/',
      querySelector: (selector: string) => (selector === 'meta[name="pkhex-engine-dotnet-url"]' ? meta : null),
    } as unknown as Document
    const dotnet = fakeDotnet()

    await wasmHost({ load: dotnet.load }).ready()

    expect(dotnet.loaded).toEqual(['https://example.com/app/_framework/dotnet.js'])
  })

  it('prefers the given URL over the one the Vite plugin puts on the page', async () => {
    globalThis.document = {
      baseURI: 'https://example.com/',
      querySelector: () => ({ content: '/_framework/dotnet.js' }),
    } as unknown as Document
    const dotnet = fakeDotnet()

    await wasmHost({ dotnetUrl: 'https://my.cdn/dotnet.js', load: dotnet.load }).ready()

    expect(dotnet.loaded).toEqual(['https://my.cdn/dotnet.js'])
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

  it('passes changes and events from .NET to every listener', () => {
    const changes: string[][] = []
    const events: string[] = []
    wasmHost({ load: fakeDotnet().load }).onChange((topics) => changes.push(['first', ...topics]))
    wasmHost().onChange((topics) => changes.push(['second', ...topics]))
    wasmHost().onEvent((event) => events.push(`first ${event}`))
    wasmHost().onEvent((event) => events.push(`second ${event}`))

    globalThis.pkhexEngineOnChange?.(['party'])
    globalThis.pkhexEngineOnEvent?.('{"type":"gameExported"}')

    expect(changes).toEqual([
      ['first', 'party'],
      ['second', 'party'],
    ])
    expect(events).toEqual(['first {"type":"gameExported"}', 'second {"type":"gameExported"}'])
  })

  it('returns the same runtime on every call', async () => {
    const dotnet = fakeDotnet()

    const first = wasmHost({ load: dotnet.load })
    const second = wasmHost({ load: dotnet.load })
    await first.ready()
    await second.ready()

    expect(second).toBe(first)
    expect(dotnet.created()).toBe(1)
  })

  it('warns when a later call asks for a different dotnetUrl', () => {
    const warn = vi.spyOn(console, 'warn').mockImplementation(() => {})

    wasmHost({ dotnetUrl: '/a/dotnet.js', load: fakeDotnet().load })
    wasmHost({ dotnetUrl: '/a/dotnet.js' })
    expect(warn).not.toHaveBeenCalled()

    wasmHost({ dotnetUrl: '/b/dotnet.js' })
    expect(warn).toHaveBeenCalledOnce()
    expect(warn.mock.calls[0][0]).toContain('/b/dotnet.js')
  })

  it('reports download progress in files', async () => {
    const host = wasmHost({ load: fakeDotnet({ progress: [[1, 3], [2, 3], [3, 3]] }).load })
    const progress: [number, number][] = []
    host.onProgress?.((loaded, total) => progress.push([loaded, total]))

    await host.ready()

    expect(progress).toEqual([[1, 3], [2, 3], [3, 3]])
  })

  it('reports the latest progress to listeners added after the download', async () => {
    const host = wasmHost({ load: fakeDotnet({ progress: [[1, 2], [2, 2]] }).load })
    await host.ready()
    const progress: [number, number][] = []

    host.onProgress?.((loaded, total) => progress.push([loaded, total]))

    expect(progress).toEqual([[2, 2]])
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
