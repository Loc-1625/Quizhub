<template>
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <div class="mb-8">
      <h1 class="page-header">Lịch sử làm bài</h1>
      <p class="text-gray-600 mt-1">Xem lại các bài thi bạn đã tham gia</p>
    </div>

    <!-- Filters -->
    <div class="card p-4 mb-6">
      <div class="flex flex-col md:flex-row gap-4">
        <div class="flex-1 relative">
          <MagnifyingGlassIcon class="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
          <input
            v-model="searchTerm"
            type="text"
            placeholder="Tìm kiếm bài thi..."
            class="input-field pl-10"
            @input="debouncedSearch"
          />
        </div>
        <select v-model="filter.trangThai" class="input-field md:w-40" @change="loadHistory">
          <option value="">Tất cả</option>
          <option value="Dat">Đạt</option>
          <option value="KhongDat">Không đạt</option>
        </select>
        <select v-model="filter.sortBy" class="input-field md:w-40" @change="loadHistory">
          <option value="ThoiGianBatDau">Ngày làm</option>
          <option value="DiemSo">Điểm số</option>
        </select>
      </div>
    </div>

    <!-- Stats Summary -->
    <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
      <div class="card p-4 text-center">
        <p class="text-2xl font-bold text-primary-600">{{ stats.totalAttempts }}</p>
        <p class="text-sm text-gray-500">Lượt làm bài</p>
      </div>
      <div class="card p-4 text-center">
        <p class="text-2xl font-bold text-green-600">{{ stats.passed }}</p>
        <p class="text-sm text-gray-500">Lần đạt</p>
      </div>
      <div class="card p-4 text-center">
        <p class="text-2xl font-bold text-blue-600">{{ stats.avgScore }}%</p>
        <p class="text-sm text-gray-500">Điểm trung bình</p>
      </div>
      <div class="card p-4 text-center">
        <p class="text-2xl font-bold text-purple-600">{{ formatDuration(stats.avgTime) }}</p>
        <p class="text-sm text-gray-500">Thời gian TB</p>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="space-y-4">
      <div v-for="i in 5" :key="i" class="card p-4 animate-pulse">
        <div class="flex items-center gap-4">
          <div class="w-16 h-16 bg-gray-200 rounded-lg"></div>
          <div class="flex-1 space-y-2">
            <div class="h-4 bg-gray-200 rounded w-3/4"></div>
            <div class="h-3 bg-gray-200 rounded w-1/2"></div>
          </div>
        </div>
      </div>
    </div>

    <!-- History List -->
    <div v-else-if="attempts.length > 0" class="space-y-4">
      <div v-for="attempt in attempts" :key="attempt.maLuotLamBai" class="card p-4 hover:shadow-md transition-shadow">
        <div class="flex items-center gap-4">
          <!-- Score Circle -->
          <div :class="[
            'w-16 h-16 rounded-xl flex items-center justify-center shrink-0',
            attempt.trangThai === 'Dat' ? 'bg-green-100' : 'bg-red-100'
          ]">
            <span :class="[
              'text-2xl font-bold',
              attempt.trangThai === 'Dat' ? 'text-green-600' : 'text-red-600'
            ]">
              {{ Math.round(attempt.diemSo || 0) }}
            </span>
          </div>

          <!-- Quiz Info -->
          <div class="flex-1 min-w-0">
            <div class="flex items-start justify-between">
              <div>
                <h3 class="font-semibold text-gray-900 line-clamp-1">{{ attempt.tieuDeBaiThi }}</h3>
                <p class="text-sm text-gray-500 mt-1">
                  {{ formatDateTime(attempt.thoiGianBatDau) }}
                </p>
              </div>
              <span :class="[
                'badge shrink-0 ml-2',
                attempt.trangThai === 'Dat' ? 'badge-success' : 'badge-error'
              ]">
                {{ attempt.trangThai === 'Dat' ? 'Đạt' : 'Chưa đạt' }}
              </span>
            </div>

            <div class="flex items-center gap-4 mt-2 text-sm text-gray-500">
              <span class="flex items-center">
                <CheckCircleIcon class="w-4 h-4 mr-1 text-green-500" />
                {{ attempt.soCauDung }}/{{ attempt.tongSoCau }} câu đúng
              </span>
              <span class="flex items-center">
                <ClockIcon class="w-4 h-4 mr-1" />
                {{ formatDuration(attempt.thoiGianLamBai) }}
              </span>
            </div>
          </div>

          <!-- Actions -->
          <div class="flex gap-2 shrink-0">
            <router-link 
              :to="{ name: 'quiz-result', params: { id: attempt.maLuotLamBai } }"
              class="btn-secondary text-sm"
            >
              <EyeIcon class="w-4 h-4 mr-1" />
              Xem chi tiết
            </router-link>
            <router-link 
              v-if="attempt.choPhepLamLai"
              :to="{ name: 'quiz-detail', params: { id: attempt.maBaiThi } }"
              class="btn-primary text-sm"
            >
              <ArrowPathIcon class="w-4 h-4 mr-1" />
              Làm lại
            </router-link>
          </div>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="text-center py-16">
      <ClipboardDocumentListIcon class="w-16 h-16 text-gray-300 mx-auto mb-4" />
      <h3 class="text-lg font-medium text-gray-900 mb-2">Chưa có lịch sử</h3>
      <p class="text-gray-500 mb-6">Bạn chưa làm bài thi nào</p>
      <router-link to="/explore" class="btn-primary">
        Khám phá bài thi
      </router-link>
    </div>

    <!-- Pagination -->
    <div v-if="pagination.totalPages > 1" class="mt-8 flex justify-center">
      <nav class="flex items-center gap-2">
        <button
          @click="goToPage(pagination.currentPage - 1)"
          :disabled="pagination.currentPage === 1"
          class="btn-secondary px-3 py-2"
        >
          <ChevronLeftIcon class="w-5 h-5" />
        </button>
        <span class="px-4 py-2 text-gray-700">
          Trang {{ pagination.currentPage }} / {{ pagination.totalPages }}
        </span>
        <button
          @click="goToPage(pagination.currentPage + 1)"
          :disabled="pagination.currentPage === pagination.totalPages"
          class="btn-secondary px-3 py-2"
        >
          <ChevronRightIcon class="w-5 h-5" />
        </button>
      </nav>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useDebounceFn } from '@vueuse/core'
import { useToast } from 'vue-toastification'
import {
  MagnifyingGlassIcon,
  CheckCircleIcon,
  ClockIcon,
  EyeIcon,
  ArrowPathIcon,
  ClipboardDocumentListIcon,
  ChevronLeftIcon,
  ChevronRightIcon
} from '@heroicons/vue/24/outline'
import { attemptService, reportService } from '@/services'

const toast = useToast()

const loading = ref(true)
const attempts = ref([])
const searchTerm = ref('')

const filter = reactive({
  trangThai: '',
  sortBy: 'ThoiGianBatDau',
  sortOrder: 'DESC',
  pageNumber: 1,
  pageSize: 10
})

const pagination = reactive({
  currentPage: 1,
  totalPages: 1,
  totalCount: 0
})

const stats = reactive({
  totalAttempts: 0,
  passed: 0,
  avgScore: 0,
  avgTime: 0
})

const formatDateTime = (dateString) => {
  if (!dateString) return ''
  // Backend trả về UTC, thêm 'Z' nếu chưa có
  const utcStr = dateString.endsWith('Z') ? dateString : dateString + 'Z'
  return new Date(utcStr).toLocaleString('vi-VN')
}

const formatDuration = (seconds) => {
  if (!seconds) return '0:00'
  const mins = Math.floor(seconds / 60)
  const secs = seconds % 60
  return `${mins}:${secs.toString().padStart(2, '0')}`
}

const loadHistory = async () => {
  loading.value = true
  try {
    const response = await attemptService.getMyAttempts({
      ...filter,
      searchTerm: searchTerm.value
    })
    attempts.value = response.data || []
    pagination.currentPage = response.pagination?.currentPage || 1
    pagination.totalPages = response.pagination?.totalPages || 1
    pagination.totalCount = response.pagination?.totalCount || 0
  } catch (error) {
    console.error('Failed to load history:', error)
    toast.error('Không thể tải lịch sử')
  } finally {
    loading.value = false
  }
}

const loadStats = async () => {
  try {
    const response = await reportService.getMyStats()
    if (response.success) {
      Object.assign(stats, response.data)
    }
  } catch (error) {
    console.error('Failed to load stats:', error)
  }
}

const debouncedSearch = useDebounceFn(() => {
  filter.pageNumber = 1
  loadHistory()
}, 300)

const goToPage = (page) => {
  if (page < 1 || page > pagination.totalPages) return
  filter.pageNumber = page
  loadHistory()
}

onMounted(() => {
  loadHistory()
  loadStats()
})
</script>
