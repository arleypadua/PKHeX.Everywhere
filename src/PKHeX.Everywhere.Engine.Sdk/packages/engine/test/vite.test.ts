import { existsSync, mkdirSync, mkdtempSync, readFileSync, rmSync, writeFileSync } from 'node:fs'
import { tmpdir } from 'node:os'
import { join, resolve } from 'node:path'
import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { build, createServer, type ViteDevServer } from 'vite'
import pkhexEngine from '../src/vite'

const packagedFramework = resolve(import.meta.dirname, '../_framework')

let root: string
let framework: string

beforeEach(() => {
  root = mkdtempSync(join(tmpdir(), 'pkhex-engine-vite-'))
  writeFileSync(join(root, 'index.html'), '<html><head></head><body><script type="module" src="/main.js"></script></body></html>')
  writeFileSync(join(root, 'main.js'), 'console.log("app")')
  framework = join(root, 'framework')
  mkdirSync(framework)
  writeFileSync(join(framework, 'dotnet.js'), 'export const dotnet = "fixture"')
  writeFileSync(join(framework, 'dotnet.native.wasm'), 'wasm')
  writeFileSync(join(framework, 'dotnet.js.br'), 'compressed')
})

afterEach(() => rmSync(root, { recursive: true, force: true }))

const buildFixture = (options: Parameters<typeof pkhexEngine>[0], base = '/') =>
  build({ root, base, logLevel: 'silent', plugins: [pkhexEngine(options)] })

describe('build', () => {
  it('copies the framework folder into the output', async () => {
    await buildFixture({ frameworkDir: framework })

    expect(readFileSync(join(root, 'dist/_framework/dotnet.js'), 'utf8')).toBe('export const dotnet = "fixture"')
    expect(existsSync(join(root, 'dist/_framework/dotnet.native.wasm'))).toBe(true)
    expect(existsSync(join(root, 'dist/_framework/dotnet.js.br'))).toBe(false)
  })

  it('points the page at the same-origin dotnet.js', async () => {
    await buildFixture({ frameworkDir: framework }, '/app/')

    expect(readFileSync(join(root, 'dist/index.html'), 'utf8')).toContain(
      '<meta name="pkhex-engine-dotnet-url" content="/app/_framework/dotnet.js">',
    )
  })

  it('fails when the framework folder is missing', async () => {
    const missing = join(root, 'missing')

    await expect(buildFixture({ frameworkDir: missing })).rejects.toThrow(missing)
  })

  it.runIf(existsSync(packagedFramework))('copies the _framework inside the package by default', async () => {
    await buildFixture({})

    expect(readFileSync(join(root, 'dist/_framework/dotnet.js'))).toEqual(readFileSync(join(packagedFramework, 'dotnet.js')))
  })
})

describe('dev server', () => {
  let server: ViteDevServer | undefined

  afterEach(() => server?.close())

  async function serve(options: Parameters<typeof pkhexEngine>[0]) {
    server = await createServer({ root, logLevel: 'silent', server: { port: 0 }, plugins: [pkhexEngine(options)] })
    await server.listen()
    return server.resolvedUrls!.local[0]
  }

  it('serves the framework folder at /_framework', async () => {
    const url = await serve({ frameworkDir: framework })

    const dotnet = await fetch(`${url}_framework/dotnet.js`)
    const wasm = await fetch(`${url}_framework/dotnet.native.wasm`)

    expect(await dotnet.text()).toBe('export const dotnet = "fixture"')
    expect(dotnet.headers.get('content-type')).toBe('text/javascript')
    expect(wasm.headers.get('content-type')).toBe('application/wasm')
  })

  it('answers 404 for files outside the framework folder', async () => {
    const url = await serve({ frameworkDir: framework })

    expect((await fetch(`${url}_framework/missing.js`)).status).toBe(404)
    expect((await fetch(`${url}_framework/..%2Fmain.js`)).status).toBe(404)
    expect((await fetch(`${url}_framework/%E0.js`)).status).toBe(404)
  })

  it('points the page at the same-origin dotnet.js', async () => {
    const url = await serve({ frameworkDir: framework })

    expect(await (await fetch(url)).text()).toContain('<meta name="pkhex-engine-dotnet-url" content="/_framework/dotnet.js">')
  })

  it.runIf(existsSync(packagedFramework))('serves the _framework inside the package by default', async () => {
    const url = await serve({})

    const served = new Uint8Array(await (await fetch(`${url}_framework/dotnet.js`)).arrayBuffer())
    expect(served).toEqual(new Uint8Array(readFileSync(join(packagedFramework, 'dotnet.js'))))
  })
})
