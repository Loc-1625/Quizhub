import { defineStore } from 'pinia'
import { ref, computed, watch } from 'vue'
import api from '@/services/api'
import router from '@/router'
import { useToast } from 'vue-toastification'

export const useAuthStore = defineStore('auth', () => {
  const toast = useToast()
  const MAX_TIMEOUT_MS = 2147483647
  const IDLE_TIMEOUT_SECONDS = Number(import.meta.env.VITE_IDLE_TIMEOUT_SECONDS || 15)
  const IDLE_WARNING_COUNTDOWN_SECONDS = Number(import.meta.env.VITE_IDLE_WARNING_COUNTDOWN_SECONDS || 15)
  const IDLE_TIMEOUT_MS = Math.max(1, IDLE_TIMEOUT_SECONDS) * 1000
  const ACTIVITY_EVENTS = ['mousemove', 'mousedown', 'keydown', 'scroll', 'touchstart']
  let tokenExpiryTimer = null
  let idleTimer = null
  let idleWarningTimer = null
  
  // State - these will be persisted automatically by pinia-plugin-persistedstate
  const user = ref(null)
  const token = ref(null)
  const loading = ref(false)
  const idleWarningVisible = ref(false)
  const idleWarningCountdown = ref(IDLE_WARNING_COUNTDOWN_SECONDS)
  
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

  function clearIdleTimer() {
    if (idleTimer) {
      clearTimeout(idleTimer)
      idleTimer = null
    }
  }

  function clearIdleWarningTimer() {
    if (idleWarningTimer) {
      clearInterval(idleWarningTimer)
      idleWarningTimer = null
    }
  }

  function resetIdleWarningState() {
    idleWarningVisible.value = false
    idleWarningCountdown.value = IDLE_WARNING_COUNTDOWN_SECONDS
  }

  function detachIdleListeners() {
    ACTIVITY_EVENTS.forEach((eventName) => {
      window.removeEventListener(eventName, resetIdleTimer)
    })
  }

  function attachIdleListeners() {
    detachIdleListeners()
    ACTIVITY_EVENTS.forEach((eventName) => {
      window.addEventListener(eventName, resetIdleTimer, { passive: true })
    })
  }

  function resetIdleTimer() {
    clearIdleTimer()

    if (!isLoggedIn.value) return
    if (idleWarningVisible.value) return

    idleTimer = setTimeout(() => {
      startIdleWarningCountdown()
    }, IDLE_TIMEOUT_MS)
  }

  function startIdleWarningCountdown() {
    clearIdleTimer()
    clearIdleWarningTimer()

    if (!isLoggedIn.value) return

    idleWarningVisible.value = true
    idleWarningCountdown.value = IDLE_WARNING_COUNTDOWN_SECONDS

    idleWarningTimer = setInterval(() => {
      if (!isLoggedIn.value) {
        clearIdleWarningTimer()
        resetIdleWarningState()
        return
      }

      if (idleWarningCountdown.value <= 1) {
        clearIdleWarningTimer()
        resetIdleWarningState()
        void handleIdleSessionExpired()
        return
      }

      idleWarningCountdown.value -= 1
    }, 1000)
  }

  async function handleIdleSessionExpired() {
    const hadToken = !!token.value
    await logout()

    if (!hadToken) return

    const currentPath = window.location.pathname + window.location.search
    const publicPages = ['/', '/explore', '/login', '/register']
    const redirect = publicPages.includes(window.location.pathname) ? undefined : currentPath

    router.push({
      name: 'login',
      query: redirect
        ? { redirect, idleExpired: '1' }
        : { idleExpired: '1' }
    })
  }

  function continueSession() {
    if (!isLoggedIn.value) return

    clearIdleWarningTimer()
    resetIdleWarningState()
    resetIdleTimer()
    toast.info('Phiên đăng nhập đã được tiếp tục.')
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

  watch(
    isLoggedIn,
    (loggedIn) => {
      if (loggedIn) {
        attachIdleListeners()
        resetIdleTimer()
      } else {
        detachIdleListeners()
        clearIdleTimer()
        clearIdleWarningTimer()
        resetIdleWarningState()
      }
    },
    { immediate: true }
  )

  // Actions
  async function login(email, password, captchaToken = null) {
    loading.value = true
    try {
      const response = await api.post('/Auth/login', { email, password, captchaToken })
      const data = response.data
      if (data.success) {
        // Backend trả về Token và User (viết hoa)
        token.value = data.token || data.Token
        user.value = data.user || data.User
        api.defaults.headers.common['Authorization'] = `Bearer ${token.value}`
        resetIdleTimer()
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
  
  async function register(data, captchaToken = null) {
    loading.value = true
    try {
      const response = await api.post('/Auth/register', {
        ...data,
        captchaToken
      })
      const resData = response.data
      if (resData.success || resData.Success) {
        const returnedToken = resData.token || resData.Token
        const returnedUser = resData.user || resData.User

        if (returnedToken && returnedUser) {
          token.value = returnedToken
          user.value = returnedUser
          api.defaults.headers.common['Authorization'] = `Bearer ${token.value}`
          resetIdleTimer()
          return {
            success: true,
            requiresEmailConfirmation: false,
            message: resData.message || resData.Message
          }
        }

        return {
          success: true,
          requiresEmailConfirmation: true,
          message: resData.message || resData.Message
        }
      } else {
        toast.error(resData.message || resData.Message || 'Đăng ký thất bại')
        return {
          success: false,
          requiresEmailConfirmation: false,
          message: resData.message || resData.Message
        }
      }
    } catch (error) {
      const message = error.response?.data?.message || error.response?.data?.Message || 'Đã xảy ra lỗi khi đăng ký'
      toast.error(message)
      return {
        success: false,
        requiresEmailConfirmation: false,
        message
      }
    } finally {
      loading.value = false
    }
  }
  
  async function logout() {
    clearTokenExpiryTimer()
    clearIdleTimer()
    clearIdleWarningTimer()
    resetIdleWarningState()
    detachIdleListeners()
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
    idleWarningVisible,
    idleWarningCountdown,
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
    continueSession,
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
