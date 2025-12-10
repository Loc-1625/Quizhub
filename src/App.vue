<template>
 <!-- TRƯỜNG HỢP 1: ĐANG BẢO TRÌ -->
  <div v-if="isMaintenanceMode" class="min-h-screen bg-gray-100 flex flex-col items-center justify-center p-4 z-50">
    <div class="bg-white p-8 rounded-2xl shadow-xl max-w-md w-full text-center transform transition-all hover:scale-105 duration-300">
      <div class="mb-6 flex justify-center">
        <!-- Icon Bảo trì (Cờ lê & Tua vít) -->
        <div class="p-4 bg-yellow-100 rounded-full">
          <svg xmlns="http://www.w3.org/2000/svg" class="h-16 w-16 text-yellow-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5.882V19.24a1.76 1.76 0 01-3.417.592l-2.147-6.15M18 13a3 3 0 100-6M5.436 13.683A4.001 4.001 0 017 6h1.832c4.1 0 7.625-1.234 9.168-3v14c-1.543-1.766-5.067-3-9.168-3H7a3.988 3.988 0 01-1.564-.317z" />
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2" /> 
            <!-- Hoặc dùng icon Construction đơn giản -->
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.806.547M8 4h8l-1 1v5.172a2 2 0 00.586 1.414l5 5c1.26 1.26.367 3.414-1.415 3.414H4.828c-1.782 0-2.674-2.154-1.414-3.414l5-5A2 2 0 009 10.172V5L8 4z" />
          </svg>
        </div>
      </div>
      
      <h1 class="text-3xl font-bold text-gray-800 mb-3">Hệ thống đang bảo trì</h1>
      <p class="text-gray-600 mb-6 leading-relaxed">
        Vui lòng quay lại sau
      </p>
    </div>
  </div>

  <!-- TRƯỜNG HỢP 2: HOẠT ĐỘNG BÌNH THƯỜNG (Code cũ của bạn) -->
  <div v-else class="min-h-screen bg-gray-50">
    <!-- Navigation -->
    <Navbar v-if="!isQuizTaking" />
    
    <!-- Main Content -->
    <main :class="{ 'pt-16': !isQuizTaking }">
      <router-view v-slot="{ Component }">
        <transition name="fade" mode="out-in">
          <component :is="Component" />
        </transition>
      </router-view>
    </main>
    
    <!-- Footer -->
    <Footer v-if="!isQuizTaking && !isAuthPage" />
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import Navbar from '@/components/layout/Navbar.vue'
import Footer from '@/components/layout/Footer.vue'

const route = useRoute()

// --- LOGIC BẢO TRÌ ---
// Kiểm tra biến môi trường VITE_IS_MAINTENANCE
const isMaintenanceMode = import.meta.env.VITE_IS_MAINTENANCE === 'true'

const isQuizTaking = computed(() => {
  return route.name === 'quiz-take' || route.name === 'quiz-result'
})

const isAuthPage = computed(() => {
  return route.name === 'login' || route.name === 'register' || route.name === 'forgot-password'
})
</script>

<style>
/* Animation cho chuyển trang mượt mà */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>