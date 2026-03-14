<template>
  <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <div class="mb-8">
      <h1 class="page-header">
        <SparklesIcon class="w-8 h-8 inline-block mr-2 text-primary-600" />
        Tạo câu hỏi bằng AI
      </h1>
      <p class="text-gray-600 mt-1">Sử dụng AI để tự động tạo câu hỏi từ nội dung của bạn</p>
    </div>

    <!-- Tab Selection -->
    <div class="card mb-6">
      <div class="flex border-b border-gray-200">
        <button
          @click="activeTab = 'upload'"
          :class="[
            'flex-1 py-4 px-6 text-center font-medium transition-colors',
            activeTab === 'upload'
              ? 'text-primary-600 border-b-2 border-primary-600 bg-primary-50'
              : 'text-gray-500 hover:text-gray-700 hover:bg-gray-50'
          ]"
        >
          <DocumentArrowUpIcon class="w-5 h-5 inline-block mr-2" />
          Trích xuất từ file
        </button>
        <button
          @click="activeTab = 'generate'"
          :class="[
            'flex-1 py-4 px-6 text-center font-medium transition-colors',
            activeTab === 'generate'
              ? 'text-primary-600 border-b-2 border-primary-600 bg-primary-50'
              : 'text-gray-500 hover:text-gray-700 hover:bg-gray-50'
          ]"
        >
          <SparklesIcon class="w-5 h-5 inline-block mr-2" />
          Tạo từ chủ đề
        </button>
      </div>

      <!-- Tab: Upload File -->
      <div v-if="activeTab === 'upload'" class="p-8">
        <div
          class="border-2 border-dashed border-gray-300 rounded-xl p-12 text-center hover:border-primary-400 transition-colors"
          @dragover.prevent
          @drop.prevent="handleDrop"
        >
          <input
            ref="fileInput"
            type="file"
            class="hidden"
            accept=".pdf,.docx,.txt"
            @change="handleFileChange"
          />
          <CloudArrowUpIcon class="w-16 h-16 text-gray-400 mx-auto mb-4" />
          <p class="text-lg text-gray-600 mb-2">
            Kéo thả file hoặc
            <button type="button" @click="$refs.fileInput.click()" class="text-primary-600 font-medium hover:text-primary-700">
              chọn file
            </button>
          </p>
          <p class="text-sm text-gray-500">Hỗ trợ: PDF, DOCX, TXT (tối đa 10MB)</p>
          
          <div v-if="selectedFile" class="mt-6 p-4 bg-gray-50 rounded-lg inline-flex items-center gap-3">
            <DocumentTextIcon class="w-8 h-8 text-primary-600" />
            <div class="text-left">
              <p class="font-medium text-gray-900">{{ selectedFile.name }}</p>
              <p class="text-sm text-gray-500">{{ formatFileSize(selectedFile.size) }}</p>
            </div>
            <button @click="clearFile" class="ml-4 text-gray-400 hover:text-red-500">
              <XMarkIcon class="w-5 h-5" />
            </button>
          </div>
        </div>

        <!-- Category selection for file extraction - REQUIRED -->
        <div class="mt-6">
          <label class="block text-sm font-medium text-gray-700 mb-2">
            Danh mục <span class="text-red-500">*</span>
          </label>
          <select v-model="uploadOptions.maDanhMuc" class="input-field max-w-xs">
            <option value="" disabled>-- Chọn danh mục --</option>
            <option v-for="cat in categories" :key="cat.maDanhMuc" :value="cat.maDanhMuc">
              {{ cat.tenDanhMuc }}
            </option>
          </select>
          <p class="text-xs text-gray-500 mt-1">Vui lòng chọn danh mục để phân loại câu hỏi</p>
        </div>

        <button
          @click="extractFromFile"
          :disabled="!selectedFile || !uploadOptions.maDanhMuc || generating"
          class="btn-primary w-full mt-6 py-3 text-base"
        >
          <ArrowPathIcon v-if="generating" class="w-5 h-5 mr-2 animate-spin" />
          <SparklesIcon v-else class="w-5 h-5 mr-2" />
          {{ generating ? 'Đang trích xuất...' : 'Trích xuất câu hỏi từ file' }}
        </button>
      </div>

      <!-- Tab: Generate from Topic -->
      <div v-if="activeTab === 'generate'" class="p-8">
        <div class="space-y-6">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">
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
              class="input-field text-base"
            ></textarea>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-2">
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
              <label class="block text-sm font-medium text-gray-700 mb-2">
                Danh mục <span class="text-red-500">*</span>
              </label>
              <select v-model="options.maDanhMuc" class="input-field">
                <option value="" disabled>-- Chọn danh mục --</option>
                <option v-for="cat in categories" :key="cat.maDanhMuc" :value="cat.maDanhMuc">
                  {{ cat.tenDanhMuc }}
                </option>
              </select>
            </div>
          </div>
          
          <p class="text-xs text-gray-500 mt-2">
            Vui lòng chọn danh mục để phân loại câu hỏi. AI sẽ tự động xác định độ khó cho từng câu.
          </p>

          <button
            @click="generateQuestions"
            :disabled="generating || !inputContent.trim() || !options.maDanhMuc"
            class="btn-primary w-full py-3 text-base mt-4"
          >
            <ArrowPathIcon v-if="generating" class="w-5 h-5 mr-2 animate-spin" />
            <SparklesIcon v-else class="w-5 h-5 mr-2" />
            {{ generating ? 'Đang tạo câu hỏi...' : 'Tạo câu hỏi bằng AI' }}
          </button>
        </div>
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
          :class="[
            'text-sm px-3 py-1 rounded border',
            currentPage === 1 
              ? 'bg-gray-100 text-gray-400 border-gray-200 cursor-not-allowed' 
              : 'btn-secondary'
          ]"
        >
          ««
        </button>
        <button 
          @click="currentPage--" 
          :disabled="currentPage === 1"
          :class="[
            'text-sm px-3 py-1 rounded border',
            currentPage === 1 
              ? 'bg-gray-100 text-gray-400 border-gray-200 cursor-not-allowed' 
              : 'btn-secondary'
          ]"
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
          :class="[
            'text-sm px-3 py-1 rounded border',
            currentPage === totalPages 
              ? 'bg-gray-100 text-gray-400 border-gray-200 cursor-not-allowed' 
              : 'btn-secondary'
          ]"
        >
          »
        </button>
        <button 
          @click="currentPage = totalPages" 
          :disabled="currentPage === totalPages"
          :class="[
            'text-sm px-3 py-1 rounded border',
            currentPage === totalPages 
              ? 'bg-gray-100 text-gray-400 border-gray-200 cursor-not-allowed' 
              : 'btn-secondary'
          ]"
        >
          »»
        </button>
        
        <!-- Page input -->
        <div class="flex items-center gap-1 ml-2">
          <span class="text-sm text-gray-500">Đến trang</span>
          <input 
            type="number" 
            v-model.number="goToPageInput"
            @keyup.enter="goToPageNumber"
            min="1"
            :max="totalPages"
            class="w-16 px-2 py-1 text-sm border border-gray-300 rounded focus:ring-primary-500 focus:border-primary-500"
          />
          <button @click="goToPageNumber" class="btn-secondary text-sm px-2 py-1">Đi</button>
        </div>
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
        Chọn tab phù hợp để tải file lên hoặc nhập chủ đề, AI sẽ tự động tạo các câu hỏi trắc nghiệm cho bạn.
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
  CheckIcon,
  DocumentArrowUpIcon,
  CloudArrowUpIcon,
  DocumentTextIcon,
  XMarkIcon
} from '@heroicons/vue/24/outline'
import { aiService, questionService, categoryService } from '@/services'

const router = useRouter()
const toast = useToast()

// Tab state
const activeTab = ref('upload')

// File upload state
const fileInput = ref(null)
const selectedFile = ref(null)
const uploadOptions = reactive({
  maDanhMuc: ''
})

// Generate from topic state
const inputContent = ref('')
const options = reactive({
  soLuong: 10,
  maDanhMuc: ''
})

// Common state
const generating = ref(false)
const saving = ref(false)
const generatedQuestions = ref([])
const categories = ref([])
const currentPage = ref(1)
const pageSize = 5 // Show 5 questions per page
const goToPageInput = ref(1)

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

const formatFileSize = (bytes) => {
  if (bytes === 0) return '0 Bytes'
  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}

const loadCategories = async () => {
  try {
    const response = await categoryService.getCategories()
    categories.value = response.data || []
  } catch (error) {
    console.error('Failed to load categories:', error)
  }
}

// File upload handlers
const handleFileChange = (event) => {
  const file = event.target.files[0]
  if (file) {
    validateAndSetFile(file)
  }
}

const handleDrop = (event) => {
  const file = event.dataTransfer.files[0]
  if (file) {
    validateAndSetFile(file)
  }
}

const validateAndSetFile = (file) => {
  const allowedTypes = ['application/pdf', 'application/vnd.openxmlformats-officedocument.wordprocessingml.document', 'text/plain']
  const maxSize = 10 * 1024 * 1024 // 10MB

  if (!allowedTypes.includes(file.type) && !file.name.endsWith('.txt')) {
    toast.error('Chỉ hỗ trợ file PDF, DOCX hoặc TXT')
    return
  }

  if (file.size > maxSize) {
    toast.error('Kích thước file tối đa là 10MB')
    return
  }

  selectedFile.value = file
}

const clearFile = () => {
  selectedFile.value = null
  if (fileInput.value) {
    fileInput.value.value = ''
  }
}

const extractFromFile = async () => {
  if (!selectedFile.value) {
    toast.error('Vui lòng chọn file')
    return
  }

  generating.value = true
  currentPage.value = 1

  try {
    const response = await aiService.extractFromFile(selectedFile.value)

    if (response.success && response.data) {
      const questions = response.data.cacCauHoi || []
      generatedQuestions.value = questions.map(q => ({
        noiDungCauHoi: q.noiDungCauHoi,
        giaiThich: q.giaiThich || '',
        mucDo: q.mucDo || 'TrungBinh', // AI decides difficulty
        maDanhMuc: uploadOptions.maDanhMuc || null, // User-selected category
        selected: true,
        cacLuaChon: (q.cacLuaChon || []).map((opt, i) => ({
          noiDung: typeof opt === 'string' ? opt : (opt.noiDung || ''),
          laDapAnDung: i === q.dapAnDung
        }))
      }))

      if (generatedQuestions.value.length === 0) {
        toast.warning('Không thể trích xuất câu hỏi từ file này')
      }
    } else {
      toast.error(response.message || 'Không thể trích xuất câu hỏi')
    }
  } catch (error) {
    console.error('Extract error:', error)
    toast.error(error.response?.data?.message || 'Lỗi khi trích xuất câu hỏi')
  } finally {
    generating.value = false
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
    const response = await aiService.generateFromTopic({
      topic: inputContent.value,
      numberOfQuestions: options.soLuong
      // No difficulty - AI will decide for each question
    })

    if (response.success && response.data) {
      const questions = response.data.cacCauHoi || []
      generatedQuestions.value = questions.map(q => ({
        noiDungCauHoi: q.noiDungCauHoi,
        giaiThich: q.giaiThich || '',
        mucDo: q.mucDo || 'TrungBinh', // AI decides difficulty
        maDanhMuc: options.maDanhMuc || null, // User-selected category
        selected: true,
        cacLuaChon: (q.cacLuaChon || []).map((opt, i) => ({
          noiDung: typeof opt === 'string' ? opt : (opt.noiDung || ''),
          laDapAnDung: i === q.dapAnDung
        }))
      }))

      if (generatedQuestions.value.length === 0) {
        toast.warning('Không thể tạo câu hỏi từ chủ đề này')
      }
    } else {
      toast.error(response.message || 'Không thể tạo câu hỏi')
    }
  } catch (error) {
    console.error('Generate error:', error)
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

const goToPageNumber = () => {
  const page = parseInt(goToPageInput.value)
  if (page >= 1 && page <= totalPages.value) {
    currentPage.value = page
  } else {
    toast.warning(`Vui lòng nhập số trang từ 1 đến ${totalPages.value}`)
    goToPageInput.value = currentPage.value
  }
}

const saveSelectedQuestions = async () => {
  const selected = generatedQuestions.value.filter(q => q.selected)
  
  if (selected.length === 0) {
    toast.error('Vui lòng chọn ít nhất một câu hỏi')
    return
  }

  saving.value = true

  try {
    // Transform data for API - ensure correct format matching backend DTO
    const questionsToSave = selected.map(q => ({
      noiDungCauHoi: q.noiDungCauHoi,
      giaiThich: q.giaiThich || '',
      mucDo: q.mucDo || 'TrungBinh',
      maDanhMuc: q.maDanhMuc || null,
      loaiCauHoi: 'MotDapAn',
      congKhai: false,
      cacLuaChon: q.cacLuaChon.map((opt, index) => ({
        noiDungDapAn: opt.noiDung || opt.noiDungDapAn || '',
        laDapAnDung: opt.laDapAnDung,
        thuTu: index
      }))
    }))

    const response = await questionService.createBulkQuestions(questionsToSave)
    
    if (response.success) {
      router.push('/questions')
    } else {
      toast.error(response.message || 'Không thể lưu câu hỏi')
    }
  } catch (error) {
    console.error('Save error:', error)
    toast.error(error.response?.data?.message || 'Không thể lưu câu hỏi')
  } finally {
    saving.value = false
  }
}

onMounted(() => {
  loadCategories()
})
</script>
