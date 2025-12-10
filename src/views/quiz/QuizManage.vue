<template>
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <!-- Header -->
    <div class="flex items-center justify-between mb-8">
      <div>
        <h1 class="page-header">Quản lý bài thi</h1>
        <p class="text-gray-600 mt-1">Quản lý các bài thi bạn đã tạo</p>
      </div>
      <div class="flex gap-3">
        <router-link to="/my-quizzes/participants" class="btn-secondary">
          <UsersIcon class="w-5 h-5 mr-2" />
          Người làm bài
        </router-link>
        <router-link to="/quiz/create" class="btn-primary">
          <PlusIcon class="w-5 h-5 mr-2" />
          Tạo bài thi mới
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
            placeholder="Tìm kiếm bài thi..."
            class="input-field pl-10"
            @input="debouncedSearch"
          />
        </div>
        <select v-model="filter.maDanhMuc" class="input-field md:w-48" @change="loadQuizzes">
          <option :value="null">Tất cả danh mục</option>
          <option v-for="cat in categories" :key="cat.maDanhMuc" :value="cat.maDanhMuc">
            {{ cat.tenDanhMuc }}
          </option>
        </select>
        <select v-model="filter.sortBy" class="input-field md:w-40" @change="loadQuizzes">
          <option value="NgayTao">Ngày tạo</option>
          <option value="TongLuotLamBai">Lượt làm</option>
          <option value="TieuDe">Tên bài thi</option>
        </select>
      </div>
    </div>

    <!-- Bulk Actions Bar -->
    <div class="card p-4 mb-6 bg-primary-50 border-primary-200">
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-3">
        <div class="flex items-center gap-3">
          <span class="text-primary-700 font-medium">
            Đã chọn {{ selectedQuizzes.length }} / {{ pagination.totalCount }} bài thi
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
            {{ isAllPagesSelected ? '✓ Đã chọn tất cả (' + pagination.totalCount + ')' : 'Chọn tất cả (' + pagination.totalCount + ')' }}
          </button>
          <button @click="clearSelection" class="btn-secondary text-sm">
            Bỏ chọn
          </button>
          <button @click="confirmBulkDelete" class="btn-danger text-sm">
            <TrashIcon class="w-4 h-4 mr-1" />
            Xóa {{ selectedQuizzes.length }} bài
          </button>
        </div>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div v-for="i in 6" :key="i" class="card animate-pulse">
        <div class="h-32 bg-gray-200"></div>
        <div class="p-4 space-y-3">
          <div class="h-4 bg-gray-200 rounded w-3/4"></div>
          <div class="h-3 bg-gray-200 rounded w-1/2"></div>
        </div>
      </div>
    </div>

    <!-- Quiz List -->
    <div v-else-if="quizzes.length > 0" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div v-for="quiz in quizzes" :key="quiz.maBaiThi" :class="[
        'card group overflow-visible relative',
        selectedQuizzes.includes(quiz.maBaiThi) ? 'ring-2 ring-primary-500 bg-primary-50' : ''
      ]">
        <!-- Checkbox overlay -->
        <div class="absolute top-3 left-3 z-20">
          <button
            @click="toggleSelect(quiz.maBaiThi)"
            :class="[
              'w-6 h-6 rounded-full border-2 flex items-center justify-center cursor-pointer shadow-md transition-all',
              selectedQuizzes.includes(quiz.maBaiThi)
                ? 'bg-primary-600 border-primary-600 text-white'
                : 'bg-white border-gray-400 hover:border-primary-500'
            ]"
          >
            <CheckIcon v-if="selectedQuizzes.includes(quiz.maBaiThi)" class="w-4 h-4" />
          </button>
        </div>
        
        <div class="relative h-32 bg-gradient-to-br from-primary-500 to-secondary-500 rounded-t-xl overflow-hidden ml-0">
          <img 
            v-if="quiz.anhBia" 
            :src="quiz.anhBia" 
            :alt="quiz.tieuDe"
            class="w-full h-full object-cover"
          />
          <div class="absolute top-2 right-2 flex gap-2 z-10">
            <span v-if="quiz.cheDoCongKhai === 'CongKhai'" class="badge badge-success">
    Công khai
  </span>

  <span v-else-if="quiz.cheDoCongKhai === 'RiengTu'" class="badge bg-gray-100 text-gray-700">
    Riêng tư
  </span>

  <span v-else class="badge bg-yellow-100 text-yellow-700">
    Có mật khẩu
  </span>
          </div>
        </div>

        <div class="p-4">
          <h3 class="font-semibold text-gray-900 line-clamp-1 mb-2">{{ quiz.tieuDe }}</h3>
          
          <div class="flex items-center gap-4 text-sm text-gray-500 mb-3">
            <span class="flex items-center">
              <DocumentTextIcon class="w-4 h-4 mr-1" />
              {{ quiz.soCauHoi }} câu
            </span>
            <span class="flex items-center">
              <UserGroupIcon class="w-4 h-4 mr-1" />
              {{ quiz.tongLuotLamBai || 0 }} lượt
            </span>
            <span class="flex items-center">
              <CalendarIcon class="w-4 h-4 mr-1" />
              {{ formatDate(quiz.ngayTao) }}
            </span>
          </div>

          <!-- Access Code -->
          <div class="flex items-center gap-2 p-2 bg-gray-50 rounded-lg mb-3">
            <span class="text-xs text-gray-500">Mã:</span>
            <code class="text-sm font-mono font-medium text-primary-600">{{ quiz.maTruyCapDinhDanh }}</code>
            <button 
              @click="copyCode(quiz.maTruyCapDinhDanh)"
              class="ml-auto p-1 hover:bg-gray-200 rounded"
            >
              <ClipboardDocumentIcon class="w-4 h-4 text-gray-500" />
            </button>
          </div>

          <!-- Actions -->
          <div class="flex gap-2">
            <router-link 
              :to="{ name: 'quiz-detail', params: { id: quiz.maBaiThi } }"
              class="btn-secondary flex-1 text-sm justify-center"
            >
              <EyeIcon class="w-4 h-4 mr-1" />
              Xem
            </router-link>
            <router-link 
              :to="{ name: 'quiz-edit', params: { id: quiz.maBaiThi } }"
              class="btn-secondary flex-1 text-sm justify-center"
            >
              <PencilIcon class="w-4 h-4 mr-1" />
              Sửa
            </router-link>
            <Menu as="div" class="relative z-20">
              <MenuButton class="btn-secondary text-sm px-2">
                <EllipsisVerticalIcon class="w-5 h-5" />
              </MenuButton>
              <transition
                enter-active-class="transition duration-100 ease-out"
                enter-from-class="transform scale-95 opacity-0"
                enter-to-class="transform scale-100 opacity-100"
                leave-active-class="transition duration-75 ease-in"
                leave-from-class="transform scale-100 opacity-100"
                leave-to-class="transform scale-95 opacity-0"
              >
                <MenuItems class="absolute right-0 mt-2 w-48 bg-white rounded-lg shadow-lg ring-1 ring-black ring-opacity-5 z-50">
                  <MenuItem v-slot="{ active }">
                    <router-link
                      :to="{ name: 'quiz-participants', params: { id: quiz.maBaiThi } }"
                      :class="[active ? 'bg-gray-50' : '', 'flex w-full items-center px-4 py-2 text-sm text-gray-700']"
                    >
                      Xem người làm bài
                    </router-link>
                  </MenuItem>
                  <MenuItem v-slot="{ active }">
                    <button
                      @click="viewQuizQuestions(quiz)"
                      :class="[active ? 'bg-gray-50' : '', 'flex w-full items-center px-4 py-2 text-sm text-gray-700']"
                    >
                      Xem câu hỏi và đáp án
                    </button>
                  </MenuItem>
                  <MenuItem v-slot="{ active }">
                    <button
                      @click="toggleVisibility(quiz)"
                      :class="[active ? 'bg-gray-50' : '', 'flex w-full items-center px-4 py-2 text-sm text-gray-700']"
                    >
                      {{ quiz.cheDoCongKhai === 'CongKhai' ? 'Chuyển riêng tư' : 'Chuyển công khai' }}
                    </button>
                  </MenuItem>
                  <MenuItem v-slot="{ active }">
                    <button
                      @click="confirmDelete(quiz)"
                      :class="[active ? 'bg-gray-50' : '', 'flex w-full items-center px-4 py-2 text-sm text-red-600']"
                    >
                      Xóa bài thi
                    </button>
                  </MenuItem>
                </MenuItems>
              </transition>
            </Menu>
          </div>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="text-center py-16">
      <DocumentTextIcon class="w-16 h-16 text-gray-300 mx-auto mb-4" />
      <h3 class="text-lg font-medium text-gray-900 mb-2">Chưa có bài thi nào</h3>
      <p class="text-gray-500 mb-6">Bắt đầu tạo bài thi đầu tiên của bạn</p>
      <router-link to="/quiz/create" class="btn-primary">
        <PlusIcon class="w-5 h-5 mr-2" />
        Tạo bài thi mới
      </router-link>
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
                    Bạn có chắc chắn muốn xóa <strong>{{ selectedQuizzes.length }}</strong> bài thi đã chọn?
                    Hành động này không thể hoàn tác.
                  </template>
                  <template v-else>
                    Bạn có chắc chắn muốn xóa bài thi "{{ quizToDelete?.tieuDe }}"?
                    Hành động này không thể hoàn tác.
                  </template>
                </p>
                <div class="mt-6 flex justify-end gap-3">
                  <button @click="showDeleteModal = false" class="btn-secondary">
                    Hủy
                  </button>
                  <button @click="isBulkDelete ? bulkDeleteQuizzes() : deleteQuiz()" class="btn-danger">
                    Xóa {{ isBulkDelete ? selectedQuizzes.length + ' bài' : '' }}
                  </button>
                </div>
              </DialogPanel>
            </TransitionChild>
          </div>
        </div>
      </Dialog>
    </TransitionRoot>

    <!-- Questions Modal -->
    <TransitionRoot appear :show="showQuestionsModal" as="template">
      <Dialog as="div" @close="showQuestionsModal = false" class="relative z-50">
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
              <DialogPanel class="w-full max-w-4xl transform rounded-2xl bg-white shadow-xl transition-all h-[700px] flex flex-col">
                <div class="p-6 border-b flex items-center justify-between shrink-0">
                  <DialogTitle as="h3" class="text-lg font-semibold text-gray-900">
                    Câu hỏi trong bài thi: {{ selectedQuiz?.tieuDe }}
                  </DialogTitle>
                  <button @click="showQuestionsModal = false" class="text-gray-400 hover:text-gray-600">
                    <XMarkIcon class="w-6 h-6" />
                  </button>
                </div>
                
                <div class="flex-1 overflow-hidden flex">
                  <div v-if="loadingQuestions" class="flex-1 flex items-center justify-center">
                    <div class="text-center py-8">
                      <ArrowPathIcon class="w-8 h-8 text-primary-600 animate-spin mx-auto" />
                      <p class="text-gray-500 mt-2">Đang tải câu hỏi...</p>
                    </div>
                  </div>
                  
                  <div v-else-if="quizQuestions.length === 0" class="flex-1 flex items-center justify-center">
                    <div class="text-center py-8">
                      <DocumentTextIcon class="w-12 h-12 text-gray-300 mx-auto" />
                      <p class="text-gray-500 mt-2">Bài thi chưa có câu hỏi nào</p>
                    </div>
                  </div>
                  
                  <template v-else>
                    <!-- Left sidebar - Question numbers -->
                    <div class="w-48 border-r overflow-y-auto p-3 bg-gray-50">
                      <div class="grid grid-cols-5 gap-1.5">
                        <button
                          v-for="(question, qIndex) in quizQuestions"
                          :key="question.maCauHoi || qIndex"
                          @click="viewingQuestionIndex = qIndex"
                          :class="[
                            'w-8 h-8 rounded-lg text-sm font-medium transition-colors flex items-center justify-center',
                            viewingQuestionIndex === qIndex
                              ? 'bg-primary-600 text-white'
                              : 'bg-white border border-gray-200 text-gray-700 hover:bg-primary-50 hover:border-primary-300'
                          ]"
                        >
                          {{ qIndex + 1 }}
                        </button>
                      </div>
                    </div>

                    <!-- Right content - Question detail -->
                    <div class="flex-1 overflow-y-auto p-6 flex flex-col">
                      <div v-if="currentViewingQuestion" class="flex flex-col h-full">
                        <!-- Navigation buttons - Fixed at top -->
                        <div class="flex justify-between items-center pb-4 border-b mb-4 shrink-0">
                          <button
                            @click="viewingQuestionIndex = Math.max(0, viewingQuestionIndex - 1)"
                            :disabled="viewingQuestionIndex === 0"
                            :class="[
                              'flex items-center gap-1 px-3 py-1.5 rounded text-sm',
                              viewingQuestionIndex === 0 
                                ? 'text-gray-400 cursor-not-allowed' 
                                : 'text-gray-700 hover:bg-gray-100'
                            ]"
                          >
                            <ChevronLeftIcon class="w-4 h-4" />
                            Câu trước
                          </button>
                          <span class="text-sm text-gray-500">
                            Câu {{ viewingQuestionIndex + 1 }} / {{ quizQuestions.length }}
                          </span>
                          <button
                            @click="viewingQuestionIndex = Math.min(quizQuestions.length - 1, viewingQuestionIndex + 1)"
                            :disabled="viewingQuestionIndex === quizQuestions.length - 1"
                            :class="[
                              'flex items-center gap-1 px-3 py-1.5 rounded text-sm',
                              viewingQuestionIndex === quizQuestions.length - 1 
                                ? 'text-gray-400 cursor-not-allowed' 
                                : 'text-gray-700 hover:bg-gray-100'
                            ]"
                          >
                            Câu sau
                            <ChevronRightIcon class="w-4 h-4" />
                          </button>
                        </div>

                        <!-- Question content - Scrollable -->
                        <div class="flex-1 overflow-y-auto space-y-4">
                        <!-- Question header -->
                        <div class="flex items-start gap-3">
                          <span class="w-10 h-10 rounded-full bg-primary-100 text-primary-700 flex items-center justify-center text-base font-semibold shrink-0">
                            {{ viewingQuestionIndex + 1 }}
                          </span>
                          <div class="flex-1">
                            <p class="text-gray-900 font-medium whitespace-pre-wrap text-base">{{ currentViewingQuestion.noiDungCauHoi }}</p>
                            <div class="flex gap-2 mt-2">
                              <span :class="[
                                'text-xs px-2 py-0.5 rounded',
                                currentViewingQuestion.mucDo === 'De' ? 'bg-green-100 text-green-700' :
                                currentViewingQuestion.mucDo === 'TrungBinh' ? 'bg-yellow-100 text-yellow-700' :
                                'bg-red-100 text-red-700'
                              ]">
                                {{ currentViewingQuestion.mucDo === 'De' ? 'Dễ' : currentViewingQuestion.mucDo === 'TrungBinh' ? 'Trung bình' : 'Khó' }}
                              </span>
                            </div>
                          </div>
                        </div>
                        
                        <!-- Answer options -->
                        <div class="ml-13 space-y-2">
                          <div 
                            v-for="(option, oIndex) in currentViewingQuestion.cacLuaChon" 
                            :key="oIndex"
                            :class="[
                              'flex items-center gap-3 p-3 rounded-lg',
                              option.laDapAnDung ? 'bg-green-50 border border-green-200' : 'bg-gray-50 border border-gray-200'
                            ]"
                          >
                            <span :class="[
                              'w-7 h-7 rounded-full flex items-center justify-center text-sm font-medium shrink-0',
                              option.laDapAnDung ? 'bg-green-500 text-white' : 'bg-gray-200 text-gray-600'
                            ]">
                              {{ String.fromCharCode(65 + oIndex) }}
                            </span>
                            <span :class="option.laDapAnDung ? 'text-green-700 font-medium' : 'text-gray-700'">
                              {{ option.noiDung }}
                            </span>
                            <CheckCircleIcon v-if="option.laDapAnDung" class="w-5 h-5 text-green-500 ml-auto" />
                          </div>
                        </div>
                        
                        <!-- Explanation -->
                        <div v-if="currentViewingQuestion.giaiThich" class="ml-13 p-4 bg-blue-50 rounded-lg border border-blue-100">
                          <p class="text-sm text-blue-800">
                            <span class="font-medium">Giải thích:</span> {{ currentViewingQuestion.giaiThich }}
                          </p>
                        </div>
                        </div>
                      </div>
                    </div>
                  </template>
                </div>
                
                <div class="p-4 border-t flex justify-end">
                  <button @click="showQuestionsModal = false" class="btn-secondary">
                    Đóng
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
import { useRouter } from 'vue-router'
import { useDebounceFn } from '@vueuse/core'
import { useToast } from 'vue-toastification'
import { Menu, MenuButton, MenuItems, MenuItem } from '@headlessui/vue'
import {
  Dialog,
  DialogPanel,
  DialogTitle,
  TransitionRoot,
  TransitionChild
} from '@headlessui/vue'
import {
  PlusIcon,
  MagnifyingGlassIcon,
  DocumentTextIcon,
  UserGroupIcon,
  UsersIcon,
  ClipboardDocumentIcon,
  EyeIcon,
  PencilIcon,
  EllipsisVerticalIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  CalendarIcon,
  TrashIcon,
  XMarkIcon,
  ArrowPathIcon,
  CheckCircleIcon,
  CheckIcon
} from '@heroicons/vue/24/outline'
import { quizService, categoryService } from '@/services'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const toast = useToast()
const authStore = useAuthStore()

const loading = ref(true)
const quizzes = ref([])
const categories = ref([])
const searchTerm = ref('')
const showDeleteModal = ref(false)
const quizToDelete = ref(null)
const selectedQuizzes = ref([])
const isBulkDelete = ref(false)

// Questions modal
const showQuestionsModal = ref(false)
const selectedQuiz = ref(null)
const quizQuestions = ref([])
const loadingQuestions = ref(false)
const viewingQuestionIndex = ref(0)

const currentViewingQuestion = computed(() => {
  return quizQuestions.value[viewingQuestionIndex.value] || null
})

const formatDate = (dateStr) => {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  return date.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })
}

const filter = reactive({
  maDanhMuc: null,
  sortBy: 'NgayTao',
  sortOrder: 'DESC',
  pageNumber: 1,
  pageSize: 12
})

const pagination = reactive({
  currentPage: 1,
  totalPages: 1,
  totalCount: 0
})

// Track if selecting all pages
const isAllPagesSelected = ref(false)
const allQuizIds = ref([])

const isAllOnPageSelected = computed(() => {
  return quizzes.value.length > 0 && quizzes.value.every(q => selectedQuizzes.value.includes(q.maBaiThi))
})

const loadQuizzes = async () => {
  loading.value = true
  try {
    const response = await quizService.getQuizzes({
      ...filter,
      timKiem: searchTerm.value,
      chiLayCuaToi: true  // Backend dùng chiLayCuaToi thay vì nguoiTaoId
    })
    quizzes.value = response.data || []
    pagination.currentPage = response.pagination?.currentPage || 1
    pagination.totalPages = response.pagination?.totalPages || 1
    pagination.totalCount = response.pagination?.totalCount || 0
  } catch (error) {
    console.error('Failed to load quizzes:', error)
    toast.error('Không thể tải danh sách bài thi')
  } finally {
    loading.value = false
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

const copyCode = (code) => {
  navigator.clipboard.writeText(code)
}

const viewQuizQuestions = async (quiz) => {
  selectedQuiz.value = quiz
  showQuestionsModal.value = true
  loadingQuestions.value = true
  quizQuestions.value = []
  viewingQuestionIndex.value = 0
  
  try {
    const response = await quizService.getQuestionsDetail(quiz.maBaiThi)
    if (response.success && response.data) {
      quizQuestions.value = response.data
    }
  } catch (error) {
    console.error('Failed to load quiz questions:', error)
    toast.error('Không thể tải danh sách câu hỏi')
  } finally {
    loadingQuestions.value = false
  }
}

const toggleVisibility = async (quiz) => {
  try {
    const response = await quizService.toggleVisibility(quiz.maBaiThi)
    if (response.success) {
      quiz.cheDoCongKhai = response.data.cheDoCongKhai
    } else {
      toast.error(response.message || 'Không thể cập nhật chế độ')
    }
  } catch (error) {
    const msg = error.response?.data?.message || 'Không thể cập nhật chế độ'
    toast.error(msg)
  }
}

const confirmDelete = (quiz) => {
  quizToDelete.value = quiz
  isBulkDelete.value = false
  showDeleteModal.value = true
}

const deleteQuiz = async () => {
  if (!quizToDelete.value) return
  
  try {
    await quizService.deleteQuiz(quizToDelete.value.maBaiThi)
    showDeleteModal.value = false
    quizToDelete.value = null
    loadQuizzes()
  } catch (error) {
    const msg = error.response?.data?.message || 'Không thể xóa bài thi'
    toast.error(msg)
    showDeleteModal.value = false
  }
}

// Bulk selection functions
const toggleSelect = (maBaiThi) => {
  const index = selectedQuizzes.value.indexOf(maBaiThi)
  if (index === -1) {
    selectedQuizzes.value.push(maBaiThi)
  } else {
    selectedQuizzes.value.splice(index, 1)
    isAllPagesSelected.value = false
  }
}

const selectAllOnPage = () => {
  if (isAllOnPageSelected.value) {
    // Deselect all on current page
    quizzes.value.forEach(q => {
      const index = selectedQuizzes.value.indexOf(q.maBaiThi)
      if (index !== -1) {
        selectedQuizzes.value.splice(index, 1)
      }
    })
    isAllPagesSelected.value = false
  } else {
    // Select all on current page
    quizzes.value.forEach(q => {
      if (!selectedQuizzes.value.includes(q.maBaiThi)) {
        selectedQuizzes.value.push(q.maBaiThi)
      }
    })
  }
}

const selectAllPages = async () => {
  if (isAllPagesSelected.value) {
    // Deselect all
    selectedQuizzes.value = []
    allQuizIds.value = []
    isAllPagesSelected.value = false
  } else {
    // Load all quiz IDs and select them
    try {
      const response = await quizService.getQuizzes({
        chiLayCuaToi: true,
        pageSize: 10000 // Get all
      })
      allQuizIds.value = (response.data || []).map(q => q.maBaiThi)
      selectedQuizzes.value = [...allQuizIds.value]
      isAllPagesSelected.value = true
    } catch (error) {
      console.error('Failed to load all quizzes:', error)
      toast.error('Không thể tải tất cả bài thi')
    }
  }
}

const clearSelection = () => {
  selectedQuizzes.value = []
  allQuizIds.value = []
  isAllPagesSelected.value = false
}

const confirmBulkDelete = () => {
  isBulkDelete.value = true
  showDeleteModal.value = true
}

const bulkDeleteQuizzes = async () => {
  if (selectedQuizzes.value.length === 0) return
  
  const totalToDelete = selectedQuizzes.value.length
  let successCount = 0
  let failCount = 0
  
  // Close modal immediately
  showDeleteModal.value = false
  
  // Delete quizzes one by one
  for (const id of selectedQuizzes.value) {
    try {
      await quizService.deleteQuiz(id)
      successCount++
    } catch (error) {
      console.error(`Failed to delete quiz ${id}:`, error)
      failCount++
    }
  }
  
  // Show result
  if (failCount === 0) {
    // Xóa thành công - không cần thông báo
  } else if (successCount > 0) {
    toast.warning(`Đã xóa ${successCount}/${totalToDelete} bài thi. ${failCount} bài không thể xóa.`)
  } else {
    toast.error('Không thể xóa bài thi. Vui lòng thử lại.')
  }
  
  selectedQuizzes.value = []
  loadQuizzes()
}

const loadCategories = async () => {
  try {
    const response = await categoryService.getCategories()
    if (response.success) {
      categories.value = response.data || []
    }
  } catch (error) {
    console.error('Failed to load categories:', error)
  }
}

onMounted(() => {
  loadCategories()
  loadQuizzes()
})
</script>
