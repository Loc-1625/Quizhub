<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-50 px-4">
    <div class="max-w-lg w-full bg-white rounded-xl shadow-sm border border-gray-100 p-8 text-center">
      <h1 class="text-2xl font-bold text-gray-900 mb-3">Xác nhận email</h1>

      <p v-if="loading" class="text-gray-600">
        Hệ thống đang xác nhận email của bạn, vui lòng chờ...
      </p>

      <p v-else-if="success" class="text-green-700">
        {{ message }}
      </p>

      <p v-else class="text-red-600">
        {{ message }}
      </p>

      <div class="mt-6">
        <router-link
          to="/login"
          class="inline-flex items-center justify-center px-4 py-2 rounded-lg bg-primary-600 text-white hover:bg-primary-700 transition-colors"
        >
          Đến trang đăng nhập
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import api from '@/services/api'

const route = useRoute()
const router = useRouter()

const loading = ref(true)
const success = ref(false)
const message = ref('')

onMounted(async () => {
  const userId = route.query.userId
  const token = route.query.token

  if (!userId || !token) {
    loading.value = false
    success.value = false
    message.value = 'Link xác nhận không hợp lệ hoặc đã bị thiếu thông tin.'
    return
  }

  try {
    const response = await api.get('/Auth/confirm-email', {
      params: {
        userId,
        token
      }
    })

    success.value = !!response.data?.success
    message.value = response.data?.message || 'Xác nhận email thành công.'

    if (success.value) {
      setTimeout(() => {
        router.replace({
          name: 'login',
          query: { emailConfirmed: '1' }
        })
      }, 1500)
    }
  } catch (error) {
    success.value = false
    message.value = error.response?.data?.message || 'Xác nhận email thất bại hoặc liên kết đã hết hạn.'
  } finally {
    loading.value = false
  }
})
</script>
