<template>
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <div class="mb-8">
      <h1 class="page-header">Quản lý nội dung</h1>
      <p class="text-gray-600 mt-1">Kiểm duyệt bài thi và câu hỏi trong hệ thống</p>
    </div>

    <!-- Tabs -->
    <div class="border-b border-gray-200 mb-6">
      <nav class="flex space-x-8">
        <button
          @click="activeTab = 'quizzes'"
          :class="[
            'py-4 px-1 border-b-2 font-medium text-sm',
            activeTab === 'quizzes'
              ? 'border-primary-500 text-primary-600'
              : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
          ]"
        >
          Bài thi ({{ quizCount }})
        </button>
        <button
          @click="activeTab = 'questions'"
          :class="[
            'py-4 px-1 border-b-2 font-medium text-sm',
            activeTab === 'questions'
              ? 'border-primary-500 text-primary-600'
              : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
          ]"
        >
          Câu hỏi ({{ questionCount }})
        </button>
      </nav>
    </div>

    <!-- Quizzes Tab -->
    <div v-if="activeTab === 'quizzes'">
      <!-- Filters -->
      <div class="card p-4 mb-6">
        <div class="flex flex-col md:flex-row gap-4">
          <div class="flex-1 relative">
            <MagnifyingGlassIcon class="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
            <input
              v-model="quizSearch"
              type="text"
              placeholder="Tìm kiếm bài thi..."
              class="input-field pl-10"
              @input="debouncedQuizSearch"
            />
          </div>
          <select v-model="quizFilter.maDanhMuc" class="input-field md:w-48" @change="loadQuizzes">
            <option value="">Tất cả danh mục</option>
            <option v-for="cat in categories" :key="cat.maDanhMuc" :value="cat.maDanhMuc">
              {{ cat.tenDanhMuc }}
            </option>
          </select>
          <select v-model="quizFilter.sortBy" class="input-field md:w-40" @change="loadQuizzes">
            <option value="NgayTao">Mới nhất</option>
            <option value="TongLuotLamBai">Làm nhiều nhất</option>
            <option value="DiemTrungBinh">Đánh giá cao</option>
          </select>
        </div>
      </div>

      <!-- Quizzes Table -->
      <div class="card overflow-hidden">
        <div class="overflow-x-auto">
          <table class="w-full">
            <thead class="bg-gray-50 border-b border-gray-200">
              <tr>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Bài thi</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Người tạo</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Lượt làm</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Ngày tạo</th>
                <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">Thao tác</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-200">
              <tr v-if="loadingQuizzes">
                <td colspan="5" class="px-6 py-12 text-center">
                  <ArrowPathIcon class="w-8 h-8 text-primary-600 animate-spin mx-auto" />
                </td>
              </tr>
              <tr v-for="quiz in quizzes" :key="quiz.maBaiThi" class="hover:bg-gray-50">
                <td class="px-6 py-4">
                  <div class="font-medium text-gray-900">{{ quiz.tieuDe }}</div>
                  <div class="text-sm text-gray-500">{{ quiz.soCauHoi }} câu hỏi</div>
                </td>
                <td class="px-6 py-4 text-sm text-gray-500">{{ quiz.tenNguoiTao }}</td>
                <td class="px-6 py-4 text-sm text-gray-500">{{ quiz.tongLuotLamBai || 0 }}</td>
                <td class="px-6 py-4 text-sm text-gray-500">{{ formatDate(quiz.ngayTao) }}</td>
                <td class="px-6 py-4 text-right">
                  <div class="flex justify-end gap-2">
                    <router-link 
                      :to="{ name: 'quiz-detail', params: { id: quiz.maBaiThi } }"
                      class="p-2 text-gray-400 hover:text-primary-600 hover:bg-primary-50 rounded-lg"
                      title="Xem chi tiết"
                    >
                      <EyeIcon class="w-5 h-5" />
                    </router-link>
                    <button @click="confirmDeleteQuiz(quiz)" class="p-2 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg" title="Xóa bài thi">
                      <TrashIcon class="w-5 h-5" />
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Questions Tab -->
    <div v-if="activeTab === 'questions'">
      <!-- Filters -->
      <div class="card p-4 mb-6">
        <div class="flex flex-col md:flex-row gap-4">
          <div class="flex-1 relative">
            <MagnifyingGlassIcon class="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
            <input
              v-model="questionSearch"
              type="text"
              placeholder="Tìm kiếm câu hỏi..."
              class="input-field pl-10"
              @input="debouncedQuestionSearch"
            />
          </div>
          <select v-model="questionFilter.maDanhMuc" class="input-field md:w-48" @change="loadQuestions">
            <option value="">Tất cả danh mục</option>
            <option v-for="cat in categories" :key="cat.maDanhMuc" :value="cat.maDanhMuc">
              {{ cat.tenDanhMuc }}
            </option>
          </select>
        </div>
      </div>

      <!-- Questions List -->
      <div class="space-y-4">
        <div v-if="loadingQuestions" class="card p-8 text-center">
          <ArrowPathIcon class="w-8 h-8 text-primary-600 animate-spin mx-auto" />
        </div>
        <div 
          v-for="question in questions" 
          :key="question.maCauHoi" 
          class="card p-4 hover:shadow-md transition-shadow"
        >
          <div class="flex items-start gap-4">
            <div class="flex-1">
              <p class="font-medium text-gray-900 mb-2">{{ question.noiDungCauHoi }}</p>
              <div class="flex flex-wrap gap-2 mb-2">
                <span v-for="option in question.cacLuaChon" :key="option.maLuaChon"
                  :class="[
                    'text-xs px-2 py-1 rounded',
                    option.laDapAnDung ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-600'
                  ]"
                >
                  {{ option.noiDung }}
                </span>
              </div>
              <div class="flex items-center gap-3 text-sm text-gray-500">
                <span>{{ question.tenNguoiTao }}</span>
                <span>{{ question.tenDanhMuc || 'Không có danh mục' }}</span>
                <span>{{ formatDate(question.ngayTao) }}</span>
              </div>
            </div>
            <div class="flex gap-2">
              <button @click="confirmDeleteQuestion(question)" class="p-2 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg">
                <TrashIcon class="w-5 h-5" />
              </button>
            </div>
          </div>
        </div>
      </div>
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
                  Bạn có chắc chắn muốn xóa {{ deleteType === 'quiz' ? 'bài thi' : 'câu hỏi' }} này?
                </p>
                <div class="mt-6 flex justify-end gap-3">
                  <button @click="showDeleteModal = false" class="btn-secondary">
                    Hủy
                  </button>
                  <button @click="confirmDelete" class="btn-danger">
                    Xóa
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
import { ref, reactive, onMounted } from 'vue'
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
  MagnifyingGlassIcon,
  EyeIcon,
  TrashIcon,
  ArrowPathIcon
} from '@heroicons/vue/24/outline'
import { adminService, quizService, questionService, categoryService } from '@/services'

const toast = useToast()

const activeTab = ref('quizzes')
const loadingQuizzes = ref(true)
const loadingQuestions = ref(true)
const quizzes = ref([])
const questions = ref([])
const categories = ref([])
const quizCount = ref(0)
const questionCount = ref(0)

const quizSearch = ref('')
const questionSearch = ref('')

const quizFilter = reactive({
  maDanhMuc: '',
  sortBy: 'NgayTao',
  pageNumber: 1,
  pageSize: 20
})

const questionFilter = reactive({
  maDanhMuc: '',
  pageNumber: 1,
  pageSize: 20
})

const showDeleteModal = ref(false)
const deleteType = ref('')
const itemToDelete = ref(null)

const formatDate = (dateString) => {
  if (!dateString) return ''
  return new Date(dateString).toLocaleDateString('vi-VN')
}

const loadQuizzes = async () => {
  loadingQuizzes.value = true
  try {
    const response = await adminService.getAllQuizzes({
      pageNumber: quizFilter.pageNumber,
      pageSize: quizFilter.pageSize,
      timKiem: quizSearch.value || undefined,
      maDanhMuc: quizFilter.maDanhMuc || undefined,
      sortBy: quizFilter.sortBy,
      sortOrder: 'DESC'
    })
    quizzes.value = response.data || []
    quizCount.value = response.pagination?.totalCount || 0
  } catch (error) {
    console.error('Failed to load quizzes:', error)
  } finally {
    loadingQuizzes.value = false
  }
}

const loadQuestions = async () => {
  loadingQuestions.value = true
  try {
    const response = await adminService.getAllQuestions({
      ...questionFilter,
      timKiem: questionSearch.value || undefined
    })
    questions.value = response.data || []
    questionCount.value = response.pagination?.totalCount || 0
  } catch (error) {
    console.error('Failed to load questions:', error)
  } finally {
    loadingQuestions.value = false
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

const debouncedQuizSearch = useDebounceFn(() => {
  quizFilter.pageNumber = 1
  loadQuizzes()
}, 300)

const debouncedQuestionSearch = useDebounceFn(() => {
  questionFilter.pageNumber = 1
  loadQuestions()
}, 300)

const confirmDeleteQuiz = (quiz) => {
  deleteType.value = 'quiz'
  itemToDelete.value = quiz
  showDeleteModal.value = true
}

const confirmDeleteQuestion = (question) => {
  deleteType.value = 'question'
  itemToDelete.value = question
  showDeleteModal.value = true
}

const confirmDelete = async () => {
  try {
    if (deleteType.value === 'quiz') {
      await quizService.deleteQuiz(itemToDelete.value.maBaiThi)
      loadQuizzes()
    } else {
      await questionService.deleteQuestion(itemToDelete.value.maCauHoi)
      loadQuestions()
    }
    showDeleteModal.value = false
  } catch (error) {
    toast.error('Không thể xóa')
  }
}

onMounted(() => {
  loadCategories()
  loadQuizzes()
  loadQuestions()
})
</script>
