<template>
 <!-- TRƯỜNG HỢP 1: ĐANG BẢO TRÌ -->
  <div v-if="isMaintenanceMode" class="min-h-screen bg-gray-100 flex flex-col items-center justify-center p-4 z-50">
    <div class="bg-white p-8 rounded-2xl shadow-xl max-w-md w-full text-center transform transition-all hover:scale-105 duration-300">
      <h1 class="text-3xl font-bold text-gray-800 mb-3">Hệ thống đang bảo trì</h1>
        Vui lòng quay lại sau
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

  <SpeedInsights />
</template>

<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import Navbar from '@/components/layout/Navbar.vue'
import Footer from '@/components/layout/Footer.vue'
import { SpeedInsights } from "@vercel/speed-insights/vue"

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