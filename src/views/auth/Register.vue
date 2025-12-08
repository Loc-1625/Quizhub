<template>
  <div class="min-h-screen flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8 bg-gray-50">
    <div class="max-w-md w-full">
      <!-- Logo -->
      <div class="text-center mb-8">
        <router-link to="/" class="inline-flex items-center">
          <span class="text-3xl font-bold text-gradient">QuizHub</span>
        </router-link>
        <h2 class="mt-6 text-3xl font-bold text-gray-900">Tạo tài khoản</h2>
        <p class="mt-2 text-gray-600">
          Đã có tài khoản?
          <router-link to="/login" class="text-primary-600 hover:text-primary-700 font-medium">
            Đăng nhập
          </router-link>
        </p>
      </div>

      <!-- Register Form -->
      <div class="card p-8">
        <form @submit.prevent="handleRegister" class="space-y-5">
          <div>
            <label for="hoTen" class="block text-sm font-medium text-gray-700 mb-1">
              Họ và tên
            </label>
            <div class="relative">
              <UserIcon class="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
              <input
                id="hoTen"
                v-model="form.hoTen"
                type="text"
                required
                placeholder="Nguyễn Văn A"
                class="input-field pl-10"
                :class="{ 'border-red-500 focus:ring-red-500': errors.hoTen }"
              />
            </div>
            <p v-if="errors.hoTen" class="mt-1 text-sm text-red-500">{{ errors.hoTen }}</p>
          </div>

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
                placeholder="Ít nhất 8 ký tự, gồm chữ hoa, thường và số"
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
            <!-- Password strength indicator -->
            <div v-if="form.password" class="mt-2">
              <div class="flex items-center gap-2 text-xs flex-wrap">
                <span :class="form.password.length >= 8 ? 'text-green-600' : 'text-gray-400'">
                  ✓ Ít nhất 8 ký tự
                </span>
                <span :class="/[a-z]/.test(form.password) ? 'text-green-600' : 'text-gray-400'">
                  ✓ Chữ thường
                </span>
                <span :class="/[A-Z]/.test(form.password) ? 'text-green-600' : 'text-gray-400'">
                  ✓ Chữ hoa
                </span>
                <span :class="/[0-9]/.test(form.password) ? 'text-green-600' : 'text-gray-400'">
                  ✓ Chữ số
                </span>
              </div>
            </div>
          </div>

          <div>
            <label for="confirmPassword" class="block text-sm font-medium text-gray-700 mb-1">
              Xác nhận mật khẩu
            </label>
            <div class="relative">
              <LockClosedIcon class="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
              <input
                id="confirmPassword"
                v-model="form.confirmPassword"
                :type="showPassword ? 'text' : 'password'"
                required
                placeholder="Nhập lại mật khẩu"
                class="input-field pl-10"
                :class="{ 'border-red-500 focus:ring-red-500': errors.confirmPassword }"
              />
            </div>
            <p v-if="errors.confirmPassword" class="mt-1 text-sm text-red-500">{{ errors.confirmPassword }}</p>
          </div>

          <div class="flex items-start">
            <input
              v-model="form.agree"
              type="checkbox"
              class="w-4 h-4 mt-0.5 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
            />
            <span class="ml-2 text-sm text-gray-600">
              Tôi đồng ý với
              <a href="#" class="text-primary-600 hover:text-primary-700">Điều khoản sử dụng</a>
              và
              <a href="#" class="text-primary-600 hover:text-primary-700">Chính sách bảo mật</a>
            </span>
          </div>
          <p v-if="errors.agree" class="text-sm text-red-500">{{ errors.agree }}</p>

          <button
            type="submit"
            :disabled="loading"
            class="btn-primary w-full py-3 text-base"
          >
            <ArrowPathIcon v-if="loading" class="w-5 h-5 mr-2 animate-spin" />
            {{ loading ? 'Đang tạo tài khoản...' : 'Đăng ký' }}
          </button>
        </form>

        <!-- Divider -->
        <div class="relative my-6">
          <div class="absolute inset-0 flex items-center">
            <div class="w-full border-t border-gray-200"></div>
          </div>
          <div class="relative flex justify-center text-sm">
            <span class="px-4 bg-white text-gray-500">Hoặc đăng ký với</span>
          </div>
        </div>

        <!-- Social Login -->
        <button
          @click="registerWithGoogle"
          class="w-full flex items-center justify-center px-4 py-3 border border-gray-300 rounded-lg shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 transition-colors"
        >
          <svg class="w-5 h-5 mr-2" viewBox="0 0 24 24">
            <path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"/>
            <path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"/>
            <path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"/>
            <path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"/>
          </svg>
          Đăng ký với Google
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import {
  UserIcon,
  EnvelopeIcon,
  LockClosedIcon,
  EyeIcon,
  EyeSlashIcon,
  ArrowPathIcon
} from '@heroicons/vue/24/outline'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const loading = ref(false)
const showPassword = ref(false)

const form = reactive({
  hoTen: '',
  email: '',
  password: '',
  confirmPassword: '',
  agree: false
})

const errors = reactive({
  hoTen: '',
  email: '',
  password: '',
  confirmPassword: '',
  agree: ''
})

const validateForm = () => {
  let valid = true
  Object.keys(errors).forEach(key => errors[key] = '')

  if (!form.hoTen || form.hoTen.length < 2) {
    errors.hoTen = 'Họ tên phải có ít nhất 2 ký tự'
    valid = false
  }

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
  } else if (form.password.length < 8) {
    errors.password = 'Mật khẩu phải có ít nhất 8 ký tự'
    valid = false
  } else if (!/[a-z]/.test(form.password)) {
    errors.password = 'Mật khẩu phải có ít nhất 1 chữ thường (a-z)'
    valid = false
  } else if (!/[A-Z]/.test(form.password)) {
    errors.password = 'Mật khẩu phải có ít nhất 1 chữ hoa (A-Z)'
    valid = false
  } else if (!/[0-9]/.test(form.password)) {
    errors.password = 'Mật khẩu phải có ít nhất 1 chữ số (0-9)'
    valid = false
  }

  if (form.password !== form.confirmPassword) {
    errors.confirmPassword = 'Mật khẩu không khớp'
    valid = false
  }

  if (!form.agree) {
    errors.agree = 'Vui lòng đồng ý với điều khoản sử dụng'
    valid = false
  }

  return valid
}

const handleRegister = async () => {
  if (!validateForm()) return

  loading.value = true
  const success = await authStore.register({
    hoTen: form.hoTen,
    email: form.email,
    password: form.password,
    confirmPassword: form.confirmPassword
  })
  loading.value = false

  if (success) {
    router.push('/dashboard')
  }
}

const registerWithGoogle = () => {
  // Redirect tới backend Google OAuth endpoint
  const backendUrl = import.meta.env.DEV ? '' : (import.meta.env.VITE_BACKEND_URL || '')
  window.location.href = `${backendUrl}/api/Auth/google-login?returnUrl=${encodeURIComponent(window.location.origin + '/auth/google-callback')}`
}
</script>
