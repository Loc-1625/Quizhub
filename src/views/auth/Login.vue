<template>
  <div class="min-h-screen flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8 bg-gray-50">
    <div class="max-w-md w-full">
      <!-- Logo -->
      <div class="text-center mb-8">
        <router-link to="/" class="inline-flex items-center">
          <span class="text-3xl font-bold text-gradient">QuizHub</span>
        </router-link>
        <h2 class="mt-6 text-3xl font-bold text-gray-900">Đăng nhập</h2>
        <p class="mt-2 text-gray-600">
          Chưa có tài khoản?
          <router-link to="/register" class="text-primary-600 hover:text-primary-700 font-medium">
            Đăng ký ngay
          </router-link>
        </p>
      </div>

      <!-- Login Form -->
      <div class="card p-8">
        <form @submit.prevent="handleLogin" class="space-y-6">
          <div>
            <label for="email" class="block text-sm font-medium text-gray-700 mb-1">
              Email
            </label>
            <div class="relative">
              <EnvelopeIcon class="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
              <input
                id="email"
                v-model="form.email"
                type="email"
                required
                placeholder="email@example.com"
                class="input-field pl-10"
                :class="{ 'border-red-500 focus:ring-red-500': errors.email }"
              />
            </div>
            <p v-if="errors.email" class="mt-1 text-sm text-red-500">{{ errors.email }}</p>
          </div>

          <div>
            <label for="password" class="block text-sm font-medium text-gray-700 mb-1">
              Mật khẩu
            </label>
            <div class="relative">
              <LockClosedIcon class="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
              <input
                id="password"
                v-model="form.password"
                :type="showPassword ? 'text' : 'password'"
                required
                placeholder="••••••••"
                class="input-field pl-10 pr-10"
                :class="{ 'border-red-500 focus:ring-red-500': errors.password }"
              />
              <button
                type="button"
                @click="showPassword = !showPassword"
                class="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
              >
                <EyeIcon v-if="!showPassword" class="w-5 h-5" />
                <EyeSlashIcon v-else class="w-5 h-5" />
              </button>
            </div>
            <p v-if="errors.password" class="mt-1 text-sm text-red-500">{{ errors.password }}</p>
          </div>

          <div class="flex items-center justify-between">
            <label class="flex items-center">
              <input
                v-model="form.remember"
                type="checkbox"
                class="w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
              />
              <span class="ml-2 text-sm text-gray-600">Nhớ mật khẩu</span>
            </label>
            <router-link to="/forgot-password" class="text-sm text-primary-600 hover:text-primary-700">
              Quên mật khẩu?
            </router-link>
          </div>

          <button
            type="submit"
            :disabled="loading"
            class="btn-primary w-full py-3 text-base"
          >
            <ArrowPathIcon v-if="loading" class="w-5 h-5 mr-2 animate-spin" />
            {{ loading ? 'Đang đăng nhập...' : 'Đăng nhập' }}
          </button>
        </form>

        <!-- Divider -->
        <div class="relative my-6">
          <div class="absolute inset-0 flex items-center">
            <div class="w-full border-t border-gray-200"></div>
          </div>
          <div class="relative flex justify-center text-sm">
            <span class="px-4 bg-white text-gray-500">Hoặc tiếp tục với</span>
          </div>
        </div>

        <!-- Social Login -->
        <div class="w-full">
            <button
              type="button" 
              @click="loginWithGoogle"
              class="w-full flex items-center justify-center px-4 py-3 border border-gray-300 rounded-lg shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 transition-colors"
            >
              <svg class="w-5 h-5 mr-2" viewBox="0 0 24 24">
                <path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"/>
                <path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"/>
                <path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"/>
                <path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"/>
              </svg>
              Tiếp tục với Google
            </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import axios from 'axios' 
import { googleTokenLogin } from 'vue3-google-login'
import { ref, reactive, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import {
  EnvelopeIcon,
  LockClosedIcon,
  EyeIcon,
  EyeSlashIcon,
  ArrowPathIcon
} from '@heroicons/vue/24/outline'
import { useAuthStore } from '@/stores/auth'


const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

const loading = ref(false)
const showPassword = ref(false)

const form = reactive({
  email: '',
  password: '',
  remember: false
})

const errors = reactive({
  email: '',
  password: ''
})

// Load thông tin đã lưu khi mount
onMounted(() => {
  const savedCredentials = localStorage.getItem('quizhub_saved_credentials')
  if (savedCredentials) {
    try {
      const { email, password } = JSON.parse(savedCredentials)
      form.email = email || ''
      form.password = password || ''
      form.remember = true
    } catch (e) {
      localStorage.removeItem('quizhub_saved_credentials')
    }
  }
})

const validateForm = () => {
  let valid = true
  errors.email = ''
  errors.password = ''

  if (!form.email) {
    errors.email = 'Vui lòng nhập email'
    valid = false
  } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) {
    errors.email = 'Email không hợp lệ'
    valid = false
  }

  if (!form.password) {
    errors.password = 'Vui lòng nhập mật khẩu'
    valid = false
  }

  return valid
}

const handleLogin = async () => {
  if (!validateForm()) return

  loading.value = true
  const success = await authStore.login(form.email, form.password)
  loading.value = false

  if (success) {
    // Lưu hoặc xóa thông tin đăng nhập theo checkbox "Ghi nhớ"
    if (form.remember) {
      localStorage.setItem('quizhub_saved_credentials', JSON.stringify({
        email: form.email,
        password: form.password
      }))
    } else {
      localStorage.removeItem('quizhub_saved_credentials')
    }
    
    // Quay lại trang trước đó nếu có, nếu không thì về explore
    const redirect = route.query.redirect || '/explore'
    router.push(redirect)
  }
}

const loginWithGoogle = async () => {
  try {
    const response = await googleTokenLogin()

    // LẤY ACCESS_TOKEN (Cái mà hình ảnh của bạn cho thấy là có)
    const token = response.access_token

    if (!token) {
      alert("Lỗi: Không lấy được Token")
      return
    }

    loading.value = true
    
    // Gửi xuống Backend
    const backendUrl = import.meta.env.DEV ? 'http://localhost:5099' : (import.meta.env.VITE_BACKEND_URL || '')
    
    // SỬA TÊN BIẾN GỬI ĐI: 'AccessToken' (Khớp với DTO Backend mới sửa ở Bước 1)
    const res = await axios.post(`${backendUrl}/api/Auth/google-login`, {
      AccessToken: token 
    })

    if (res.data.success) {
      // ... Logic lưu token và redirect giữ nguyên ...
      authStore.token = res.data.token
      authStore.user = res.data.user
      // ...
      router.push('/explore')
    } 
  } catch (error) {
    console.error(error)
    alert("Đăng nhập thất bại: " + (error.response?.data?.message || error.message))
  } finally {
    loading.value = false
  }
}
</script>
