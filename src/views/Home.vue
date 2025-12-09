<template>
  <div class="min-h-screen">
    <!-- Hero Section -->
    <section class="relative overflow-hidden bg-primary-600 pt-20 pb-32">
      <div class="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="text-center">
          <h1 class="text-4xl md:text-5xl lg:text-6xl font-extrabold text-white mb-6">
            Nền tảng Thi Trắc nghiệm
            <span class="block text-primary-200">Thông Minh</span>
          </h1>
          <p class="text-lg md:text-xl text-primary-100 max-w-3xl mx-auto mb-8">
            Tạo, chia sẻ và tham gia các bài thi trắc nghiệm với công nghệ AI và Real-time. 
            Tiết kiệm 70% thời gian tạo đề với AI import thông minh.
          </p>
          
          <div class="flex flex-col sm:flex-row items-center justify-center gap-4">
            <router-link 
              to="/explore" 
              class="w-full sm:w-auto px-8 py-4 bg-white text-primary-600 font-semibold rounded-xl shadow-lg hover:shadow-xl hover:-translate-y-0.5 transition-all duration-200"
            >
              Khám phá bài thi
            </router-link>
            <router-link 
              v-if="!authStore.isLoggedIn"
              to="/register" 
              class="w-full sm:w-auto px-8 py-4 bg-primary-500 text-white font-semibold rounded-xl border-2 border-white/30 hover:bg-primary-400 transition-all duration-200"
            >
              Đăng ký miễn phí
            </router-link>
            <router-link 
              v-else
              to="/quiz/create" 
              class="w-full sm:w-auto px-8 py-4 bg-primary-500 text-white font-semibold rounded-xl border-2 border-white/30 hover:bg-primary-400 transition-all duration-200"
            >
              Tạo bài thi mới
            </router-link>
          </div>

          <!-- Join by Code -->
          <div class="mt-8 max-w-md mx-auto relative z-10">
            <div class="flex bg-white/10 backdrop-blur rounded-xl p-1">
              <input
                v-model="joinCode"
                @keyup.enter="joinQuiz"
                type="text"
                placeholder="Nhập mã bài thi để tham gia..."
                class="flex-1 px-4 py-3 bg-transparent text-white placeholder-white/60 focus:outline-none"
              />
              <button
                @click="joinQuiz"
                class="px-6 py-3 bg-white text-primary-600 font-semibold rounded-lg hover:bg-primary-50 transition-colors"
              >
                Tham gia
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Wave Divider -->
      <div class="absolute bottom-0 left-0 right-0 pointer-events-none">
        <svg viewBox="0 0 1440 120" fill="none" xmlns="http://www.w3.org/2000/svg">
          <path d="M0 120L60 110C120 100 240 80 360 70C480 60 600 60 720 65C840 70 960 80 1080 85C1200 90 1320 90 1380 90L1440 90V120H1380C1320 120 1200 120 1080 120C960 120 840 120 720 120C600 120 480 120 360 120C240 120 120 120 60 120H0Z" fill="#f9fafb"/>
        </svg>
      </div>
    </section>

    <!-- Features Section -->
    <section class="py-20 bg-gray-50">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="text-center mb-16">
          <h2 class="text-3xl md:text-4xl font-bold text-gray-900 mb-4">
            Tại sao chọn QuizHub?
          </h2>
          <p class="text-gray-600 max-w-2xl mx-auto">
            Nền tảng hiện đại với đầy đủ tính năng để tạo và tổ chức thi trắc nghiệm trực tuyến
          </p>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
          <!-- Feature 1 -->
          <div class="card p-6 hover:-translate-y-1 transition-transform duration-300">
            <div class="w-12 h-12 bg-primary-100 rounded-xl flex items-center justify-center mb-4">
              <SparklesIcon class="w-6 h-6 text-primary-600" />
            </div>
            <h3 class="text-lg font-semibold text-gray-900 mb-2">AI Import Thông minh</h3>
            <p class="text-gray-600">
              Tự động trích xuất câu hỏi từ file Word, PDF. Tiết kiệm 70% thời gian với Google Gemini AI.
            </p>
          </div>

          <!-- Feature 2 -->
          <div class="card p-6 hover:-translate-y-1 transition-transform duration-300">
            <div class="w-12 h-12 bg-green-100 rounded-xl flex items-center justify-center mb-4">
              <BoltIcon class="w-6 h-6 text-green-600" />
            </div>
            <h3 class="text-lg font-semibold text-gray-900 mb-2">Real-time với SignalR</h3>
            <p class="text-gray-600">
              Đồng hồ đếm ngược đồng bộ, tự động nộp bài khi hết giờ. Đảm bảo công bằng tuyệt đối.
            </p>
          </div>

          <!-- Feature 3 -->
          <div class="card p-6 hover:-translate-y-1 transition-transform duration-300">
            <div class="w-12 h-12 bg-purple-100 rounded-xl flex items-center justify-center mb-4">
              <ShareIcon class="w-6 h-6 text-purple-600" />
            </div>
            <h3 class="text-lg font-semibold text-gray-900 mb-2">Chia sẻ Dễ dàng</h3>
            <p class="text-gray-600">
              Mỗi bài thi có mã code và link riêng. Chia sẻ nhanh chóng với bạn bè, lớp học.
            </p>
          </div>
        </div>
      </div>
    </section>

    <!-- Popular Quizzes Section -->
    <section class="py-20">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex items-center justify-between mb-8">
          <div>
            <h2 class="text-2xl md:text-3xl font-bold text-gray-900">Bài thi phổ biến</h2>
            <p class="text-gray-600 mt-1">Khám phá các bài thi được yêu thích nhất</p>
          </div>
          <router-link to="/explore" class="btn-secondary">
            Xem tất cả
            <ArrowRightIcon class="w-4 h-4 ml-2" />
          </router-link>
        </div>

        <div v-if="loading" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          <div v-for="i in 4" :key="i" class="card animate-pulse">
            <div class="h-32 bg-gray-200"></div>
            <div class="p-4 space-y-3">
              <div class="h-4 bg-gray-200 rounded w-3/4"></div>
              <div class="h-3 bg-gray-200 rounded w-1/2"></div>
            </div>
          </div>
        </div>

        <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          <QuizCard v-for="quiz in popularQuizzes" :key="quiz.maBaiThi" :quiz="quiz" />
        </div>

        <div v-if="!loading && popularQuizzes.length === 0" class="text-center py-12">
          <DocumentTextIcon class="w-16 h-16 text-gray-300 mx-auto mb-4" />
          <p class="text-gray-500">Chưa có bài thi nào</p>
        </div>
      </div>
    </section>

    <!-- CTA Section -->
    <section class="py-20 bg-primary-600">
      <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
        <h2 class="text-3xl md:text-4xl font-bold text-white mb-4">
          Sẵn sàng bắt đầu?
        </h2>
        <p class="text-primary-100 text-lg mb-8">
          Tham gia cùng hàng nghìn người dùng đang sử dụng QuizHub mỗi ngày
        </p>
        <div class="flex flex-col sm:flex-row items-center justify-center gap-4">
          <router-link 
            v-if="!authStore.isLoggedIn"
            to="/register" 
            class="w-full sm:w-auto px-8 py-4 bg-white text-primary-600 font-semibold rounded-xl shadow-lg hover:shadow-xl transition-all"
          >
            Đăng ký miễn phí
          </router-link>
          <router-link 
            to="/explore" 
            class="w-full sm:w-auto px-8 py-4 bg-transparent text-white font-semibold rounded-xl border-2 border-white hover:bg-white/10 transition-all"
          >
            Khám phá ngay
          </router-link>
        </div>
      </div>
    </section>

    <!-- Stats Section -->
    <section class="py-16 bg-gray-50">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="grid grid-cols-2 md:grid-cols-4 gap-8 text-center">
          <div>
            <div class="text-3xl md:text-4xl font-bold text-primary-600">0</div>
            <div class="text-gray-600 mt-1">Người dùng</div>
          </div>
          <div>
            <div class="text-3xl md:text-4xl font-bold text-primary-600">0</div>
            <div class="text-gray-600 mt-1">Bài thi</div>
          </div>
          <div>
            <div class="text-3xl md:text-4xl font-bold text-primary-600">0</div>
            <div class="text-gray-600 mt-1">Câu hỏi</div>
          </div>
          <div>
            <div class="text-3xl md:text-4xl font-bold text-primary-600">0</div>
            <div class="text-gray-600 mt-1">Lượt làm bài</div>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import {
  SparklesIcon,
  BoltIcon,
  ShareIcon,
  ArrowRightIcon,
  DocumentTextIcon
} from '@heroicons/vue/24/outline'
import { useAuthStore } from '@/stores/auth'
import { quizService } from '@/services'
import QuizCard from '@/components/quiz/QuizCard.vue'

const router = useRouter()
const authStore = useAuthStore()

const joinCode = ref('')
const popularQuizzes = ref([])
const loading = ref(true)

const joinQuiz = () => {
  const code = joinCode.value.trim()
  if (code) {
    router.push(`/join/${code}`)
  }
}

onMounted(async () => {
  try {
    const response = await quizService.getPublicQuizzes({ pageNumber: 1, pageSize: 8 })
    popularQuizzes.value = response.data || []
  } catch (error) {
    console.error('Failed to load quizzes:', error)
  } finally {
    loading.value = false
  }
})
</script>
