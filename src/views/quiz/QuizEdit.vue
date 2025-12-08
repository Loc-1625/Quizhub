<template>
  <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <div class="mb-8">
      <h1 class="page-header">Chỉnh sửa bài thi</h1>
    </div>

    <div v-if="loading" class="card p-8 text-center">
      <ArrowPathIcon class="w-12 h-12 text-primary-600 animate-spin mx-auto" />
      <p class="mt-4 text-gray-600">Đang tải...</p>
    </div>

    <form v-else @submit.prevent="handleSubmit" class="space-y-6">
      <!-- Basic Info -->
      <div class="card p-6">
        <h2 class="section-title mb-4">Thông tin cơ bản</h2>
        
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">
              Tiêu đề bài thi <span class="text-red-500">*</span>
            </label>
            <input
              v-model="form.tieuDe"
              type="text"
              required
              placeholder="Nhập tiêu đề bài thi"
              class="input-field"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">
              Mô tả
            </label>
            <textarea
              v-model="form.moTa"
              rows="3"
              placeholder="Mô tả ngắn về bài thi"
              class="input-field"
            ></textarea>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">
                Danh mục
              </label>
              <select v-model="form.maDanhMuc" class="input-field">
                <option value="">-- Chọn danh mục --</option>
                <option v-for="cat in categories" :key="cat.maDanhMuc" :value="cat.maDanhMuc">
                  {{ cat.tenDanhMuc }}
                </option>
              </select>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">
                Thời gian làm bài (phút) <span class="text-red-500">*</span>
              </label>
              <input
                v-model.number="form.thoiGianLamBai"
                type="number"
                min="1"
                max="300"
                required
                class="input-field"
              />
            </div>
          </div>
        </div>
      </div>

      <!-- Settings -->
      <div class="card p-6">
        <h2 class="section-title mb-4">Cài đặt</h2>
        
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">
              Chế độ công khai
            </label>
            <select v-model="form.cheDoCongKhai" class="input-field">
              <option value="CongKhai">Công khai - Ai cũng có thể tìm thấy và tham gia (hoặc qua mã truy cập)</option>
              <option value="CoMatKhau">Có mật khẩu - Cần nhập mật khẩu để tham gia (kể cả qua mã truy cập)</option>
              <option value="RiengTu">Riêng tư - Chỉ người tạo bài thi mới truy cập được</option>
            </select>
          </div>

          <div v-if="form.cheDoCongKhai === 'CoMatKhau'">
            <label class="block text-sm font-medium text-gray-700 mb-1">
              Mật khẩu bài thi <span class="text-red-500">*</span>
            </label>
            <input
              v-model="form.matKhau"
              type="text"
              placeholder="Nhập mật khẩu để bảo vệ bài thi"
              class="input-field"
              required
            />
          </div>

          <div class="flex items-center justify-between py-3 border-t border-gray-100">
            <div>
              <p class="font-medium text-gray-900">Cho phép xem lại đáp án</p>
              <p class="text-sm text-gray-500">Người làm có thể xem đáp án đúng và bài làm của mình sau khi nộp</p>
            </div>
            <button
              type="button"
              @click="toggleXemLai"
              :class="[
                'relative inline-flex h-6 w-11 items-center rounded-full transition-colors',
                form.choPhepXemLai ? 'bg-primary-600' : 'bg-gray-200'
              ]"
            >
              <span
                :class="[
                  'inline-block h-4 w-4 transform rounded-full bg-white transition-transform',
                  form.choPhepXemLai ? 'translate-x-6' : 'translate-x-1'
                ]"
              />
            </button>
          </div>


        </div>
      </div>

      <!-- Submit -->
      <div class="flex justify-end gap-4">
        <router-link to="/my-quizzes" class="btn-secondary">
          Hủy
        </router-link>
        <button
          type="submit"
          :disabled="submitting"
          class="btn-primary"
        >
          <ArrowPathIcon v-if="submitting" class="w-5 h-5 mr-2 animate-spin" />
          {{ submitting ? 'Đang lưu...' : 'Lưu thay đổi' }}
        </button>
      </div>
    </form>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useToast } from 'vue-toastification'
import { ArrowPathIcon } from '@heroicons/vue/24/outline'
import { quizService, categoryService } from '@/services'

const route = useRoute()
const router = useRouter()
const toast = useToast()

const loading = ref(true)
const submitting = ref(false)
const categories = ref([])

const form = reactive({
  tieuDe: '',
  moTa: '',
  maDanhMuc: '',
  thoiGianLamBai: 30,
  cheDoCongKhai: 'RiengTu',
  matKhau: '',
  hienThiDapAnSauKhiNop: true,
  choPhepXemLai: true,
  cacCauHoi: []  // Luôn gửi mảng rỗng để không thay đổi câu hỏi
})

// Toggle xem lại - đồng bộ cả 2 giá trị
const toggleXemLai = () => {
  form.choPhepXemLai = !form.choPhepXemLai
  form.hienThiDapAnSauKhiNop = form.choPhepXemLai
}

const loadQuiz = async () => {
  try {
    // Load categories
    const catResponse = await categoryService.getCategories()
    categories.value = catResponse.data || []
    
    const response = await quizService.getQuizById(route.params.id)
    if (response.success) {
      const quiz = response.data
      Object.assign(form, {
        tieuDe: quiz.tieuDe,
        moTa: quiz.moTa,
        maDanhMuc: quiz.maDanhMuc || '',
        thoiGianLamBai: quiz.thoiGianLamBai,
        cheDoCongKhai: quiz.cheDoCongKhai,
        matKhau: '',
        hienThiDapAnSauKhiNop: quiz.hienThiDapAnSauKhiNop,
        choPhepXemLai: quiz.choPhepXemLai,
        // Không gửi cacCauHoi để cho phép cập nhật metadata mà không đụng vào câu hỏi
        cacCauHoi: []
      })
    } else {
      toast.error('Không tìm thấy bài thi')
      router.push('/my-quizzes')
    }
  } catch (error) {
    toast.error('Không thể tải bài thi')
    router.push('/my-quizzes')
  } finally {
    loading.value = false
  }
}

const handleSubmit = async () => {
  submitting.value = true
  try {
    const data = {
      tieuDe: form.tieuDe,
      moTa: form.moTa || null,
      maDanhMuc: form.maDanhMuc || null,
      thoiGianLamBai: form.thoiGianLamBai,
      cheDoCongKhai: form.cheDoCongKhai,
      matKhau: form.cheDoCongKhai === 'CoMatKhau' ? form.matKhau : null,
      hienThiDapAnSauKhiNop: form.hienThiDapAnSauKhiNop,
      choPhepXemLai: form.choPhepXemLai,
      cacCauHoi: []
    }
    
    console.log('Sending update data:', data)
    
    const response = await quizService.updateQuiz(route.params.id, data)
    if (response.success) {
      router.push('/my-quizzes')
    } else {
      toast.error(response.message || 'Không thể cập nhật bài thi')
    }
  } catch (error) {
    console.error('Update quiz error:', error.response?.data)
    toast.error(error.response?.data?.message || 'Không thể cập nhật bài thi')
  } finally {
    submitting.value = false
  }
}

onMounted(() => {
  loadQuiz()
})
</script>
