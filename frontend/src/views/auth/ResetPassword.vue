<template>
  <div class="min-h-screen flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8 bg-gray-50">
    <div class="max-w-md w-full">
      <!-- Logo -->
      <div class="text-center mb-8">
        <router-link to="/" class="inline-flex items-center">
          <span class="text-3xl font-bold text-gradient">QuizHub</span>
        </router-link>
        <h2 class="mt-6 text-3xl font-bold text-gray-900">Đặt lại mật khẩu</h2>
        <p class="mt-2 text-gray-600">
          Nhập mật khẩu mới cho tài khoản của bạn
        </p>
      </div>

      <!-- Form -->
      <div class="card p-8">
        <form @submit.prevent="handleSubmit" class="space-y-6">
          <div>
            <label for="password" class="block text-sm font-medium text-gray-700 mb-1">
              Mật khẩu mới
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
                :class="{ 'border-red-500 focus:ring-red-500': passwordError }"
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
            <p v-if="passwordError" class="mt-1 text-sm text-red-500">{{ passwordError }}</p>
            <!-- Password strength indicator -->
            <div v-if="form.password" class="mt-2">
              <div class="flex flex-wrap items-center gap-2 text-xs">
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
                placeholder="Nhập lại mật khẩu mới"
                class="input-field pl-10"
              />
            </div>
            <p v-if="form.password !== form.confirmPassword && form.confirmPassword" class="mt-1 text-sm text-red-500">
              Mật khẩu không khớp
            </p>
          </div>

          <button
            type="submit"
            :disabled="loading || form.password !== form.confirmPassword"
            class="btn-primary w-full py-3 text-base"
          >
            <ArrowPathIcon v-if="loading" class="w-5 h-5 mr-2 animate-spin" />
            {{ loading ? 'Đang xử lý...' : 'Đặt lại mật khẩu' }}
          </button>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { LockClosedIcon, EyeIcon, EyeSlashIcon, ArrowPathIcon } from '@heroicons/vue/24/outline'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

const loading = ref(false)
const showPassword = ref(false)
const passwordError = ref('')

const form = reactive({
  password: '',
  confirmPassword: ''
})

const validatePassword = (password) => {
  if (password.length < 8) {
    return 'Mật khẩu phải có ít nhất 8 ký tự'
  }
  if (!/[a-z]/.test(password)) {
    return 'Mật khẩu phải có ít nhất 1 chữ thường'
  }
  if (!/[A-Z]/.test(password)) {
    return 'Mật khẩu phải có ít nhất 1 chữ hoa'
  }
  if (!/[0-9]/.test(password)) {
    return 'Mật khẩu phải có ít nhất 1 chữ số'
  }
  return ''
}

const handleSubmit = async () => {
  if (form.password !== form.confirmPassword) return
  
  passwordError.value = validatePassword(form.password)
  if (passwordError.value) return

  const email = route.query.email
  const token = route.query.token

  if (!email || !token) {
    router.push('/forgot-password')
    return
  }

  loading.value = true
  const success = await authStore.resetPassword(email, token, form.password)
  loading.value = false

  if (success) {
    router.push('/login')
  }
}
</script>
