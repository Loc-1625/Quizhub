<template>
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <!-- Header -->
    <div class="flex items-center justify-between mb-8">
      <div>
        <div class="flex items-center gap-2 text-sm text-gray-500 mb-2">
          <router-link to="/my-quizzes" class="hover:text-primary-600">Quản lý bài thi</router-link>
          <ChevronRightIcon class="w-4 h-4" />
          <span>Người tham gia</span>
        </div>
        <h1 class="page-header">Danh sách người làm bài</h1>
        <p class="text-gray-600 mt-1">
          {{ quizTitle ? `Bài thi: ${quizTitle}` : 'Tổng hợp tất cả bài thi của bạn' }}
        </p>
      </div>
      <div class="flex gap-3">
        <router-link 
          v-if="quizId" 
          :to="{ name: 'quiz-participants-all' }" 
          class="btn-secondary"
        >
          <UsersIcon class="w-5 h-5 mr-2" />
          Xem tất cả
        </router-link>
        <button @click="exportToCSV" class="btn-secondary">
          <ArrowDownTrayIcon class="w-5 h-5 mr-2" />
          Xuất CSV
        </button>
      </div>
    </div>

    <!-- Statistics Cards -->
    <div class="grid grid-cols-2 gap-4 mb-6">
      <div class="card p-4">
        <div class="flex items-center">
          <div class="p-2 bg-blue-100 rounded-lg">
            <UsersIcon class="w-6 h-6 text-blue-600" />
          </div>
          <div class="ml-4">
            <p class="text-sm text-gray-500">Tổng lượt làm</p>
            <p class="text-xl font-semibold">{{ thongKe.tongSoNguoiLamBai || 0 }}</p>
          </div>
        </div>
      </div>
      <div class="card p-4">
        <div class="flex items-center">
          <div class="p-2 bg-green-100 rounded-lg">
            <CheckCircleIcon class="w-6 h-6 text-green-600" />
          </div>
          <div class="ml-4">
            <p class="text-sm text-gray-500">Hoàn thành</p>
            <p class="text-xl font-semibold">{{ thongKe.soNguoiHoanThanh || 0 }}</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Filters -->
    <div class="card p-4 mb-6">
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <!-- 1. Tìm kiếm theo tên/email -->
        <div class="relative">
          <MagnifyingGlassIcon class="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
          <input
            v-model="searchTerm"
            type="text"
            placeholder="Tìm theo tên, email..."
            class="input-field pl-10"
            @input="debouncedSearch"
          />
        </div>
        
        <!-- 2. Tìm kiếm theo tên bài thi -->
        <div class="relative">
          <MagnifyingGlassIcon class="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
          <input
            v-model="searchQuizName"
            type="text"
            placeholder="Tìm theo tên bài thi..."
            class="input-field pl-10"
            @input="debouncedSearch"
          />
        </div>
        
        <!-- 3. Dropdown trạng thái -->
        <select v-model="filter.trangThai" class="input-field" @change="loadParticipants">
          <option value="">Tất cả trạng thái</option>
          <option value="HoanThanh">Hoàn thành</option>
          <option value="TuDongNop">Tự động nộp</option>
          <option value="DangLam">Đang làm</option>
        </select>
        
        <!-- 4. Dropdown sắp xếp -->
        <div class="flex gap-2">
          <select v-model="filter.sortBy" class="input-field flex-1" @change="loadParticipants">
            <option value="ThoiGianBatDau">Thời gian</option>
            <option value="Diem">Điểm số</option>
          </select>
          <button 
            @click="toggleSortOrder" 
            class="btn-secondary px-3"
            :title="filter.sortOrder === 'DESC' ? 'Giảm dần' : 'Tăng dần'"
          >
            <ArrowsUpDownIcon class="w-5 h-5" />
          </button>
        </div>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="space-y-4">
      <div v-for="i in 5" :key="i" class="card p-4 animate-pulse">
        <div class="flex items-center gap-4">
          <div class="w-10 h-10 bg-gray-200 rounded-full"></div>
          <div class="flex-1 space-y-2">
            <div class="h-4 bg-gray-200 rounded w-1/4"></div>
            <div class="h-3 bg-gray-200 rounded w-1/3"></div>
          </div>
          <div class="h-8 w-16 bg-gray-200 rounded"></div>
        </div>
      </div>
    </div>

    <!-- Participants Table -->
    <div v-else-if="participants.length > 0" class="card overflow-hidden">
      <div class="overflow-x-auto">
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Người làm bài
              </th>
              <th v-if="!quizId" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Bài thi
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Thời gian
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Kết quả
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Trạng thái
              </th>
              <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                Chi tiết
              </th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr v-for="participant in participants" :key="participant.maLuotLamBai" class="hover:bg-gray-50">
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="flex items-center">
                  <div class="flex-shrink-0 h-10 w-10">
                    <img 
                      v-if="participant.anhDaiDien" 
                      :src="participant.anhDaiDien" 
                      :alt="participant.tenNguoiLamBai"
                      class="h-10 w-10 rounded-full object-cover"
                    />
                    <div v-else class="h-10 w-10 rounded-full bg-primary-100 flex items-center justify-center">
                      <span class="text-primary-600 font-medium text-sm">
                        {{ getInitials(participant.tenNguoiLamBai) }}
                      </span>
                    </div>
                  </div>
                  <div class="ml-4">
                    <div class="text-sm font-medium text-gray-900">
                      {{ participant.tenNguoiLamBai }}
                    </div>
                    <div v-if="participant.email" class="text-sm text-gray-500">
                      {{ participant.email }}
                    </div>
                    <div v-else class="text-sm text-gray-400 italic">
                      Khách
                    </div>
                  </div>
                </div>
              </td>
              <td v-if="!quizId" class="px-6 py-4">
                <div class="text-sm text-gray-900 max-w-xs truncate" :title="participant.tieuDeBaiThi">
                  {{ participant.tieuDeBaiThi }}
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="text-sm text-gray-900">{{ formatDate(participant.thoiGianBatDau) }}</div>
                <div class="text-sm text-gray-500">
                  {{ participant.thoiGianLamBaiThucTe ? formatDuration(participant.thoiGianLamBaiThucTe) : '-' }}
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <div v-if="participant.diem !== null" class="flex items-baseline gap-0.5">
                  <span :class="[
                    'text-lg font-semibold',
                    getScoreColorClass(participant.diem)
                  ]">
                    {{ participant.diem?.toFixed(1) }}
                  </span>
                  <span :class="[
                    'text-sm',
                    getScoreColorClass(participant.diem)
                  ]">/10</span>
                </div>
                <span v-else class="text-gray-400">-</span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span :class="getStatusClass(participant.trangThai)">
                  {{ getStatusText(participant.trangThai) }}
                </span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-right">
                <router-link 
                  v-if="participant.trangThai === 'HoanThanh' || participant.trangThai === 'TuDongNop'"
                  :to="{ name: 'quiz-result', params: { id: participant.maBaiThi, luotLamBaiId: participant.maLuotLamBai } }"
                  class="text-primary-600 hover:text-primary-900 text-sm font-medium"
                >
                  Xem chi tiết
                </router-link>
                <span v-else class="text-gray-400 text-sm">-</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="text-center py-16">
      <UsersIcon class="w-16 h-16 text-gray-300 mx-auto mb-4" />
      <h3 class="text-lg font-medium text-gray-900 mb-2">Chưa có ai làm bài</h3>
      <p class="text-gray-500">Chia sẻ bài thi để mọi người tham gia làm bài</p>
    </div>

    <!-- Pagination -->
    <div v-if="pagination.totalPages > 1" class="mt-6 flex justify-center">
      <nav class="flex items-center gap-2">
        <button
          @click="goToPage(pagination.currentPage - 1)"
          :disabled="pagination.currentPage === 1"
          :class="[
            'px-3 py-2 rounded border',
            pagination.currentPage === 1 
              ? 'bg-gray-100 text-gray-400 border-gray-200 cursor-not-allowed' 
              : 'btn-secondary'
          ]"
        >
          <ChevronLeftIcon class="w-5 h-5" />
        </button>
        <span class="px-4 py-2 text-gray-700">
          Trang {{ pagination.currentPage }} / {{ pagination.totalPages }}
        </span>
        <button
          @click="goToPage(pagination.currentPage + 1)"
          :disabled="pagination.currentPage === pagination.totalPages"
          :class="[
            'px-3 py-2 rounded border',
            pagination.currentPage === pagination.totalPages 
              ? 'bg-gray-100 text-gray-400 border-gray-200 cursor-not-allowed' 
              : 'btn-secondary'
          ]"
        >
          <ChevronRightIcon class="w-5 h-5" />
        </button>
        
        <!-- Page input -->
        <div class="flex items-center gap-1 ml-2">
          <input 
            type="number" 
            v-model.number="goToPageInput"
            @keyup.enter="goToPageNumber"
            min="1"
            :max="pagination.totalPages"
            placeholder="Trang"
            class="w-16 px-2 py-2 text-sm border border-gray-300 rounded focus:ring-primary-500 focus:border-primary-500"
          />
          <button @click="goToPageNumber" class="btn-secondary px-3 py-2 text-sm">Đi</button>
        </div>
      </nav>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useDebounceFn } from '@vueuse/core'
import { useToast } from 'vue-toastification'
import {
  UsersIcon,
  CheckCircleIcon,
  MagnifyingGlassIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  ArrowDownTrayIcon,
  ArrowsUpDownIcon
} from '@heroicons/vue/24/outline'
import { ChevronRightIcon as ChevronRightSolidIcon } from '@heroicons/vue/20/solid'
import { quizService } from '@/services'

const route = useRoute()
const toast = useToast()

const quizId = computed(() => route.params.id || null)
const quizTitle = ref('')

const loading = ref(true)
const participants = ref([])
const myQuizzes = ref([])
const searchTerm = ref('')
const searchQuizName = ref('')

const thongKe = ref({
  tongSoNguoiLamBai: 0,
  soNguoiHoanThanh: 0,
  soNguoiDangLam: 0,
  soNguoiDat: 0,
  soNguoiKhongDat: 0,
  diemTrungBinh: null,
  diemCaoNhat: null,
  diemThapNhat: null
})

const filter = reactive({
  maBaiThi: null,
  trangThai: '',
  sortBy: 'ThoiGianBatDau',
  sortOrder: 'DESC',
  pageNumber: 1,
  pageSize: 20
})

const pagination = reactive({
  currentPage: 1,
  totalPages: 1,
  totalCount: 0
})

const loadParticipants = async () => {
  loading.value = true
  try {
    let response
    const params = {
      timKiem: searchTerm.value || undefined,
      timKiemBaiThi: searchQuizName.value || undefined,
      trangThai: filter.trangThai || undefined,
      sortBy: filter.sortBy,
      sortOrder: filter.sortOrder,
      pageNumber: filter.pageNumber,
      pageSize: filter.pageSize
    }

    if (quizId.value) {
      // Load participants for specific quiz
      response = await quizService.getParticipants(quizId.value, params)
      
      // Get quiz title
      if (!quizTitle.value) {
        const quizResponse = await quizService.getQuizById(quizId.value)
        if (quizResponse.success) {
          quizTitle.value = quizResponse.data.tieuDe
        }
      }
    } else {
      // Load all participants across all quizzes
      if (filter.maBaiThi) {
        params.maBaiThi = filter.maBaiThi
      }
      response = await quizService.getAllParticipants(params)
    }

    if (response.success) {
      participants.value = response.data || []
      thongKe.value = response.thongKe || {}
      pagination.currentPage = response.pagination?.currentPage || 1
      pagination.totalPages = response.pagination?.totalPages || 1
      pagination.totalCount = response.pagination?.totalCount || 0
    }
  } catch (error) {
    console.error('Failed to load participants:', error)
    toast.error('Không thể tải danh sách người làm bài')
  } finally {
    loading.value = false
  }
}

const loadMyQuizzes = async () => {
  if (quizId.value) return // Only load when viewing all participants
  
  try {
    const response = await quizService.getQuizzes({
      chiLayCuaToi: true,
      pageSize: 100
    })
    if (response.success) {
      myQuizzes.value = response.data || []
    }
  } catch (error) {
    console.error('Failed to load quizzes:', error)
  }
}

const debouncedSearch = useDebounceFn(() => {
  filter.pageNumber = 1
  loadParticipants()
}, 300)

const goToPage = (page) => {
  if (page < 1 || page > pagination.totalPages) return
  filter.pageNumber = page
  loadParticipants()
}

const goToPageInput = ref(1)

const goToPageNumber = () => {
  const page = parseInt(goToPageInput.value)
  if (page >= 1 && page <= pagination.totalPages) {
    goToPage(page)
  } else {
    toast.warning(`Vui lòng nhập số trang từ 1 đến ${pagination.totalPages}`)
    goToPageInput.value = pagination.currentPage
  }
}

const toggleSortOrder = () => {
  filter.sortOrder = filter.sortOrder === 'DESC' ? 'ASC' : 'DESC'
  loadParticipants()
}

const getInitials = (name) => {
  if (!name) return '?'
  return name.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase()
}

const formatDate = (dateStr) => {
  if (!dateStr) return ''
  // Backend trả về UTC, thêm 'Z' nếu chưa có để JavaScript hiểu đúng là UTC
  const utcStr = dateStr.endsWith('Z') ? dateStr : dateStr + 'Z'
  const date = new Date(utcStr)
  return date.toLocaleString('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const formatDuration = (seconds) => {
  if (!seconds) return '-'
  const mins = Math.floor(seconds / 60)
  const secs = seconds % 60
  return `${mins}:${secs.toString().padStart(2, '0')}`
}

const getStatusClass = (status) => {
  const classes = {
    'HoanThanh': 'badge badge-success',
    'TuDongNop': 'badge badge-info',
    'DangLam': 'badge badge-warning',
    'HetGio': 'badge badge-danger',
    'DaHuy': 'badge badge-secondary'
  }
  return classes[status] || 'badge'
}

const getScoreColorClass = (score) => {
  if (score < 5) return 'text-red-600'
  if (score < 8) return 'text-orange-500'
  return 'text-green-600'
}

const getStatusText = (status) => {
  const texts = {
    'HoanThanh': 'Hoàn thành',
    'TuDongNop': 'Tự động nộp',
    'DangLam': 'Đang làm',
    'HetGio': 'Hết giờ',
    'DaHuy': 'Đã hủy'
  }
  return texts[status] || status
}

const exportToCSV = () => {
  if (participants.value.length === 0) {
    toast.warning('Không có dữ liệu để xuất')
    return
  }

  const headers = ['Tên', 'Email', 'Bài thi', 'Thời gian bắt đầu', 'Thời gian làm bài', 'Điểm', 'Số câu đúng', 'Tổng câu hỏi', 'Trạng thái', 'Đạt']
  const rows = participants.value.map(p => [
    p.tenNguoiLamBai,
    p.email || 'Khách',
    p.tieuDeBaiThi,
    formatDate(p.thoiGianBatDau),
    p.thoiGianLamBaiThucTe ? formatDuration(p.thoiGianLamBaiThucTe) : '-',
    p.diem?.toFixed(1) || '-',
    p.soCauDung,
    p.tongSoCauHoi,
    getStatusText(p.trangThai),
    p.daDat ? 'Đạt' : 'Không đạt'
  ])

  const csvContent = [headers, ...rows]
    .map(row => row.map(cell => `"${cell}"`).join(','))
    .join('\n')

  const blob = new Blob(['\ufeff' + csvContent], { type: 'text/csv;charset=utf-8;' })
  const link = document.createElement('a')
  link.href = URL.createObjectURL(blob)
  link.download = `participants_${new Date().toISOString().split('T')[0]}.csv`
  link.click()
}

// Watch for route changes
watch(() => route.params.id, () => {
  quizTitle.value = ''
  filter.pageNumber = 1
  filter.maBaiThi = null
  loadParticipants()
  loadMyQuizzes()
})

onMounted(() => {
  loadParticipants()
  loadMyQuizzes()
})
</script>
