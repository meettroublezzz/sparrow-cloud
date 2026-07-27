import { defineConfig, loadEnv } from 'vite'
import type { ProxyOptions } from 'vite'
import vue from '@vitejs/plugin-vue'
import path from 'path'

const backendProxyTimeout = 32 * 60 * 1000

function createBackendProxy(target: string): ProxyOptions {
  return {
    target,
    changeOrigin: true,
    secure: false,
    ws: true,
    timeout: backendProxyTimeout,
    proxyTimeout: backendProxyTimeout,
  }
}

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), 'VITE_')
  const backendTarget = env.VITE_BACKEND_ORIGIN || 'http://localhost:5064'

  return {
    plugins: [vue()],

    resolve: {
      alias: {
        '@': path.resolve(__dirname, './src')
      }
    },

    server: {
      proxy: {
        '/api': createBackendProxy(backendTarget),
        '/storages': createBackendProxy(backendTarget),
        '/swagger': createBackendProxy(backendTarget),
      },
    },
  }
})
