import { join, resolve } from 'node:path'
import { defaultClientConditions, defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import pkhexEngine from '@pkhex-everywhere/engine/vite'

const host = resolve(import.meta.dirname, '../PKHeX.Everywhere.Engine.Host/bin')
const debugFramework = join(host, 'Debug/net10.0/wwwroot/_framework')
const publishedFramework = join(host, 'Release/net10.0/publish/wwwroot/_framework')

export default defineConfig(({ command }) => ({
  plugins: [
    react(),
    pkhexEngine({ frameworkDir: command === 'serve' ? debugFramework : publishedFramework }),
  ],
  resolve: { preserveSymlinks: true, conditions: ['source', ...defaultClientConditions] },
  server: { port: 5173, strictPort: true },
}))
