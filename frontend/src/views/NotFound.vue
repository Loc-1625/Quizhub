<template>
  <div class="min-h-screen flex items-center justify-center px-4">
    <div class="text-center">
      <!-- 404 Illustration -->
      <div class="relative mb-8">
        <div class="text-9xl font-bold text-gray-200">404</div>
        <div class="absolute inset-0 flex items-center justify-center">
          <ExclamationTriangleIcon class="w-24 h-24 text-primary-500 opacity-50" />
        </div>
      </div>

      <h1 class="text-3xl font-bold text-gray-900 mb-4">
        Không tìm thấy trang
      </h1>
      
      <p class="text-gray-600 mb-8 max-w-md mx-auto">
        Xin lỗi, trang bạn đang tìm kiếm không tồn tại hoặc đã bị di chuyển.
      </p>

      <div class="flex flex-col sm:flex-row items-center justify-center gap-4">
        <router-link to="/" class="btn-primary px-6">
          <HomeIcon class="w-5 h-5 mr-2" />
          Về trang chủ
        </router-link>
        
        <button @click="goBack" class="btn-outline px-6">
          <ArrowLeftIcon class="w-5 h-5 mr-2" />
          Quay lại
        </button>
      </div>

      <!-- Helpful Links -->
      <div class="mt-12">
        <p class="text-sm text-gray-500 mb-4">Có thể bạn đang tìm:</p>
        <div class="flex flex-wrap justify-center gap-4 text-sm">
          <router-link to="/explore" class="text-primary-600 hover:text-primary-700 hover:underline">
            Khám phá bài thi
          </router-link>
          <router-link to="/dashboard" class="text-primary-600 hover:text-primary-700 hover:underline">
            Dashboard
          </router-link>
          <router-link to="/questions" class="text-primary-600 hover:text-primary-700 hover:underline">
            Ngân hàng câu hỏi
          </router-link>
          <router-link to="/history" class="text-primary-600 hover:text-primary-700 hover:underline">
            Lịch sử làm bài
          </router-link>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { useRouter } from 'vue-router'
import {
  ExclamationTriangleIcon,
  HomeIcon,
  ArrowLeftIcon
} from '@heroicons/vue/24/outline'

const router = useRouter()

function goBack() {
  if (window.history.length > 1) {
    router.back()
  } else {
    router.push('/')
  }
}
</script>
