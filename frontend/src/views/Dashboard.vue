<template>
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <!-- Header -->
    <div class="mb-8">
      <h1 class="page-header">Dashboard</h1>
      <p class="text-gray-600 mt-1">Xin chào, {{ authStore.fullName }}!</p>
    </div>

    <!-- Quick Stats -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
      <div class="card p-6">
        <div class="flex items-center">
          <div class="w-12 h-12 bg-primary-100 rounded-xl flex items-center justify-center">
            <DocumentTextIcon class="w-6 h-6 text-primary-600" />
          </div>
          <div class="ml-4">
            <p class="text-2xl font-bold text-gray-900">{{ stats.totalQuizzes }}</p>
            <p class="text-sm text-gray-500">Bài thi đã tạo</p>
          </div>
        </div>
      </div>

      <div class="card p-6">
        <div class="flex items-center">
          <div class="w-12 h-12 bg-green-100 rounded-xl flex items-center justify-center">
            <QuestionMarkCircleIcon class="w-6 h-6 text-green-600" />
          </div>
          <div class="ml-4">
            <p class="text-2xl font-bold text-gray-900">{{ stats.totalQuestions }}</p>
            <p class="text-sm text-gray-500">Câu hỏi</p>
          </div>
        </div>
      </div>

      <div class="card p-6">
        <div class="flex items-center">
          <div class="w-12 h-12 bg-purple-100 rounded-xl flex items-center justify-center">
            <ClipboardDocumentCheckIcon class="w-6 h-6 text-purple-600" />
          </div>
          <div class="ml-4">
            <p class="text-2xl font-bold text-gray-900">{{ stats.totalAttempts }}</p>
            <p class="text-sm text-gray-500">Lượt làm bài</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Quick Actions -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-8">
      <router-link to="/quiz/create" class="card p-6 hover:border-primary-500 transition-colors group">
        <div class="flex items-center">
          <div class="w-12 h-12 bg-primary-100 rounded-xl flex items-center justify-center group-hover:bg-primary-200 transition-colors">
            <PlusIcon class="w-6 h-6 text-primary-600" />
          </div>
          <div class="ml-4">
            <h3 class="font-semibold text-gray-900">Tạo bài thi mới</h3>
            <p class="text-sm text-gray-500">Tạo bài thi từ ngân hàng câu hỏi</p>
          </div>
        </div>
      </router-link>

      <router-link to="/questions/import" class="card p-6 hover:border-secondary-500 transition-colors group">
        <div class="flex items-center">
          <div class="w-12 h-12 bg-secondary-100 rounded-xl flex items-center justify-center group-hover:bg-secondary-200 transition-colors">
            <SparklesIcon class="w-6 h-6 text-secondary-600" />
          </div>
          <div class="ml-4">
            <h3 class="font-semibold text-gray-900">Import bằng AI</h3>
            <p class="text-sm text-gray-500">Tự động trích xuất từ file</p>
          </div>
        </div>
      </router-link>

      <router-link to="/explore" class="card p-6 hover:border-green-500 transition-colors group">
        <div class="flex items-center">
          <div class="w-12 h-12 bg-green-100 rounded-xl flex items-center justify-center group-hover:bg-green-200 transition-colors">
            <MagnifyingGlassIcon class="w-6 h-6 text-green-600" />
          </div>
          <div class="ml-4">
            <h3 class="font-semibold text-gray-900">Khám phá bài thi</h3>
            <p class="text-sm text-gray-500">Tìm bài thi để làm</p>
          </div>
        </div>
      </router-link>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-2 gap-8">
      <!-- Recent Quizzes -->
      <div class="card">
        <div class="p-6 border-b border-gray-100">
          <div class="flex items-center justify-between">
            <h2 class="section-title">Bài thi gần đây</h2>
            <router-link to="/my-quizzes" class="text-sm text-primary-600 hover:text-primary-700">
              Xem tất cả →
            </router-link>
          </div>
        </div>
        
        <div v-if="loadingQuizzes" class="p-6">
          <div v-for="i in 3" :key="i" class="flex items-center space-x-4 mb-4 animate-pulse">
            <div class="w-12 h-12 bg-gray-200 rounded-lg"></div>
            <div class="flex-1">
              <div class="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
              <div class="h-3 bg-gray-200 rounded w-1/2"></div>
            </div>
          </div>
        </div>

        <div v-else-if="recentQuizzes.length === 0" class="p-6 text-center">
          <DocumentTextIcon class="w-12 h-12 text-gray-300 mx-auto mb-2" />
          <p class="text-gray-500">Chưa có bài thi nào</p>
          <router-link to="/quiz/create" class="btn-primary mt-4">
            Tạo bài thi đầu tiên
          </router-link>
        </div>

        <div v-else class="divide-y divide-gray-100">
          <router-link
            v-for="quiz in recentQuizzes"
            :key="quiz.maBaiThi"
            :to="{ name: 'quiz-detail', params: { id: quiz.maBaiThi } }"
            class="flex items-center p-4 hover:bg-gray-50 transition-colors"
          >
            <div class="w-12 h-12 bg-gradient-to-br from-primary-500 to-secondary-500 rounded-lg flex items-center justify-center flex-shrink-0">
              <DocumentTextIcon class="w-6 h-6 text-white" />
            </div>
            <div class="ml-4 flex-1 min-w-0">
              <h3 class="font-medium text-gray-900 truncate">{{ quiz.tieuDe }}</h3>
              <p class="text-sm text-gray-500">
                {{ quiz.soCauHoi }} câu hỏi • {{ quiz.tongLuotLamBai || 0 }} lượt làm
              </p>
            </div>
          </router-link>
        </div>
      </div>

      <!-- Recent History -->
      <div class="card">
        <div class="p-6 border-b border-gray-100">
          <div class="flex items-center justify-between">
            <h2 class="section-title">Lịch sử làm bài</h2>
            <router-link to="/history" class="text-sm text-primary-600 hover:text-primary-700">
              Xem tất cả →
            </router-link>
          </div>
        </div>

        <div v-if="loadingHistory" class="p-6">
          <div v-for="i in 3" :key="i" class="flex items-center space-x-4 mb-4 animate-pulse">
            <div class="w-12 h-12 bg-gray-200 rounded-lg"></div>
            <div class="flex-1">
              <div class="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
              <div class="h-3 bg-gray-200 rounded w-1/2"></div>
            </div>
          </div>
        </div>

        <div v-else-if="recentHistory.length === 0" class="p-6 text-center">
          <ClipboardDocumentCheckIcon class="w-12 h-12 text-gray-300 mx-auto mb-2" />
          <p class="text-gray-500">Chưa có lịch sử làm bài</p>
          <router-link to="/explore" class="btn-primary mt-4">
            Khám phá bài thi
          </router-link>
        </div>

        <div v-else class="divide-y divide-gray-100">
          <router-link
            v-for="attempt in recentHistory"
            :key="attempt.maLuotLamBai"
            :to="{ name: 'quiz-result', params: { id: attempt.maBaiThi, luotLamBaiId: attempt.maLuotLamBai } }"
            class="flex items-center p-4 hover:bg-gray-50 transition-colors"
          >
            <div :class="[
              'w-12 h-12 rounded-lg flex items-center justify-center flex-shrink-0',
              (attempt.diem ?? 0) >= 5 ? 'bg-green-100' : 'bg-red-100'
            ]">
              <span :class="[
                'text-lg font-bold',
                (attempt.diem ?? 0) >= 5 ? 'text-green-600' : 'text-red-600'
              ]">
                {{ Number.isInteger(attempt.diem) ? attempt.diem : (attempt.diem?.toFixed(1) || '0') }}
              </span>
            </div>
            <div class="ml-4 flex-1 min-w-0">
              <h3 class="font-medium text-gray-900 truncate">{{ attempt.tieuDeBaiThi }}</h3>
              <p class="text-sm text-gray-500">
                {{ formatDate(attempt.thoiGianNopBai) }}
              </p>
            </div>
          </router-link>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import {
  DocumentTextIcon,
  QuestionMarkCircleIcon,
  ClipboardDocumentCheckIcon,
  PlusIcon,
  SparklesIcon,
  MagnifyingGlassIcon
} from '@heroicons/vue/24/outline'
import { useAuthStore } from '@/stores/auth'
import { quizService, questionService } from '@/services'

const authStore = useAuthStore()

const stats = reactive({
  totalQuizzes: 0,
  totalQuestions: 0,
  totalAttempts: 0
})

const recentQuizzes = ref([])
const recentHistory = ref([])
const loadingQuizzes = ref(true)
const loadingHistory = ref(true)

const formatDate = (date) => {
  if (!date) return ''
  // Backend trả về UTC, thêm 'Z' nếu chưa có
  const utcStr = date.endsWith('Z') ? date : date + 'Z'
  return new Date(utcStr).toLocaleDateString('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

onMounted(async () => {
  // Load recent quizzes
  try {
    const response = await quizService.getQuizzes({
      chiLayCuaToi: true,
      pageSize: 5,
      sortBy: 'NgayTao',
      sortOrder: 'DESC'
    })
    recentQuizzes.value = response.data || []
    stats.totalQuizzes = response.pagination?.totalCount || 0
  } catch (error) {
    console.error('Failed to load quizzes:', error)
  } finally {
    loadingQuizzes.value = false
  }

  // Load question count
  try {
    const response = await questionService.getCount()
    stats.totalQuestions = response.data?.count || 0
  } catch (error) {
    console.error('Failed to load question count:', error)
  }

  // Load history
  try {
    const response = await quizService.getHistory(1, 5)
    recentHistory.value = response.data || []
    stats.totalAttempts = response.pagination?.totalCount || 0
  } catch (error) {
    console.error('Failed to load history:', error)
  } finally {
    loadingHistory.value = false
  }
})
</script>
