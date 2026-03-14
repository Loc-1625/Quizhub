<template>
  <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <div class="mb-8">
      <h1 class="page-header">
        <SparklesIcon class="w-8 h-8 inline-block mr-2 text-primary-600" />
        Tạo câu hỏi bằng AI
      </h1>
      <p class="text-gray-600 mt-1">Sử dụng AI để tự động tạo câu hỏi từ nội dung của bạn</p>
    </div>

    <!-- Input Section -->
    <div class="card p-6 mb-6">
      <h2 class="section-title mb-4">Nhập nội dung</h2>
      
      <div class="space-y-4">
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">
            Chủ đề / Nội dung <span class="text-red-500">*</span>
          </label>
          <textarea
            v-model="inputContent"
            rows="6"
            required
            placeholder="Nhập chủ đề, đoạn văn bản, hoặc nội dung bạn muốn tạo câu hỏi...

Ví dụ:
- Lịch sử Việt Nam thời kỳ phong kiến
- Đoạn văn về định luật Newton
- Các công thức hóa học cơ bản..."
            class="input-field"
          ></textarea>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">
              Số lượng câu hỏi
            </label>
            <select v-model="options.soLuong" class="input-field">
              <option :value="5">5 câu</option>
              <option :value="10">10 câu</option>
              <option :value="15">15 câu</option>
              <option :value="20">20 câu</option>
            </select>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">
              Độ khó
            </label>
            <select v-model="options.mucDo" class="input-field">
              <option value="De">Dễ</option>
              <option value="TrungBinh">Trung bình</option>
              <option value="Kho">Khó</option>
              <option value="HonHop">Hỗn hợp</option>
            </select>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">
              Danh mục
            </label>
            <select v-model="options.maDanhMuc" class="input-field">
              <option value="">Không có</option>
              <option v-for="cat in categories" :key="cat.maDanhMuc" :value="cat.maDanhMuc">
                {{ cat.tenDanhMuc }}
              </option>
            </select>
          </div>
        </div>

        <button
          @click="generateQuestions"
          :disabled="generating || !inputContent.trim()"
          class="btn-primary w-full"
        >
          <ArrowPathIcon v-if="generating" class="w-5 h-5 mr-2 animate-spin" />
          <SparklesIcon v-else class="w-5 h-5 mr-2" />
          {{ generating ? 'Đang tạo câu hỏi...' : 'Tạo câu hỏi bằng AI' }}
        </button>
      </div>
    </div>

    <!-- Progress - Simple loading without percentage -->
    <div v-if="generating" class="card p-6 mb-6">
      <div class="flex items-center justify-center">
        <ArrowPathIcon class="w-8 h-8 text-primary-600 animate-spin mr-3" />
        <p class="text-gray-600">
          <SparklesIcon class="w-5 h-5 inline-block mr-1 text-primary-600" />
          AI đang phân tích nội dung và tạo câu hỏi...
        </p>
      </div>
    </div>

    <!-- Generated Questions with Pagination -->
    <div v-if="generatedQuestions.length > 0" class="space-y-4">
      <div class="flex items-center justify-between">
        <h2 class="section-title">Câu hỏi được tạo ({{ generatedQuestions.length }})</h2>
        <div class="flex gap-2">
          <button @click="selectAll" class="btn-secondary text-sm">
            Chọn tất cả
          </button>
          <button @click="deselectAll" class="btn-secondary text-sm">
            Bỏ chọn tất cả
          </button>
        </div>
      </div>

      <!-- Pagination Info -->
      <div v-if="totalPages > 1" class="flex items-center justify-between text-sm text-gray-500">
        <span>Trang {{ currentPage }} / {{ totalPages }}</span>
        <span>Hiển thị {{ paginatedQuestions.length }} / {{ generatedQuestions.length }} câu</span>
      </div>

      <div 
        v-for="(question, index) in paginatedQuestions" 
        :key="getQuestionIndex(index)"
        :class="[
          'card p-4 transition-all cursor-pointer',
          question.selected ? 'ring-2 ring-primary-500 bg-primary-50' : 'hover:shadow-md'
        ]"
        @click="question.selected = !question.selected"
      >
        <div class="flex items-start gap-3">
          <div :class="[
            'w-6 h-6 rounded border-2 flex items-center justify-center shrink-0 transition-colors',
            question.selected 
              ? 'bg-primary-600 border-primary-600' 
              : 'border-gray-300'
          ]">
            <CheckIcon v-if="question.selected" class="w-4 h-4 text-white" />
          </div>

          <div class="flex-1">
            <div class="flex items-start justify-between mb-2">
              <h3 class="font-medium text-gray-900">
                Câu {{ getQuestionIndex(index) + 1 }}: {{ question.noiDungCauHoi }}
              </h3>
              <span :class="[
                'badge shrink-0 ml-2',
                question.mucDo === 'De' ? 'badge-success' : 
                question.mucDo === 'Kho' ? 'badge-error' : 'badge-warning'
              ]">
                {{ question.mucDo === 'De' ? 'Dễ' : question.mucDo === 'Kho' ? 'Khó' : 'TB' }}
              </span>
            </div>

            <div class="grid grid-cols-1 md:grid-cols-2 gap-2">
              <div 
                v-for="(option, optIndex) in question.cacLuaChon" 
                :key="optIndex"
                :class="[
                  'flex items-center p-2 rounded text-sm',
                  option.laDapAnDung ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-600'
                ]"
              >
                <span class="w-5 h-5 rounded-full mr-2 flex items-center justify-center text-xs font-medium" 
                  :class="option.laDapAnDung ? 'bg-green-500 text-white' : 'bg-gray-300'">
                  {{ String.fromCharCode(65 + optIndex) }}
                </span>
                {{ option.noiDung || option.noiDungDapAn }}
              </div>
            </div>

            <p v-if="question.giaiThich" class="mt-2 text-sm text-gray-500 italic">
              {{ question.giaiThich }}
            </p>
          </div>
        </div>
      </div>

      <!-- Pagination Controls -->
      <div v-if="totalPages > 1" class="flex items-center justify-center gap-2 mt-4">
        <button 
          @click="currentPage = 1" 
          :disabled="currentPage === 1"
          class="btn-secondary text-sm px-3 py-1"
        >
          ««
        </button>
        <button 
          @click="currentPage--" 
          :disabled="currentPage === 1"
          class="btn-secondary text-sm px-3 py-1"
        >
          «
        </button>
        
        <template v-for="page in visiblePages" :key="page">
          <button 
            v-if="page !== '...'"
            @click="currentPage = page"
            :class="[
              'text-sm px-3 py-1 rounded',
              currentPage === page 
                ? 'bg-primary-600 text-white' 
                : 'btn-secondary'
            ]"
          >
            {{ page }}
          </button>
          <span v-else class="px-2 text-gray-400">...</span>
        </template>
        
        <button 
          @click="currentPage++" 
          :disabled="currentPage === totalPages"
          class="btn-secondary text-sm px-3 py-1"
        >
          »
        </button>
        <button 
          @click="currentPage = totalPages" 
          :disabled="currentPage === totalPages"
          class="btn-secondary text-sm px-3 py-1"
        >
          »»
        </button>
      </div>

      <!-- Save Actions -->
      <div class="flex justify-end gap-4 mt-6">
        <button @click="clearGenerated" class="btn-secondary">
          Xóa kết quả
        </button>
        <button
          @click="saveSelectedQuestions"
          :disabled="saving || selectedCount === 0"
          class="btn-primary"
        >
          <ArrowPathIcon v-if="saving" class="w-5 h-5 mr-2 animate-spin" />
          {{ saving ? 'Đang lưu...' : `Lưu ${selectedCount} câu hỏi` }}
        </button>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else-if="!generating" class="text-center py-16">
      <SparklesIcon class="w-16 h-16 text-gray-300 mx-auto mb-4" />
      <h3 class="text-lg font-medium text-gray-900 mb-2">Tạo câu hỏi tự động bằng AI</h3>
      <p class="text-gray-500 max-w-md mx-auto">
        Chỉ cần nhập chủ đề hoặc nội dung, AI sẽ tự động tạo các câu hỏi trắc nghiệm chất lượng cho bạn.
      </p>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'vue-toastification'
import {
  SparklesIcon,
  ArrowPathIcon,
  CheckIcon
} from '@heroicons/vue/24/outline'
import { aiService, questionService, categoryService } from '@/services'

const router = useRouter()
const toast = useToast()

const inputContent = ref('')
const generating = ref(false)
const saving = ref(false)
const generatedQuestions = ref([])
const categories = ref([])
const currentPage = ref(1)
const pageSize = 5 // Show 5 questions per page

const options = reactive({
  soLuong: 10,
  mucDo: 'HonHop',
  maDanhMuc: ''
})

const selectedCount = computed(() => {
  return generatedQuestions.value.filter(q => q.selected).length
})

// Pagination computed properties
const totalPages = computed(() => {
  return Math.ceil(generatedQuestions.value.length / pageSize)
})

const paginatedQuestions = computed(() => {
  const start = (currentPage.value - 1) * pageSize
  const end = start + pageSize
  return generatedQuestions.value.slice(start, end)
})

const visiblePages = computed(() => {
  const pages = []
  const total = totalPages.value
  const current = currentPage.value
  
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

const getQuestionIndex = (index) => {
  return (currentPage.value - 1) * pageSize + index
}

const loadCategories = async () => {
  try {
    const response = await categoryService.getCategories()
    categories.value = response.data || []
  } catch (error) {
    console.error('Failed to load categories:', error)
  }
}

const generateQuestions = async () => {
  if (!inputContent.value.trim()) {
    toast.error('Vui lòng nhập nội dung')
    return
  }

  generating.value = true
  currentPage.value = 1 // Reset to first page

  try {
    const response = await aiService.generateQuestions({
      noiDung: inputContent.value,
      soLuong: options.soLuong,
      mucDo: options.mucDo !== 'HonHop' ? options.mucDo : null,
      maDanhMuc: options.maDanhMuc || null
    })

    if (response.success && response.data) {
      generatedQuestions.value = response.data.map(q => ({
        ...q,
        selected: true,
        maDanhMuc: options.maDanhMuc
      }))
    } else {
      toast.error(response.message || 'Không thể tạo câu hỏi')
    }
  } catch (error) {
    toast.error(error.response?.data?.message || 'Không thể tạo câu hỏi. Vui lòng thử lại.')
  } finally {
    generating.value = false
  }
}

const selectAll = () => {
  generatedQuestions.value.forEach(q => q.selected = true)
}

const deselectAll = () => {
  generatedQuestions.value.forEach(q => q.selected = false)
}

const clearGenerated = () => {
  generatedQuestions.value = []
}

const saveSelectedQuestions = async () => {
  const selected = generatedQuestions.value.filter(q => q.selected)
  
  if (selected.length === 0) {
    toast.error('Vui lòng chọn ít nhất một câu hỏi')
    return
  }

  saving.value = true

  try {
    const response = await questionService.createBulkQuestions(selected)
    
    if (response.success) {
      router.push('/questions')
    } else {
      toast.error(response.message || 'Không thể lưu câu hỏi')
    }
  } catch (error) {
    toast.error(error.response?.data?.message || 'Không thể lưu câu hỏi')
  } finally {
    saving.value = false
  }
}

onMounted(() => {
  loadCategories()
})
</script>
