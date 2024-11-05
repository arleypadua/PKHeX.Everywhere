import { resolve } from 'path'
import { defineConfig } from 'vite'

export default defineConfig(({ mode }) => ({
    build: {
        lib: {
            entry: resolve(__dirname, 'lib/main.ts'),
            formats: ['iife'],
            name: 'pkhex.web',
            fileName: 'pkhex-web.js',
        },
        outDir: '../wwwroot/js',
    }
}))