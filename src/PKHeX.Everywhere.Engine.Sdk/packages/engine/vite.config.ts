import { defineConfig } from 'vite'

export default defineConfig({
  build: {
    lib: { entry: { index: 'src/index.ts', vite: 'src/vite.ts' }, formats: ['es'] },
    rolldownOptions: { external: ['crypto-js', 'vite', /^node:/] },
    minify: false,
  },
})
