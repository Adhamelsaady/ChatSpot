import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
    plugins: [react()],
    server: {
        port: 4000,
        strictPort: true,
        proxy: {
            '/api': {
                target: 'https://localhost:7184',
                changeOrigin: true,
                secure: false,
            },
            '/chathub': {
                target: 'https://localhost:7184',
                changeOrigin: true,
                secure: false,
                ws: true,
            }
        }
    }
})