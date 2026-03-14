<template>
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <div class="mb-8">
      <h1 class="page-header">Quản lý nội dung</h1>
      <p class="text-gray-600 mt-1">Kiểm duyệt bài thi và câu hỏi trong hệ thống</p>
    </div>

    <!-- Tabs -->
    <div class="border-b border-gray-200 mb-6">
      <nav class="flex space-x-8">
        <button @click="activeTab = 'quizzes'" :class="[
          'py-4 px-1 border-b-2 font-medium text-sm',
          activeTab === 'quizzes'
            ? 'border-primary-500 text-primary-600'
            : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
        ]">
          Bài thi
        </button>
        <button @click="activeTab = 'questions'" :class="[
          'py-4 px-1 border-b-2 font-medium text-sm',
          activeTab === 'questions'
            ? 'border-primary-500 text-primary-600'
            : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
        ]">
          Câu hỏi
        </button>
      </nav>
    </div>

    <div v-if="activeTab === 'quizzes'">
      <!-- Filters -->
      <div class="card p-4 mb-6">
        <div class="flex flex-col md:flex-row gap-4">
          <div class="flex-1 relative">
            <MagnifyingGlassIcon class="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
            <input v-model="quizSearch" type="text" placeholder="Tìm kiếm bài thi..." class="input-field pl-10" @input="debouncedQuizSearch" />
          </div>
          <input v-model="quizFilter.tenNguoiTao" type="text" placeholder="Tên người tạo..." class="input-field md:w-48" @input="debouncedQuizSearch" />
          <select v-model="quizFilter.maDanhMuc" class="input-field md:w-48" @change="loadQuizzes">
            <option value="">Tất cả danh mục</option>
            <option v-for="cat in categories" :key="cat.maDanhMuc" :value="cat.maDanhMuc">
              {{ cat.tenDanhMuc }}
            </option>
          </select>
          <select v-model="quizFilter.sortBy" class="input-field md:w-40" @change="loadQuizzes">
            <option value="NgayTao">Mới nhất</option>
            <option value="TongLuotLamBai">Làm nhiều nhất</option>
            <option value="XepHangTrungBinh">Đánh giá cao</option>
          </select>
        </div>
      </div>

      <div class="mb-4 flex items-center justify-between">
        <p class="text-sm text-gray-600">
         <span class="font-bold text-primary-600 text-base">{{ quizCount }}</span> bài thi
        </p>
      </div>

      <!-- Quizzes Table -->
      <!-- Quizzes Table (Đã sửa để khớp với API mới) -->
      <div class="card overflow-hidden">
        <div class="overflow-x-auto">
          <table class="w-full">
            <thead class="bg-gray-50 border-b border-gray-200">
              <tr>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Bài thi</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Người tạo</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Lượt làm</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Ngày tạo</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Đánh giá</th>
                <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">Thao tác</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-200">
              
              <!-- Loading -->
              <tr v-if="loadingQuizzes">
                <td colspan="6" class="px-6 py-12 text-center">
                  <ArrowPathIcon class="w-8 h-8 text-primary-600 animate-spin mx-auto" />
                </td>
              </tr>

              <!-- Empty -->
              <tr v-else-if="quizzes.length === 0">
                <td colspan="6" class="px-6 py-12 text-center text-gray-500">
                  Không tìm thấy bài thi nào.
                </td>
              </tr>

              <!-- Data Rows -->
              <tr v-for="quiz in quizzes" :key="quiz.id" class="hover:bg-gray-50">
                <!-- quiz.id thay vì quiz.maBaiThi -->
                
                <td class="px-6 py-4">
                  <div class="font-medium text-gray-900">{{ quiz.tieuDe }}</div>
                  
                  <!-- SỬA: ContentManagementDto không có soCauHoi, tạm thời ẩn đi hoặc hiển thị danh mục -->
                  <div class="text-sm text-gray-500 mt-1" v-if="quiz.danhMuc">
                    <span class="px-2 py-0.5 bg-gray-100 rounded text-xs">{{ quiz.danhMuc }}</span>
                  </div>
                </td>

                <td class="px-6 py-4 text-sm text-gray-500">{{ quiz.tenNguoiTao }}</td>
                <td class="px-6 py-4 text-sm text-gray-500">{{ quiz.tongLuotLamBai || 0 }}</td>
                <td class="px-6 py-4 text-sm text-gray-500">{{ formatDate(quiz.ngayTao) }}</td>
                
                <!-- Cột Đánh giá -->
                <td class="px-6 py-4 text-sm">
                  <div v-if="quiz.xepHangTrungBinh > 0" class="flex items-center text-yellow-500 font-medium">
                    <span>{{ quiz.xepHangTrungBinh }}</span>
                    <StarIcon class="w-4 h-4 ml-1 fill-current" />
                  </div>
                  <span v-else class="text-gray-400 text-xs">Chưa có</span>
                </td>

                <td class="px-6 py-4 text-right">
                  <div class="flex justify-end gap-2">
                    <!-- params id: quiz.id -->
                    <router-link 
                      :to="{ name: 'quiz-detail', params: { id: quiz.id } }"
                      class="p-2 text-gray-400 hover:text-primary-600 hover:bg-primary-50 rounded-lg"
                      title="Xem chi tiết"
                    >
                      <EyeIcon class="w-5 h-5" />
                    </router-link>
                    
                    <!-- Hàm xóa dùng quiz (để lấy id bên trong) -->
                    <button @click="confirmDeleteQuiz(quiz)"
                      class="p-2 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg" title="Xóa bài thi">
                      <TrashIcon class="w-5 h-5" />
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

    <!-- Pagination BÀI THI  -->
    <div v-if="quizPagination.totalPages > 1" class="mt-8 flex justify-center pb-8">
      <nav class="flex items-center gap-2">
        <!-- Nút Trước -->
        <button 
          @click="changeQuizPage(quizPagination.currentPage - 1)" 
          :disabled="quizPagination.currentPage === 1" 
          :class="[
            'px-3 py-2 rounded border transition-colors',
            quizPagination.currentPage === 1
              ? 'bg-gray-100 text-gray-400 border-gray-200 cursor-not-allowed'
              : 'bg-white border-gray-300 text-gray-700 hover:bg-gray-50'
          ]"
        >
          <ChevronLeftIcon class="w-5 h-5" />
        </button>

        <!-- Thông tin trang -->
        <span class="px-4 py-2 text-gray-700 font-medium">
          Trang {{ quizPagination.currentPage }} / {{ quizPagination.totalPages }}
        </span>

        <!-- Nút Sau -->
        <button 
          @click="changeQuizPage(quizPagination.currentPage + 1)"
          :disabled="quizPagination.currentPage === quizPagination.totalPages" 
          :class="[
            'px-3 py-2 rounded border transition-colors',
            quizPagination.currentPage === quizPagination.totalPages
              ? 'bg-gray-100 text-gray-400 border-gray-200 cursor-not-allowed'
              : 'bg-white border-gray-300 text-gray-700 hover:bg-gray-50'
          ]"
        >
          <ChevronRightIcon class="w-5 h-5" />
        </button>

        <!-- Ô nhập trang -->
        <div class="flex items-center gap-1 ml-2">
          <input 
            type="number" 
            v-model.number="quizPageInput" 
            @keyup.enter="goToQuizPageInput" 
            min="1"
            :max="quizPagination.totalPages" 
            placeholder="Trang"
            class="w-16 px-2 py-2 text-sm border border-gray-300 rounded focus:ring-primary-500 focus:border-primary-500 text-center" 
          />
          <button 
            @click="goToQuizPageInput" 
            class="px-3 py-2 text-sm bg-white border border-gray-300 text-gray-700 rounded hover:bg-gray-50 transition-colors"
          >
            Đi
          </button>
        </div>
      </nav>
    </div>
    </div>

    <div v-if="activeTab === 'questions'">
      <!-- Filters -->
      <div class="card p-4 mb-6">
        <div class="flex flex-col md:flex-row gap-4">
          <div class="flex-1 relative">
            <MagnifyingGlassIcon class="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
            <input v-model="questionSearch" type="text" placeholder="Tìm kiếm câu hỏi..." class="input-field pl-10" @input="debouncedQuestionSearch" />
          </div>
          <input v-model="questionFilter.tenNguoiTao" type="text" placeholder="Tên người tạo..." class="input-field md:w-48" @input="debouncedQuestionSearch" />
          <select v-model="questionFilter.maDanhMuc" class="input-field md:w-48" @change="loadQuestions">
            <option value="">Tất cả danh mục</option>
            <option v-for="cat in categories" :key="cat.maDanhMuc" :value="cat.maDanhMuc">
              {{ cat.tenDanhMuc }}
            </option>
          </select>
        </div>
      </div>

      <div class="mb-4 flex items-center justify-between">
        <p class="text-sm text-gray-600">
          <span class="font-bold text-primary-600 text-base">{{ questionCount }}</span> câu hỏi
        </p>
      </div>

      <!-- Questions List -->
      <div class="space-y-4">
        <div v-if="loadingQuestions" class="card p-8 text-center">
          <ArrowPathIcon class="w-8 h-8 text-primary-600 animate-spin mx-auto" />
        </div>
        <div v-else-if="questions.length === 0" class="card p-12 text-center text-gray-500">
           Không tìm thấy câu hỏi nào.
        </div>
        <div v-for="question in questions" :key="question.id" class="card p-4 hover:shadow-md transition-shadow">
          <div class="flex items-start gap-4">
            <div class="flex-1">
              <p class="font-medium text-gray-900 mb-2">{{ question.tieuDe }}</p>
              <p class="text-xs text-gray-500 mb-2">
                Người tạo: <span class="font-medium text-gray-700">{{ question.tenNguoiTao }}</span>
            </p>
              <div class="flex items-center gap-3 text-sm text-gray-500 mt-2">
                <span v-if="question.danhMuc" class="px-2 py-1 bg-gray-100 text-gray-600 rounded text-xs">
                  {{ question.danhMuc }}
                </span>
                <span :class="[
                  'badge',
                  question.trangThai === 'CongKhai' ? 'badge-success' :
                    question.trangThai === 'DaXoa' ? 'badge-error' : 'badge-warning'
                ]">
                  {{ question.trangThai === 'CongKhai' ? 'Công khai' : question.trangThai === 'DaXoa' ? 'Đã xóa' :
                    'Riêng tư' }}
                </span>
                <span>{{ formatDate(question.ngayTao) }}</span>
              </div>
            </div>
            <div class="flex gap-2">
              <button @click="openQuestionDetail(question)"
                class="p-2 text-gray-400 hover:text-primary-600 hover:bg-primary-50 rounded-lg"
                title="Xem chi tiết">
                <EyeIcon class="w-5 h-5" />
              </button>
              <button @click="confirmDeleteQuestion(question)"
                class="p-2 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg">
                <TrashIcon class="w-5 h-5" />
              </button>

            </div>
          </div>
        </div>
      </div>

    <!-- Pagination CÂU HỎI -->
    <div v-if="questionPagination.totalPages > 1" class="mt-8 flex justify-center pb-8">
      <nav class="flex items-center gap-2">

        <button 
          @click="changeQuestionPage(questionPagination.currentPage - 1)" 
          :disabled="questionPagination.currentPage === 1" 
          :class="[
            'px-3 py-2 rounded border transition-colors',
            questionPagination.currentPage === 1
              ? 'bg-gray-100 text-gray-400 border-gray-200 cursor-not-allowed'
              : 'bg-white border-gray-300 text-gray-700 hover:bg-gray-50'
          ]"
        >
          <ChevronLeftIcon class="w-5 h-5" />
        </button>

        <span class="px-4 py-2 text-gray-700 font-medium">
          Trang {{ questionPagination.currentPage }} / {{ questionPagination.totalPages }}
        </span>

        <button 
          @click="changeQuestionPage(questionPagination.currentPage + 1)"
          :disabled="questionPagination.currentPage === questionPagination.totalPages" 
          :class="[
            'px-3 py-2 rounded border transition-colors',
            questionPagination.currentPage === questionPagination.totalPages
              ? 'bg-gray-100 text-gray-400 border-gray-200 cursor-not-allowed'
              : 'bg-white border-gray-300 text-gray-700 hover:bg-gray-50'
          ]"
        >
          <ChevronRightIcon class="w-5 h-5" />
        </button>

        <div class="flex items-center gap-1 ml-2">
          <input 
            type="number" 
            v-model.number="questionPageInput" 
            @keyup.enter="goToQuestionPageInput" 
            min="1"
            :max="questionPagination.totalPages" 
            placeholder="Trang"
            class="w-16 px-2 py-2 text-sm border border-gray-300 rounded focus:ring-primary-500 focus:border-primary-500 text-center" 
          />
          <button 
            @click="goToQuestionPageInput" 
            class="px-3 py-2 text-sm bg-white border border-gray-300 text-gray-700 rounded hover:bg-gray-50 transition-colors"
          >
            Đi
          </button>
        </div>
      </nav>
    </div>
    </div>

    <!-- Question Detail Modal -->
    <TransitionRoot appear :show="showQuestionDetailModal" as="template">
      <Dialog as="div" @close="showQuestionDetailModal = false" class="relative z-50">
        <TransitionChild
          enter="ease-out duration-300"
          enter-from="opacity-0"
          enter-to="opacity-100"
          leave="ease-in duration-200"
          leave-from="opacity-100"
          leave-to="opacity-0"
        >
          <div class="fixed inset-0 bg-black/30" />
        </TransitionChild>

        <div class="fixed inset-0 overflow-y-auto">
          <div class="flex min-h-full items-center justify-center p-4">
            <TransitionChild
              enter="ease-out duration-300"
              enter-from="opacity-0 scale-95"
              enter-to="opacity-100 scale-100"
              leave="ease-in duration-200"
              leave-from="opacity-100 scale-100"
              leave-to="opacity-0 scale-95"
            >
              <DialogPanel class="w-[640px] max-w-[95vw] transform bg-white rounded-xl shadow-xl transition-all">
                <!-- Header -->
                <div class="p-5 border-b border-gray-100 flex items-center justify-between">
                  <DialogTitle class="text-lg font-semibold text-gray-900 flex items-center gap-2">
                    <QuestionMarkCircleIcon class="w-5 h-5 text-primary-600" />
                    Chi tiết câu hỏi
                  </DialogTitle>
                  <button type="button" @click="showQuestionDetailModal = false" class="p-2 rounded-lg hover:bg-gray-100">
                    <svg class="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" /></svg>
                  </button>
                </div>
                <!-- Body -->
                <div class="p-6 overflow-y-auto" style="min-height: 340px; max-height: 65vh;">
                  <div class="space-y-4">
                    <div>
                      <label class="block text-sm font-medium text-gray-700 mb-1">Nội dung câu hỏi</label>
                      <div class="input-field-sm bg-gray-50 py-2 whitespace-pre-wrap break-words">{{ selectedQuestion?.noiDungCauHoi || selectedQuestion?.tieuDe }}</div>
                    </div>

                    <div>
                      <label class="block text-sm font-medium text-gray-700 mb-2">Các lựa chọn ({{ selectedQuestion?.cacLuaChon?.length || 0 }} đáp án)</label>
                      <div class="space-y-2">
                        <div v-for="(option, index) in selectedQuestion?.cacLuaChon || []" :key="option.maLuaChon || index" :class="[
                          'flex gap-2 p-2 rounded-lg',
                          option.laDapAnDung ? 'bg-green-50' : 'bg-gray-50'
                        ]">
                          <div class="flex items-center gap-2 self-center shrink-0">
                            <span class="w-6 h-6 rounded-full bg-gray-200 flex items-center justify-center text-xs font-medium text-gray-600">
                              {{ String.fromCharCode(65 + index) }}
                            </span>
                            <span :class="[
                              'w-6 h-6 rounded-full flex items-center justify-center',
                              option.laDapAnDung ? 'bg-green-500 text-white' : 'border-2 border-gray-300'
                            ]">
                              <svg v-if="option.laDapAnDung" class="w-4 h-4" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7" /></svg>
                            </span>
                          </div>
                          <div class="input-field-sm flex-1 min-w-0 bg-transparent py-2 whitespace-pre-wrap break-all">{{ option.noiDung || option.noiDungDapAn }}</div>
                        </div>
                      </div>
                    </div>

                    <div v-if="selectedQuestion?.giaiThich">
                      <label class="block text-sm font-medium text-gray-700 mb-1">Giải thích</label>
                      <div class="input-field-sm bg-gray-50 min-h-[38px] flex items-center">{{ selectedQuestion.giaiThich }}</div>
                    </div>
                  </div>
                </div>
                <!-- Footer -->
                <div class="p-4 border-t border-gray-100 flex justify-between items-center bg-gray-50 rounded-b-xl">
                  <button type="button" @click="showQuestionDetailModal = false" class="btn-secondary px-5">Đóng</button>
                </div>
              </DialogPanel>
            </TransitionChild>
          </div>
        </div>
      </Dialog>
    </TransitionRoot>

    <!-- Delete Confirmation Modal (Giữ nguyên) -->
    <TransitionRoot appear :show="showDeleteModal" as="template">
      <Dialog as="div" @close="showDeleteModal = false" class="relative z-50">
        <TransitionChild enter="duration-300 ease-out" enter-from="opacity-0" enter-to="opacity-100"
          leave="duration-200 ease-in" leave-from="opacity-100" leave-to="opacity-0">
          <div class="fixed inset-0" />
        </TransitionChild>

        <div class="fixed inset-0 overflow-y-auto">
          <div class="flex min-h-full items-center justify-center p-4">
            <TransitionChild enter="duration-300 ease-out" enter-from="opacity-0 scale-95"
              enter-to="opacity-100 scale-100" leave="duration-200 ease-in" leave-from="opacity-100 scale-100"
              leave-to="opacity-0 scale-95">
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
  ArrowPathIcon,
  StarIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  QuestionMarkCircleIcon
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
  pageSize: 10,
  tenNguoiTao: ''
})

const questionFilter = reactive({
  maDanhMuc: '',
  pageNumber: 1,
  pageSize: 10,
  tenNguoiTao: ''
})

const showDeleteModal = ref(false)
const deleteType = ref('')
const itemToDelete = ref(null)

const questionPagination = reactive({
  currentPage: 1,
  totalPages: 1,
  totalCount: 0
})
const questionPageInput = ref(1)

const formatDate = (dateString) => {
  if (!dateString) return ''
  return new Date(dateString).toLocaleDateString('vi-VN')
}

const quizPagination = reactive({
  currentPage: 1,
  totalPages: 1,
  totalCount: 0
})
const quizPageInput = ref(1)

const changeQuizPage = (page) => {
  if (page < 1 || page > quizPagination.totalPages) return
  quizFilter.pageNumber = page
  loadQuizzes()
}

const goToQuizPageInput = () => {
  let page = parseInt(quizPageInput.value)
  if (!page || page < 1) page = 1
  if (page > quizPagination.totalPages) page = quizPagination.totalPages

  quizPageInput.value = page
  changeQuizPage(page)
}

const showQuestionDetailModal = ref(false)
const selectedQuestion = ref(null)
const openQuestionDetail = async (question) => {
  try {
    const res = await questionService.getQuestionAnswerByAdmin(question.id)
    selectedQuestion.value = res.data || res
    showQuestionDetailModal.value = true
  } catch (e) {
    toast.error('Không lấy được chi tiết câu hỏi')
  }
}

const loadQuizzes = async () => {
  loadingQuizzes.value = true
  try {
    const response = await adminService.getAllContent({
      loaiNoiDung: 'BaiThi',
      searchTerm: quizSearch.value || undefined,
      tenNguoiTao: quizFilter.tenNguoiTao || undefined,
      pageNumber: quizFilter.pageNumber,
      pageSize: quizFilter.pageSize,
      maDanhMuc: quizFilter.maDanhMuc || undefined,
      sortBy: quizFilter.sortBy,
      sortOrder: 'DESC'
    })
    quizzes.value = response.data || []
    if (response.pagination) {
      quizPagination.totalCount = response.pagination.totalCount
      quizPagination.currentPage = response.pagination.currentPage
      quizPagination.totalPages = response.pagination.totalPages
      quizPageInput.value = quizPagination.currentPage
      quizCount.value = response.pagination.totalCount // Cập nhật số trên Tab
    }
  } catch (error) {
    console.error('Failed to load quizzes:', error)
  } finally {
    loadingQuizzes.value = false
  }
}

const loadQuestions = async () => {
  loadingQuestions.value = true
  try {
    const response = await adminService.getAllContent({
      loaiNoiDung: 'CauHoi',
      searchTerm: questionSearch.value || undefined,
      tenNguoiTao: questionFilter.tenNguoiTao || undefined,
      pageNumber: questionFilter.pageNumber,
      pageSize: questionFilter.pageSize,
      maDanhMuc: questionFilter.maDanhMuc || undefined,
      daXoa: false
    })

    questions.value = response.data || []

    if (response.pagination) {
      questionPagination.totalCount = response.pagination.totalCount
      questionPagination.currentPage = response.pagination.currentPage
      questionPagination.totalPages = response.pagination.totalPages
      questionPageInput.value = questionPagination.currentPage
      questionCount.value = response.pagination.totalCount
    }

  } catch (error) {
    console.error('Failed to load questions:', error)
    toast.error("Lỗi tải câu hỏi")
  } finally {
    loadingQuestions.value = false
  }
}

// Hàm chuyển trang
const changeQuestionPage = (page) => {
  if (page < 1 || page > questionPagination.totalPages) return
  questionFilter.pageNumber = page
  loadQuestions()
}

// Hàm đi tới trang nhập trong ô input
const goToQuestionPageInput = () => {
  let page = parseInt(questionPageInput.value)
  if (!page) page = 1
  if (page < 1) page = 1
  if (page > questionPagination.totalPages) page = questionPagination.totalPages

  questionPageInput.value = page // Reset lại số trong ô nếu nhập sai
  changeQuestionPage(page)
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
    // Kiểm tra xem item có tồn tại không
    if (!itemToDelete.value || !itemToDelete.value.id) {
      toast.error("Lỗi: Không tìm thấy ID đối tượng cần xóa");
      return;
    }

    if (deleteType.value === 'quiz') {
      await adminService.deleteContent('BaiThi', itemToDelete.value.id)

      toast.success('Đã xóa bài thi thành công')
      loadQuizzes() // Tải lại danh sách
    } else {
      await adminService.deleteContent('CauHoi', itemToDelete.value.id)

      loadQuestions() // Tải lại danh sách
    }

    showDeleteModal.value = false
  } catch (error) {
    console.error("Lỗi xóa:", error);
    toast.error('Không thể xóa. ' + (error.response?.data?.message || 'Lỗi hệ thống'))
  }
}
onMounted(() => {
  loadCategories()
  loadQuizzes()
  loadQuestions()
})
</script>
