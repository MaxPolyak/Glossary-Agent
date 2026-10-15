import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  server: {
    // Явно IPv4: на Windows Vite по умолчанию может сесть на адрес, по которому localhost не открывается
    host: '127.0.0.1',
    port: 5173,
    // Запросы на /api уходят в бэкенд. Так браузер думает, что всё на одном адресе,
    // и на бэкенде не нужно настраивать CORS.
    proxy: {
      '/api': 'http://localhost:5080',
    },
    fs: {
      // примеры текстов берём из backend/tests/samples, это выше корня фронта
      allow: ['..'],
    },
  },
})
