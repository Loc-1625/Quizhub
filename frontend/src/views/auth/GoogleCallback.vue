<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-50">
    <div class="text-center">
      <div class="animate-spin rounded-full h-16 w-16 border-b-2 border-primary-600 mx-auto mb-4"></div>
      <h2 class="text-xl font-semibold text-gray-900 mb-2">Đang xử lý đăng nhập...</h2>
      <p class="text-gray-600">Vui lòng đợi trong giây lát</p>
      <p v-if="error" class="mt-4 text-red-500">{{ error }}</p>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useToast } from 'vue-toastification'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const toast = useToast()
const error = ref('')

onMounted(async () => {
  try {
    // Lấy token từ URL query params (backend sẽ redirect về với token)
    const token = route.query.token
    const userJson = route.query.user
    
    if (token) {
      // Lưu token
      authStore.token = token
      
      // Luôn fetch user info từ API để đảm bảo dữ liệu đầy đủ
      await authStore.fetchCurrentUser()
      
      router.push('/dashboard')
    } else {
      // Kiểm tra lỗi
      const errorMsg = route.query.error
      if (errorMsg) {
        error.value = decodeURIComponent(errorMsg)
        toast.error(error.value)
      } else {
        error.value = 'Không nhận được thông tin đăng nhập'
        toast.error(error.value)
      }
      
      setTimeout(() => {
        router.push('/login')
      }, 2000)
    }
  } catch (err) {
    console.error('Google callback error:', err)
    error.value = 'Đã xảy ra lỗi khi xử lý đăng nhập'
    toast.error(error.value)
    
    setTimeout(() => {
      router.push('/login')
    }, 2000)
  }
})
</script>
