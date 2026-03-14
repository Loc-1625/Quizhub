<template>
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <div class="flex items-center justify-between mb-8">
      <div>
        <h1 class="page-header">Quản lý danh mục</h1>
        <p class="text-gray-600 mt-1">Tổ chức câu hỏi và bài thi theo danh mục</p>
      </div>
      <button @click="openCreateModal" class="btn-primary">
        <PlusIcon class="w-5 h-5 mr-2" />
        Thêm danh mục
      </button>
    </div>

    <!-- Categories Grid -->
    <div v-if="loading" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div v-for="i in 6" :key="i" class="card p-6 animate-pulse">
        <div class="h-6 bg-gray-200 rounded w-3/4 mb-4"></div>
        <div class="h-4 bg-gray-200 rounded w-1/2"></div>
      </div>
    </div>

    <div v-else-if="categories.length > 0" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div 
        v-for="category in categories" 
        :key="category.maDanhMuc" 
        class="card p-6 hover:shadow-md transition-shadow group"
      >
        <div class="flex items-start justify-between mb-4">
          <div class="w-12 h-12 rounded-xl flex items-center justify-center bg-primary-100">
            <FolderIcon class="w-6 h-6 text-primary-600" />
          </div>
          <div class="flex gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
            <button @click="editCategory(category)" class="p-2 text-gray-400 hover:text-primary-600 hover:bg-primary-50 rounded-lg">
              <PencilIcon class="w-4 h-4" />
            </button>
            <button @click="confirmDelete(category)" class="p-2 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg">
              <TrashIcon class="w-4 h-4" />
            </button>
          </div>
        </div>

        <h3 class="font-semibold text-gray-900 mb-2">{{ category.tenDanhMuc }}</h3>
        <p v-if="category.moTa" class="text-sm text-gray-500 mb-4 line-clamp-2">{{ category.moTa }}</p>
        
        <div class="flex items-center justify-between text-sm">
          <span class="text-gray-500">{{ category.soCauHoi || 0 }} câu hỏi</span>
          <span class="text-gray-500">{{ category.soBaiThi || 0 }} bài thi</span>
        </div>
      </div>
    </div>

    <div v-else class="text-center py-16">
      <FolderIcon class="w-16 h-16 text-gray-300 mx-auto mb-4" />
      <h3 class="text-lg font-medium text-gray-900 mb-2">Chưa có danh mục nào</h3>
      <p class="text-gray-500 mb-6">Tạo danh mục để tổ chức câu hỏi tốt hơn</p>
      <button @click="openCreateModal" class="btn-primary">
        <PlusIcon class="w-5 h-5 mr-2" />
        Thêm danh mục đầu tiên
      </button>
    </div>

    <!-- Create/Edit Modal -->
    <TransitionRoot appear :show="showModal" as="template">
      <Dialog as="div" @close="closeModal" class="relative z-50">
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
                  {{ isEditing ? 'Chỉnh sửa danh mục' : 'Thêm danh mục mới' }}
                </DialogTitle>
                
                <form @submit.prevent="saveCategory" class="space-y-4">
                  <div>
                    <label class="block text-sm font-medium text-gray-700 mb-1">
                      Tên danh mục <span class="text-red-500">*</span>
                    </label>
                    <input 
                      v-model="form.tenDanhMuc" 
                      type="text" 
                      required 
                      placeholder="Nhập tên danh mục"
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
                      placeholder="Mô tả ngắn về danh mục"
                      class="input-field" 
                    ></textarea>
                  </div>

                  <div class="flex justify-end gap-3 mt-6">
                    <button type="button" @click="closeModal" class="btn-secondary">
                      Hủy
                    </button>
                    <button type="submit" :disabled="saving" class="btn-primary">
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
                  Bạn có chắc chắn muốn xóa danh mục "{{ categoryToDelete?.tenDanhMuc }}"?
                  Các câu hỏi và bài thi trong danh mục này sẽ không bị xóa.
                </p>
                <div class="mt-6 flex justify-end gap-3">
                  <button @click="showDeleteModal = false" class="btn-secondary">
                    Hủy
                  </button>
                  <button @click="deleteCategory" class="btn-danger">
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
import { useToast } from 'vue-toastification'
import {
  Dialog,
  DialogPanel,
  DialogTitle,
  TransitionRoot,
  TransitionChild
} from '@headlessui/vue'
import {
  PlusIcon,
  FolderIcon,
  PencilIcon,
  TrashIcon
} from '@heroicons/vue/24/outline'
import { categoryService } from '@/services'

const toast = useToast()

const loading = ref(true)
const saving = ref(false)
const categories = ref([])

const showModal = ref(false)
const isEditing = ref(false)
const showDeleteModal = ref(false)
const categoryToDelete = ref(null)

const form = reactive({
  maDanhMuc: null,
  tenDanhMuc: '',
  moTa: ''
})

const loadCategories = async () => {
  loading.value = true
  try {
    const response = await categoryService.getCategories()
    categories.value = response.data || []
  } catch (error) {
    console.error('Failed to load categories:', error)
    toast.error('Không thể tải danh sách danh mục')
  } finally {
    loading.value = false
  }
}

const openCreateModal = () => {
  isEditing.value = false
  form.maDanhMuc = null
  form.tenDanhMuc = ''
  form.moTa = ''
  showModal.value = true
}

const editCategory = (category) => {
  isEditing.value = true
  form.maDanhMuc = category.maDanhMuc
  form.tenDanhMuc = category.tenDanhMuc
  form.moTa = category.moTa || ''
  showModal.value = true
}

const closeModal = () => {
  showModal.value = false
}

const saveCategory = async () => {
  saving.value = true
  try {
    if (isEditing.value) {
      await categoryService.updateCategory(form.maDanhMuc, form)
    } else {
      await categoryService.createCategory(form)
    }
    closeModal()
    loadCategories()
  } catch (error) {
    toast.error(error.response?.data?.message || 'Không thể lưu danh mục')
  } finally {
    saving.value = false
  }
}

const confirmDelete = (category) => {
  categoryToDelete.value = category
  showDeleteModal.value = true
}

const deleteCategory = async () => {
  try {
    await categoryService.deleteCategory(categoryToDelete.value.maDanhMuc)
    showDeleteModal.value = false
    loadCategories()
  } catch (error) {
    toast.error('Không thể xóa danh mục')
  }
}

onMounted(() => {
  loadCategories()
})
</script>
