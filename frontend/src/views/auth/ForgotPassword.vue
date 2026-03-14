<template>
  <div class="min-h-screen flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8 bg-gray-50">
    <div class="max-w-md w-full">
      <!-- Logo -->
      <div class="text-center mb-8">
        <router-link to="/" class="inline-flex items-center">
          <span class="text-3xl font-bold text-gradient">QuizHub</span>
        </router-link>
        <h2 class="mt-6 text-3xl font-bold text-gray-900">Quên mật khẩu?</h2>
        <p class="mt-2 text-gray-600">
          Nhập email của bạn và chúng tôi sẽ gửi link đặt lại mật khẩu
        </p>
      </div>

      <!-- Success Message -->
      <div v-if="sent" class="card p-8 text-center">
        <div class="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
          <CheckCircleIcon class="w-8 h-8 text-green-600" />
        </div>
        <h3 class="text-lg font-semibold text-gray-900 mb-2">Email đã được gửi!</h3>
        <p class="text-gray-600 mb-6">
          Vui lòng kiểm tra hộp thư của bạn và làm theo hướng dẫn để đặt lại mật khẩu.
        </p>
        <router-link to="/login" class="btn-primary">
          Quay lại đăng nhập
        </router-link>
      </div>

      <!-- Form -->
      <div v-else class="card p-8">
        <form @submit.prevent="handleSubmit" class="space-y-6">
          <div>
            <label for="email" class="block text-sm font-medium text-gray-700 mb-1">
              Email
            </label>
            <div class="relative">
              <EnvelopeIcon class="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
              <input
                id="email"
                v-model="email"
                type="email"
                required
                placeholder="email@example.com"
                class="input-field pl-10"
              />
            </div>
          </div>

          <button
            type="submit"
            :disabled="loading"
            class="btn-primary w-full py-3 text-base"
          >
            <ArrowPathIcon v-if="loading" class="w-5 h-5 mr-2 animate-spin" />
            {{ loading ? 'Đang gửi...' : 'Gửi link đặt lại mật khẩu' }}
          </button>
        </form>

        <div class="mt-6 text-center">
          <router-link to="/login" class="text-sm text-primary-600 hover:text-primary-700">
            ← Quay lại đăng nhập
          </router-link>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { EnvelopeIcon, ArrowPathIcon, CheckCircleIcon } from '@heroicons/vue/24/outline'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()

const email = ref('')
const loading = ref(false)
const sent = ref(false)

const handleSubmit = async () => {
  if (!email.value) return

  loading.value = true
  const success = await authStore.forgotPassword(email.value)
  loading.value = false

  if (success) {
    sent.value = true
  }
}
</script>
