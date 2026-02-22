import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  test: {
    environment: 'jsdom',
    globals: true,
    include: ['src/**/*.{spec,test}.{js,ts}'],
    setupFiles: ['src/test/setup.js']
  },
  server: {
    port: 5173,
    proxy: {
      '/api': { target: 'http://localhost:5086', changeOrigin: true },
      '/hubs': { target: 'http://localhost:5086', ws: true }
    }
  }
})
