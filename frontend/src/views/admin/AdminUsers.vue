<template>
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <div class="mb-8">
      <h1 class="page-header">Quản lý người dùng</h1>
      <p class="text-gray-600 mt-1">Quản lý tài khoản người dùng hệ thống</p>
    </div>

    <!-- Filters -->
    <div class="card p-4 mb-6">
      <div class="flex flex-col md:flex-row gap-4">
        <div class="flex-1 relative">
          <MagnifyingGlassIcon class="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
          <input
            v-model="searchTerm"
            type="text"
            placeholder="Tìm kiếm theo tên, email..."
            class="input-field pl-10"
            @input="debouncedSearch"
          />
        </div>
        <select v-model="filter.vaiTro" class="input-field md:w-40" @change="loadUsers">
          <option value="">Tất cả vai trò</option>
          <option value="Admin">Admin</option>
          <option value="User">Người dùng</option>
        </select>
        <select v-model="filter.trangThai" class="input-field md:w-48" @change="loadUsers">
          <option value="">Tất cả trạng thái</option>
          <option value="HoatDong">Hoạt động</option>
          <option value="BiKhoa">Bị khóa</option>
        </select>
      </div>
    </div>

    <!-- Users Table -->
    <div class="card overflow-hidden">
      <div class="overflow-x-auto">
        <table class="w-full">
          <thead class="bg-gray-50 border-b border-gray-200">
            <tr>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Người dùng
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Vai trò
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Trạng thái
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Ngày tạo
              </th>
              <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                Thao tác
              </th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-200">
            <tr v-if="loading">
              <td colspan="5" class="px-6 py-12 text-center">
                <ArrowPathIcon class="w-8 h-8 text-primary-600 animate-spin mx-auto" />
              </td>
            </tr>
            <tr v-else-if="users.length === 0">
              <td colspan="5" class="px-6 py-12 text-center text-gray-500">
                Không tìm thấy người dùng nào
              </td>
            </tr>
            <tr v-for="user in users" :key="user.maNguoiDung" class="hover:bg-gray-50">
              <td class="px-6 py-4">
                <div class="flex items-center">
                  <div v-if="user?.anhDaiDien" class="w-10 h-10 rounded-full from-primary-500 to-secondary-500 flex items-center justify-center text-white font-medium">
              <img :src="getAvatarUrl(user.anhDaiDien)" alt="Avatar" class="w-10 h-10 rounded-full object-cover" />
            </div>
            <div v-else class="w-10 h-10 rounded-full bg-gradient-to-br from-primary-500 to-secondary-500 flex items-center justify-center text-white font-medium">
              {{ user.hoTen?.charAt(0) || 'U' }}
            </div>
                  <div class="ml-4">
                    <div class="font-medium text-gray-900">{{ user.hoTen }}</div>
                    <div class="text-sm text-gray-500">{{ user.email }}</div>
                  </div>
                </div>
              </td>
              <td class="px-6 py-4">
                <span :class="[
                  'badge',
                  isAdmin(user) ? 'badge-error' : 'badge-info'
                ]">
                  {{ isAdmin(user) ? 'Admin' : 'Người dùng' }}
                </span>
              </td>
              <td class="px-6 py-4">
                <span :class="[
                  'badge',
                  user.trangThaiKichHoat ? 'badge-success' : 'badge-error'
                ]">
                  {{ user.trangThaiKichHoat ? 'Hoạt động' : 'Bị khóa' }}
                </span>
              </td>
              <td class="px-6 py-4 text-sm text-gray-500">
                {{ formatDate(user.ngayTao) }}
              </td>
              <td class="px-6 py-4 text-right">
                <div class="flex justify-end gap-2">
                  <button @click="editUser(user)" class="p-2 text-gray-400 hover:text-primary-600 hover:bg-primary-50 rounded-lg">
                    <PencilIcon class="w-5 h-5" />
                  </button>
                  <button 
                    @click="toggleUserStatus(user)"
                    :disabled="isSelf(user)"
                    :title="isSelf(user) ? 'Không thể khóa chính mình' : (user.trangThaiKichHoat ? 'Khóa tài khoản' : 'Mở khóa tài khoản')"
                    :class="[
                      'p-2 rounded-lg',
                      isSelf(user) ? 'text-gray-300 cursor-not-allowed' :
                      user.trangThaiKichHoat 
                        ? 'text-gray-400 hover:text-red-600 hover:bg-red-50' 
                        : 'text-gray-400 hover:text-green-600 hover:bg-green-50'
                    ]"
                  >
                    <LockClosedIcon v-if="user.trangThaiKichHoat" class="w-5 h-5" />
                    <LockOpenIcon v-else class="w-5 h-5" />
                  </button>
                  <button @click="confirmDelete(user)" class="p-2 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg">
                    <TrashIcon class="w-5 h-5" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <div v-if="pagination.totalPages > 1" class="px-6 py-4 border-t border-gray-200 flex items-center justify-between">
        <p class="text-sm text-gray-500">
          Hiển thị {{ (pagination.currentPage - 1) * filter.pageSize + 1 }} - 
          {{ Math.min(pagination.currentPage * filter.pageSize, pagination.totalCount) }} 
          trong {{ pagination.totalCount }} người dùng
        </p>
        <nav class="flex items-center gap-2">
          <button
            @click="goToPage(pagination.currentPage - 1)"
            :disabled="pagination.currentPage === 1"
            class="btn-secondary px-3 py-1.5"
          >
            Trước
          </button>
          <button
            @click="goToPage(pagination.currentPage + 1)"
            :disabled="pagination.currentPage === pagination.totalPages"
            class="btn-secondary px-3 py-1.5"
          >
            Sau
          </button>
        </nav>
      </div>
    </div>

    <!-- Edit Role Modal -->
    <TransitionRoot appear :show="showEditModal" as="template">
      <Dialog as="div" @close="closeModals" class="relative z-50">
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
                  Đổi vai trò người dùng
                </DialogTitle>
                
                <div class="mb-4 p-3 bg-gray-50 rounded-lg">
                  <p class="font-medium text-gray-900">{{ userForm.hoTen }}</p>
                  <p class="text-sm text-gray-500">{{ userForm.email }}</p>
                </div>

                <form @submit.prevent="saveUser" class="space-y-4">
                  <div>
                    <label class="block text-sm font-medium text-gray-700 mb-1">Vai trò</label>
                    <select v-model="userForm.vaiTro" class="input-field" :disabled="userForm.isSelf">
                      <option value="User">Người dùng</option>
                      <option value="Admin">Admin</option>
                    </select>
                    <p v-if="userForm.isSelf" class="text-sm text-amber-600 mt-1">
                      Bạn không thể thay đổi vai trò của chính mình
                    </p>
                  </div>

                  <div class="flex justify-end gap-3 mt-6">
                    <button type="button" @click="closeModals" class="btn-secondary">
                      Hủy
                    </button>
                    <button type="submit" :disabled="saving || userForm.isSelf" class="btn-primary">
                      {{ saving ? 'Đang lưu...' : 'Lưu' }}
                    </button>
                  </div>
                </form>
              </DialogPanel>
            </TransitionChild>
          </div>
        </div>
      </Dialog>
    </TransitionRoot>

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
                  Bạn có chắc chắn muốn xóa người dùng "{{ userToDelete?.hoTen }}"?
                </p>
                <div class="mt-6 flex justify-end gap-3">
                  <button @click="showDeleteModal = false" class="btn-secondary">
                    Hủy
                  </button>
                  <button @click="deleteUser" class="btn-danger">
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
import { useAuthStore } from '@/stores/auth'
import {
  Dialog,
  DialogPanel,
  DialogTitle,
  TransitionRoot,
  TransitionChild
} from '@headlessui/vue'
import {
  MagnifyingGlassIcon,
  PencilIcon,
  TrashIcon,
  LockClosedIcon,
  LockOpenIcon,
  ArrowPathIcon
} from '@heroicons/vue/24/outline'
import { adminService } from '@/services'

const toast = useToast()
const authStore = useAuthStore()

// Helper functions
const isAdmin = (user) => {
  const roles = user.roles || user.Roles || []
  return roles.includes('Admin')
}

const isSelf = (user) => {
  return user.id === authStore.userId
}

const loading = ref(true)
const saving = ref(false)
const users = ref([])
const searchTerm = ref('')

const showEditModal = ref(false)
const showDeleteModal = ref(false)
const userToDelete = ref(null)

const filter = reactive({
  vaiTro: '',
  trangThai: '',
  pageNumber: 1,
  pageSize: 10
})

const pagination = reactive({
  currentPage: 1,
  totalPages: 1,
  totalCount: 0
})

const userForm = reactive({
  id: null,
  hoTen: '',
  email: '',
  vaiTro: 'User',
  isSelf: false
})

const formatDate = (dateString) => {
  if (!dateString) return ''
  return new Date(dateString).toLocaleDateString('vi-VN')
}

const loadUsers = async () => {
  loading.value = true
  try {
    const response = await adminService.getUsers({
      ...filter,
      searchTerm: searchTerm.value,
      role: filter.vaiTro,            // Gửi giá trị của 'vaiTro' vào tham số tên là 'role'
      activeStatus: filter.trangThai  // Gửi giá trị của 'trangThai' vào tham số tên là 'activeStatus'
    })
    users.value = response.data || []
    pagination.currentPage = response.pagination?.currentPage || 1
    pagination.totalPages = response.pagination?.totalPages || 1
    pagination.totalCount = response.pagination?.totalCount || 0
  } catch (error) {
    console.error('Failed to load users:', error)
    toast.error('Không thể tải danh sách người dùng')
  } finally {
    loading.value = false
  }
}

const debouncedSearch = useDebounceFn(() => {
  filter.pageNumber = 1
  loadUsers()
}, 300)

const goToPage = (page) => {
  if (page < 1 || page > pagination.totalPages) return
  filter.pageNumber = page
  loadUsers()
}

const editUser = (user) => {
  const userIsSelf = isSelf(user)
  Object.assign(userForm, {
    id: user.id,
    hoTen: user.hoTen,
    email: user.email,
    vaiTro: isAdmin(user) ? 'Admin' : 'User',
    isSelf: userIsSelf
  })
  showEditModal.value = true
}

const closeModals = () => {
  showEditModal.value = false
  Object.assign(userForm, {
    id: null,
    hoTen: '',
    email: '',
    vaiTro: 'User',
    isSelf: false
  })
}

const saveUser = async () => {
  if (userForm.isSelf) return
  
  saving.value = true
  try {
    await adminService.updateUserRole(userForm.id, userForm.vaiTro)
    closeModals()
    loadUsers()
  } catch (error) {
    toast.error(error.response?.data?.message || 'Không thể cập nhật vai trò')
  } finally {
    saving.value = false
  }
}

const toggleUserStatus = async (user) => {
  if (isSelf(user)) {
    toast.error('Bạn không thể khóa chính mình')
    return
  }
  try {
    await adminService.toggleUserStatus(user.id)
    user.trangThaiKichHoat = !user.trangThaiKichHoat
  } catch (error) {
    toast.error('Không thể cập nhật trạng thái')
  }
}

const confirmDelete = (user) => {
  if (isSelf(user)) {
    toast.error('Bạn không thể xóa chính mình')
    return
  }
  userToDelete.value = user
  showDeleteModal.value = true
}

const deleteUser = async () => {
  try {
    await adminService.deleteUser(userToDelete.value.id)
    showDeleteModal.value = false
    loadUsers()
  } catch (error) {
    toast.error('Không thể xóa người dùng')
  }
}

onMounted(() => {
  loadUsers()
})

const getAvatarUrl = (avatar) => {
  if (!avatar) return null
  // If already full URL (starts with http), return as is
  if (avatar.startsWith('http') || avatar.startsWith('https')) return avatar
  // Otherwise, prepend the API base URL (without /api)
  return `${import.meta.env.VITE_API_URL?.replace('/api', '') || ''}${avatar}`
}
</script>
