<template>
  <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <!-- Loading -->
    <div v-if="loading" class="flex justify-center py-12">
      <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
    </div>

    <!-- Error -->
    <div v-else-if="error" class="card p-8 text-center">
      <ExclamationCircleIcon class="w-16 h-16 text-red-500 mx-auto mb-4" />
      <h2 class="text-xl font-semibold text-gray-900 mb-2">Không tìm thấy người dùng</h2>
      <p class="text-gray-600 mb-4">{{ error }}</p>
      <router-link to="/" class="btn-primary">
        Về trang chủ
      </router-link>
    </div>

    <!-- Profile Content -->
    <div v-else-if="user">
      <!-- Header -->
      <div class="card p-6 mb-6">
        <div class="flex items-start gap-6">
          <div class="flex-shrink-0">
            <img
              v-if="user.avatar"
              :src="getAvatarUrl(user.avatar)"
              :alt="user.hoTen"
              class="w-24 h-24 rounded-full object-cover"
            />
            <div
              v-else
              class="w-24 h-24 rounded-full bg-gradient-to-br from-primary-500 to-primary-600 flex items-center justify-center text-white text-3xl font-bold"
            >
              {{ user.hoTen?.charAt(0)?.toUpperCase() || 'U' }}
            </div>
          </div>
          
          <div class="flex-1">
            <h1 class="text-2xl font-bold text-gray-900">{{ user.hoTen }}</h1>
            <p class="text-gray-600">@{{ user.tenDangNhap }}</p>
            <p v-if="user.gioiThieu" class="text-gray-700 mt-2">{{ user.gioiThieu }}</p>
            
            <div class="flex items-center gap-4 mt-4 text-sm text-gray-500">
              <div class="flex items-center">
                <CalendarIcon class="w-4 h-4 mr-1" />
                Tham gia {{ formatDate(user.ngayTao) }}
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Stats -->
      <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
        <div class="card p-4 text-center">
          <div class="text-2xl font-bold text-primary-600">{{ stats.soBaiThiTao || 0 }}</div>
          <div class="text-sm text-gray-600">Bài thi tạo</div>
        </div>
        <div class="card p-4 text-center">
          <div class="text-2xl font-bold text-green-600">{{ stats.soCauHoiTao || 0 }}</div>
          <div class="text-sm text-gray-600">Câu hỏi tạo</div>
        </div>
        <div class="card p-4 text-center">
          <div class="text-2xl font-bold text-blue-600">{{ stats.soNguoiLamBai || 0 }}</div>
          <div class="text-sm text-gray-600">Lượt làm bài</div>
        </div>
        <div class="card p-4 text-center">
          <div class="text-2xl font-bold text-yellow-600">{{ stats.diemTrungBinh?.toFixed(1) || '0' }}</div>
          <div class="text-sm text-gray-600">Điểm TB</div>
        </div>
      </div>

      <!-- Public Quizzes -->
      <div class="card p-6">
        <h2 class="text-xl font-semibold text-gray-900 mb-4">
          <BookOpenIcon class="w-6 h-6 inline-block mr-2 text-primary-600" />
          Bài thi công khai
        </h2>

        <div v-if="loadingQuizzes" class="flex justify-center py-8">
          <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-primary-600"></div>
        </div>

        <div v-else-if="quizzes.length === 0" class="text-center py-8 text-gray-500">
          Người dùng chưa có bài thi công khai nào
        </div>

        <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <router-link
            v-for="quiz in quizzes"
            :key="quiz.id"
            :to="{ name: 'quiz-detail', params: { id: quiz.id }}"
            class="block p-4 border border-gray-200 rounded-lg hover:border-primary-500 hover:shadow-sm transition-all"
          >
            <div class="flex items-start gap-3">
              <div class="p-2 rounded-lg bg-primary-100 text-primary-600">
                <DocumentTextIcon class="w-6 h-6" />
              </div>
              <div class="flex-1 min-w-0">
                <h3 class="font-medium text-gray-900 truncate">{{ quiz.tieuDe }}</h3>
                <p class="text-sm text-gray-500 mt-1">
                  {{ quiz.soCauHoi }} câu • {{ quiz.luotLam }} lượt làm
                </p>
                <div class="flex items-center gap-2 mt-2">
                  <span 
                    class="inline-block px-2 py-0.5 rounded text-xs font-medium"
                    :class="getDifficultyClass(quiz.doKho)"
                  >
                    {{ getDifficultyLabel(quiz.doKho) }}
                  </span>
                  <span v-if="quiz.danhMuc" class="text-xs text-gray-500">
                    {{ quiz.danhMuc.tenDanhMuc }}
                  </span>
                </div>
              </div>
            </div>
          </router-link>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import {
  ExclamationCircleIcon,
  CalendarIcon,
  BookOpenIcon,
  DocumentTextIcon
} from '@heroicons/vue/24/outline'
import { quizService } from '@/services'
import api from '@/services/api'

const route = useRoute()

const loading = ref(true)
const loadingQuizzes = ref(false)
const error = ref('')
const user = ref(null)
const stats = ref({})
const quizzes = ref([])

onMounted(async () => {
  await loadProfile()
})

async function loadProfile() {
  loading.value = true
  error.value = ''
  
  try {
    const userId = route.params.userId
    
    // Load user profile
    const response = await api.get(`/Auth/public-profile/${userId}`)
    user.value = response.data.data || response.data
    stats.value = user.value.thongKe || {}
    
    // Load public quizzes
    await loadPublicQuizzes(userId)
  } catch (err) {
    console.error('Load profile error:', err)
    error.value = err.response?.data?.message || 'Không thể tải thông tin người dùng'
  } finally {
    loading.value = false
  }
}

async function loadPublicQuizzes(userId) {
  loadingQuizzes.value = true
  try {
    const response = await quizService.getAll({
      nguoiTaoId: userId,
      coBaiCongKhai: true,
      trangThai: 'DaXuatBan',
      pageSize: 10
    })
    quizzes.value = response.data || []
  } catch (err) {
    console.error('Load quizzes error:', err)
  } finally {
    loadingQuizzes.value = false
  }
}

function getAvatarUrl(avatar) {
  if (avatar.startsWith('http')) return avatar
  return `${import.meta.env.VITE_API_URL?.replace('/api', '')}${avatar}`
}

function formatDate(date) {
  if (!date) return ''
  return new Date(date).toLocaleDateString('vi-VN', {
    year: 'numeric',
    month: 'long'
  })
}

function getDifficultyClass(doKho) {
  const classes = {
    'De': 'bg-green-100 text-green-700',
    'TrungBinh': 'bg-yellow-100 text-yellow-700',
    'Kho': 'bg-red-100 text-red-700'
  }
  return classes[doKho] || 'bg-gray-100 text-gray-700'
}

function getDifficultyLabel(doKho) {
  const labels = {
    'De': 'Dễ',
    'TrungBinh': 'Trung bình',
    'Kho': 'Khó'
  }
  return labels[doKho] || doKho
}
</script>
