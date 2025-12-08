<template>
  <div class="min-h-screen bg-gray-50">
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

const isQuizTaking = computed(() => {
  return route.name === 'quiz-take' || route.name === 'quiz-result'
})

const isAuthPage = computed(() => {
  return route.name === 'login' || route.name === 'register' || route.name === 'forgot-password'
})
</script>
