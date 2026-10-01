import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

const devServer = 'http://localhost:5173'

export default defineConfig({
  plugins: [react()],
  base: './',
  resolve: { preserveSymlinks: true },
  server: { port: 5173, strictPort: true, origin: devServer },
  build: {
    outDir: '../PKHeX.Web/wwwroot/react',
    emptyOutDir: true,
    rolldownOptions: {
      input: 'src/main.tsx',
      preserveEntrySignatures: 'exports-only',
      output: {
        entryFileNames: 'pages.js',
        chunkFileNames: 'chunks/[name]-[hash].js',
        assetFileNames: 'assets/[name]-[hash][extname]',
      },
    },
  },
})
