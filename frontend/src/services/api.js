import axios from 'axios'
import { useAuthStore } from '@/stores/auth'
import router from '@/router'

let isHandlingSessionExpired = false

const baseURL = import.meta.env.DEV ? '/api' : (import.meta.env.VITE_API_URL || '/api')

const api = axios.create({
  baseURL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json'
  }
})

// Request interceptor
api.interceptors.request.use(
  (config) => {
    const authStore = useAuthStore()
    if (authStore.token) {
      config.headers.Authorization = `Bearer ${authStore.token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// Response interceptor
api.interceptors.response.use(
  (response) => {
    return response
  },
  (error) => {
    // Chỉ tự động logout khi:
    // 1. Lỗi 401 (Unauthorized)
    // 2. Không phải đang ở trang login/register
    // 3. Không phải đang gọi API auth (login, register, etc.)
    if (error.response?.status === 401) {
      const isAuthEndpoint = error.config?.url?.includes('/Auth/login') || 
                            error.config?.url?.includes('/Auth/register') ||
                            error.config?.url?.includes('/Auth/me')
      
      if (!isAuthEndpoint && !isHandlingSessionExpired) {
        isHandlingSessionExpired = true
        const authStore = useAuthStore()
        const hadToken = !!authStore.token
        authStore.logout()
        
        if (hadToken) {
          // Lưu URL hiện tại để quay lại sau khi đăng nhập
          const currentPath = window.location.pathname + window.location.search
          const publicPages = ['/', '/explore', '/login', '/register']
          const redirect = publicPages.includes(window.location.pathname) ? undefined : currentPath

          router.push({
            name: 'login',
            query: redirect
              ? { redirect, sessionExpired: '1' }
              : { sessionExpired: '1' }
          }).finally(() => {
            isHandlingSessionExpired = false
          })
        } else {
          isHandlingSessionExpired = false
        }
      }
    }
    return Promise.reject(error)
  }
)

export default api
