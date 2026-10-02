import { cpSync, createReadStream, existsSync, statSync } from 'node:fs'
import { dirname, extname, join, relative, resolve } from 'node:path'
import { fileURLToPath } from 'node:url'
import type { Plugin } from 'vite'
import { dotnetUrlMeta } from './dotnetUrlMeta'

export interface PkhexEngineOptions {
  frameworkDir?: string
}

const contentTypes: Record<string, string> = {
  '.js': 'text/javascript',
  '.wasm': 'application/wasm',
  '.json': 'application/json',
  '.map': 'application/json',
}

const packagedFramework = resolve(dirname(fileURLToPath(import.meta.url)), '../_framework')

function assertExists(frameworkDir: string) {
  if (!existsSync(frameworkDir)) throw new Error(`The .NET runtime folder ${frameworkDir} is missing.`)
}

export default function pkhexEngine({ frameworkDir = packagedFramework }: PkhexEngineOptions = {}): Plugin {
  let base = '/'
  return {
    name: 'pkhex-engine',
    configResolved(config) {
      base = config.base
    },
    configureServer(server) {
      assertExists(frameworkDir)
      server.middlewares.use(`${base}_framework`, (req, res) => {
        const file = join(frameworkDir, decodeURIComponent(new URL(req.url ?? '/', 'http://localhost').pathname))
        if (relative(frameworkDir, file).startsWith('..') || !existsSync(file) || !statSync(file).isFile()) {
          res.statusCode = 404
          return res.end()
        }
        res.setHeader('Content-Type', contentTypes[extname(file)] ?? 'application/octet-stream')
        res.setHeader('Cache-Control', 'no-cache')
        createReadStream(file).pipe(res)
      })
    },
    transformIndexHtml: () => [
      { tag: 'meta', attrs: { name: dotnetUrlMeta, content: `${base}_framework/dotnet.js` }, injectTo: 'head-prepend' },
    ],
    buildStart() {
      if (this.environment.config.command === 'build') assertExists(frameworkDir)
    },
    writeBundle({ dir }) {
      if (this.environment.config.consumer !== 'client') return
      cpSync(frameworkDir, join(dir!, '_framework'), { recursive: true, filter: (source) => !/\.(br|gz)$/.test(source) })
    },
  }
}
