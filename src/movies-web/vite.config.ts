import react from '@vitejs/plugin-react'
import { defineConfig } from 'vitest/config'

// In development the API runs separately (dotnet run, or docker compose), so the dev server
// forwards API calls to it. In Docker, nginx does the same job (see nginx.conf).
const apiUrl = process.env.API_URL ?? 'http://localhost:5080'

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': apiUrl,
      '/swagger': apiUrl,
      '/openapi': apiUrl,
    },
  },
  test: {
    environment: 'jsdom',
    setupFiles: './src/test/setup.ts',
  },
})
