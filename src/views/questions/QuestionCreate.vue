<template>
  <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <div class="mb-8">
      <h1 class="text-2xl font-bold text-gray-900">Tạo câu hỏi mới</h1>
      <p class="text-gray-600 text-base mt-2">Thêm câu hỏi vào ngân hàng đề của bạn</p>
    </div>

    <form @submit.prevent="handleSubmit" class="space-y-6">
      <!-- Category & Level - Moved to top -->
      <div class="card p-6">
        <h2 class="text-base font-semibold text-gray-900 mb-4">Phân loại</h2>
        
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label class="block text-base font-medium text-gray-700 mb-2">
              Danh mục <span class="text-red-500">*</span>
            </label>
            <select v-model="form.maDanhMuc" class="input-field py-2.5 text-base">
              <option value="" disabled>Không có danh mục</option>
              <option v-for="cat in categories" :key="cat.maDanhMuc" :value="cat.maDanhMuc">
                {{ cat.tenDanhMuc }}
              </option>
            </select>
          </div>

          <div>
            <label class="block text-base font-medium text-gray-700 mb-2">
              Độ khó
            </label>
            <select v-model="form.mucDo" class="input-field py-2.5 text-base">
              <option value="De">Dễ</option>
              <option value="TrungBinh">Trung bình</option>
              <option value="Kho">Khó</option>
            </select>
          </div>
        </div>
      </div>

      <!-- Question Content -->
      <div class="card p-6">
        <h2 class="text-base font-semibold text-gray-900 mb-4">Nội dung câu hỏi</h2>
        
        <div>
          <label class="block text-base font-medium text-gray-700 mb-2">
            Nội dung câu hỏi <span class="text-red-500">*</span>
          </label>
          <textarea
            v-model="form.noiDungCauHoi"
            rows="1"
            required
            placeholder="Nhập nội dung câu hỏi..."
            class="textarea-auto text-base"
            @input="autoResizeTextarea"
          ></textarea>
        </div>
      </div>

      <!-- Answer Options -->
      <div class="card p-6">
        <h2 class="text-base font-semibold text-gray-900 mb-4">Đáp án (4 lựa chọn)</h2>

        <div class="space-y-3">
          <div 
            v-for="(option, index) in form.cacLuaChon" 
            :key="index"
            class="flex items-center gap-3 p-3 bg-gray-50 rounded-lg"
          >
            <span class="w-8 h-8 rounded-full bg-gray-200 flex items-center justify-center text-sm font-medium text-gray-600 shrink-0">
              {{ String.fromCharCode(65 + index) }}
            </span>
            <button
              type="button"
              @click="toggleCorrect(index)"
              :class="[
                'w-7 h-7 rounded-full border-2 flex items-center justify-center transition-colors shrink-0',
                option.laDapAnDung 
                  ? 'bg-green-500 border-green-500 text-white' 
                  : 'border-gray-300 hover:border-green-400'
              ]"
            >
              <CheckIcon v-if="option.laDapAnDung" class="w-4 h-4" />
            </button>
            
            <textarea
              v-model="option.noiDung"
              rows="1"
              required
              :placeholder="`Đáp án ${String.fromCharCode(65 + index)}`"
              class="textarea-auto flex-1 text-base"
              @input="autoResizeTextarea"
            ></textarea>
          </div>
        </div>

        <p v-if="!hasCorrectAnswer" class="mt-3 text-base text-red-500">
          Vui lòng chọn ít nhất một đáp án đúng
        </p>
        <p v-else-if="!allOptionsFilled" class="mt-3 text-base text-red-500">
          Vui lòng điền đủ 4 đáp án, không được để trống ô nào
        </p>
      </div>

      <!-- Giải thích - Moved below answers -->
      <div class="card p-6">
        <h2 class="text-base font-semibold text-gray-900 mb-4">Giải thích đáp án</h2>
        <div>
          <label class="block text-base font-medium text-gray-700 mb-2">
            Giải thích (hiển thị sau khi trả lời)
          </label>
          <textarea
            v-model="form.giaiThich"
            rows="1"
            placeholder="Giải thích tại sao đáp án đúng..."
            class="textarea-auto text-base"
            @input="autoResizeTextarea"
          ></textarea>
        </div>
      </div>

      <!-- Submit -->
      <div class="flex justify-end gap-4 pt-2">
        <button type="button" @click="resetForm" class="btn-secondary text-base px-5 py-2.5">
          Làm mới
        </button>
        <button
          type="button"
          @click="saveAndContinue"
          :disabled="submitting || !isFormValid || !form.maDanhMuc"
          class="btn-secondary text-base px-5 py-2.5"
        >
          Lưu và tạo tiếp
        </button>
        <button
          type="submit"
          :disabled="submitting || !isFormValid || !form.maDanhMuc"
          class="btn-primary text-base px-5 py-2.5"
        >
          <ArrowPathIcon v-if="submitting" class="w-5 h-5 mr-2 animate-spin" />
          {{ submitting ? 'Đang lưu...' : 'Lưu câu hỏi' }}
        </button>
      </div>
    </form>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'vue-toastification'
import {
  CheckIcon,
  ArrowPathIcon
} from '@heroicons/vue/24/outline'
import { questionService, categoryService } from '@/services'

const router = useRouter()
const toast = useToast()

const submitting = ref(false)
const categories = ref([])

const createEmptyOption = () => ({
  noiDung: '',
  laDapAnDung: false
})

const form = reactive({
  noiDungCauHoi: '',
  giaiThich: '',
  loaiCauHoi: 'MotDapAn',
  maDanhMuc: '',
  mucDo: 'De',
  cacLuaChon: [
    createEmptyOption(),
    createEmptyOption(),
    createEmptyOption(),
    createEmptyOption()
  ]
})

const hasCorrectAnswer = computed(() => {
  return form.cacLuaChon.some(opt => opt.laDapAnDung)
})

const allOptionsFilled = computed(() => {
  return form.cacLuaChon.length === 4 && form.cacLuaChon.every(opt => opt.noiDung.trim() !== '')
})

const isFormValid = computed(() => {
  return form.noiDungCauHoi.trim() !== '' && 
         hasCorrectAnswer.value &&
         allOptionsFilled.value
})

const loadCategories = async () => {
  try {
    const response = await categoryService.getCategories()
    categories.value = response.data || []
  } catch (error) {
    console.error('Failed to load categories:', error)
  }
}

const toggleCorrect = (index) => {
  if (form.loaiCauHoi === 'MotDapAn') {
    // Single answer: uncheck all others
    form.cacLuaChon.forEach((opt, i) => {
      opt.laDapAnDung = i === index
    })
  } else {
    // Multiple answers: toggle
    form.cacLuaChon[index].laDapAnDung = !form.cacLuaChon[index].laDapAnDung
  }
}

const resetForm = () => {
  form.noiDungCauHoi = ''
  form.giaiThich = ''
  form.loaiCauHoi = 'MotDapAn'
  form.maDanhMuc = ''
  form.mucDo = 'De'
  form.cacLuaChon = [
    createEmptyOption(),
    createEmptyOption(),
    createEmptyOption(),
    createEmptyOption()
  ]
}

const prepareData = () => {
  return {
    noiDungCauHoi: form.noiDungCauHoi,
    giaiThich: form.giaiThich || null,
    maDanhMuc: form.maDanhMuc || null,
    mucDo: form.mucDo,
    congKhai: false,
    cacLuaChon: form.cacLuaChon.map((opt, index) => ({
      noiDungDapAn: opt.noiDung,
      laDapAnDung: opt.laDapAnDung,
      thuTu: index
    }))
  }
}

const handleSubmit = async () => {
  if (!form.noiDungCauHoi.trim()) {
    toast.error('Vui lòng nhập nội dung câu hỏi')
    return
  }
  
  if (!allOptionsFilled.value) {
    toast.error('Vui lòng điền đủ 4 đáp án, không được để trống ô nào')
    return
  }
  
  if (!hasCorrectAnswer.value) {
    toast.error('Vui lòng chọn ít nhất một đáp án đúng')
    return
  }

  submitting.value = true
  try {
    const data = prepareData()
    const response = await questionService.createQuestion(data)
    
    if (response.success) {
      router.push('/questions')
    } else {
      toast.error(response.message || 'Không thể tạo câu hỏi')
    }
  } catch (error) {
    toast.error(error.response?.data?.message || 'Không thể tạo câu hỏi')
  } finally {
    submitting.value = false
  }
}

const saveAndContinue = async () => {
  if (!form.noiDungCauHoi.trim()) {
    toast.error('Vui lòng nhập nội dung câu hỏi')
    return
  }
  
  if (!allOptionsFilled.value) {
    toast.error('Vui lòng điền đủ 4 đáp án, không được để trống ô nào')
    return
  }
  
  if (!hasCorrectAnswer.value) {
    toast.error('Vui lòng chọn ít nhất một đáp án đúng')
    return
  }

  submitting.value = true
  try {
    const data = prepareData()
    const response = await questionService.createQuestion(data)
    
    if (response.success) {
      resetForm()
    } else {
      toast.error(response.message || 'Không thể tạo câu hỏi')
    }
  } catch (error) {
    toast.error(error.response?.data?.message || 'Không thể tạo câu hỏi')
  } finally {
    submitting.value = false
  }
}

// Auto resize textarea to fit content
const autoResizeTextarea = (event) => {
  const textarea = event.target
  textarea.style.height = 'auto'
  textarea.style.height = textarea.scrollHeight + 'px'
}

onMounted(() => {
  loadCategories()
})
</script>
