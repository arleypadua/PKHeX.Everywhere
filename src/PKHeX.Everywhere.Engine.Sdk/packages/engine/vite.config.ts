import { defineConfig } from 'vite'

export default defineConfig({
  build: {
    lib: { entry: 'src/index.ts', formats: ['es'], fileName: 'index' },
    rolldownOptions: { external: ['crypto-js'] },
    minify: false,
  },
})
