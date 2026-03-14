<template>
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <div class="mb-8">
      <h1 class="page-header">Báo cáo & Thống kê</h1>
      <p class="text-gray-600 mt-1">Xem báo cáo chi tiết về hoạt động hệ thống</p>
    </div>

    <!-- Date Range Filter -->
    <div class="card p-4 mb-6">
      <div class="flex flex-col md:flex-row items-center gap-4">
        <span class="text-sm font-medium text-gray-700">Khoảng thời gian:</span>
        <div class="flex gap-2">
          <button
            v-for="range in dateRanges"
            :key="range.value"
            @click="selectedRange = range.value; loadReports()"
            :class="[
              'px-4 py-2 rounded-lg text-sm font-medium transition-colors',
              selectedRange === range.value
                ? 'bg-primary-600 text-white'
                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
            ]"
          >
            {{ range.label }}
          </button>
        </div>
        <div class="flex items-center gap-2 ml-auto">
          <input v-model="customStart" type="date" class="input-field py-1.5" />
          <span class="text-gray-500">đến</span>
          <input v-model="customEnd" type="date" class="input-field py-1.5" />
          <button @click="applyCustomRange" class="btn-secondary py-1.5">Áp dụng</button>
        </div>
      </div>
    </div>

    <!-- Summary Stats -->
    <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
      <div class="card p-4">
        <p class="text-sm text-gray-500">Người dùng mới</p>
        <p class="text-2xl font-bold text-primary-600">{{ stats.newUsers }}</p>
        <p :class="[
          'text-sm mt-1',
          stats.newUsersChange >= 0 ? 'text-green-600' : 'text-red-600'
        ]">
          {{ stats.newUsersChange >= 0 ? '+' : '' }}{{ stats.newUsersChange }}% so với kỳ trước
        </p>
      </div>
      <div class="card p-4">
        <p class="text-sm text-gray-500">Bài thi mới</p>
        <p class="text-2xl font-bold text-green-600">{{ stats.newQuizzes }}</p>
        <p :class="[
          'text-sm mt-1',
          stats.newQuizzesChange >= 0 ? 'text-green-600' : 'text-red-600'
        ]">
          {{ stats.newQuizzesChange >= 0 ? '+' : '' }}{{ stats.newQuizzesChange }}% so với kỳ trước
        </p>
      </div>
      <div class="card p-4">
        <p class="text-sm text-gray-500">Lượt làm bài</p>
        <p class="text-2xl font-bold text-blue-600">{{ stats.totalAttempts }}</p>
        <p :class="[
          'text-sm mt-1',
          stats.attemptsChange >= 0 ? 'text-green-600' : 'text-red-600'
        ]">
          {{ stats.attemptsChange >= 0 ? '+' : '' }}{{ stats.attemptsChange }}% so với kỳ trước
        </p>
      </div>
      <div class="card p-4">
        <p class="text-sm text-gray-500">Điểm trung bình</p>
        <p class="text-2xl font-bold text-purple-600">{{ stats.avgScore }}%</p>
        <p :class="[
          'text-sm mt-1',
          stats.scoreChange >= 0 ? 'text-green-600' : 'text-red-600'
        ]">
          {{ stats.scoreChange >= 0 ? '+' : '' }}{{ stats.scoreChange }}% so với kỳ trước
        </p>
      </div>
    </div>

    <!-- Charts Section -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
      <!-- Daily Activity Chart -->
      <div class="card p-6">
        <h2 class="section-title mb-4">Hoạt động theo ngày</h2>
        <div class="h-64 flex items-center justify-center bg-gray-50 rounded-lg">
          <div class="text-center text-gray-500">
            <ChartBarIcon class="w-12 h-12 mx-auto mb-2 text-gray-300" />
            <p>Biểu đồ số lượt làm bài theo ngày</p>
          </div>
        </div>
      </div>

      <!-- Score Distribution Chart -->
      <div class="card p-6">
        <h2 class="section-title mb-4">Phân bố điểm số</h2>
        <div class="h-64 flex items-center justify-center bg-gray-50 rounded-lg">
          <div class="text-center text-gray-500">
            <ChartBarIcon class="w-12 h-12 mx-auto mb-2 text-gray-300" />
            <p>Biểu đồ phân bố điểm số</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Top Performers & Popular Quizzes -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
      <!-- Top Users -->
      <div class="card p-6">
        <h2 class="section-title mb-4">Người dùng tích cực nhất</h2>
        <div class="space-y-4">
          <div v-for="(user, index) in topUsers" :key="user.id" class="flex items-center gap-3">
            <span :class="[
              'w-8 h-8 rounded-full flex items-center justify-center font-bold text-sm',
              index === 0 ? 'bg-yellow-100 text-yellow-600' :
              index === 1 ? 'bg-gray-100 text-gray-600' :
              index === 2 ? 'bg-orange-100 text-orange-600' :
              'bg-gray-50 text-gray-500'
            ]">
              {{ index + 1 }}
            </span>
            <div class="flex-1">
              <p class="font-medium text-gray-900">{{ user.hoTen }}</p>
              <p class="text-sm text-gray-500">{{ user.soLuotLamBai }} lượt làm bài</p>
            </div>
            <span class="text-sm font-medium text-primary-600">{{ user.diemTrungBinh }}%</span>
          </div>
          <div v-if="topUsers.length === 0" class="text-center py-8 text-gray-500">
            Chưa có dữ liệu
          </div>
        </div>
      </div>

      <!-- Popular Quizzes -->
      <div class="card p-6">
        <h2 class="section-title mb-4">Bài thi phổ biến nhất</h2>
        <div class="space-y-4">
          <div v-for="(quiz, index) in popularQuizzes" :key="quiz.maBaiThi" class="flex items-center gap-3">
            <span :class="[
              'w-8 h-8 rounded-full flex items-center justify-center font-bold text-sm',
              index === 0 ? 'bg-yellow-100 text-yellow-600' :
              index === 1 ? 'bg-gray-100 text-gray-600' :
              index === 2 ? 'bg-orange-100 text-orange-600' :
              'bg-gray-50 text-gray-500'
            ]">
              {{ index + 1 }}
            </span>
            <div class="flex-1">
              <p class="font-medium text-gray-900">{{ quiz.tieuDe }}</p>
              <p class="text-sm text-gray-500">{{ quiz.tenNguoiTao }}</p>
            </div>
            <span class="text-sm font-medium text-primary-600">{{ quiz.tongLuotLamBai }} lượt</span>
          </div>
          <div v-if="popularQuizzes.length === 0" class="text-center py-8 text-gray-500">
            Chưa có dữ liệu
          </div>
        </div>
      </div>
    </div>

    <!-- Export Options -->
    <div class="card p-6">
      <h2 class="section-title mb-4">Xuất báo cáo</h2>
      <div class="flex flex-wrap gap-4">
        <button @click="exportReport('users')" class="btn-secondary">
          <DocumentArrowDownIcon class="w-5 h-5 mr-2" />
          Xuất danh sách người dùng
        </button>
        <button @click="exportReport('quizzes')" class="btn-secondary">
          <DocumentArrowDownIcon class="w-5 h-5 mr-2" />
          Xuất danh sách bài thi
        </button>
        <button @click="exportReport('attempts')" class="btn-secondary">
          <DocumentArrowDownIcon class="w-5 h-5 mr-2" />
          Xuất lịch sử làm bài
        </button>
        <button @click="exportReport('full')" class="btn-primary">
          <DocumentArrowDownIcon class="w-5 h-5 mr-2" />
          Xuất báo cáo tổng hợp
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useToast } from 'vue-toastification'
import {
  ChartBarIcon,
  DocumentArrowDownIcon
} from '@heroicons/vue/24/outline'
import { reportService } from '@/services'

const toast = useToast()

const selectedRange = ref('7days')
const customStart = ref('')
const customEnd = ref('')

const dateRanges = [
  { value: '7days', label: '7 ngày' },
  { value: '30days', label: '30 ngày' },
  { value: '90days', label: '3 tháng' },
  { value: 'year', label: '1 năm' }
]

const stats = reactive({
  newUsers: 0,
  newUsersChange: 0,
  newQuizzes: 0,
  newQuizzesChange: 0,
  totalAttempts: 0,
  attemptsChange: 0,
  avgScore: 0,
  scoreChange: 0
})

const topUsers = ref([])
const popularQuizzes = ref([])

const loadReports = async () => {
  try {
    const [statsRes, usersRes, quizzesRes] = await Promise.all([
      reportService.getStats({ range: selectedRange.value }),
      reportService.getTopUsers({ range: selectedRange.value, limit: 5 }),
      reportService.getTopQuizzes({ range: selectedRange.value, limit: 5 })
    ])

    if (statsRes.success) {
      Object.assign(stats, statsRes.data)
    }

    topUsers.value = usersRes.data || []
    popularQuizzes.value = quizzesRes.data || []
  } catch (error) {
    console.error('Failed to load reports:', error)
    toast.error('Không thể tải báo cáo')
  }
}

const applyCustomRange = () => {
  if (customStart.value && customEnd.value) {
    selectedRange.value = 'custom'
    loadReports()
  }
}

const exportReport = async (type) => {
  try {
    const response = await reportService.exportReport({
      type,
      range: selectedRange.value,
      startDate: customStart.value,
      endDate: customEnd.value
    })
    
    // Download file
    const blob = new Blob([response.data], { type: 'text/csv' })
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = `report_${type}_${new Date().toISOString().split('T')[0]}.csv`
    link.click()
    window.URL.revokeObjectURL(url)
  } catch (error) {
    toast.error('Không thể xuất báo cáo')
  }
}

onMounted(() => {
  loadReports()
})
</script>
