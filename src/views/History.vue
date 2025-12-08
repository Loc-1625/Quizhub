<template>
  <div class="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <div class="mb-8">
      <h1 class="page-header">
        <ClockIcon class="w-8 h-8 inline-block mr-2 text-primary-600" />
        Lịch sử làm bài
      </h1>
      <p class="text-gray-600 mt-1">Xem lại các bài thi bạn đã làm</p>
    </div>

    <!-- Filter -->
    <div class="card p-4 mb-6">
      <div class="flex flex-wrap gap-4">
        <div class="flex-1 min-w-[200px]">
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Tìm kiếm bài thi..."
            class="input-field"
            @input="debouncedSearch"
          />
        </div>
        
        <select v-model="selectedSort" @change="loadHistory" class="input-field w-auto">
          <option value="newest">Mới nhất</option>
          <option value="oldest">Cũ nhất</option>
          <option value="highest">Điểm cao nhất</option>
          <option value="lowest">Điểm thấp nhất</option>
        </select>
      </div>
    </div>

    <!-- Stats Summary -->
    <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
      <div class="card p-4 text-center">
        <div class="text-2xl font-bold text-primary-600">{{ stats.tongLuotLam || 0 }}</div>
        <div class="text-sm text-gray-600">Tổng lượt làm</div>
      </div>
      <div class="card p-4 text-center">
        <div class="text-2xl font-bold text-green-600">{{ stats.diemTrungBinh?.toFixed(1) || '0' }}</div>
        <div class="text-sm text-gray-600">Điểm TB</div>
      </div>
      <div class="card p-4 text-center">
        <div class="text-2xl font-bold text-blue-600">{{ stats.diemCaoNhat?.toFixed(1) || '0' }}</div>
        <div class="text-sm text-gray-600">Điểm cao nhất</div>
      </div>
      <div class="card p-4 text-center">
        <div class="text-2xl font-bold text-yellow-600">{{ stats.tyLeDat || 0 }}%</div>
        <div class="text-sm text-gray-600">Tỷ lệ đạt</div>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="card p-8 text-center">
      <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600 mx-auto"></div>
      <p class="text-gray-500 mt-4">Đang tải...</p>
    </div>

    <!-- Empty -->
    <div v-else-if="history.length === 0" class="card p-8 text-center">
      <ClockIcon class="w-16 h-16 text-gray-300 mx-auto mb-4" />
      <h3 class="text-lg font-medium text-gray-900 mb-2">Chưa có lịch sử làm bài</h3>
      <p class="text-gray-500 mb-4">Bắt đầu làm bài thi để xem lịch sử của bạn tại đây</p>
      <router-link to="/explore" class="btn-primary">
        Khám phá bài thi
      </router-link>
    </div>

    <!-- History List -->
    <div v-else class="space-y-4">
      <div
        v-for="item in history"
        :key="item.id"
        class="card p-4 hover:shadow-md transition-shadow"
      >
        <div class="flex items-start gap-4">
          <!-- Score Circle -->
          <div
            class="flex-shrink-0 w-16 h-16 rounded-full flex items-center justify-center text-white font-bold text-lg"
            :class="getScoreClass(item.diem)"
          >
            {{ item.diem?.toFixed(0) || 0 }}
          </div>

          <!-- Info -->
          <div class="flex-1 min-w-0">
            <div class="flex items-start justify-between">
              <div>
                <h3 class="font-semibold text-gray-900">{{ item.baiThi?.tieuDe || 'Bài thi không xác định' }}</h3>
                <div class="flex items-center gap-4 mt-1 text-sm text-gray-500">
                  <span>
                    <ClockIcon class="w-4 h-4 inline-block mr-1" />
                    {{ formatDate(item.ngayLamBai) }}
                  </span>
                  <span>
                    {{ item.soCauDung || 0 }}/{{ item.tongSoCau || 0 }} câu đúng
                  </span>
                  <span>
                    {{ formatDuration(item.thoiGianLam) }}
                  </span>
                </div>
              </div>
              
              <div class="flex items-center gap-2">
                <span
                  v-if="item.dat"
                  class="px-2 py-1 text-xs font-medium rounded-full bg-green-100 text-green-700"
                >
                  Đạt
                </span>
                <span
                  v-else
                  class="px-2 py-1 text-xs font-medium rounded-full bg-red-100 text-red-700"
                >
                  Chưa đạt
                </span>
              </div>
            </div>

            <!-- Progress Bar -->
            <div class="mt-3">
              <div class="flex items-center justify-between text-sm mb-1">
                <span class="text-gray-600">Độ chính xác</span>
                <span class="font-medium">{{ getAccuracy(item) }}%</span>
              </div>
              <div class="w-full bg-gray-200 rounded-full h-2">
                <div
                  class="h-2 rounded-full transition-all"
                  :class="getProgressClass(item)"
                  :style="{ width: `${getAccuracy(item)}%` }"
                ></div>
              </div>
            </div>

            <!-- Actions -->
            <div class="mt-4 flex items-center gap-2">
              <router-link
                :to="{ name: 'quiz-result', params: { id: item.baiThiId, luotLamBaiId: item.id }}"
                class="btn-outline text-sm py-1.5"
              >
                <EyeIcon class="w-4 h-4 mr-1" />
                Xem chi tiết
              </router-link>
              <router-link
                v-if="item.baiThi"
                :to="{ name: 'quiz-take', params: { id: item.baiThiId }}"
                class="text-sm text-primary-600 hover:text-primary-700"
              >
                Làm lại
              </router-link>
            </div>
          </div>
        </div>
      </div>

      <!-- Pagination -->
      <div v-if="totalPages > 1" class="flex justify-center gap-2 mt-6">
        <button
          @click="goToPage(currentPage - 1)"
          :disabled="currentPage === 1"
          class="px-3 py-2 border rounded-lg disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50"
        >
          <ChevronLeftIcon class="w-5 h-5" />
        </button>
        
        <template v-for="page in visiblePages" :key="page">
          <button
            v-if="page !== '...'"
            @click="goToPage(page)"
            class="px-4 py-2 border rounded-lg transition-colors"
            :class="page === currentPage 
              ? 'bg-primary-600 text-white border-primary-600' 
              : 'hover:bg-gray-50'"
          >
            {{ page }}
          </button>
          <span v-else class="px-2 py-2">...</span>
        </template>

        <button
          @click="goToPage(currentPage + 1)"
          :disabled="currentPage === totalPages"
          class="px-3 py-2 border rounded-lg disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50"
        >
          <ChevronRightIcon class="w-5 h-5" />
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import {
  ClockIcon,
  EyeIcon,
  ChevronLeftIcon,
  ChevronRightIcon
} from '@heroicons/vue/24/outline'
import { attemptService } from '@/services'
import { useToast } from 'vue-toastification'

const toast = useToast()

const loading = ref(false)
const history = ref([])
const stats = ref({})
const searchQuery = ref('')
const selectedSort = ref('newest')
const currentPage = ref(1)
const totalPages = ref(1)
const pageSize = 10

let searchTimeout = null

onMounted(() => {
  loadHistory()
  loadStats()
})

async function loadHistory() {
  loading.value = true
  try {
    const params = {
      pageNumber: currentPage.value,
      pageSize,
      search: searchQuery.value || undefined
    }
    
    // Sort mapping
    switch (selectedSort.value) {
      case 'oldest':
        params.sortBy = 'ngayLamBai'
        params.sortDesc = false
        break
      case 'highest':
        params.sortBy = 'diem'
        params.sortDesc = true
        break
      case 'lowest':
        params.sortBy = 'diem'
        params.sortDesc = false
        break
      default:
        params.sortBy = 'ngayLamBai'
        params.sortDesc = true
    }
    
    const response = await attemptService.getMyAttempts(params)
    // Map API response to expected format
    const mappedData = (response.data || []).map(item => ({
      id: item.maLuotLamBai,
      baiThiId: item.maBaiThi,
      baiThi: { tieuDe: item.tieuDeBaiThi },
      ngayLamBai: item.thoiGianBatDau,
      diem: item.diem,
      soCauDung: item.soCauDung,
      tongSoCau: item.tongSoCauHoi,
      thoiGianLam: item.thoiGianLamBaiThucTe,
      dat: item.diem >= 5, // Assuming 5 is passing score
      choPhepXemLai: item.choPhepXemLai
    }))
    history.value = mappedData
    totalPages.value = response.pagination?.totalPages || 1
  } catch (err) {
    console.error('Load history error:', err)
    toast.error('Không thể tải lịch sử làm bài')
  } finally {
    loading.value = false
  }
}

async function loadStats() {
  try {
    const response = await attemptService.getMyStats()
    stats.value = response.data || {}
  } catch (err) {
    console.error('Load stats error:', err)
  }
}

function debouncedSearch() {
  if (searchTimeout) clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    currentPage.value = 1
    loadHistory()
  }, 300)
}

function goToPage(page) {
  if (page < 1 || page > totalPages.value) return
  currentPage.value = page
  loadHistory()
}

const visiblePages = computed(() => {
  const pages = []
  const total = totalPages.value
  const current = currentPage.value
  
  if (total <= 7) {
    for (let i = 1; i <= total; i++) pages.push(i)
  } else {
    pages.push(1)
    
    if (current > 3) pages.push('...')
    
    for (let i = Math.max(2, current - 1); i <= Math.min(total - 1, current + 1); i++) {
      pages.push(i)
    }
    
    if (current < total - 2) pages.push('...')
    
    pages.push(total)
  }
  
  return pages
})

function formatDate(date) {
  if (!date) return ''
  // Backend trả về UTC, thêm 'Z' nếu chưa có
  const utcStr = date.endsWith('Z') ? date : date + 'Z'
  return new Date(utcStr).toLocaleString('vi-VN', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

function formatDuration(seconds) {
  if (!seconds) return '0 phút'
  const mins = Math.floor(seconds / 60)
  const secs = seconds % 60
  if (mins === 0) return `${secs} giây`
  return `${mins} phút ${secs > 0 ? secs + ' giây' : ''}`
}

function getAccuracy(item) {
  if (!item.tongSoCau) return 0
  return Math.round((item.soCauDung / item.tongSoCau) * 100)
}

function getScoreClass(score) {
  if (score >= 8) return 'bg-gradient-to-br from-emerald-500 to-teal-600'
  if (score >= 6) return 'bg-gradient-to-br from-primary-500 to-primary-600'
  if (score >= 4) return 'bg-gradient-to-br from-amber-500 to-yellow-600'
  return 'bg-gradient-to-br from-orange-500 to-red-500'
}

function getProgressClass(item) {
  const acc = getAccuracy(item)
  if (acc >= 80) return 'bg-green-500'
  if (acc >= 60) return 'bg-blue-500'
  if (acc >= 40) return 'bg-yellow-500'
  return 'bg-red-500'
}
</script>
