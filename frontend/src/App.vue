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

  <div
    v-if="showIdleWarning"
    class="fixed inset-0 z-[100] flex items-center justify-center bg-black/50 p-4"
  >
    <div class="w-full max-w-md rounded-2xl bg-white p-6 shadow-2xl">
      <h2 class="text-xl font-semibold text-gray-900">Phiên sắp hết hạn</h2>
      <p class="mt-3 text-sm text-gray-600">
        Bạn đã không hoạt động trong thời gian dài. Hệ thống sẽ tự đăng xuất sau
        <span class="font-bold text-red-600">{{ formattedIdleWarningCountdown }}</span>
        .
      </p>

      <div class="mt-6 flex justify-end gap-3">
        <button
          type="button"
          class="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-indigo-500"
          @click="handleContinueSession"
        >
          Tiếp tục
        </button>
      </div>
    </div>
  </div>

  <SpeedInsights />
</template>

<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import Navbar from '@/components/layout/Navbar.vue'
import Footer from '@/components/layout/Footer.vue'
import { SpeedInsights } from "@vercel/speed-insights/vue"
import { useAuthStore } from '@/stores/auth'

const route = useRoute()
const authStore = useAuthStore()

// --- LOGIC BẢO TRÌ ---
// Kiểm tra biến môi trường VITE_IS_MAINTENANCE
const isMaintenanceMode = import.meta.env.VITE_IS_MAINTENANCE === 'true'

const isQuizTaking = computed(() => {
  return route.name === 'quiz-take' || route.name === 'quiz-result'
})

const isAuthPage = computed(() => {
  return route.name === 'login' || route.name === 'register' || route.name === 'forgot-password'
})

const showIdleWarning = computed(() => authStore.idleWarningVisible)
const idleWarningCountdown = computed(() => authStore.idleWarningCountdown)
const formattedIdleWarningCountdown = computed(() => formatAsMinutesSeconds(idleWarningCountdown.value))

function formatAsMinutesSeconds(totalSeconds) {
  const safeSeconds = Math.max(0, Number(totalSeconds) || 0)
  const minutes = Math.floor(safeSeconds / 60)
  const seconds = safeSeconds % 60
  return `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`
}

function handleContinueSession() {
  authStore.continueSession()
}
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