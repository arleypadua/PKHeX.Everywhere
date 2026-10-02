import { cpSync, createReadStream, existsSync, statSync } from 'node:fs'
import { extname, join, relative, resolve } from 'node:path'
import { defineConfig, type Plugin } from 'vite'
import react from '@vitejs/plugin-react'

const host = resolve(import.meta.dirname, '../PKHeX.Everywhere.Engine.Host/bin')
const debugFramework = join(host, 'Debug/net10.0/wwwroot/_framework')
const publishedFramework = join(host, 'Release/net10.0/publish/wwwroot/_framework')

const contentTypes: Record<string, string> = {
  '.js': 'text/javascript',
  '.wasm': 'application/wasm',
  '.json': 'application/json',
  '.map': 'application/json',
}

function engineHost(): Plugin {
  return {
    name: 'engine-host',
    configureServer(server) {
      server.middlewares.use('/_framework', (req, res) => {
        const file = join(debugFramework, new URL(req.url ?? '/', 'http://localhost').pathname)
        if (relative(debugFramework, file).startsWith('..') || !existsSync(file) || !statSync(file).isFile()) {
          res.statusCode = 404
          return res.end()
        }
        res.setHeader('Content-Type', contentTypes[extname(file)] ?? 'application/octet-stream')
        res.setHeader('Cache-Control', 'no-cache')
        createReadStream(file).pipe(res)
      })
    },
    writeBundle({ dir }) {
      if (!existsSync(publishedFramework))
        throw new Error(`${publishedFramework} is missing. Run 'dotnet publish ../PKHeX.Everywhere.Engine.Host -c Release' first.`)
      cpSync(publishedFramework, join(dir!, '_framework'), {
        recursive: true,
        filter: (source) => !/\.(br|gz)$/.test(source),
      })
    },
  }
}

const blazorBuild = defineConfig({
  plugins: [react()],
  base: './',
  publicDir: false,
  resolve: { preserveSymlinks: true },
  build: {
    outDir: '../PKHeX.Web/wwwroot/react',
    emptyOutDir: true,
    rolldownOptions: {
      input: { pages: 'src/main.tsx', sentry: 'src/startSentry.ts' },
      preserveEntrySignatures: 'exports-only',
      output: {
        entryFileNames: '[name].js',
        chunkFileNames: 'chunks/[name]-[hash].js',
        assetFileNames: 'assets/[name]-[hash][extname]',
      },
    },
  },
})

export default defineConfig(({ mode }) =>
  mode === 'blazor'
    ? blazorBuild
    : {
        plugins: [react(), engineHost()],
        resolve: { preserveSymlinks: true },
        server: { port: 5173, strictPort: true },
      },
)
