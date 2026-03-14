<template>
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4 mb-8">
      <div>
        <h1 class="page-header">Ngân hàng câu hỏi</h1>
        <p class="text-gray-600 mt-1">Quản lý tất cả câu hỏi của bạn</p>
      </div>
      <div class="flex gap-3">
        <router-link to="/questions/import" class="btn-secondary">
          <SparklesIcon class="w-5 h-5 mr-2" />
          Import bằng AI
        </router-link>
        <router-link to="/questions/create" class="btn-primary">
          <PlusIcon class="w-5 h-5 mr-2" />
          Tạo câu hỏi
        </router-link>
      </div>
    </div>

    <!-- Filters -->
    <div class="card p-4 mb-6">
      <div class="flex flex-col md:flex-row gap-4">
        <div class="flex-1 relative">
          <MagnifyingGlassIcon class="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
          <input
            v-model="searchTerm"
            type="text"
            placeholder="Tìm kiếm câu hỏi..."
            class="input-field pl-10"
            @input="debouncedSearch"
          />
        </div>
        <select v-model="filter.maDanhMuc" class="input-field md:w-48" @change="loadQuestions">
          <option value="">Tất cả danh mục</option>
          <option v-for="cat in categories" :key="cat.maDanhMuc" :value="cat.maDanhMuc">
            {{ cat.tenDanhMuc }}
          </option>
        </select>
        <select v-model="filter.mucDo" class="input-field md:w-40" @change="loadQuestions">
          <option value="">Tất cả độ khó</option>
          <option value="De">Dễ</option>
          <option value="TrungBinh">Trung bình</option>
          <option value="Kho">Khó</option>
        </select>
        <!-- <select v-model="filter.loaiCauHoi" class="input-field md:w-40" @change="loadQuestions">
          <option value="">Tất cả loại</option>
          <option value="MotDapAn">Một đáp án</option>
          <option value="NhieuDapAn">Nhiều đáp án</option>
        </select> -->
      </div>
    </div>

    <!-- Stats -->
    <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
      <div class="card p-4 text-center">
        <p class="text-2xl font-bold text-primary-600">{{ stats.total }}</p>
        <p class="text-sm text-gray-500">Tổng câu hỏi</p>
      </div>
      <div class="card p-4 text-center">
        <p class="text-2xl font-bold text-green-600">{{ stats.easy }}</p>
        <p class="text-sm text-gray-500">Câu dễ</p>
      </div>
      <div class="card p-4 text-center">
        <p class="text-2xl font-bold text-yellow-600">{{ stats.medium }}</p>
        <p class="text-sm text-gray-500">Câu trung bình</p>
      </div>
      <div class="card p-4 text-center">
        <p class="text-2xl font-bold text-red-600">{{ stats.hard }}</p>
        <p class="text-sm text-gray-500">Câu khó</p>
      </div>
    </div>

    <!-- Bulk Actions Bar -->
    <div class="card p-4 mb-6 bg-primary-50 border-primary-200">
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-3">
        <div class="flex items-center gap-3">
          <span class="text-primary-700 font-medium">
            Đã chọn {{ selectedQuestions.length }} / {{ stats.total }} câu hỏi
          </span>
        </div>
        <div class="flex flex-wrap gap-2">
          <button 
            @click="selectAllOnPage" 
            :class="[
              'text-sm px-3 py-1.5 rounded border',
              isAllOnPageSelected 
                ? 'bg-primary-100 text-primary-700 border-primary-300' 
                : 'bg-white text-gray-700 border-gray-300 hover:bg-gray-50'
            ]"
          >
            {{ isAllOnPageSelected ? '✓ Đã chọn trang này' : 'Chọn trang này' }}
          </button>
          <button 
            @click="selectAllPages" 
            :class="[
              'text-sm px-3 py-1.5 rounded border',
              isAllPagesSelected 
                ? 'bg-primary-100 text-primary-700 border-primary-300' 
                : 'bg-white text-gray-700 border-gray-300 hover:bg-gray-50'
            ]"
          >
            {{ isAllPagesSelected ? '✓ Đã chọn tất cả (' + stats.total + ')' : 'Chọn tất cả (' + stats.total + ')' }}
          </button>
          <button @click="clearSelection" class="btn-secondary text-sm">
            Bỏ chọn
          </button>
          <button @click="confirmBulkDelete" class="btn-danger text-sm">
            <TrashIcon class="w-4 h-4 mr-1" />
            Xóa {{ selectedQuestions.length }} câu
          </button>
        </div>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="space-y-4">
      <div v-for="i in 5" :key="i" class="card p-4 animate-pulse">
        <div class="flex items-start gap-4">
          <div class="w-12 h-12 bg-gray-200 rounded-lg"></div>
          <div class="flex-1 space-y-2">
            <div class="h-4 bg-gray-200 rounded w-3/4"></div>
            <div class="h-3 bg-gray-200 rounded w-1/2"></div>
          </div>
        </div>
      </div>
    </div>

    <!-- Questions List -->
    <div v-else-if="questions.length > 0" class="space-y-4">
      <div 
        v-for="(question, index) in questions" 
        :key="question.maCauHoi" 
        :class="[
          'card p-4 hover:shadow-md transition-shadow group',
          selectedQuestions.includes(question.maCauHoi) ? 'ring-2 ring-primary-500 bg-primary-50' : ''
        ]"
      >
        <div class="flex items-start gap-4">
          <!-- Checkbox -->
          <button
            @click="toggleSelect(question.maCauHoi)"
            :class="[
              'mt-3 w-6 h-6 rounded-full border-2 flex items-center justify-center cursor-pointer transition-all shrink-0',
              selectedQuestions.includes(question.maCauHoi)
                ? 'bg-primary-600 border-primary-600 text-white'
                : 'bg-white border-gray-300 hover:border-primary-500'
            ]"
          >
            <CheckIcon v-if="selectedQuestions.includes(question.maCauHoi)" class="w-4 h-4" />
          </button>
          
          <span class="w-10 h-10 bg-primary-100 text-primary-600 rounded-lg flex items-center justify-center font-semibold shrink-0">
            {{ (filter.pageNumber - 1) * filter.pageSize + index + 1 }}
          </span>
          
          <div class="flex-1 min-w-0">
            <div class="flex items-start justify-between mb-2">
              <h3 class="text-gray-900 font-medium">{{ question.noiDungCauHoi }}</h3>
              <div class="flex gap-2 ml-4 opacity-0 group-hover:opacity-100 transition-opacity">
                <router-link 
                  :to="{ name: 'question-edit', params: { id: question.maCauHoi } }"
                  class="p-2 text-gray-400 hover:text-primary-600 hover:bg-primary-50 rounded-lg"
                >
                  <PencilIcon class="w-5 h-5" />
                </router-link>
                <button 
                  @click="confirmDelete(question)"
                  class="p-2 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg"
                >
                  <TrashIcon class="w-5 h-5" />
                </button>
              </div>
            </div>

            <!-- Answer Options -->
            <div class="grid grid-cols-1 md:grid-cols-2 gap-2 mb-3">
              <div 
                v-for="option in question.cacLuaChon" 
                :key="option.maLuaChon"
                :class="[
                  'flex items-center p-2 rounded-lg text-sm',
                  option.laDapAnDung ? 'bg-green-50 text-green-700' : 'bg-gray-50 text-gray-600'
                ]"
              >
                <CheckCircleIcon 
                  v-if="option.laDapAnDung" 
                  class="w-4 h-4 mr-2 text-green-500 shrink-0" 
                />
                <span v-else class="w-4 h-4 mr-2 rounded-full border border-gray-300 shrink-0"></span>
                <span class="line-clamp-1">{{ option.noiDungDapAn || option.noiDung }}</span>
              </div>
            </div>

            <!-- Meta Info -->
            <div class="flex flex-wrap items-center gap-3 text-sm">
              <span 
                v-if="question.tenDanhMuc"
                class="px-2 py-1 bg-gray-100 text-gray-600 rounded"
              >
                {{ question.tenDanhMuc }}
              </span>
              <span :class="[
                'badge',
                question.mucDo === 'De' ? 'badge-success' : 
                question.mucDo === 'Kho' ? 'badge-error' : 'badge-warning'
              ]">
                {{ question.mucDo === 'De' ? 'Dễ' : question.mucDo === 'Kho' ? 'Khó' : 'Trung bình' }}
              </span>
              <span class="text-gray-400">
                {{ formatDate(question.ngayTao) }}
              </span>
              <span v-if="question.soLanSuDung" class="text-gray-400">
                Đã dùng {{ question.soLanSuDung }} lần
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="text-center py-16">
      <QuestionMarkCircleIcon class="w-16 h-16 text-gray-300 mx-auto mb-4" />
      <h3 class="text-lg font-medium text-gray-900 mb-2">Chưa có câu hỏi nào</h3>
      <p class="text-gray-500">Tạo câu hỏi đầu tiên để bắt đầu xây dựng ngân hàng đề</p>
    </div>

    <!-- Pagination -->
    <div v-if="pagination.totalPages > 1" class="mt-8 flex justify-center">
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

    <!-- Delete Confirmation Modal -->
    <TransitionRoot appear :show="showDeleteModal" as="template">
      <Dialog as="div" @close="showDeleteModal = false" class="relative z-50">
        <TransitionChild
          enter="duration-300 ease-out"
          enter-from="opacity-0"
          enter-to="opacity-100"
          leave="duration-200 ease-in"
          leave-from="opacity-100"
          leave-to="opacity-0"
        >
          <div class="fixed inset-0 bg-black/50" />
        </TransitionChild>

        <div class="fixed inset-0 overflow-y-auto">
          <div class="flex min-h-full items-center justify-center p-4">
            <TransitionChild
              enter="duration-300 ease-out"
              enter-from="opacity-0 scale-95"
              enter-to="opacity-100 scale-100"
              leave="duration-200 ease-in"
              leave-from="opacity-100 scale-100"
              leave-to="opacity-0 scale-95"
            >
              <DialogPanel class="w-full max-w-md transform rounded-2xl bg-white p-6 shadow-xl transition-all">
                <DialogTitle as="h3" class="text-lg font-semibold text-gray-900">
                  Xác nhận xóa
                </DialogTitle>
                <p class="mt-2 text-gray-600">
                  <template v-if="isBulkDelete">
                    Bạn có chắc chắn muốn xóa <strong>{{ selectedQuestions.length }}</strong> câu hỏi đã chọn? Hành động không thể hoàn tác.
                  </template>
                  <template v-else>
                    Bạn có chắc chắn muốn xóa câu hỏi này? Hành động không thể hoàn tác.
                  </template>
                </p>
                <div class="mt-6 flex justify-end gap-3">
                  <button @click="showDeleteModal = false" class="btn-secondary">
                    Hủy
                  </button>
                  <button @click="isBulkDelete ? bulkDeleteQuestions() : deleteQuestion()" class="btn-danger">
                    Xóa {{ isBulkDelete ? selectedQuestions.length + ' câu' : '' }}
                  </button>
                </div>
              </DialogPanel>
            </TransitionChild>
          </div>
        </div>
      </Dialog>
    </TransitionRoot>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { useDebounceFn } from '@vueuse/core'
import { useToast } from 'vue-toastification'
import {
  Dialog,
  DialogPanel,
  DialogTitle,
  TransitionRoot,
  TransitionChild
} from '@headlessui/vue'
import {
  PlusIcon,
  SparklesIcon,
  MagnifyingGlassIcon,
  QuestionMarkCircleIcon,
  PencilIcon,
  TrashIcon,
  CheckCircleIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  CheckIcon
} from '@heroicons/vue/24/outline'
import { questionService, categoryService } from '@/services'

const toast = useToast()

const loading = ref(true)
const questions = ref([])
const categories = ref([])
const searchTerm = ref('')
const showDeleteModal = ref(false)
const questionToDelete = ref(null)
const selectedQuestions = ref([])
const isBulkDelete = ref(false)

const filter = reactive({
  maDanhMuc: '',
  mucDo: '',
  loaiCauHoi: '',
  pageNumber: 1,
  pageSize: 10
})

const pagination = reactive({
  currentPage: 1,
  totalPages: 1,
  totalCount: 0
})

// Stats from API
const stats = reactive({
  total: 0,
  easy: 0,
  medium: 0,
  hard: 0
})

// Track if selecting all pages
const isAllPagesSelected = ref(false)
const allQuestionIds = ref([])

const isAllOnPageSelected = computed(() => {
  return questions.value.length > 0 && questions.value.every(q => selectedQuestions.value.includes(q.maCauHoi))
})

const loadQuestions = async () => {
  loading.value = true
  try {
    const response = await questionService.getQuestions({
      ...filter,
      timKiem: searchTerm.value
    })
    questions.value = response.data || []
    pagination.currentPage = response.pagination?.currentPage || 1
    pagination.totalPages = response.pagination?.totalPages || 1
    pagination.totalCount = response.pagination?.totalCount || 0
    goToPageInput.value = pagination.currentPage
  } catch (error) {
    console.error('Failed to load questions:', error)
    toast.error('Không thể tải danh sách câu hỏi')
  } finally {
    loading.value = false
  }
}

const loadStats = async () => {
  try {
    const response = await questionService.getStats()
    if (response.success && response.data) {
      stats.total = response.data.total || 0
      stats.easy = response.data.easy || 0
      stats.medium = response.data.medium || 0
      stats.hard = response.data.hard || 0
    }
  } catch (error) {
    console.error('Failed to load stats:', error)
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
  loadQuestions()
}, 300)

const goToPage = (page) => {
  if (page < 1 || page > pagination.totalPages) return
  filter.pageNumber = page
  loadQuestions()
  // Scroll to top of page
  window.scrollTo({ top: 0, behavior: 'smooth' })
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

const formatDate = (dateString) => {
  if (!dateString) return ''
  return new Date(dateString).toLocaleDateString('vi-VN')
}

const confirmDelete = (question) => {
  questionToDelete.value = question
  isBulkDelete.value = false
  showDeleteModal.value = true
}

const deleteQuestion = async () => {
  if (!questionToDelete.value) return
  
  try {
    await questionService.deleteQuestion(questionToDelete.value.maCauHoi)
    showDeleteModal.value = false
    loadQuestions()
    loadStats()
  } catch (error) {
    showDeleteModal.value = false
    const message = error.response?.data?.message || 'Không thể xóa câu hỏi'
    toast.error(message)
  }
}

// Bulk selection functions
const toggleSelect = (maCauHoi) => {
  const index = selectedQuestions.value.indexOf(maCauHoi)
  if (index === -1) {
    selectedQuestions.value.push(maCauHoi)
  } else {
    selectedQuestions.value.splice(index, 1)
    isAllPagesSelected.value = false
  }
}

const selectAllOnPage = () => {
  if (isAllOnPageSelected.value) {
    // Deselect all on current page
    questions.value.forEach(q => {
      const index = selectedQuestions.value.indexOf(q.maCauHoi)
      if (index !== -1) {
        selectedQuestions.value.splice(index, 1)
      }
    })
    isAllPagesSelected.value = false
  } else {
    // Select all on current page
    questions.value.forEach(q => {
      if (!selectedQuestions.value.includes(q.maCauHoi)) {
        selectedQuestions.value.push(q.maCauHoi)
      }
    })
  }
}

const selectAllPages = async () => {
  if (isAllPagesSelected.value) {
    // Deselect all
    selectedQuestions.value = []
    allQuestionIds.value = []
    isAllPagesSelected.value = false
  } else {
    // Load all question IDs and select them
    try {
      const response = await questionService.getQuestions({
        pageSize: 10000 // Get all
      })
      allQuestionIds.value = (response.data || []).map(q => q.maCauHoi)
      selectedQuestions.value = [...allQuestionIds.value]
      isAllPagesSelected.value = true
    } catch (error) {
      console.error('Failed to load all questions:', error)
      toast.error('Không thể tải tất cả câu hỏi')
    }
  }
}

const clearSelection = () => {
  selectedQuestions.value = []
  allQuestionIds.value = []
  isAllPagesSelected.value = false
}

const confirmBulkDelete = () => {
  isBulkDelete.value = true
  showDeleteModal.value = true
}

const bulkDeleteQuestions = async () => {
  if (selectedQuestions.value.length === 0) return
  
  const totalToDelete = selectedQuestions.value.length
  let successCount = 0
  let failCount = 0
  
  // Close modal immediately to show progress
  showDeleteModal.value = false
  
  // Delete questions one by one to handle errors gracefully
  const failedReasons = []
  for (const id of selectedQuestions.value) {
    try {
      await questionService.deleteQuestion(id)
      successCount++
    } catch (error) {
      console.error(`Failed to delete question ${id}:`, error)
      const reason = error.response?.data?.message || 'Không thể xóa'
      failedReasons.push(reason)
      failCount++
    }
  }
  
  // Show result
  if (failCount === 0) {
    // Success - no toast needed
  } else if (successCount > 0) {
    toast.warning(`Đã xóa ${successCount}/${totalToDelete} câu hỏi. ${failCount} câu không thể xóa (đang sử dụng trong bài thi).`)
  } else {
    const uniqueReasons = [...new Set(failedReasons)]
    toast.error(uniqueReasons[0] || 'Không thể xóa câu hỏi. Vui lòng thử lại.')
  }
  
  selectedQuestions.value = []
  loadQuestions()
  loadStats()
}

onMounted(() => {
  loadCategories()
  loadQuestions()
  loadStats()
})
</script>
