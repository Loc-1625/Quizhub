import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { quizService, attemptService } from '@/services'

export const useQuizStore = defineStore('quiz', () => {
  const currentQuiz = ref(null)
  const currentSession = ref(null)
  const questions = ref([])
  const answers = ref({})
  const timeRemaining = ref(0)
  const isSubmitting = ref(false)
  const isLoading = ref(false)

  const currentQuestionIndex = ref(0)

  const currentQuestion = computed(() => {
    return questions.value[currentQuestionIndex.value] || null
  })

  const totalQuestions = computed(() => questions.value.length)

  const answeredCount = computed(() => {
    return Object.keys(answers.value).filter(key => answers.value[key] !== null && answers.value[key] !== undefined).length
  })

  const progress = computed(() => {
    if (totalQuestions.value === 0) return 0
    return Math.round((answeredCount.value / totalQuestions.value) * 100)
  })

  const isAllAnswered = computed(() => {
    return answeredCount.value === totalQuestions.value
  })

  async function startQuiz(quizId, password = null) {
    isLoading.value = true
    try {
      const response = await attemptService.startQuiz(quizId, { matKhau: password })
      if (response.success) {
        currentSession.value = response.data
        currentQuiz.value = response.data.baiThi
        questions.value = response.data.cacCauHoi || []
        timeRemaining.value = response.data.thoiGianConLai || (currentQuiz.value.thoiGianLamBai * 60)
        
        // Initialize answers
        answers.value = {}
        questions.value.forEach(q => {
          answers.value[q.maCauHoi] = null
        })
        
        currentQuestionIndex.value = 0
        return response
      }
      throw new Error(response.message || 'Không thể bắt đầu bài thi')
    } catch (error) {
      throw error
    } finally {
      isLoading.value = false
    }
  }

  async function saveAnswer(questionId, selectedOptions) {
    answers.value[questionId] = selectedOptions
    
    // Auto save to server
    if (currentSession.value) {
      try {
        await attemptService.saveAnswer(currentSession.value.maLuotLamBai, {
          maCauHoi: questionId,
          cacLuaChonDaChon: Array.isArray(selectedOptions) ? selectedOptions : [selectedOptions]
        })
      } catch (error) {
        console.error('Failed to save answer:', error)
      }
    }
  }

  async function submitQuiz() {
    if (!currentSession.value) return null
    
    isSubmitting.value = true
    try {
      const response = await attemptService.submitQuiz(currentSession.value.maLuotLamBai)
      if (response.success) {
        return response.data
      }
      throw new Error(response.message || 'Không thể nộp bài')
    } catch (error) {
      throw error
    } finally {
      isSubmitting.value = false
    }
  }

  function goToQuestion(index) {
    if (index >= 0 && index < questions.value.length) {
      currentQuestionIndex.value = index
    }
  }

  function nextQuestion() {
    if (currentQuestionIndex.value < questions.value.length - 1) {
      currentQuestionIndex.value++
    }
  }

  function prevQuestion() {
    if (currentQuestionIndex.value > 0) {
      currentQuestionIndex.value--
    }
  }

  function decrementTime() {
    if (timeRemaining.value > 0) {
      timeRemaining.value--
    }
  }

  function resetQuiz() {
    currentQuiz.value = null
    currentSession.value = null
    questions.value = []
    answers.value = {}
    timeRemaining.value = 0
    currentQuestionIndex.value = 0
    isSubmitting.value = false
  }

  function isQuestionAnswered(questionId) {
    const answer = answers.value[questionId]
    if (Array.isArray(answer)) {
      return answer.length > 0
    }
    return answer !== null && answer !== undefined
  }

  return {
    // State
    currentQuiz,
    currentSession,
    questions,
    answers,
    timeRemaining,
    isSubmitting,
    isLoading,
    currentQuestionIndex,
    
    // Getters
    currentQuestion,
    totalQuestions,
    answeredCount,
    progress,
    isAllAnswered,
    
    // Actions
    startQuiz,
    saveAnswer,
    submitQuiz,
    goToQuestion,
    nextQuestion,
    prevQuestion,
    decrementTime,
    resetQuiz,
    isQuestionAnswered
  }
})
