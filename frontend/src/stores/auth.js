import { defineStore } from 'pinia'
import { ref, computed, watch } from 'vue'
import api from '@/services/api'
import router from '@/router'
import { useToast } from 'vue-toastification'

export const useAuthStore = defineStore('auth', () => {
  const toast = useToast()
  const MAX_TIMEOUT_MS = 2147483647
  let tokenExpiryTimer = null
  
  // State - these will be persisted automatically by pinia-plugin-persistedstate
  const user = ref(null)
  const token = ref(null)
  const loading = ref(false)
  
  // Getters
  const isLoggedIn = computed(() => !!token.value && !!user.value)
  const isAdmin = computed(() => {
    const roles = user.value?.roles || user.value?.Roles || []
    return roles.includes('Admin')
  })
  const userId = computed(() => user.value?.id || user.value?.Id || null)
  const fullName = computed(() => user.value?.hoTen || user.value?.HoTen || 'Người dùng')
  const avatar = computed(() => {
    const avatarPath = user.value?.anhDaiDien || user.value?.AnhDaiDien || null
    if (!avatarPath) return null
    // If already full URL (starts with http), return as is
    if (avatarPath.startsWith('http')) return avatarPath
    // Otherwise, prepend the API base URL (without /api)
    return `${import.meta.env.VITE_API_URL?.replace('/api', '') || ''}${avatarPath}`
  })

  function clearTokenExpiryTimer() {
    if (tokenExpiryTimer) {
      clearTimeout(tokenExpiryTimer)
      tokenExpiryTimer = null
    }
  }

  function getTokenExpirationDate(authToken) {
    if (!authToken) return null

    try {
      const parts = authToken.split('.')
      if (parts.length !== 3) return null

      const payload = parts[1]
        .replace(/-/g, '+')
        .replace(/_/g, '/')
      const paddedPayload = payload.padEnd(payload.length + (4 - payload.length % 4) % 4, '=')
      const decodedPayload = JSON.parse(atob(paddedPayload))

      if (!decodedPayload.exp) return null
      return new Date(decodedPayload.exp * 1000)
    } catch {
      return null
    }
  }

  async function handleTokenExpired() {
    const hadToken = !!token.value
    await logout()

    if (!hadToken) return

    const currentPath = window.location.pathname + window.location.search
    const publicPages = ['/', '/explore', '/login', '/register']
    const redirect = publicPages.includes(window.location.pathname) ? undefined : currentPath

    router.push({
      name: 'login',
      query: redirect
        ? { redirect, sessionExpired: '1' }
        : { sessionExpired: '1' }
    })
  }

  function scheduleTokenExpiryCheck(authToken) {
    clearTokenExpiryTimer()

    const expirationDate = getTokenExpirationDate(authToken)
    if (!expirationDate) return

    const remainingMs = expirationDate.getTime() - Date.now()

    if (remainingMs <= 0) {
      handleTokenExpired()
      return
    }

    if (remainingMs > MAX_TIMEOUT_MS) {
      tokenExpiryTimer = setTimeout(() => {
        scheduleTokenExpiryCheck(authToken)
      }, MAX_TIMEOUT_MS)
      return
    }

    tokenExpiryTimer = setTimeout(() => {
      handleTokenExpired()
    }, remainingMs)
  }

  watch(
    token,
    (newToken) => {
      if (!newToken) {
        clearTokenExpiryTimer()
        return
      }
      scheduleTokenExpiryCheck(newToken)
    },
    { immediate: true }
  )

  // Actions
  async function login(email, password) {
    loading.value = true
    try {
      const response = await api.post('/Auth/login', { email, password })
      const data = response.data
      if (data.success) {
        // Backend trả về Token và User (viết hoa)
        token.value = data.token || data.Token
        user.value = data.user || data.User
        api.defaults.headers.common['Authorization'] = `Bearer ${token.value}`
        return true
      } else {
        toast.error(data.message || data.Message || 'Đăng nhập thất bại')
        return false
      }
    } catch (error) {
      const message = error.response?.data?.message || error.response?.data?.Message || 'Đã xảy ra lỗi khi đăng nhập'
      toast.error(message)
      return false
    } finally {
      loading.value = false
    }
  }
  
  async function register(data) {
    loading.value = true
    try {
      const response = await api.post('/Auth/register', data)
      const resData = response.data
      if (resData.success || resData.Success) {
        // Backend trả về Token và User (viết hoa)
        token.value = resData.token || resData.Token
        user.value = resData.user || resData.User
        api.defaults.headers.common['Authorization'] = `Bearer ${token.value}`
        return true
      } else {
        toast.error(resData.message || resData.Message || 'Đăng ký thất bại')
        return false
      }
    } catch (error) {
      const message = error.response?.data?.message || error.response?.data?.Message || 'Đã xảy ra lỗi khi đăng ký'
      toast.error(message)
      return false
    } finally {
      loading.value = false
    }
  }
  
  async function logout() {
    clearTokenExpiryTimer()
    token.value = null
    user.value = null
    delete api.defaults.headers.common['Authorization']
  }
  
  async function fetchCurrentUser() {
    if (!token.value) return
    
    try {
      api.defaults.headers.common['Authorization'] = `Bearer ${token.value}`
      const response = await api.get('/Auth/me')
      // API trả về UserInfoDto trực tiếp (không có wrapper)
      user.value = response.data
    } catch (error) {
      console.error('Failed to fetch user:', error)
      // Chỉ logout nếu là lỗi 401 (unauthorized)
      if (error.response?.status === 401) {
        token.value = null
        user.value = null
        delete api.defaults.headers.common['Authorization']
      }
    }
  }
  
  async function updateProfile(data) {
    loading.value = true
    try {
      const response = await api.put('/Auth/profile', data)
      if (response.data.success) {
        // Fetch lại user data để đảm bảo đồng bộ (bao gồm anhDaiDien)
        await fetchCurrentUser()
        return true
      }
      return false
    } catch (error) {
      const message = error.response?.data?.message || 'Đã xảy ra lỗi'
      toast.error(message)
      return false
    } finally {
      loading.value = false
    }
  }
  
  async function uploadAvatar(file) {
    const formData = new FormData()
    formData.append('file', file)
    
    try {
      const response = await api.post('/Auth/profile/avatar', formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      })
      if (response.data.success) {
        user.value.anhDaiDien = response.data.data.avatarUrl
        return true
      }
      return false
    } catch (error) {
      toast.error('Không thể tải ảnh lên')
      return false
    }
  }
  
  async function changePassword(currentPassword, newPassword) {
    loading.value = true
    try {
      const response = await api.post('/Auth/change-password', {
        currentPassword,
        newPassword
      })
      if (response.data.success) {
        return true
      }
      return false
    } catch (error) {
      const message = error.response?.data?.message || 'Đổi mật khẩu thất bại'
      toast.error(message)
      return false
    } finally {
      loading.value = false
    }
  }
  
  async function forgotPassword(email) {
    loading.value = true
    try {
      const response = await api.post('/Auth/forgot-password', { email })
      if (response.data.success) {
        return true
      }
      return false
    } catch (error) {
      const message = error.response?.data?.message || 'Đã xảy ra lỗi'
      toast.error(message)
      return false
    } finally {
      loading.value = false
    }
  }
  
  async function resetPassword(email, resetToken, newPassword) {
    loading.value = true
    try {
      const response = await api.post('/Auth/reset-password', {
        email,
        token: resetToken,
        newPassword,
        confirmPassword: newPassword // Backend yêu cầu confirmPassword
      })
      if (response.data.success) {
        return true
      }
      return false
    } catch (error) {
      const message = error.response?.data?.message || 'Đã xảy ra lỗi'
      toast.error(message)
      return false
    } finally {
      loading.value = false
    }
  }
  
  // Initialize - restore axios header on app start
  function init() {
    if (token.value) {
      api.defaults.headers.common['Authorization'] = `Bearer ${token.value}`
      // Verify token is still valid by fetching current user
      fetchCurrentUser()
    }
  }
  
  return {
    // State
    user,
    token,
    loading,
    // Getters
    isLoggedIn,
    isAdmin,
    userId,
    fullName,
    avatar,
    // Actions
    login,
    register,
    logout,
    fetchCurrentUser,
    updateProfile,
    uploadAvatar,
    changePassword,
    forgotPassword,
    resetPassword,
    init
  }
}, {
  // Persist configuration - automatically saves to localStorage
  persist: {
    key: 'quizhub-auth',
    storage: localStorage,
    paths: ['user', 'token'] // Only persist user and token
  }
})
