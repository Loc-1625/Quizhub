<template>
  <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <!-- Header -->
    <div class="card p-6 mb-6">
      <div class="flex flex-col md:flex-row md:items-center gap-6">
        <!-- Avatar -->
        <div class="relative">
          <img 
            v-if="user?.anhDaiDien" 
            :src="getAvatarUrl(user.anhDaiDien)" 
            :alt="user.hoTen"
            class="w-24 h-24 rounded-full object-cover border-4 border-white shadow-[0_0_15px_rgba(0,0,0,0.15)]"
          />
          <div 
            v-else 
            class="w-24 h-24 rounded-full bg-gradient-to-br from-primary-500 to-secondary-500 flex items-center justify-center text-white text-3xl font-bold shadow-[0_0_15px_rgba(0,0,0,0.15)]"
          >
            {{ user?.hoTen?.charAt(0) || 'U' }}
          </div>
          <label class="absolute bottom-0 right-0 p-2 bg-white rounded-full shadow-md cursor-pointer hover:bg-gray-50">
            <CameraIcon class="w-5 h-5 text-gray-600" />
            <input type="file" accept="image/*" class="hidden" @change="uploadAvatar" />
          </label>
        </div>

        <!-- User Info -->
        <div class="flex-1">
          <h1 class="text-2xl font-bold text-gray-900">{{ user?.hoTen }}</h1>
          <p class="text-gray-500">{{ user?.email }}</p>
        </div>
      </div>
    </div>

    <!-- Tabs -->
    <div class="border-b border-gray-200 mb-6">
      <nav class="flex space-x-8">
        <button
          v-for="tab in tabs"
          :key="tab.id"
          @click="activeTab = tab.id"
          :class="[
            'py-4 px-1 border-b-2 font-medium text-sm whitespace-nowrap',
            activeTab === tab.id
              ? 'border-primary-500 text-primary-600'
              : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
          ]"
        >
          <component :is="tab.icon" class="w-5 h-5 inline-block mr-2" />
          {{ tab.name }}
        </button>
      </nav>
    </div>

    <!-- Tab Content -->
    <div v-if="activeTab === 'info'" class="space-y-6">
      <form @submit.prevent="updateProfile" class="card p-6">
        <h2 class="section-title mb-4">Thông tin cá nhân</h2>
        
        <div class="space-y-4">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Họ và tên</label>
              <input v-model="profile.hoTen" type="text" required class="input-field" />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Email</label>
              <input :value="user?.email" type="email" disabled class="input-field bg-gray-50" />
            </div>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Số điện thoại</label>
            <input v-model="profile.soDienThoai" type="tel" class="input-field" placeholder="Nhập số điện thoại" />
          </div>
        </div>

        <div class="mt-6 flex justify-end">
          <button type="submit" :disabled="saving" class="btn-primary">
            <ArrowPathIcon v-if="saving" class="w-5 h-5 mr-2 animate-spin" />
            {{ saving ? 'Đang lưu...' : 'Lưu thay đổi' }}
          </button>
        </div>
      </form>
    </div>

    <div v-if="activeTab === 'security'" class="space-y-6">
      <!-- Change Password -->
      <form @submit.prevent="changePassword" class="card p-6">
        <h2 class="section-title mb-4">Đổi mật khẩu</h2>
        
        <div class="space-y-4 max-w-md">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Mật khẩu hiện tại</label>
            <input v-model="passwordForm.currentPassword" type="password" required class="input-field" />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Mật khẩu mới</label>
            <input 
              v-model="passwordForm.newPassword" 
              type="password" 
              required 
              minlength="8" 
              placeholder="Ít nhất 8 ký tự, gồm chữ và số"
              class="input-field"
              :class="{ 'border-red-500 focus:ring-red-500': passwordErrors.newPassword }"
            />
            <p v-if="passwordErrors.newPassword" class="mt-1 text-sm text-red-500">{{ passwordErrors.newPassword }}</p>
            <!-- Password strength indicator -->
            <div v-if="passwordForm.newPassword" class="mt-2">
              <div class="flex flex-wrap items-center gap-2 text-xs">
                <span :class="passwordForm.newPassword.length >= 8 ? 'text-green-600' : 'text-gray-400'">
                  ✓ Ít nhất 8 ký tự
                </span>
                <span :class="/[a-z]/.test(passwordForm.newPassword) ? 'text-green-600' : 'text-gray-400'">
                  ✓ Chữ thường
                </span>
                <span :class="/[A-Z]/.test(passwordForm.newPassword) ? 'text-green-600' : 'text-gray-400'">
                  ✓ Chữ hoa
                </span>
                <span :class="/[0-9]/.test(passwordForm.newPassword) ? 'text-green-600' : 'text-gray-400'">
                  ✓ Chữ số
                </span>
              </div>
            </div>
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Xác nhận mật khẩu mới</label>
            <input v-model="passwordForm.confirmPassword" type="password" required class="input-field" />
          </div>
        </div>

        <div class="mt-6 flex justify-end">
          <button type="submit" :disabled="changingPassword" class="btn-primary">
            <ArrowPathIcon v-if="changingPassword" class="w-5 h-5 mr-2 animate-spin" />
            {{ changingPassword ? 'Đang xử lý...' : 'Đổi mật khẩu' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { useToast } from 'vue-toastification'
import { useAuthStore } from '@/stores/auth'
import {
  CameraIcon,
  UserIcon,
  ShieldCheckIcon,
  ArrowPathIcon
} from '@heroicons/vue/24/outline'
import { authService } from '@/services'

const toast = useToast()
const authStore = useAuthStore()

const user = computed(() => authStore.user)
const activeTab = ref('info')
const saving = ref(false)
const changingPassword = ref(false)

// Helper function to get full avatar URL
const getAvatarUrl = (avatar) => {
  if (!avatar) return null
  // If already full URL (starts with http), return as is
  if (avatar.startsWith('http') || avatar.startsWith('https')) return avatar
  // Otherwise, prepend the API base URL (without /api)
  return `${import.meta.env.VITE_API_URL?.replace('/api', '') || ''}${avatar}`
}

const tabs = [
  { id: 'info', name: 'Thông tin', icon: UserIcon },
  { id: 'security', name: 'Bảo mật', icon: ShieldCheckIcon }
]

const profile = reactive({
  hoTen: '',
  soDienThoai: ''
})

const passwordForm = reactive({
  currentPassword: '',
  newPassword: '',
  confirmPassword: ''
})

const passwordErrors = reactive({
  newPassword: '',
  confirmPassword: ''
})

const validatePassword = (password) => {
  if (password.length < 8) {
    return 'Mật khẩu phải có ít nhất 8 ký tự'
  }
  if (!/[a-z]/.test(password)) {
    return 'Mật khẩu phải có ít nhất 1 chữ thường'
  }
  if (!/[A-Z]/.test(password)) {
    return 'Mật khẩu phải có ít nhất 1 chữ hoa'
  }
  if (!/[0-9]/.test(password)) {
    return 'Mật khẩu phải có ít nhất 1 chữ số'
  }
  return ''
}

const loadProfile = () => {
  if (user.value) {
    // Hỗ trợ cả camelCase và PascalCase từ API
    profile.hoTen = user.value.hoTen || user.value.HoTen || ''
    profile.soDienThoai = user.value.soDienThoai || user.value.SoDienThoai || user.value.phoneNumber || ''
  }
}

const updateProfile = async () => {
  saving.value = true
  try {
    // Đảm bảo gửi đúng format backend cần
    const data = {
      hoTen: profile.hoTen,
      soDienThoai: profile.soDienThoai || null
    }
    const response = await authService.updateProfile(data)
    if (response.success) {
      await authStore.fetchCurrentUser()
    } else {
      toast.error(response.message || 'Không thể cập nhật')
    }
  } catch (error) {
    console.error('Update profile error:', error.response?.data)
    const errorMsg = error.response?.data?.errors 
      ? Object.values(error.response.data.errors).flat().join(', ')
      : error.response?.data?.message || 'Không thể cập nhật'
    toast.error(errorMsg)
  } finally {
    saving.value = false
  }
}

const changePassword = async () => {
  // Reset errors
  passwordErrors.newPassword = ''
  passwordErrors.confirmPassword = ''

  // Validate new password
  passwordErrors.newPassword = validatePassword(passwordForm.newPassword)
  if (passwordErrors.newPassword) {
    return
  }

  if (passwordForm.newPassword !== passwordForm.confirmPassword) {
    passwordErrors.confirmPassword = 'Mật khẩu xác nhận không khớp'
    toast.error('Mật khẩu xác nhận không khớp')
    return
  }

  changingPassword.value = true
  try {
    const response = await authService.changePassword({
      currentPassword: passwordForm.currentPassword,
      newPassword: passwordForm.newPassword,
      confirmPassword: passwordForm.confirmPassword
    })
    
    if (response.success) {
      passwordForm.currentPassword = ''
      passwordForm.newPassword = ''
      passwordForm.confirmPassword = ''
    } else {
      toast.error(response.message || 'Không thể đổi mật khẩu')
    }
  } catch (error) {
    toast.error(error.response?.data?.message || 'Không thể đổi mật khẩu')
  } finally {
    changingPassword.value = false
  }
}

const uploadAvatar = async (event) => {
  const file = event.target.files[0]
  if (!file) return

  if (!file.type.startsWith('image/')) {
    toast.error('Vui lòng chọn file ảnh')
    return
  }

  const formData = new FormData()
  formData.append('file', file)

  try {
    const response = await authService.uploadAvatar(formData)
    if (response.success) {
      await authStore.fetchCurrentUser()
    }
  } catch (error) {
    toast.error('Không thể upload ảnh')
  }
}

// Watch user changes to update form
watch(user, (newUser) => {
  if (newUser) {
    loadProfile()
  }
}, { immediate: true })

onMounted(async () => {
  // Nếu chưa có user, fetch lại
  if (!user.value) {
    await authStore.fetchCurrentUser()
  }
  loadProfile()
})
</script>
