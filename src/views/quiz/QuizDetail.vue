<template>
  <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <!-- Nút quay lại -->
    <button
      @click="goBack"
      class="mb-4 inline-flex items-center text-gray-600 hover:text-primary-600 transition-colors group"
    >
      <ArrowLeftIcon class="w-5 h-5 mr-2 group-hover:-translate-x-1 transition-transform" />
      <span>Quay lại</span>
    </button>

    <!-- Loading -->
    <div v-if="loading" class="card animate-pulse">
      <div class="h-48 bg-gray-200"></div>
      <div class="p-6 space-y-4">
        <div class="h-6 bg-gray-200 rounded w-3/4"></div>
        <div class="h-4 bg-gray-200 rounded w-1/2"></div>
      </div>
    </div>

    <!-- Error -->
    <div v-else-if="error" class="card p-8 text-center">
      <ExclamationCircleIcon class="w-16 h-16 text-red-500 mx-auto mb-4" />
      <h2 class="text-xl font-semibold text-gray-900 mb-2">Không tìm thấy bài thi</h2>
      <p class="text-gray-600 mb-4">{{ error }}</p>
      <router-link to="/explore" class="btn-primary">
        Khám phá bài thi khác
      </router-link>
    </div>

    <!-- Quiz Detail -->
    <div v-else-if="quiz" class="space-y-6">
      <!-- Header -->
      <div class="card overflow-hidden">
        <div class="h-48 bg-gradient-to-br from-primary-500 to-secondary-500 relative">
          <img 
            v-if="quiz.anhBia" 
            :src="quiz.anhBia" 
            :alt="quiz.tieuDe"
            class="w-full h-full object-cover"
          />
          <div class="absolute inset-0 bg-black/30"></div>
          <div class="absolute bottom-4 left-4 right-4">
            <span :class="[
              'badge',
              quiz.cheDoCongKhai === 'CongKhai' ? 'badge-success' : 
              quiz.cheDoCongKhai === 'CoMatKhau' ? 'bg-yellow-100 text-yellow-700' : 'bg-gray-100 text-gray-700'
            ]">
              {{ quiz.cheDoCongKhai === 'CongKhai' ? 'Công khai' : 
                 quiz.cheDoCongKhai === 'CoMatKhau' ? 'Có mật khẩu' : 'Riêng tư' }}
            </span>
          </div>
        </div>

        <div class="p-6">
          <div class="flex items-center gap-3 mb-2">
            <h1 class="text-2xl font-bold text-gray-900">{{ quiz.tieuDe }}</h1>
            <span class="inline-flex items-center px-2 py-1 bg-primary-100 text-primary-700 rounded text-sm font-mono">
              Mã truy cập: {{ quiz.maTruyCapDinhDanh }}
            </span>
          </div>
          <p v-if="quiz.moTa" class="text-gray-600 mb-4">{{ quiz.moTa }}</p>

          <!-- Stats -->
          <div class="flex flex-wrap gap-4 text-sm text-gray-500">
            <div class="flex items-center">
              <DocumentTextIcon class="w-5 h-5 mr-1" />
              {{ quiz.soCauHoi }} câu hỏi
            </div>
            <div class="flex items-center">
              <ClockIcon class="w-5 h-5 mr-1" />
              {{ quiz.thoiGianLamBai }} phút
            </div>
            <div class="flex items-center">
              <UserGroupIcon class="w-5 h-5 mr-1" />
              {{ quiz.tongLuotLamBai || 0 }} lượt làm
            </div>
            <div v-if="reviewStats.diemTrungBinh > 0" class="flex items-center">
              <div class="flex mr-1">
                <StarIcon v-for="s in 5" :key="s" :class="[
                  'w-5 h-5',
                  s <= Math.round(reviewStats.diemTrungBinh) ? 'text-yellow-400 fill-yellow-400' : 'text-gray-300'
                ]" />
              </div>
              <span class="font-medium">{{ reviewStats.diemTrungBinh.toFixed(1) }}</span>
              <span class="text-gray-400 ml-1">({{ reviewStats.tongDanhGia }} đánh giá)</span>
            </div>
          </div>

          <!-- Author -->
          <div class="flex items-center mt-4 pt-4 border-t border-gray-100">
            <div v-if="quiz?.anhDaiDienNguoiTao" class="w-10 h-10 rounded-full from-primary-500 to-secondary-500 flex items-center justify-center">
              <img :src="getAvatarUrl(quiz.anhDaiDienNguoiTao)" alt="Avatar" class="w-10 h-10 rounded-full object-cover" />
            </div>
            <div v-else class="w-10 h-10 rounded-full bg-gradient-to-br from-primary-500 to-secondary-500 flex items-center justify-center">
              <span class="text-white font-medium">{{ getInitials(quiz.tenNguoiTao) }}</span>
            </div>
            <div class="ml-3">
              <p class="font-medium text-gray-900">{{ quiz.tenNguoiTao }}</p>
              <p class="text-sm text-gray-500">Ngày tạo: {{ formatDate(quiz.ngayTao) }}</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Password Input (if needed) -->
      <div v-if="quiz.cheDoCongKhai === 'CoMatKhau' && !passwordVerified" class="card p-6">
        <h3 class="font-semibold text-gray-900 mb-4">
          <LockClosedIcon class="w-5 h-5 inline mr-2" />
          Bài thi yêu cầu mật khẩu
        </h3>
        <div class="flex gap-3">
          <input
            v-model="password"
            type="password"
            placeholder="Nhập mật khẩu bài thi"
            class="input-field flex-1"
            @keyup.enter="verifyPassword"
          />
          <button @click="verifyPassword" class="btn-primary" :disabled="verifying">
            {{ verifying ? 'Đang xác thực...' : 'Xác nhận' }}
          </button>
        </div>
        <p v-if="passwordError" class="mt-2 text-sm text-red-500">{{ passwordError }}</p>
      </div>

      <!-- Start Quiz -->
      <div v-if="quiz.cheDoCongKhai !== 'CoMatKhau' || passwordVerified" class="card p-6">
        <div v-if="!authStore.isLoggedIn" class="mb-4">
          <label class="block text-sm font-medium text-gray-700 mb-1">
            Tên của bạn (không bắt buộc)
          </label>
          <input
            v-model="guestName"
            type="text"
            placeholder="Nhập tên của bạn"
            class="input-field"
          />
        </div>

        <div class="flex flex-col sm:flex-row gap-4">
          <button
            @click="startQuiz"
            :disabled="starting"
            class="btn-primary flex-1 py-3 text-base"
          >
            <PlayIcon v-if="!starting" class="w-5 h-5 mr-2" />
            <ArrowPathIcon v-else class="w-5 h-5 mr-2 animate-spin" />
            {{ starting ? 'Đang bắt đầu...' : 'Bắt đầu làm bài' }}
          </button>
          
          <button @click="shareQuiz" class="btn-secondary py-3">
            <ShareIcon class="w-5 h-5 mr-2" />
            Chia sẻ
          </button>
        </div>

        <!-- Quiz Info -->
        <div class="mt-4 p-4 bg-blue-50 rounded-lg text-sm text-blue-800">
          <ul class="space-y-1">
            <li>• Thời gian làm bài: <strong>{{ quiz.thoiGianLamBai }} phút</strong></li>
            <li>• Số câu hỏi: <strong>{{ quiz.soCauHoi }} câu</strong></li>
            <li>• {{ quiz.hienThiDapAnSauKhiNop ? 'Xem đáp án sau khi nộp bài' : 'Không hiển thị đáp án' }}</li>
          </ul>
        </div>
      </div>

      <!-- Reviews Section -->
      <div class="card p-6">
        <div class="flex items-center justify-between mb-4">
          <div class="flex items-center gap-2">
            <h3 class="font-semibold text-gray-900">Đánh giá & Bình luận</h3>
            <span v-if="reviewStats.tongDanhGia > 0" class="text-sm text-gray-500">
              ({{ reviewStats.tongDanhGia }})
            </span>
          </div>
          <button 
            v-if="authStore.isLoggedIn"
            @click="showReviewForm = !showReviewForm" 
            class="text-primary-600 text-sm hover:text-primary-700"
          >
            {{ showReviewForm ? 'Đóng' : 'Viết đánh giá' }}
          </button>
        </div>

        <!-- Review Form -->
        <div v-if="showReviewForm" class="mb-6 p-4 bg-gray-50 rounded-lg">
          <div class="mb-3">
            <label class="block text-sm font-medium text-gray-700 mb-1">Đánh giá (tối đa 5 sao)</label>
            <div class="flex gap-1">
              <button
                v-for="star in 5"
                :key="star"
                @click="newReview.diemSo = star"
                class="p-1"
              >
                <StarIcon 
                  :class="[
                    'w-6 h-6',
                    star <= newReview.diemSo ? 'text-yellow-400 fill-yellow-400' : 'text-gray-300'
                  ]" 
                />
              </button>
            </div>
          </div>
          <textarea
            v-model="newReview.noiDung"
            placeholder="Viết nhận xét của bạn..."
            rows="3"
            class="input-field"
          ></textarea>
          <button @click="submitReview" class="btn-primary mt-3">
            Gửi đánh giá
          </button>
        </div>

        <!-- Reviews List -->
        <div v-if="reviews.length > 0" class="space-y-4">
          <div v-for="review in reviews" :key="review.maDanhGia" class="border-b border-gray-100 pb-4 last:border-0">
            <!-- Edit Mode -->
            <div v-if="editingReviewId === review.maDanhGia" class="p-4 bg-gray-50 rounded-lg">
              <div class="mb-3">
                <label class="block text-sm font-medium text-gray-700 mb-1">Đánh giá</label>
                <div class="flex gap-1">
                  <button
                    v-for="star in 5"
                    :key="star"
                    @click="editReview.xepHang = star"
                    class="p-1"
                  >
                    <StarIcon 
                      :class="[
                        'w-6 h-6',
                        star <= editReview.xepHang ? 'text-yellow-400 fill-yellow-400' : 'text-gray-300'
                      ]" 
                    />
                  </button>
                </div>
              </div>
              <textarea
                v-model="editReview.binhLuan"
                placeholder="Nhận xét của bạn..."
                rows="3"
                class="input-field"
              ></textarea>
              <div class="flex gap-2 mt-3">
                <button @click="saveEditReview(review.maDanhGia)" class="btn-primary">
                  Lưu
                </button>
                <button @click="cancelEditReview" class="btn-secondary">
                  Hủy
                </button>
              </div>
            </div>
            
            <!-- View Mode -->
            <div v-else>
              <div class="flex items-center justify-between mb-2">
                <div class="flex items-center">
                  <div class="w-8 h-8 rounded-full bg-gradient-to-br from-primary-500 to-secondary-500 flex items-center justify-center">
                    <span class="text-white text-xs font-medium">{{ getInitials(review.tenNguoiDung) }}</span>
                  </div>
                  <span class="ml-2 font-medium text-gray-900">{{ review.tenNguoiDung }}</span>
                </div>
                <div class="flex items-center gap-2">
                  <div class="flex items-center">
                    <StarIcon v-for="s in 5" :key="s" :class="[
                      'w-4 h-4',
                      s <= review.xepHang ? 'text-yellow-400 fill-yellow-400' : 'text-gray-300'
                    ]" />
                  </div>
                  <!-- Edit/Delete buttons for own reviews -->
                  <div v-if="authStore.user && review.nguoiDungId === authStore.user.id" class="flex items-center gap-1 ml-2">
                    <button 
                      @click="startEditReview(review)" 
                      class="p-1 text-gray-400 hover:text-primary-600 transition-colors"
                      title="Sửa đánh giá"
                    >
                      <PencilIcon class="w-4 h-4" />
                    </button>
                    <button 
                      @click="confirmDeleteReview(review.maDanhGia)" 
                      class="p-1 text-gray-400 hover:text-red-600 transition-colors"
                      title="Xóa đánh giá"
                    >
                      <TrashIcon class="w-4 h-4" />
                    </button>
                  </div>
                </div>
              </div>
              <p class="text-gray-600 text-sm">{{ review.binhLuan }}</p>
              <p class="text-xs text-gray-400 mt-1">{{ formatDate(review.ngayTao) }}</p>
            </div>
          </div>
        </div>
        <p v-else class="text-gray-500 text-center py-4">Chưa có đánh giá nào</p>

        <!-- Pagination -->
        <div v-if="reviewPagination.totalPages > 1" class="mt-6 flex justify-center">
          <nav class="flex items-center gap-2">
            <button
              @click="goToReviewPage(reviewPagination.currentPage - 1)"
              :disabled="reviewPagination.currentPage === 1"
              class="btn-secondary px-3 py-2 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <ChevronLeftIcon class="w-4 h-4" />
            </button>
            
            <span class="text-sm text-gray-600">
              Trang {{ reviewPagination.currentPage }} / {{ reviewPagination.totalPages }}
            </span>
            
            <button
              @click="goToReviewPage(reviewPagination.currentPage + 1)"
              :disabled="reviewPagination.currentPage === reviewPagination.totalPages"
              class="btn-secondary px-3 py-2 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <ChevronRightIcon class="w-4 h-4" />
            </button>
          </nav>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useToast } from 'vue-toastification'
import {
  DocumentTextIcon,
  ClockIcon,
  UserGroupIcon,
  CheckCircleIcon,
  LockClosedIcon,
  PlayIcon,
  ShareIcon,
  ArrowPathIcon,
  ArrowLeftIcon,
  ExclamationCircleIcon,
  StarIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  PencilIcon,
  TrashIcon
} from '@heroicons/vue/24/outline'
import { useAuthStore } from '@/stores/auth'
import { quizService, reviewService } from '@/services'

const route = useRoute()
const router = useRouter()
const toast = useToast()
const authStore = useAuthStore()

const loading = ref(true)
const starting = ref(false)
const error = ref('')
const quiz = ref(null)
const password = ref('')
const passwordError = ref('')
const passwordVerified = ref(false)
const verifying = ref(false)
const guestName = ref('')
const showReviewForm = ref(false)
const reviews = ref([])
const reviewStats = reactive({
  diemTrungBinh: 0,
  tongDanhGia: 0
})
const reviewPagination = reactive({
  currentPage: 1,
  totalPages: 1,
  pageSize: 10
})

const newReview = reactive({
  diemSo: 5,
  noiDung: ''
})

// Edit review state
const editingReviewId = ref(null)
const editReview = reactive({
  xepHang: 5,
  binhLuan: ''
})

const getInitials = (name) => {
  if (!name) return '?'
  return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2)
}

const formatDate = (date) => {
  return new Date(date).toLocaleDateString('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  })
}

// Quay lại trang trước hoặc trang Khám phá
const goBack = () => {
  // Nếu có history thì quay lại, không thì về trang Khám phá
  if (window.history.length > 2) {
    router.back()
  } else {
    router.push('/explore')
  }
}

const loadQuiz = async () => {
  loading.value = true
  error.value = ''
  
  try {
    let response
    if (route.params.id === 'code' && route.query.code) {
      response = await quizService.getQuizByCode(route.query.code)
    } else {
      response = await quizService.getQuizById(route.params.id)
    }
    
    if (response.success) {
      quiz.value = response.data
      loadReviews()
      loadReviewStats()
    } else {
      error.value = response.message || 'Không tìm thấy bài thi'
    }
  } catch (err) {
    error.value = err.response?.data?.message || 'Không thể tải bài thi'
  } finally {
    loading.value = false
  }
}

const loadReviews = async (page = 1) => {
  if (!quiz.value) return
  try {
    const response = await reviewService.getReviews(quiz.value.maBaiThi, page, reviewPagination.pageSize)
    reviews.value = response.data || []
    if (response.pagination) {
      reviewPagination.currentPage = response.pagination.currentPage
      reviewPagination.totalPages = response.pagination.totalPages
    }
  } catch (err) {
    console.error('Failed to load reviews:', err)
  }
}

const loadReviewStats = async () => {
  if (!quiz.value) return
  try {
    const response = await reviewService.getStatistics(quiz.value.maBaiThi)
    if (response.success && response.data) {
      reviewStats.diemTrungBinh = response.data.diemTrungBinh || 0
      reviewStats.tongDanhGia = response.data.tongDanhGia || 0
    }
  } catch (err) {
    console.error('Failed to load review stats:', err)
  }
}

const goToReviewPage = (page) => {
  if (page >= 1 && page <= reviewPagination.totalPages) {
    loadReviews(page)
  }
}

const verifyPassword = async () => {
  if (!password.value) {
    passwordError.value = 'Vui lòng nhập mật khẩu'
    return
  }
  
  verifying.value = true
  passwordError.value = ''
  
  try {
    const response = await quizService.verifyPassword(quiz.value.maBaiThi, password.value)
    if (response.success) {
      passwordVerified.value = true
    } else {
      passwordError.value = response.message || 'Mật khẩu không đúng'
    }
  } catch (err) {
    passwordError.value = err.response?.data?.message || 'Mật khẩu không đúng'
  } finally {
    verifying.value = false
  }
}

const startQuiz = async () => {
  starting.value = true
  
  try {
    const response = await quizService.startQuiz({
      maBaiThi: quiz.value.maBaiThi,
      tenNguoiThamGia: guestName.value || undefined,
      matKhau: password.value || undefined
    })
    
    if (response.success) {
      router.push({
        name: 'quiz-take',
        params: { id: quiz.value.maBaiThi },
        query: { session: response.data.maLuotLamBai }
      })
    } else {
      toast.error(response.message || 'Không thể bắt đầu bài thi')
    }
  } catch (err) {
    toast.error(err.response?.data?.message || 'Không thể bắt đầu bài thi')
  } finally {
    starting.value = false
  }
}

const shareQuiz = () => {
  const url = window.location.href
  if (navigator.share) {
    navigator.share({
      title: quiz.value.tieuDe,
      text: `Tham gia bài thi: ${quiz.value.tieuDe}`,
      url: url
    })
  } else {
    navigator.clipboard.writeText(url)
  }
}

const submitReview = async () => {
  if (!newReview.noiDung.trim()) {
    toast.error('Vui lòng nhập nội dung đánh giá')
    return
  }
  
  try {
    await reviewService.createReview({
      maBaiThi: quiz.value.maBaiThi,
      xepHang: newReview.diemSo,  // Backend dùng xepHang thay vì diemSo
      binhLuan: newReview.noiDung  // Backend dùng binhLuan thay vì noiDung
    })
    showReviewForm.value = false
    newReview.diemSo = 5
    newReview.noiDung = ''
    loadReviews()
    loadReviewStats()
  } catch (err) {
    toast.error(err.response?.data?.message || 'Không thể gửi đánh giá')
  }
}

// Edit review functions
const startEditReview = (review) => {
  editingReviewId.value = review.maDanhGia
  editReview.xepHang = review.xepHang
  editReview.binhLuan = review.binhLuan || ''
}

const cancelEditReview = () => {
  editingReviewId.value = null
  editReview.xepHang = 5
  editReview.binhLuan = ''
}

const saveEditReview = async (reviewId) => {
  try {
    await reviewService.updateReview(reviewId, {
      xepHang: editReview.xepHang,
      binhLuan: editReview.binhLuan
    })
    cancelEditReview()
    loadReviews()
    loadReviewStats()
  } catch (err) {
    toast.error(err.response?.data?.message || 'Không thể cập nhật đánh giá')
  }
}

const confirmDeleteReview = async (reviewId) => {
  if (!confirm('Bạn có chắc muốn xóa đánh giá này?')) return
  
  try {
    await reviewService.deleteReview(reviewId)
    loadReviews()
    loadReviewStats()
  } catch (err) {
    toast.error(err.response?.data?.message || 'Không thể xóa đánh giá')
  }
}

onMounted(() => {
  loadQuiz()
})

const getAvatarUrl = (avatar) => {
  if (!avatar) return null
  // If already full URL (starts with http), return as is
  if (avatar.startsWith('http') || avatar.startsWith('https')) return avatar
  // Otherwise, prepend the API base URL (without /api)
  return `${import.meta.env.VITE_API_URL?.replace('/api', '') || ''}${avatar}`
}
</script>
