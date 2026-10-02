import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import pkhexEngine from '@pkhex-everywhere/engine/vite'

export default defineConfig(({ mode }) => ({
  plugins: [react(), mode === 'self-hosted' && pkhexEngine()],
}))
