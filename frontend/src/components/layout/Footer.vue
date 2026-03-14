<template>
  <footer class="bg-white border-t border-gray-200 mt-auto">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <div class="grid grid-cols-1 md:grid-cols-4 gap-8">
        <!-- Brand -->
        <div class="col-span-1 md:col-span-2">
          <div class="flex items-center mb-4">
            <span class="text-2xl font-bold text-gradient">QuizHub</span>
          </div>
          <p class="text-gray-600 mb-4 max-w-md">
            Nền tảng tạo và chia sẻ đề thi trắc nghiệm thông minh. 
            Sử dụng công nghệ AI và Real-time để mang đến trải nghiệm học tập tốt nhất.
          </p>
        </div>

        <!-- Links -->
        <div>
          <h3 class="text-sm font-semibold text-gray-900 uppercase tracking-wider mb-4">
            Sản phẩm
          </h3>
          <ul class="space-y-3">
            <li>
              <router-link to="/explore" class="text-gray-600 hover:text-primary-600 transition-colors">
                Khám phá bài thi
              </router-link>
            </li>
            <li>
              <router-link to="/quiz/create" class="text-gray-600 hover:text-primary-600 transition-colors">
                Tạo bài thi
              </router-link>
            </li>
            <li>
              <router-link to="/questions/import" class="text-gray-600 hover:text-primary-600 transition-colors">
                Import bằng AI
              </router-link>
            </li>
          </ul>
        </div>

        <div>
          <h3 class="text-sm font-semibold text-gray-900 uppercase tracking-wider mb-4">
            Hỗ trợ
          </h3>
          <ul class="space-y-3">
            <li>
              <a href="#" class="text-gray-600 hover:text-primary-600 transition-colors">
                Hướng dẫn sử dụng
              </a>
            </li>
            <li>
              <a href="#" class="text-gray-600 hover:text-primary-600 transition-colors">
                Câu hỏi thường gặp
              </a>
            </li>
            <li>
              <a href="#" class="text-gray-600 hover:text-primary-600 transition-colors">
                Liên hệ
              </a>
            </li>
            <li>
              <a href="#" class="text-gray-600 hover:text-primary-600 transition-colors">
                Điều khoản sử dụng
              </a>
            </li>
          </ul>
        </div>
      </div>

      <div class="mt-8 pt-8 border-t border-gray-200">
        <p class="text-gray-500 text-sm text-center">
          © {{ currentYear }} QuizHub - Đồ án học tập
        </p>
      </div>
    </div>
  </footer>
</template>

<script setup>
const currentYear = new Date().getFullYear()
</script>
