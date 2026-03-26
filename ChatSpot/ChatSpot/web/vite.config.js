import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// Dev-only: proxy to ASP.NET. Use HTTP (5215) — same as launchSettings "https" profile's http URL — so Node
// doesn't need to trust the dev HTTPS cert. If you only run the "http" profile, it's still localhost:5215.
const API_DEV_TARGET = 'https://chatspot-production-640b.up.railway.app/'

export default defineConfig({
    plugins: [react()],
    server: {
        port: 4000,
        strictPort: false,
        proxy: {
            '/api': {
                target: API_DEV_TARGET,
                changeOrigin: true,
            },
            '/chatHub': {
                target: API_DEV_TARGET,
                changeOrigin: true,
                ws: true,
            }
        }
    }
})
