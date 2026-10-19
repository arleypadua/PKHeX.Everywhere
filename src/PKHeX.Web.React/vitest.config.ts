import { defineConfig } from 'vitest/config'
import { defaultClientConditions } from 'vite'

export default defineConfig({
  resolve: { preserveSymlinks: true, conditions: ['source', ...defaultClientConditions] },
  test: { environment: 'jsdom' },
})
