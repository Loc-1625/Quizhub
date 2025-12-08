<template>
  <router-link 
    :to="{ name: 'quiz-detail', params: { id: quiz.maBaiThi } }"
    class="card group cursor-pointer"
  >
    <!-- Cover Image -->
    <div class="relative h-32 bg-gradient-to-br from-primary-500 to-secondary-500 overflow-hidden">
      <img 
        v-if="quiz.anhBia" 
        :src="quiz.anhBia" 
        :alt="quiz.tieuDe"
        class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
      />
      <div v-else class="absolute inset-0 flex items-center justify-center">
        <DocumentTextIcon class="w-12 h-12 text-white/50" />
      </div>
      
      <!-- Status Badge -->
      <div class="absolute top-2 right-2">
        <span 
          v-if="quiz.cheDoCongKhai === 'CongKhai'" 
          class="badge badge-success"
        >
          <GlobeAltIcon class="w-3 h-3 mr-1" />
          Công khai
        </span>
        <span 
          v-else-if="quiz.cheDoCongKhai === 'CoMatKhau'" 
          class="badge bg-yellow-100 text-yellow-700"
        >
          <LockClosedIcon class="w-3 h-3 mr-1" />
          Có mật khẩu
        </span>
        <span v-else class="badge bg-gray-100 text-gray-700">
          <LockClosedIcon class="w-3 h-3 mr-1" />
          Riêng tư
        </span>
      </div>
    </div>

    <!-- Content -->
    <div class="p-4">
      <h3 class="font-semibold text-gray-900 line-clamp-2 group-hover:text-primary-600 transition-colors">
        {{ quiz.tieuDe }}
      </h3>
      
      <p v-if="quiz.moTa" class="text-sm text-gray-500 line-clamp-2 mt-1">
        {{ quiz.moTa }}
      </p>

      <!-- Quiz Code -->
      <div v-if="quiz.maTruyCapDinhDanh" class="mt-2 inline-flex items-center px-2 py-1 bg-primary-50 text-primary-700 rounded text-xs font-mono">
        Mã truy cập: {{ quiz.maTruyCapDinhDanh }}
      </div>

      <!-- Stats -->
      <div class="flex items-center gap-4 mt-3 text-sm text-gray-500">
        <div class="flex items-center">
          <DocumentTextIcon class="w-4 h-4 mr-1" />
          {{ quiz.soCauHoi }} câu
        </div>
        <div class="flex items-center">
          <ClockIcon class="w-4 h-4 mr-1" />
          {{ quiz.thoiGianLamBai }} phút
        </div>
      </div>

      <div class="flex items-center gap-4 mt-2 text-sm text-gray-500">
        <div class="flex items-center">
          <UserGroupIcon class="w-4 h-4 mr-1" />
          {{ quiz.tongLuotLamBai || 0 }} lượt
        </div>
        <div v-if="quiz.diemTrungBinh" class="flex items-center">
          <StarIcon class="w-4 h-4 mr-1 text-yellow-400 fill-yellow-400" />
          {{ quiz.diemTrungBinh.toFixed(1) }}
        </div>
      </div>

      <!-- Author -->
      <div class="flex items-center justify-between mt-3 pt-3 border-t border-gray-100">
        <div class="flex items-center">
          <div class="w-6 h-6 rounded-full bg-gradient-to-br from-primary-500 to-secondary-500 flex items-center justify-center">
            <span class="text-white text-xs font-medium">
              {{ getInitials(quiz.tenNguoiTao) }}
            </span>
          </div>
          <span class="ml-2 text-sm text-gray-600 truncate">{{ quiz.tenNguoiTao }}</span>
        </div>
        <div class="flex items-center text-xs text-gray-400">
          <CalendarIcon class="w-3.5 h-3.5 mr-1" />
          {{ formatDate(quiz.ngayTao) }}
        </div>
      </div>
    </div>
  </router-link>
</template>

<script setup>
import {
  DocumentTextIcon,
  ClockIcon,
  UserGroupIcon,
  StarIcon,
  GlobeAltIcon,
  LockClosedIcon,
  CalendarIcon
} from '@heroicons/vue/24/outline'
import { StarIcon as StarSolidIcon } from '@heroicons/vue/24/solid'

const props = defineProps({
  quiz: {
    type: Object,
    required: true
  }
})

const getInitials = (name) => {
  if (!name) return '?'
  return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2)
}

const formatDate = (dateStr) => {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  return date.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })
}
</script>
