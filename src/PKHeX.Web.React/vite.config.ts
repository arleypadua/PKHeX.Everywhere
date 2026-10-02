import { join, resolve } from 'node:path'
import { defaultClientConditions, defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import pkhexEngine from '@pkhex-everywhere/engine/vite'

const host = resolve(import.meta.dirname, '../PKHeX.Everywhere.Engine.Host/bin')

export default defineConfig(({ command }) => ({
  plugins: [
    react(),
    pkhexEngine({
      frameworkDir: join(host, command === 'serve' ? 'Debug/net10.0/wwwroot/_framework' : 'Release/net10.0/publish/wwwroot/_framework'),
    }),
  ],
  resolve: { preserveSymlinks: true, conditions: ['source', ...defaultClientConditions] },
  server: { port: 5173, strictPort: true },
}))
