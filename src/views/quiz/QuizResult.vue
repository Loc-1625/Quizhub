<template>
  <div class="min-h-screen bg-gray-50">
    <!-- Loading -->
    <div v-if="loading" class="flex items-center justify-center min-h-screen">
      <ArrowPathIcon class="w-12 h-12 text-primary-600 animate-spin" />
    </div>

    <template v-else-if="result">
      <!-- Hero Section - Compact -->
      <div :class="[
        'py-8',
        result.diem >= 8 
          ? 'bg-gradient-to-br from-emerald-500 to-teal-600'
          : result.diem >= 5
            ? 'bg-gradient-to-br from-primary-500 to-primary-700'
            : 'bg-gradient-to-br from-amber-500 to-orange-600'
      ]">
        <div class="max-w-3xl mx-auto px-4 text-center text-white">
          <h1 class="text-xl md:text-2xl font-bold mb-1">
            {{ result.diem >= 8 ? 'Xuất sắc!' : result.diem >= 5 ? 'Tốt lắm!' : 'Cố gắng lên!' }}
          </h1>
          <p class="text-sm opacity-90">{{ result.tieuDeBaiThi }}</p>

          <div class="mt-4">
            <div class="text-4xl md:text-5xl font-bold">
              {{ result.diem.toFixed(1) }}<span class="text-xl">/10</span>
            </div>
            <p class="text-sm opacity-90 mt-1">Điểm số của bạn</p>
          </div>
        </div>
      </div>

      <div class="max-w-3xl mx-auto px-4 py-6">
        <!-- Progress Ring + Stats -->
        <div class="card p-6 mb-6">
          <div class="flex flex-col md:flex-row items-center gap-6">
            <!-- Progress Ring -->
            <div class="relative w-32 h-32 flex-shrink-0">
              <svg class="w-32 h-32 transform -rotate-90">
                <!-- Background circle -->
                <circle
                  cx="64"
                  cy="64"
                  r="56"
                  stroke="#e5e7eb"
                  stroke-width="12"
                  fill="none"
                />
                <!-- Progress circle -->
                <circle
                  cx="64"
                  cy="64"
                  r="56"
                  :stroke="result.diem >= 8 ? '#10b981' : result.diem >= 5 ? '#6366f1' : '#f59e0b'"
                  stroke-width="12"
                  fill="none"
                  stroke-linecap="round"
                  :stroke-dasharray="351.86"
                  :stroke-dashoffset="351.86 - (351.86 * correctPercentage / 100)"
                  class="transition-all duration-1000 ease-out"
                />
              </svg>
              <div class="absolute inset-0 flex flex-col items-center justify-center">
                <span class="text-2xl font-bold text-gray-900">{{ correctPercentage }}%</span>
                <span class="text-xs text-gray-500">Chính xác</span>
              </div>
            </div>

            <!-- Stats Grid -->
            <div class="flex-1 grid grid-cols-2 gap-4 w-full">
              <div class="flex items-center gap-3 p-3 bg-green-50 rounded-xl">
                <div class="w-10 h-10 rounded-full bg-green-100 flex items-center justify-center">
                  <CheckCircleIcon class="w-5 h-5 text-green-600" />
                </div>
                <div>
                  <div class="text-lg font-bold text-green-600">{{ result.soCauDung }}</div>
                  <div class="text-xs text-gray-500">Câu đúng</div>
                </div>
              </div>
              <div class="flex items-center gap-3 p-3 bg-red-50 rounded-xl">
                <div class="w-10 h-10 rounded-full bg-red-100 flex items-center justify-center">
                  <XCircleIcon class="w-5 h-5 text-red-600" />
                </div>
                <div>
                  <div class="text-lg font-bold text-red-600">{{ result.soCauSai }}</div>
                  <div class="text-xs text-gray-500">Câu sai</div>
                </div>
              </div>
              <div class="flex items-center gap-3 p-3 bg-gray-50 rounded-xl">
                <div class="w-10 h-10 rounded-full bg-gray-200 flex items-center justify-center">
                  <MinusCircleIcon class="w-5 h-5 text-gray-600" />
                </div>
                <div>
                  <div class="text-lg font-bold text-gray-600">{{ result.soCauChuaLam }}</div>
                  <div class="text-xs text-gray-500">Bỏ qua</div>
                </div>
              </div>
              <div class="flex items-center gap-3 p-3 bg-primary-50 rounded-xl">
                <div class="w-10 h-10 rounded-full bg-primary-100 flex items-center justify-center">
                  <ClockIcon class="w-5 h-5 text-primary-600" />
                </div>
                <div>
                  <div class="text-lg font-bold text-primary-600">{{ formatAvgTime }}</div>
                  <div class="text-xs text-gray-500">TB/câu</div>
                </div>
              </div>
            </div>
          </div>

          <!-- Thời gian tổng -->
          <div class="mt-4 pt-4 border-t border-gray-100 flex items-center justify-center gap-2 text-sm text-gray-500">
            <ClockIcon class="w-4 h-4" />
            <span>Thời gian làm bài: <strong class="text-gray-700">{{ formatTime(result.thoiGianLamBaiThucTe) }}</strong></span>
          </div>
        </div>

        <!-- Nút Xem đáp án -->
        <div v-if="result.choPhepXemLai && result.chiTietCauTraLoi && !showAnswers" class="card p-5 text-center mb-6">
          <EyeIcon class="w-10 h-10 mx-auto text-gray-400 mb-2" />
          <h3 class="font-semibold text-gray-900 mb-1">Xem lại đáp án</h3>
          <p class="text-gray-500 text-sm mb-3">
            Xem chi tiết {{ result.chiTietCauTraLoi.length }} câu hỏi và đáp án đúng
          </p>
          <button @click="showAnswers = true" class="btn-primary px-6">
            <EyeIcon class="w-5 h-5 mr-2" />
            Hiển thị đáp án
          </button>
        </div>

        <!-- Review Answers - Layout 2 cột -->
        <div v-if="result.choPhepXemLai && result.chiTietCauTraLoi && showAnswers" class="card overflow-hidden mb-6">
          <div class="p-3 border-b border-gray-100 bg-gray-50 flex items-center justify-between">
            <h3 class="font-semibold text-gray-900 text-sm">Xem lại đáp án</h3>
            <button @click="showAnswers = false" class="text-gray-500 hover:text-gray-700">
              <XMarkIcon class="w-5 h-5" />
            </button>
          </div>

          <div class="flex flex-col md:flex-row">
            <!-- Sidebar - Danh sách câu hỏi dạng grid -->
            <div class="md:w-44 border-b md:border-b-0 md:border-r border-gray-100 bg-gray-50 p-3">
              <div class="grid grid-cols-5 md:grid-cols-4 gap-2">
                <button
                  v-for="(answer, index) in result.chiTietCauTraLoi"
                  :key="answer.maCauHoi"
                  @click="currentReviewIndex = index"
                  :class="[
                    'w-9 h-9 rounded-lg text-sm font-medium transition-all flex items-center justify-center',
                    currentReviewIndex === index
                      ? 'bg-primary-600 text-white shadow-md ring-2 ring-primary-300'
                      : answer.laDapAnDung
                        ? 'bg-green-100 text-green-700 hover:bg-green-200'
                        : 'bg-red-100 text-red-700 hover:bg-red-200'
                  ]"
                >
                  {{ index + 1 }}
                </button>
              </div>
            </div>

            <!-- Content - Chi tiết câu hỏi -->
            <div class="flex-1 p-4 min-w-0">
              <div 
                v-if="result.chiTietCauTraLoi[currentReviewIndex]"
                :key="result.chiTietCauTraLoi[currentReviewIndex].maCauHoi"
              >
                <div class="flex items-start justify-between mb-3">
                  <span class="badge badge-primary">Câu {{ currentReviewIndex + 1 }}</span>
                  <span :class="[
                    'badge',
                    result.chiTietCauTraLoi[currentReviewIndex].laDapAnDung ? 'badge-success' : 'badge-danger'
                  ]">
                    {{ result.chiTietCauTraLoi[currentReviewIndex].laDapAnDung ? 'Đúng' : 'Sai' }}
                  </span>
                </div>
                
                <p class="font-medium text-gray-900 mb-4 whitespace-pre-wrap break-words">
                  {{ result.chiTietCauTraLoi[currentReviewIndex].noiDungCauHoi }}
                </p>

                <div class="space-y-2">
                  <div
                    v-for="option in result.chiTietCauTraLoi[currentReviewIndex].cacLuaChon"
                    :key="option.maLuaChon"
                    :class="[
                      'p-3 rounded-lg border-2 overflow-hidden',
                      option.laDapAnDung 
                        ? 'border-green-500 bg-green-50'
                        : option.maLuaChon === result.chiTietCauTraLoi[currentReviewIndex].maLuaChonDaChon && !option.laDapAnDung
                          ? 'border-red-500 bg-red-50'
                          : 'border-gray-200'
                    ]"
                  >
                    <div class="flex items-start gap-3">
                      <span :class="[
                        'w-6 h-6 rounded-full flex items-center justify-center text-xs font-medium flex-shrink-0',
                        option.laDapAnDung 
                          ? 'bg-green-500 text-white'
                          : option.maLuaChon === result.chiTietCauTraLoi[currentReviewIndex].maLuaChonDaChon && !option.laDapAnDung
                            ? 'bg-red-500 text-white'
                            : 'bg-gray-200 text-gray-700'
                      ]">
                        {{ String.fromCharCode(65 + option.thuTu) }}
                      </span>
                      <span :class="[
                        'flex-1 min-w-0 break-all',
                        option.laDapAnDung ? 'text-green-700' : 
                        option.maLuaChon === result.chiTietCauTraLoi[currentReviewIndex].maLuaChonDaChon && !option.laDapAnDung ? 'text-red-700' : 'text-gray-700'
                      ]">
                        {{ option.noiDungDapAn }}
                      </span>
                      <CheckCircleIcon v-if="option.laDapAnDung" class="w-5 h-5 flex-shrink-0 text-green-500" />
                      <XCircleIcon v-else-if="option.maLuaChon === result.chiTietCauTraLoi[currentReviewIndex].maLuaChonDaChon" class="w-5 h-5 flex-shrink-0 text-red-500" />
                    </div>
                  </div>
                </div>

                <div v-if="result.chiTietCauTraLoi[currentReviewIndex].giaiThich" class="mt-4 p-3 bg-blue-50 rounded-lg">
                  <p class="text-sm text-blue-800 break-words">
                    <strong>Giải thích:</strong> {{ result.chiTietCauTraLoi[currentReviewIndex].giaiThich }}
                  </p>
                </div>

                <!-- Navigation Buttons -->
                <div class="flex justify-between mt-4 pt-4 border-t border-gray-200">
                  <button
                    @click="currentReviewIndex--"
                    :disabled="currentReviewIndex === 0"
                    :class="[
                      'px-3 py-2 rounded-lg text-sm font-medium transition-all flex items-center gap-1',
                      currentReviewIndex === 0
                        ? 'bg-gray-100 text-gray-400 cursor-not-allowed'
                        : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
                    ]"
                  >
                    <ChevronLeftIcon class="w-4 h-4" />
                    Trước
                  </button>
                  <button
                    @click="currentReviewIndex++"
                    :disabled="currentReviewIndex === result.chiTietCauTraLoi.length - 1"
                    :class="[
                      'px-3 py-2 rounded-lg text-sm font-medium transition-all flex items-center gap-1',
                      currentReviewIndex === result.chiTietCauTraLoi.length - 1
                        ? 'bg-gray-100 text-gray-400 cursor-not-allowed'
                        : 'bg-primary-600 text-white hover:bg-primary-700'
                    ]"
                  >
                    Sau
                    <ChevronRightIcon class="w-4 h-4" />
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Actions - Đặt dưới cùng -->
        <div class="flex flex-col sm:flex-row gap-3">
          <button 
            @click="$router.back()"
            class="btn-secondary flex-1 justify-center py-3"
          >
            <ArrowLeftIcon class="w-5 h-5 mr-2" />
            Quay lại
          </button>
          <router-link 
            :to="{ name: 'quiz-detail', params: { id: result.maBaiThi } }"
            class="btn-primary flex-1 justify-center py-3"
          >
            <ArrowPathIcon class="w-5 h-5 mr-2" />
            Làm lại
          </router-link>
          <router-link to="/explore" class="btn-secondary flex-1 justify-center py-3">
            <MagnifyingGlassIcon class="w-5 h-5 mr-2" />
            Khám phá bài thi khác
          </router-link>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useToast } from 'vue-toastification'
import {
  CheckCircleIcon,
  ExclamationCircleIcon,
  XCircleIcon,
  ArrowPathIcon,
  ArrowLeftIcon,
  MagnifyingGlassIcon,
  ShareIcon,
  HandThumbUpIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  EyeIcon,
  XMarkIcon,
  ClockIcon,
  MinusCircleIcon
} from '@heroicons/vue/24/outline'
import { quizService } from '@/services'

const route = useRoute()
const router = useRouter()
const toast = useToast()

const loading = ref(true)
const result = ref(null)
const currentReviewIndex = ref(0)
const showAnswers = ref(false)

// Computed properties
const correctPercentage = computed(() => {
  if (!result.value || !result.value.tongSoCauHoi) return 0
  return Math.round((result.value.soCauDung / result.value.tongSoCauHoi) * 100)
})

const avgTimePerQuestion = computed(() => {
  if (!result.value || !result.value.tongSoCauHoi || !result.value.thoiGianLamBaiThucTe) return 0
  return Math.round(result.value.thoiGianLamBaiThucTe / result.value.tongSoCauHoi)
})

const formatAvgTime = computed(() => {
  const seconds = avgTimePerQuestion.value
  if (seconds < 60) return `${seconds}s`
  const mins = Math.floor(seconds / 60)
  const secs = seconds % 60
  return secs > 0 ? `${mins}m ${secs}s` : `${mins}m`
})

const formatTime = (seconds) => {
  const mins = Math.floor(seconds / 60)
  const secs = seconds % 60
  return `${mins}:${secs.toString().padStart(2, '0')}`
}

const shareResult = () => {
  const text = `Tôi đạt ${result.value.diem.toFixed(1)}/10 điểm trong bài thi "${result.value.tieuDeBaiThi}" trên QuizHub!`
  
  if (navigator.share) {
    navigator.share({
      title: 'Kết quả bài thi - QuizHub',
      text: text,
      url: window.location.href
    })
  } else {
    navigator.clipboard.writeText(text)
  }
}

onMounted(async () => {
  try {
    const response = await quizService.getResult(route.params.luotLamBaiId)
    if (response.success) {
      result.value = response.data
    } else {
      toast.error('Không thể tải kết quả')
      router.push({ name: 'home' })
    }
  } catch (error) {
    toast.error('Không thể tải kết quả')
    router.push({ name: 'home' })
  } finally {
    loading.value = false
  }
})
</script>
