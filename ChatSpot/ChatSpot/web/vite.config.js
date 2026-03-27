import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

// Config runs in Node — use loadEnv(), not import.meta.env (that is for app source only).
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  // Used only when VITE_API_URL is unset: browser hits same origin and Vite proxies /api + /chatHub here.
  // Default HTTP Kestrel URL so Node does not need to trust the ASP.NET dev HTTPS cert.
  const raw = env.VITE_DEV_PROXY_TARGET || 'http://localhost:5215/api'
  const proxyTarget = raw.replace(/\/+$/, '')

  return {
    plugins: [react()],
    server: {
      port: 4000,
      strictPort: false,
      proxy: {
        '/api': {
          target: proxyTarget,
          changeOrigin: true,
        },
        '/chatHub': {
          target: proxyTarget,
          changeOrigin: true,
          ws: true,
        },
      },
    },
  }
})
