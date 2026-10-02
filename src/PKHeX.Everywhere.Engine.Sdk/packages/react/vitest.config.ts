import { defineConfig } from 'vitest/config'
import { defaultClientConditions } from 'vite'

export default defineConfig({
  resolve: { conditions: ['source', ...defaultClientConditions] },
  test: { environment: 'jsdom' },
})
