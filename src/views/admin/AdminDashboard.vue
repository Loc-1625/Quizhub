<template>
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <div class="mb-8">
      <h1 class="page-header">Bảng điều khiển Admin</h1>
      <p class="text-gray-600 mt-1">Tổng quan hệ thống QuizHub</p>
      <!-- Period Selector -->
      <div class="flex items-center gap-2 mt-4">
        <span class="text-sm text-gray-500">Xem theo:</span>
        <select v-model="selectedPeriod" class="input-field w-36">
          <option value="today">Hôm nay</option>
          <option value="week">Tuần này</option>
          <option value="month">Tháng này</option>
        </select>
      </div>
    </div>

    <!-- Stats Cards -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
      <div class="card p-6">
        <div class="flex items-center">
          <div class="p-3 bg-primary-100 rounded-lg">
            <UsersIcon class="w-6 h-6 text-primary-600" />
          </div>
          <div class="ml-4">
            <p class="text-sm text-gray-500">Người dùng</p>
            <p class="text-2xl font-bold text-gray-900">{{ stats.totalUsers }}</p>
          </div>
        </div>
        <div class="mt-4 text-sm text-green-600 flex items-center">
          <ArrowUpIcon class="w-4 h-4 mr-1" />
          +{{ currentPeriodStats.users }} {{ periodLabel }}
        </div>
      </div>

      <div class="card p-6">
        <div class="flex items-center">
          <div class="p-3 bg-green-100 rounded-lg">
            <DocumentTextIcon class="w-6 h-6 text-green-600" />
          </div>
          <div class="ml-4">
            <p class="text-sm text-gray-500">Bài thi</p>
            <p class="text-2xl font-bold text-gray-900">{{ stats.totalQuizzes }}</p>
          </div>
        </div>
        <div class="mt-4 text-sm text-green-600 flex items-center">
          <ArrowUpIcon class="w-4 h-4 mr-1" />
          +{{ currentPeriodStats.quizzes }} {{ periodLabel }}
        </div>
      </div>

      <div class="card p-6">
        <div class="flex items-center">
          <div class="p-3 bg-blue-100 rounded-lg">
            <QuestionMarkCircleIcon class="w-6 h-6 text-blue-600" />
          </div>
          <div class="ml-4">
            <p class="text-sm text-gray-500">Câu hỏi</p>
            <p class="text-2xl font-bold text-gray-900">{{ stats.totalQuestions }}</p>
          </div>
        </div>
        <div class="mt-4 text-sm text-green-600 flex items-center">
          <ArrowUpIcon class="w-4 h-4 mr-1" />
          +{{ currentPeriodStats.questions }} {{ periodLabel }}
        </div>
      </div>

      <div class="card p-6">
        <div class="flex items-center">
          <div class="p-3 bg-purple-100 rounded-lg">
            <AcademicCapIcon class="w-6 h-6 text-purple-600" />
          </div>
          <div class="ml-4">
            <p class="text-sm text-gray-500">Lượt làm bài</p>
            <p class="text-2xl font-bold text-gray-900">{{ stats.totalAttempts }}</p>
          </div>
        </div>
        <div class="mt-4 text-sm text-green-600 flex items-center">
          <ArrowUpIcon class="w-4 h-4 mr-1" />
          +{{ currentPeriodStats.attempts }} {{ periodLabel }}
        </div>
      </div>
    </div>

    <!-- Charts -->
    <div class="grid grid-cols-1 gap-6 mb-8">
      <div class="card p-6">
        <div class="flex items-center justify-between mb-4">
          <h2 class="section-title">Thống kê hoạt động {{ periodLabel }}</h2>
        </div>
        <div class="h-72">
          <Bar v-if="chartData" :data="chartData" :options="chartOptions" />
          <div v-else class="h-full flex items-center justify-center bg-gray-50 rounded-lg">
            <div class="text-center text-gray-500">
              <ArrowPathIcon class="w-8 h-8 mx-auto mb-2 text-gray-300 animate-spin" />
              <p>Đang tải biểu đồ...</p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Quick Actions -->
    <div class="mb-8">
      <h2 class="section-title mb-4">Thao tác nhanh</h2>
      <div class="grid grid-cols-2 md:grid-cols-3 gap-4">
        <router-link to="/admin/users" class="card p-4 text-center hover:shadow-md transition-shadow">
          <UsersIcon class="w-8 h-8 mx-auto mb-2 text-primary-600" />
          <p class="font-medium text-gray-900">Quản lý người dùng</p>
        </router-link>
        <router-link to="/admin/content" class="card p-4 text-center hover:shadow-md transition-shadow">
          <DocumentTextIcon class="w-8 h-8 mx-auto mb-2 text-green-600" />
          <p class="font-medium text-gray-900">Quản lý nội dung</p>
        </router-link>
        <router-link to="/admin/categories" class="card p-4 text-center hover:shadow-md transition-shadow">
          <FolderIcon class="w-8 h-8 mx-auto mb-2 text-blue-600" />
          <p class="font-medium text-gray-900">Danh mục</p>
        </router-link>
      </div>
    </div>

    <!-- Recent Activity & Top Quizzes -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <!-- Recent Users -->
      <div class="card p-6">
        <div class="flex items-center justify-between mb-4">
          <h2 class="section-title">Người dùng mới</h2>
          <router-link to="/admin/users" class="text-primary-600 text-sm hover:underline">
            Xem tất cả →
          </router-link>
        </div>
        
        <div class="space-y-4">
          <div v-for="user in recentUsers" :key="user.id" class="flex items-center gap-3">
            <div class="w-10 h-10 rounded-full bg-gradient-to-br from-primary-500 to-secondary-500 flex items-center justify-center text-white font-medium">
              {{ user.hoTen?.charAt(0) || 'U' }}
            </div>
            <div class="flex-1 min-w-0">
              <p class="font-medium text-gray-900 truncate">{{ user.hoTen }}</p>
              <p class="text-sm text-gray-500 truncate">{{ user.email }}</p>
            </div>
            <span class="text-xs text-gray-400">{{ formatDate(user.ngayTao) }}</span>
          </div>
          <div v-if="recentUsers.length === 0" class="text-center text-gray-500 py-4">
            Chưa có người dùng mới
          </div>
        </div>
      </div>

      <!-- Top Quizzes -->
      <div class="card p-6">
        <div class="flex items-center justify-between mb-4">
          <h2 class="section-title">Bài thi phổ biến</h2>
          <router-link to="/admin/content" class="text-primary-600 text-sm hover:underline">
            Xem tất cả →
          </router-link>
        </div>
        
        <div class="space-y-4">
          <div v-for="(quiz, index) in topQuizzes" :key="quiz.maBaiThi" class="flex items-center gap-3">
            <span :class="[
              'w-8 h-8 rounded-lg flex items-center justify-center font-bold text-sm',
              index === 0 ? 'bg-yellow-100 text-yellow-600' :
              index === 1 ? 'bg-gray-100 text-gray-600' :
              index === 2 ? 'bg-orange-100 text-orange-600' :
              'bg-gray-50 text-gray-500'
            ]">
              {{ index + 1 }}
            </span>
            <div class="flex-1 min-w-0">
              <p class="font-medium text-gray-900 truncate">{{ quiz.tieuDe }}</p>
              <p class="text-sm text-gray-500">{{ quiz.tenNguoiTao }}</p>
            </div>
            <span class="text-sm text-gray-500">{{ quiz.tongLuotLamBai }} lượt</span>
          </div>
          <div v-if="topQuizzes.length === 0" class="text-center text-gray-500 py-4">
            Chưa có bài thi nào
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import {
  UsersIcon,
  DocumentTextIcon,
  QuestionMarkCircleIcon,
  AcademicCapIcon,
  ArrowUpIcon,
  FolderIcon,
  ArrowPathIcon
} from '@heroicons/vue/24/outline'
import { Bar } from 'vue-chartjs'
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend
} from 'chart.js'
import { adminService, reportService } from '@/services'

// Register Chart.js components
ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend)

const selectedPeriod = ref('today') // 'today', 'week' or 'month'

const stats = reactive({
  totalUsers: 0,
  totalQuizzes: 0,
  totalQuestions: 0,
  totalAttempts: 0,
  newUsersToday: 0,
  newQuizzesToday: 0,
  newQuestionsToday: 0,
  newAttemptsToday: 0,
  newUsersThisWeek: 0,
  newQuizzesThisWeek: 0,
  newQuestionsThisWeek: 0,
  newAttemptsThisWeek: 0,
  newUsersThisMonth: 0,
  newQuizzesThisMonth: 0,
  newQuestionsThisMonth: 0,
  newAttemptsThisMonth: 0
})

const recentUsers = ref([])
const topQuizzes = ref([])
const dataLoaded = ref(false)

// Computed for period label
const periodLabel = computed(() => {
  switch (selectedPeriod.value) {
    case 'today': return 'hôm nay'
    case 'week': return 'tuần này'
    case 'month': return 'tháng này'
    default: return 'tuần này'
  }
})

// Computed for current period stats based on selection
const currentPeriodStats = computed(() => {
  switch (selectedPeriod.value) {
    case 'today':
      return {
        users: stats.newUsersToday,
        quizzes: stats.newQuizzesToday,
        questions: stats.newQuestionsToday,
        attempts: stats.newAttemptsToday
      }
    case 'week':
      return {
        users: stats.newUsersThisWeek,
        quizzes: stats.newQuizzesThisWeek,
        questions: stats.newQuestionsThisWeek,
        attempts: stats.newAttemptsThisWeek
      }
    case 'month':
      return {
        users: stats.newUsersThisMonth,
        quizzes: stats.newQuizzesThisMonth,
        questions: stats.newQuestionsThisMonth,
        attempts: stats.newAttemptsThisMonth
      }
    default:
      return { users: 0, quizzes: 0, questions: 0, attempts: 0 }
  }
})

// Chart data computed from stats based on selected period
const chartData = computed(() => {
  if (!dataLoaded.value) return null
  const periodStats = currentPeriodStats.value
  
  // Different colors for different periods
  const colors = {
    today: ['#f59e0b', '#ef4444', '#06b6d4', '#ec4899'],
    week: ['#6366f1', '#22c55e', '#3b82f6', '#a855f7'],
    month: ['#8b5cf6', '#10b981', '#0ea5e9', '#d946ef']
  }
  
  return {
    labels: ['Người dùng mới', 'Bài thi mới', 'Câu hỏi mới', 'Lượt làm bài'],
    datasets: [
      {
        label: periodLabel.value.charAt(0).toUpperCase() + periodLabel.value.slice(1),
        backgroundColor: colors[selectedPeriod.value] || colors.week,
        data: [
          periodStats.users,
          periodStats.quizzes,
          periodStats.questions,
          periodStats.attempts
        ],
        borderRadius: 8
      }
    ]
  }
})

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      display: false
    },
    tooltip: {
      backgroundColor: '#1f2937',
      titleColor: '#fff',
      bodyColor: '#fff',
      padding: 12,
      cornerRadius: 8
    }
  },
  scales: {
    y: {
      beginAtZero: true,
      ticks: {
        stepSize: 1
      },
      grid: {
        color: '#f3f4f6'
      }
    },
    x: {
      grid: {
        display: false
      }
    }
  }
}

const formatDate = (dateString) => {
  if (!dateString) return ''
  return new Date(dateString).toLocaleDateString('vi-VN')
}

const loadDashboardData = async () => {
  try {
    const [statsRes, usersRes, quizzesRes] = await Promise.all([
      reportService.getAdminStats(),
      adminService.getRecentUsers({ limit: 5 }),
      reportService.getTopQuizzes({ limit: 5 })
    ])

    if (statsRes.success && statsRes.data) {
      // Map backend field names to frontend field names
      stats.totalUsers = statsRes.data.tongNguoiDung || 0
      stats.totalQuizzes = statsRes.data.tongBaiThi || 0
      stats.totalQuestions = statsRes.data.tongCauHoi || 0
      stats.totalAttempts = statsRes.data.tongLuotLamBai || 0
      // Today stats
      stats.newUsersToday = statsRes.data.nguoiDungMoiHomNay || 0
      stats.newQuizzesToday = statsRes.data.baiThiMoiHomNay || 0
      stats.newQuestionsToday = statsRes.data.cauHoiMoiHomNay || 0
      stats.newAttemptsToday = statsRes.data.luotLamBaiHomNay || 0
      // Week stats
      stats.newUsersThisWeek = statsRes.data.nguoiDungMoi7Ngay || 0
      stats.newQuizzesThisWeek = statsRes.data.baiThiMoi7Ngay || 0
      stats.newQuestionsThisWeek = statsRes.data.cauHoiMoi7Ngay || 0
      stats.newAttemptsThisWeek = statsRes.data.luotLamBai7Ngay || 0
      // Month stats
      stats.newUsersThisMonth = statsRes.data.nguoiDungMoi30Ngay || 0
      stats.newQuizzesThisMonth = statsRes.data.baiThiMoi30Ngay || 0
      stats.newQuestionsThisMonth = statsRes.data.cauHoiMoi30Ngay || 0
      stats.newAttemptsThisMonth = statsRes.data.luotLamBai30Ngay || 0
    }

    if (usersRes.success) {
      recentUsers.value = usersRes.data || []
    }

    if (quizzesRes.success) {
      topQuizzes.value = quizzesRes.data || []
    }
    
    dataLoaded.value = true
  } catch (error) {
    console.error('Failed to load dashboard data:', error)
    dataLoaded.value = true
  }
}

onMounted(() => {
  loadDashboardData()
})
</script>
