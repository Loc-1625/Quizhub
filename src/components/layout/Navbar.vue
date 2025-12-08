<template>
  <nav class="fixed top-0 left-0 right-0 z-50 bg-white/80 backdrop-blur-lg border-b border-gray-200">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="flex items-center justify-between h-16">
        <!-- Logo -->
        <router-link to="/" class="flex items-center">
          <span class="text-2xl font-bold text-gradient">QuizHub</span>
        </router-link>

        <!-- Desktop Navigation -->
        <div class="hidden md:flex items-center space-x-1">
          <router-link to="/explore" class="nav-link">
            <MagnifyingGlassIcon class="w-5 h-5 mr-1" />
            Khám phá
          </router-link>
          
          <template v-if="authStore.isLoggedIn">
            <router-link to="/dashboard" class="nav-link">
              <HomeIcon class="w-5 h-5 mr-1" />
              Dashboard
            </router-link>
            <router-link to="/my-quizzes" class="nav-link">
              <DocumentTextIcon class="w-5 h-5 mr-1" />
              Bài thi của tôi
            </router-link>
            <router-link to="/questions" class="nav-link">
              <QuestionMarkCircleIcon class="w-5 h-5 mr-1" />
              Ngân hàng câu hỏi
            </router-link>
          </template>
        </div>

        <!-- Right Side -->
        <div class="flex items-center space-x-3">
          <!-- Join Quiz by Code -->
          <div class="hidden sm:block relative">
            <input
              v-model="joinCode"
              @keyup.enter="joinQuiz"
              type="text"
              placeholder="Nhập mã bài thi"
              class="w-40 px-3 py-1.5 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
            />
            <button
              @click="joinQuiz"
              class="absolute right-1 top-1/2 -translate-y-1/2 p-1 text-gray-400 hover:text-primary-600"
            >
              <ArrowRightIcon class="w-4 h-4" />
            </button>
          </div>

          <!-- Auth Buttons / User Menu -->
          <template v-if="!authStore.isLoggedIn">
            <router-link to="/login" class="btn-secondary text-sm">
              Đăng nhập
            </router-link>
            <router-link to="/register" class="btn-primary text-sm">
              Đăng ký
            </router-link>
          </template>

          <template v-else>
            <!-- Admin Link -->
            <router-link
              v-if="authStore.isAdmin"
              to="/admin"
              class="nav-link text-orange-600 hover:text-orange-700"
            >
              <CogIcon class="w-5 h-5" />
            </router-link>

            <!-- User Menu -->
            <Menu as="div" class="relative">
              <MenuButton class="flex items-center space-x-2 p-1.5 rounded-lg hover:bg-gray-100">
                <img
                  v-if="authStore.avatar"
                  :src="authStore.avatar"
                  :alt="authStore.fullName"
                  class="w-8 h-8 rounded-full object-cover"
                />
                <div v-else class="w-8 h-8 rounded-full bg-gradient-to-br from-primary-500 to-secondary-500 flex items-center justify-center">
                  <span class="text-white text-sm font-medium">{{ initials }}</span>
                </div>
                <ChevronDownIcon class="w-4 h-4 text-gray-500" />
              </MenuButton>

              <transition
                enter-active-class="transition duration-100 ease-out"
                enter-from-class="transform scale-95 opacity-0"
                enter-to-class="transform scale-100 opacity-100"
                leave-active-class="transition duration-75 ease-in"
                leave-from-class="transform scale-100 opacity-100"
                leave-to-class="transform scale-95 opacity-0"
              >
                <MenuItems class="absolute right-0 mt-2 w-56 origin-top-right bg-white rounded-xl shadow-lg ring-1 ring-black ring-opacity-5 focus:outline-none divide-y divide-gray-100">
                  <div class="px-4 py-3">
                    <p class="text-sm font-medium text-gray-900">{{ authStore.fullName }}</p>
                    <p class="text-sm text-gray-500 truncate">{{ authStore.user?.email }}</p>
                  </div>

                  <div class="py-1">
                    <MenuItem v-slot="{ active }">
                      <router-link
                        to="/profile"
                        :class="[active ? 'bg-gray-50' : '', 'flex items-center px-4 py-2 text-sm text-gray-700']"
                      >
                        <UserIcon class="w-4 h-4 mr-3" />
                        Hồ sơ cá nhân
                      </router-link>
                    </MenuItem>
                    <MenuItem v-slot="{ active }">
                      <router-link
                        to="/history"
                        :class="[active ? 'bg-gray-50' : '', 'flex items-center px-4 py-2 text-sm text-gray-700']"
                      >
                        <ClockIcon class="w-4 h-4 mr-3" />
                        Lịch sử làm bài
                      </router-link>
                    </MenuItem>
                  </div>

                  <div class="py-1">
                    <MenuItem v-slot="{ active }">
                      <button
                        @click="handleLogout"
                        :class="[active ? 'bg-gray-50' : '', 'flex w-full items-center px-4 py-2 text-sm text-red-600']"
                      >
                        <ArrowRightOnRectangleIcon class="w-4 h-4 mr-3" />
                        Đăng xuất
                      </button>
                    </MenuItem>
                  </div>
                </MenuItems>
              </transition>
            </Menu>
          </template>

          <!-- Mobile Menu Button -->
          <button
            @click="mobileMenuOpen = !mobileMenuOpen"
            class="md:hidden p-2 rounded-lg hover:bg-gray-100"
          >
            <Bars3Icon v-if="!mobileMenuOpen" class="w-6 h-6" />
            <XMarkIcon v-else class="w-6 h-6" />
          </button>
        </div>
      </div>
    </div>

    <!-- Mobile Menu -->
    <transition
      enter-active-class="transition duration-200 ease-out"
      enter-from-class="opacity-0 -translate-y-2"
      enter-to-class="opacity-100 translate-y-0"
      leave-active-class="transition duration-150 ease-in"
      leave-from-class="opacity-100 translate-y-0"
      leave-to-class="opacity-0 -translate-y-2"
    >
      <div v-if="mobileMenuOpen" class="md:hidden bg-white border-t border-gray-200">
        <div class="px-4 py-3 space-y-1">
          <!-- Join Code Input -->
          <div class="flex space-x-2 pb-3 border-b border-gray-200">
            <input
              v-model="joinCode"
              type="text"
              placeholder="Nhập mã bài thi"
              class="flex-1 px-3 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500"
            />
            <button @click="joinQuiz" class="btn-primary">
              Tham gia
            </button>
          </div>

          <router-link
            to="/explore"
            class="mobile-nav-link"
            @click="mobileMenuOpen = false"
          >
            <MagnifyingGlassIcon class="w-5 h-5 mr-3" />
            Khám phá bài thi
          </router-link>

          <template v-if="authStore.isLoggedIn">
            <router-link
              to="/dashboard"
              class="mobile-nav-link"
              @click="mobileMenuOpen = false"
            >
              <HomeIcon class="w-5 h-5 mr-3" />
              Dashboard
            </router-link>
            <router-link
              to="/my-quizzes"
              class="mobile-nav-link"
              @click="mobileMenuOpen = false"
            >
              <DocumentTextIcon class="w-5 h-5 mr-3" />
              Bài thi của tôi
            </router-link>
            <router-link
              to="/questions"
              class="mobile-nav-link"
              @click="mobileMenuOpen = false"
            >
              <QuestionMarkCircleIcon class="w-5 h-5 mr-3" />
              Ngân hàng câu hỏi
            </router-link>
            <router-link
              to="/history"
              class="mobile-nav-link"
              @click="mobileMenuOpen = false"
            >
              <ClockIcon class="w-5 h-5 mr-3" />
              Lịch sử làm bài
            </router-link>
            
            <div class="pt-3 border-t border-gray-200">
              <button
                @click="handleLogout"
                class="mobile-nav-link text-red-600 w-full"
              >
                <ArrowRightOnRectangleIcon class="w-5 h-5 mr-3" />
                Đăng xuất
              </button>
            </div>
          </template>

          <template v-else>
            <div class="pt-3 border-t border-gray-200 flex space-x-3">
              <router-link
                to="/login"
                class="flex-1 btn-secondary justify-center"
                @click="mobileMenuOpen = false"
              >
                Đăng nhập
              </router-link>
              <router-link
                to="/register"
                class="flex-1 btn-primary justify-center"
                @click="mobileMenuOpen = false"
              >
                Đăng ký
              </router-link>
            </div>
          </template>
        </div>
      </div>
    </transition>
  </nav>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { Menu, MenuButton, MenuItems, MenuItem } from '@headlessui/vue'
import {
  MagnifyingGlassIcon,
  HomeIcon,
  DocumentTextIcon,
  QuestionMarkCircleIcon,
  UserIcon,
  ClockIcon,
  CogIcon,
  ArrowRightIcon,
  ArrowRightOnRectangleIcon,
  ChevronDownIcon,
  Bars3Icon,
  XMarkIcon
} from '@heroicons/vue/24/outline'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const mobileMenuOpen = ref(false)
const joinCode = ref('')

const initials = computed(() => {
  const name = authStore.fullName || ''
  return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2)
})

const joinQuiz = () => {
  if (joinCode.value.trim()) {
    router.push({ name: 'quiz-detail', params: { id: 'code' }, query: { code: joinCode.value.trim() } })
    joinCode.value = ''
    mobileMenuOpen.value = false
  }
}

const handleLogout = () => {
  authStore.logout()
  mobileMenuOpen.value = false
  router.push({ name: 'home' })
}
</script>

<style scoped>
.nav-link {
  @apply flex items-center px-3 py-2 text-sm font-medium text-gray-600 
         rounded-lg hover:text-primary-600 hover:bg-primary-50 transition-colors;
}

.nav-link.router-link-active {
  @apply text-primary-600 bg-primary-50;
}

.mobile-nav-link {
  @apply flex items-center px-3 py-3 text-sm font-medium text-gray-700 
         rounded-lg hover:bg-gray-50;
}

.mobile-nav-link.router-link-active {
  @apply text-primary-600 bg-primary-50;
}
</style>
