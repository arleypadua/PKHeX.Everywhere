import { defineConfig, type Plugin } from 'vite'
import react from '@vitejs/plugin-react'

const blazor = process.env.BLAZOR_URL ?? 'http://localhost:5062'

// dotnet watch injects its hot reload client into HTML it serves, but in dev Vite serves the HTML.
const dotnetHotReload: Plugin = {
  name: 'dotnet-hot-reload',
  apply: 'serve',
  transformIndexHtml: () => [{ tag: 'script', attrs: { src: '/_framework/aspnetcore-browser-refresh.js' }, injectTo: 'body' }],
}

export default defineConfig({
  plugins: [react(), dotnetHotReload],
  build: { outDir: 'dist', assetsDir: 'shell-assets' },
  server: {
    port: 5173,
    proxy: {
      '^/(_framework|_content|css|js|data|font|plugins|favicon\\.svg|PKHeX\\.Web\\.styles\\.css|appsettings.*)': {
        target: blazor,
        ws: true,
      },
    },
  },
})
