<template>
  <div class="min-h-screen bg-gray-100">
    <!-- Header -->
    <div class="bg-white border-b border-gray-200 sticky top-0 z-50">
      <div class="max-w-5xl mx-auto px-4 py-3">
        <div class="flex items-center justify-between">
          <div class="flex items-center">
            <div class="w-8 h-8 bg-gradient-to-br from-primary-500 to-secondary-500 rounded-lg flex items-center justify-center">
              <span class="text-white font-bold">Q</span>
            </div>
            <span class="ml-2 font-semibold text-gray-900 hidden sm:block">{{ session?.tieuDe }}</span>
          </div>

          <!-- Timer -->
          <div :class="[
            'flex items-center px-4 py-2 rounded-lg font-mono text-lg font-bold',
            timeRemaining <= 60 ? 'bg-red-100 text-red-600 animate-pulse' : 
            timeRemaining <= 300 ? 'bg-yellow-100 text-yellow-700' : 
            'bg-primary-100 text-primary-700'
          ]">
            <ClockIcon class="w-5 h-5 mr-2" />
            {{ formatTime(timeRemaining) }}
          </div>

          <!-- Submit Button -->
          <button
            @click="showConfirmSubmit = true"
            class="btn-primary"
          >
            <PaperAirplaneIcon class="w-5 h-5 mr-1" />
            Nộp bài
          </button>
        </div>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex items-center justify-center min-h-[60vh]">
      <div class="text-center">
        <ArrowPathIcon class="w-12 h-12 text-primary-600 animate-spin mx-auto mb-4" />
        <p class="text-gray-600">Đang tải bài thi...</p>
      </div>
    </div>

    <template v-else-if="session">
      <div class="max-w-5xl mx-auto px-4 py-6">
        <div class="grid grid-cols-1 lg:grid-cols-4 gap-6">
          <!-- Main Content -->
          <div class="lg:col-span-3 space-y-6">
            <!-- Question Card -->
            <div class="card p-6">
              <div class="flex items-center justify-between mb-4">
                <span class="badge badge-primary">
                  Câu {{ currentQuestionIndex + 1 }} / {{ session.cacCauHoi.length }}
                </span>
                <span class="text-sm text-gray-500">
                  {{ currentQuestion?.diem || 1 }} điểm
                </span>
              </div>

              <h2 class="text-lg font-medium text-gray-900 mb-6 whitespace-pre-wrap">
                {{ currentQuestion?.noiDungCauHoi }}
              </h2>

              <!-- Answers -->
              <div class="space-y-3">
                <button
                  v-for="(option, index) in currentQuestion?.cacLuaChon"
                  :key="option.maLuaChon"
                  @click="selectAnswer(option.maLuaChon)"
                  :class="[
                    'w-full p-4 rounded-lg border-2 text-left transition-all duration-200',
                    answers[currentQuestion.maCauHoi] === option.maLuaChon
                      ? 'border-primary-500 bg-primary-50 ring-2 ring-primary-200'
                      : 'border-gray-200 hover:border-primary-300 hover:bg-gray-50'
                  ]"
                >
                  <div class="flex items-start">
                    <span :class="[
                      'flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center font-medium mr-3',
                      answers[currentQuestion.maCauHoi] === option.maLuaChon
                        ? 'bg-primary-500 text-white'
                        : 'bg-gray-200 text-gray-700'
                    ]">
                      {{ String.fromCharCode(65 + index) }}
                    </span>
                    <span class="text-gray-900 pt-1">{{ option.noiDungDapAn }}</span>
                  </div>
                </button>
              </div>
            </div>

            <!-- Navigation -->
            <div class="flex items-center justify-between">
              <button
                @click="prevQuestion"
                :disabled="currentQuestionIndex === 0"
                class="btn-secondary"
              >
                <ChevronLeftIcon class="w-5 h-5 mr-1" />
                Câu trước
              </button>

              <button
                v-if="currentQuestionIndex < session.cacCauHoi.length - 1"
                @click="nextQuestion"
                class="btn-primary"
              >
                Câu tiếp
                <ChevronRightIcon class="w-5 h-5 ml-1" />
              </button>
              <button
                v-else
                @click="showConfirmSubmit = true"
                class="btn-primary bg-green-600 hover:bg-green-700"
              >
                Nộp bài
                <PaperAirplaneIcon class="w-5 h-5 ml-1" />
              </button>
            </div>
          </div>

          <!-- Sidebar - Question Navigation -->
          <div class="lg:col-span-1">
            <div class="card p-4 sticky top-20">
              <h3 class="font-semibold text-gray-900 mb-4">Danh sách câu hỏi</h3>
              <div class="grid grid-cols-5 gap-2">
                <button
                  v-for="(question, index) in session.cacCauHoi"
                  :key="question.maCauHoi"
                  @click="goToQuestion(index)"
                  :class="[
                    'w-full aspect-square rounded-lg font-medium text-sm transition-all',
                    currentQuestionIndex === index
                      ? 'bg-primary-600 text-white ring-2 ring-primary-300'
                      : answers[question.maCauHoi]
                        ? 'bg-green-100 text-green-700 hover:bg-green-200'
                        : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
                  ]"
                >
                  {{ index + 1 }}
                </button>
              </div>

              <!-- Progress -->
              <div class="mt-4 pt-4 border-t border-gray-100">
                <div class="flex justify-between text-sm mb-2">
                  <span class="text-gray-500">Đã làm</span>
                  <span class="font-medium text-gray-900">
                    {{ answeredCount }} / {{ session.cacCauHoi.length }}
                  </span>
                </div>
                <div class="w-full h-2 bg-gray-200 rounded-full overflow-hidden">
                  <div 
                    class="h-full bg-green-500 transition-all duration-300"
                    :style="{ width: `${(answeredCount / session.cacCauHoi.length) * 100}%` }"
                  ></div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </template>

    <!-- Confirm Submit Modal -->
    <TransitionRoot appear :show="showConfirmSubmit" as="template">
      <Dialog as="div" @close="showConfirmSubmit = false" class="relative z-50">
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
                <DialogTitle as="h3" class="text-lg font-semibold text-gray-900 mb-4">
                  Xác nhận nộp bài
                </DialogTitle>
                
                <div class="mb-6">
                  <p class="text-gray-600 mb-4">
                    Bạn có chắc chắn muốn nộp bài không?
                  </p>
                  <div class="p-4 bg-gray-50 rounded-lg">
                    <div class="flex justify-between text-sm mb-2">
                      <span class="text-gray-500">Số câu đã làm:</span>
                      <span class="font-medium">{{ answeredCount }} / {{ session?.cacCauHoi?.length }}</span>
                    </div>
                    <div class="flex justify-between text-sm">
                      <span class="text-gray-500">Số câu chưa làm:</span>
                      <span :class="unansweredCount > 0 ? 'text-red-600 font-medium' : 'font-medium'">
                        {{ unansweredCount }}
                      </span>
                    </div>
                  </div>
                  <p v-if="unansweredCount > 0" class="mt-3 text-sm text-yellow-600">
                    ⚠️ Bạn còn {{ unansweredCount }} câu chưa trả lời!
                  </p>
                </div>

                <div class="flex justify-end gap-3">
                  <button @click="showConfirmSubmit = false" class="btn-secondary">
                    Tiếp tục làm bài
                  </button>
                  <button @click="submitQuiz" :disabled="submitting" class="btn-primary">
                    <ArrowPathIcon v-if="submitting" class="w-5 h-5 mr-2 animate-spin" />
                    {{ submitting ? 'Đang nộp...' : 'Nộp bài' }}
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
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useToast } from 'vue-toastification'
import {
  Dialog,
  DialogPanel,
  DialogTitle,
  TransitionRoot,
  TransitionChild
} from '@headlessui/vue'
import {
  ClockIcon,
  PaperAirplaneIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  ArrowPathIcon
} from '@heroicons/vue/24/outline'
import { quizService } from '@/services'
import signalRService from '@/services/signalr'

const route = useRoute()
const router = useRouter()
const toast = useToast()

const loading = ref(true)
const session = ref(null)
const currentQuestionIndex = ref(0)
const answers = ref({})
const timeRemaining = ref(0)
const showConfirmSubmit = ref(false)
const submitting = ref(false)
let timerInterval = null

const currentQuestion = computed(() => session.value?.cacCauHoi?.[currentQuestionIndex.value])

const answeredCount = computed(() => Object.keys(answers.value).length)

const unansweredCount = computed(() => {
  if (!session.value) return 0
  return session.value.cacCauHoi.length - answeredCount.value
})

const formatTime = (seconds) => {
  const mins = Math.floor(seconds / 60)
  const secs = seconds % 60
  return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`
}

const selectAnswer = async (maLuaChon) => {
  const questionId = currentQuestion.value.maCauHoi
  answers.value[questionId] = maLuaChon

  // Auto-save to server
  try {
    await quizService.saveAnswer(session.value.maLuotLamBai, {
      maCauHoi: questionId,
      maLuaChonDaChon: maLuaChon  // ✅ Đúng tên field backend mong đợi
    })
  } catch (error) {
    console.error('Failed to save answer:', error)
  }
}

const prevQuestion = () => {
  if (currentQuestionIndex.value > 0) {
    currentQuestionIndex.value--
  }
}

const nextQuestion = () => {
  if (currentQuestionIndex.value < session.value.cacCauHoi.length - 1) {
    currentQuestionIndex.value++
  }
}

const goToQuestion = (index) => {
  currentQuestionIndex.value = index
}

const submitQuiz = async () => {
  submitting.value = true
  
  try {
    const response = await quizService.submitQuiz(session.value.maLuotLamBai, {
      cacCauTraLoi: Object.entries(answers.value).map(([maCauHoi, maLuaChonDaChon]) => ({
        maCauHoi,
        maLuaChonDaChon  // ✅ Đúng tên field backend mong đợi
      }))
    })

    if (response.success) {
      router.replace({
        name: 'quiz-result',
        params: { 
          id: session.value.maBaiThi,
          luotLamBaiId: session.value.maLuotLamBai 
        }
      })
    } else {
      toast.error(response.message || 'Không thể nộp bài')
    }
  } catch (error) {
    toast.error(error.response?.data?.message || 'Không thể nộp bài')
  } finally {
    submitting.value = false
    showConfirmSubmit.value = false
  }
}

const loadSession = async () => {
  const sessionId = route.query.session
  if (!sessionId) {
    router.push({ name: 'quiz-detail', params: { id: route.params.id } })
    return
  }

  try {
    const response = await quizService.getSession(sessionId)
    if (response.success) {
      session.value = response.data
      timeRemaining.value = response.data.thoiGianConLai
      
      // Load saved answers
      session.value.cacCauHoi.forEach(q => {
        if (q.daChon) {
          answers.value[q.maCauHoi] = q.daChon
        }
      })

      // Start timer
      startTimer()
      
      // Connect SignalR
      connectSignalR()
    } else {
      toast.error('Không thể tải bài thi')
      router.push({ name: 'quiz-detail', params: { id: route.params.id } })
    }
  } catch (error) {
    toast.error('Không thể tải bài thi')
    router.push({ name: 'quiz-detail', params: { id: route.params.id } })
  } finally {
    loading.value = false
  }
}

const startTimer = () => {
  timerInterval = setInterval(() => {
    if (timeRemaining.value > 0) {
      timeRemaining.value--
      
      // Warning at 5 minutes
      if (timeRemaining.value === 300) {
        toast.warning('Còn 5 phút!')
      }
      // Warning at 1 minute
      if (timeRemaining.value === 60) {
        toast.warning('Còn 1 phút!')
      }
    } else {
      // Auto submit
      clearInterval(timerInterval)
      toast.info('Hết giờ! Bài thi sẽ được nộp tự động.')
      submitQuiz()
    }
  }, 1000)
}

const connectSignalR = async () => {
  try {
    await signalRService.start()
    await signalRService.joinQuizRoom(session.value.maBaiThi)

    signalRService.onCountdown((data) => {
      timeRemaining.value = data.secondsRemaining
    })

    signalRService.onAutoSubmit((data) => {
      if (data.luotLamBaiId === session.value.maLuotLamBai) {
        toast.info('Bài thi đã được nộp tự động')
        router.replace({
          name: 'quiz-result',
          params: { 
            id: session.value.maBaiThi,
            luotLamBaiId: session.value.maLuotLamBai 
          }
        })
      }
    })
  } catch (error) {
    console.error('SignalR connection failed:', error)
  }
}

// Warn before leaving
const handleBeforeUnload = (e) => {
  e.preventDefault()
  e.returnValue = ''
}

onMounted(() => {
  loadSession()
  window.addEventListener('beforeunload', handleBeforeUnload)
})

onUnmounted(() => {
  if (timerInterval) clearInterval(timerInterval)
  window.removeEventListener('beforeunload', handleBeforeUnload)
  if (session.value) {
    signalRService.leaveQuizRoom(session.value.maBaiThi)
  }
})
</script>
