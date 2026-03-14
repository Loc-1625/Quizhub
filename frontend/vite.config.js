import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  },
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5099',
        changeOrigin: true,
        secure: false
      },
      '/hubs': {
        target: 'http://localhost:5099',
        changeOrigin: true,
        ws: true,
        secure: false
      },
      '/uploads': {
        target: 'http://localhost:5099',
        changeOrigin: true,
        secure: false
      }
    }
  }
})
