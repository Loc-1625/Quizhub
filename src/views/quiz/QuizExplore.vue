<template>
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <!-- Header -->
    <div class="mb-8">
      <h1 class="page-header">Khám phá bài thi</h1>
      <p class="text-gray-600 mt-1">Tìm kiếm và tham gia các bài thi công khai</p>
    </div>

    <!-- Search & Filters -->
    <div class="card p-4 mb-8">
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
        
        <select v-model="filter.maDanhMuc" class="input-field md:w-48" @change="loadQuizzes">
          <option value="">Tất cả danh mục</option>
          <option v-for="cat in categories" :key="cat.maDanhMuc" :value="cat.maDanhMuc">
            {{ cat.tenDanhMuc }}
          </option>
        </select>

        <select v-model="filter.sortBy" class="input-field md:w-48" @change="loadQuizzes">
          <option value="NgayTao">Mới nhất</option>
          <option value="TongLuotLamBai">Phổ biến nhất</option>
          <option value="XepHangTrungBinh">Đánh giá cao</option>
        </select>
      </div>
    </div>

    <!-- Join by Code -->
    <div class="card p-6 mb-8 bg-gradient-to-r from-primary-50 to-secondary-50 border-primary-200">
      <div class="flex flex-col md:flex-row items-center justify-between gap-4">
        <div>
          <h3 class="font-semibold text-gray-900">Có mã bài thi?</h3>
          <p class="text-sm text-gray-600">Nhập mã để tham gia bài thi riêng tư</p>
        </div>
        <div class="flex w-full md:w-auto gap-2">
          <input
            v-model="joinCode"
            type="text"
            placeholder="Nhập mã bài thi"
            class="input-field md:w-48"
            @keyup.enter="joinQuiz"
          />
          <button @click="joinQuiz" class="btn-primary whitespace-nowrap">
            Tham gia
          </button>
        </div>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
      <div v-for="i in 8" :key="i" class="card animate-pulse">
        <div class="h-32 bg-gray-200"></div>
        <div class="p-4 space-y-3">
          <div class="h-4 bg-gray-200 rounded w-3/4"></div>
          <div class="h-3 bg-gray-200 rounded w-1/2"></div>
        </div>
      </div>
    </div>

    <!-- Quiz Grid -->
    <div v-else-if="quizzes.length > 0" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
      <QuizCard v-for="quiz in quizzes" :key="quiz.maBaiThi" :quiz="quiz" />
    </div>

    <!-- Empty State -->
    <div v-else class="text-center py-12">
      <DocumentTextIcon class="w-16 h-16 text-gray-300 mx-auto mb-4" />
      <h3 class="text-lg font-medium text-gray-900 mb-2">Không tìm thấy bài thi</h3>
      <p class="text-gray-500 mb-4">Thử thay đổi từ khóa tìm kiếm hoặc bộ lọc</p>
      <button @click="resetFilter" class="btn-secondary">
        Xóa bộ lọc
      </button>
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
        
        <template v-for="page in visiblePages" :key="page">
          <button
            v-if="page !== '...'"
            @click="goToPage(page)"
            :class="[
              'px-4 py-2 rounded-lg font-medium transition-colors',
              page === pagination.currentPage
                ? 'bg-primary-600 text-white'
                : 'bg-white text-gray-700 hover:bg-gray-50 border border-gray-300'
            ]"
          >
            {{ page }}
          </button>
          <span v-else class="px-2 text-gray-500">...</span>
        </template>

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
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useDebounceFn } from '@vueuse/core'
import {
  MagnifyingGlassIcon,
  DocumentTextIcon,
  ChevronLeftIcon,
  ChevronRightIcon
} from '@heroicons/vue/24/outline'
import { quizService, categoryService } from '@/services'
import QuizCard from '@/components/quiz/QuizCard.vue'

const router = useRouter()

const loading = ref(true)
const quizzes = ref([])
const categories = ref([])
const searchTerm = ref('')
const joinCode = ref('')

const filter = reactive({
  maDanhMuc: '',
  sortBy: 'NgayTao',  // Mặc định: Mới nhất
  sortOrder: 'DESC',
  pageNumber: 1,
  pageSize: 8  // Hiển thị tối đa 8 bài thi
})

const pagination = reactive({
  currentPage: 1,
  totalPages: 1,
  totalCount: 0
})

const visiblePages = computed(() => {
  const pages = []
  const total = pagination.totalPages
  const current = pagination.currentPage

  if (total <= 7) {
    for (let i = 1; i <= total; i++) pages.push(i)
  } else {
    if (current <= 3) {
      pages.push(1, 2, 3, 4, '...', total)
    } else if (current >= total - 2) {
      pages.push(1, '...', total - 3, total - 2, total - 1, total)
    } else {
      pages.push(1, '...', current - 1, current, current + 1, '...', total)
    }
  }
  return pages
})

const loadQuizzes = async () => {
  loading.value = true
  try {
    const params = {
      pageNumber: filter.pageNumber,
      pageSize: filter.pageSize,
      sortBy: filter.sortBy,
      sortOrder: filter.sortOrder
    }
    
    // Thêm filter theo danh mục nếu có
    if (filter.maDanhMuc) {
      params.maDanhMuc = filter.maDanhMuc
    }
    
    // Thêm từ khóa tìm kiếm nếu có
    if (searchTerm.value.trim()) {
      params.timKiem = searchTerm.value.trim()
    }
    
    const response = await quizService.getPublicQuizzes(params)
    quizzes.value = response.data || []
    pagination.currentPage = response.pagination?.currentPage || 1
    pagination.totalPages = response.pagination?.totalPages || 1
    pagination.totalCount = response.pagination?.totalCount || 0
  } catch (error) {
    console.error('Failed to load quizzes:', error)
  } finally {
    loading.value = false
  }
}

const loadCategories = async () => {
  try {
    const response = await categoryService.getCategories()
    categories.value = response.data || []
  } catch (error) {
    console.error('Failed to load categories:', error)
  }
}

const debouncedSearch = useDebounceFn(() => {
  filter.pageNumber = 1
  loadQuizzes()
}, 300)

const goToPage = (page) => {
  if (page < 1 || page > pagination.totalPages) return
  filter.pageNumber = page
  loadQuizzes()
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

const resetFilter = () => {
  searchTerm.value = ''
  filter.maDanhMuc = ''
  filter.sortBy = 'NgayTao'  // Mặc định: Mới nhất
  filter.pageNumber = 1
  loadQuizzes()
}

const joinQuiz = async () => {
  if (!joinCode.value.trim()) return
  router.push({ name: 'quiz-detail', params: { id: 'code' }, query: { code: joinCode.value.trim() } })
}

onMounted(() => {
  loadQuizzes()
  loadCategories()
})
</script>
