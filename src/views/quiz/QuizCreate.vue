<template>
  <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <div class="mb-8">
      <h1 class="page-header">Tạo bài thi mới</h1>
      <p class="text-gray-600 mt-1">Tạo bài thi từ ngân hàng câu hỏi của bạn</p>
    </div>

    <form @submit.prevent="handleSubmit" class="space-y-6">
      <!-- Basic Info -->
      <div class="card p-6">
        <h2 class="section-title mb-4">Thông tin cơ bản</h2>
        
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">
              Tiêu đề bài thi <span class="text-red-500">*</span>
            </label>
            <input
              v-model="form.tieuDe"
              type="text"
              required
              placeholder="Nhập tiêu đề bài thi"
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
              placeholder="Mô tả ngắn về bài thi"
              class="input-field"
            ></textarea>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">
                Danh mục
              </label>
              <select v-model="form.maDanhMuc" class="input-field">
                <option value="">-- Chọn danh mục --</option>
                <option v-for="cat in categories" :key="cat.maDanhMuc" :value="cat.maDanhMuc">
                  {{ cat.tenDanhMuc }}
                </option>
              </select>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">
                Thời gian làm bài (phút) <span class="text-red-500">*</span>
              </label>
              <input
                v-model.number="form.thoiGianLamBai"
                type="number"
                min="1"
                max="300"
                required
                class="input-field"
              />
            </div>
          </div>
        </div>
      </div>

      <!-- Settings -->
      <div class="card p-6">
        <h2 class="section-title mb-4">Cài đặt</h2>
        
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">
              Chế độ công khai
            </label>
            <select v-model="form.cheDoCongKhai" class="input-field">
              <option value="CongKhai">Công khai - Ai cũng có thể tìm thấy và tham gia (hoặc qua mã truy cập)</option>
              <option value="CoMatKhau">Có mật khẩu - Cần nhập mật khẩu để tham gia (kể cả qua mã truy cập)</option>
              <option value="RiengTu">Riêng tư - Chỉ người tạo bài thi mới truy cập được</option>
            </select>
          </div>

          <div v-if="form.cheDoCongKhai === 'CoMatKhau'">
            <label class="block text-sm font-medium text-gray-700 mb-1">
              Mật khẩu bài thi <span class="text-red-500">*</span>
            </label>
            <input
              v-model="form.matKhau"
              type="text"
              placeholder="Nhập mật khẩu để bảo vệ bài thi"
              class="input-field"
              required
            />
          </div>

          <div class="flex items-center justify-between py-3 border-t border-gray-100">
            <div>
              <p class="font-medium text-gray-900">Cho phép xem lại đáp án</p>
              <p class="text-sm text-gray-500">Người làm có thể xem đáp án đúng và bài làm của mình sau khi nộp</p>
            </div>
            <button
              type="button"
              @click="toggleXemLai"
              :class="[
                'relative inline-flex h-6 w-11 items-center rounded-full transition-colors',
                form.choPhepXemLai ? 'bg-primary-600' : 'bg-gray-200'
              ]"
            >
              <span
                :class="[
                  'inline-block h-4 w-4 transform rounded-full bg-white transition-transform',
                  form.choPhepXemLai ? 'translate-x-6' : 'translate-x-1'
                ]"
              />
            </button>
          </div>

        </div>
      </div>

      <!-- Questions Selection -->
      <div class="card p-6">
        <div class="flex items-center justify-between mb-4">
          <h2 class="section-title">Câu hỏi</h2>
          <div class="flex gap-2">
            <button type="button" @click="openNewQuestionModal" class="btn-secondary">
              <PlusIcon class="w-5 h-5 mr-1" />
              Tạo mới
            </button>
            <button type="button" @click="showQuestionModal = true" class="btn-secondary">
              <FolderIcon class="w-5 h-5 mr-1" />
              Từ ngân hàng
            </button>
            <button type="button" @click="showAIImportModal = true" class="btn-secondary">
              <SparklesIcon class="w-5 h-5 mr-1" />
              Import AI
            </button>
          </div>
        </div>

        <div v-if="form.cacCauHoi.length === 0" class="text-center py-8 border-2 border-dashed border-gray-200 rounded-lg">
          <QuestionMarkCircleIcon class="w-12 h-12 text-gray-300 mx-auto mb-2" />
          <p class="text-gray-500">Chưa có câu hỏi nào</p>
          <div class="flex justify-center gap-3 mt-4">
            <button type="button" @click="openNewQuestionModal" class="btn-primary">
              Tạo câu hỏi mới
            </button>
            <button type="button" @click="showQuestionModal = true" class="btn-secondary">
              Chọn từ ngân hàng
            </button>
          </div>
        </div>

        <!-- Optimized Question List with pagination -->
        <div v-else>
          <!-- Summary bar -->
          <div class="flex items-center justify-between mb-3 p-3 bg-primary-50 rounded-lg">
            <div class="flex items-center gap-4">
              <span class="text-primary-700 font-medium">{{ form.cacCauHoi.length }} câu hỏi</span>
              <span class="text-primary-600">Điểm mỗi câu: {{ (10 / form.cacCauHoi.length).toFixed(2) }}</span>
            </div>
            <div class="flex items-center gap-2">
              <button 
                type="button" 
                @click="shuffleQuestions"
                class="text-sm text-primary-600 hover:text-primary-700"
                title="Trộn ngẫu nhiên thứ tự câu hỏi"
              >
                Trộn
              </button>
              <button 
                type="button" 
                @click="showAllQuestions = !showAllQuestions"
                class="text-sm text-primary-600 hover:text-primary-700"
              >
                {{ showAllQuestions ? 'Thu gọn' : 'Xem tất cả' }}
              </button>
              <button 
                type="button" 
                @click="form.cacCauHoi = []"
                class="text-sm text-red-500 hover:text-red-600"
              >
                Xóa tất cả
              </button>
            </div>
          </div>

          <!-- Collapsed view (default) -->
          <div v-if="!showAllQuestions" class="space-y-2">
            <div 
              v-for="(question, index) in displayedQuestions" 
              :key="index"
              class="flex items-center p-3 bg-gray-50 rounded-lg group hover:bg-gray-100 cursor-pointer"
              @click="openQuestionDetail(question, index)"
            >
              <span class="w-7 h-7 bg-primary-100 text-primary-600 rounded-full flex items-center justify-center font-medium text-xs mr-3 flex-shrink-0">
                {{ index + 1 }}
              </span>
              <p class="flex-1 text-gray-900 truncate text-sm">{{ question.noiDungCauHoi }}</p>
              <button 
                type="button"
                @click.stop="removeQuestion(index)"
                class="p-1 text-gray-400 hover:text-red-500"
              >
                <XMarkIcon class="w-4 h-4" />
              </button>
            </div>
            
            <div v-if="form.cacCauHoi.length > 5" class="text-center py-2">
              <button 
                type="button" 
                @click="showAllQuestions = true"
                class="text-sm text-primary-600 hover:text-primary-700"
              >
                + {{ form.cacCauHoi.length - 5 }} câu hỏi khác
              </button>
            </div>
          </div>

          <!-- Expanded view with drag & drop -->
          <draggable
            v-else
            v-model="form.cacCauHoi"
            item-key="maCauHoi"
            handle=".drag-handle"
            class="space-y-2 max-h-96 overflow-y-auto"
          >
            <template #item="{ element, index }">
              <div 
                class="flex items-center p-3 bg-gray-50 rounded-lg group hover:bg-gray-100 cursor-pointer"
                @click="openQuestionDetail(element, index)"
              >
                <button type="button" class="drag-handle cursor-move p-1 mr-2 text-gray-400 hover:text-gray-600" @click.stop>
                  <Bars3Icon class="w-4 h-4" />
                </button>
                <span class="w-7 h-7 bg-primary-100 text-primary-600 rounded-full flex items-center justify-center font-medium text-xs mr-3 flex-shrink-0">
                  {{ index + 1 }}
                </span>
                <div class="flex-1 min-w-0">
                  <p class="text-gray-900 truncate text-sm">{{ element.noiDungCauHoi }}</p>
                  <p class="text-xs text-gray-500">{{ element.cacLuaChon?.length || 0 }} đáp án</p>
                </div>
                <button 
                  type="button"
                  @click.stop="removeQuestion(index)"
                  class="p-1 text-gray-400 hover:text-red-500"
                >
                  <XMarkIcon class="w-4 h-4" />
                </button>
              </div>
            </template>
          </draggable>
        </div>
      </div>

      <!-- Submit -->
      <div class="flex justify-end gap-4">
        <router-link to="/my-quizzes" class="btn-secondary">
          Hủy
        </router-link>
        <button
          type="submit"
          :disabled="submitting || form.cacCauHoi.length === 0"
          class="btn-primary"
        >
          <ArrowPathIcon v-if="submitting" class="w-5 h-5 mr-2 animate-spin" />
          {{ submitting ? 'Đang tạo...' : 'Tạo bài thi' }}
        </button>
      </div>
    </form>

    <!-- Question Selection Modal -->
    <TransitionRoot appear :show="showQuestionModal" as="template">
      <Dialog as="div" @close="showQuestionModal = false" class="relative z-50">
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
              <DialogPanel class="w-full max-w-3xl transform rounded-2xl bg-white shadow-xl transition-all">
                <div class="p-6 border-b border-gray-100">
                  <DialogTitle as="h3" class="text-lg font-semibold text-gray-900">
                    Chọn câu hỏi từ ngân hàng
                  </DialogTitle>
                  <div class="mt-4 flex gap-3">
                    <div class="flex-1 relative">
                      <MagnifyingGlassIcon class="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
                      <input
                        v-model="questionSearch"
                        type="text"
                        placeholder="Tìm câu hỏi..."
                        class="input-field pl-10"
                        @input="resetAndLoadQuestions"
                      />
                    </div>
                    <select v-model="questionFilter.maDanhMuc" class="input-field w-36" @change="resetAndLoadQuestions">
                      <option value="">Tất cả danh mục</option>
                      <option v-for="cat in categories" :key="cat.maDanhMuc" :value="cat.maDanhMuc">
                        {{ cat.tenDanhMuc }}
                      </option>
                    </select>
                    <select v-model="questionFilter.trangThaiChon" class="input-field w-36" @change="resetAndLoadQuestions">
                      <option value="">Tất cả</option>
                      <option value="chuaChon">Chưa chọn</option>
                      <option value="daChon">Đã chọn</option>
                    </select>
                  </div>
                </div>

                <div class="max-h-96 overflow-y-auto p-6">
                  <div v-if="loadingQuestions" class="text-center py-8">
                    <ArrowPathIcon class="w-8 h-8 text-primary-600 animate-spin mx-auto" />
                  </div>
                  
                  <div v-else-if="filteredModalQuestions.length === 0" class="text-center py-8">
                    <QuestionMarkCircleIcon class="w-12 h-12 text-gray-300 mx-auto mb-2" />
                    <p class="text-gray-500">Không tìm thấy câu hỏi</p>
                    <router-link to="/questions/create" class="text-primary-600 text-sm mt-2 inline-block">
                      Tạo câu hỏi mới →
                    </router-link>
                  </div>

                  <div v-else class="space-y-2">
                    <label
                      v-for="question in filteredModalQuestions"
                      :key="question.maCauHoi"
                      :class="[
                        'flex items-start p-3 rounded-lg border cursor-pointer transition-colors',
                        isQuestionInQuiz(question.maCauHoi) 
                          ? 'border-primary-300 bg-primary-50' 
                          : 'border-gray-200 hover:border-primary-300'
                      ]"
                    >
                      <input
                        type="checkbox"
                        :checked="isQuestionSelected(question.maCauHoi)"
                        @change="toggleQuestionSelection(question)"
                        class="mt-1 w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
                      />
                      <div class="ml-3 flex-1">
                        <div class="flex items-center gap-2">
                          <p class="text-gray-900">{{ question.noiDungCauHoi }}</p>
                          <span v-if="isQuestionInQuiz(question.maCauHoi)" class="text-xs bg-primary-100 text-primary-700 px-2 py-0.5 rounded">
                            Đã thêm
                          </span>
                        </div>
                        <p class="text-sm text-gray-500 mt-1">
                          {{ question.tenDanhMuc || 'Không có danh mục' }} •
                          {{ question.cacLuaChon?.length || 0 }} đáp án •
                          {{ question.mucDo || 'Trung bình' }}
                        </p>
                      </div>
                    </label>
                  </div>
                </div>

                <div class="p-6 border-t border-gray-100">
                  <!-- Pagination -->
                  <div v-if="questionPagination.totalPages > 1" class="flex items-center justify-center gap-2 mb-4">
                    <button
                      @click="goToQuestionPage(questionPagination.currentPage - 1)"
                      :disabled="questionPagination.currentPage === 1"
                      :class="[
                        'px-2 py-1 rounded border text-sm',
                        questionPagination.currentPage === 1 
                          ? 'bg-gray-100 text-gray-400 border-gray-200 cursor-not-allowed' 
                          : 'bg-white text-gray-700 border-gray-300 hover:bg-gray-50'
                      ]"
                    >
                      <ChevronLeftIcon class="w-4 h-4" />
                    </button>
                    <span class="text-sm text-gray-600">
                      Trang {{ questionPagination.currentPage }} / {{ questionPagination.totalPages }}
                    </span>
                    <button
                      @click="goToQuestionPage(questionPagination.currentPage + 1)"
                      :disabled="questionPagination.currentPage === questionPagination.totalPages"
                      :class="[
                        'px-2 py-1 rounded border text-sm',
                        questionPagination.currentPage === questionPagination.totalPages 
                          ? 'bg-gray-100 text-gray-400 border-gray-200 cursor-not-allowed' 
                          : 'bg-white text-gray-700 border-gray-300 hover:bg-gray-50'
                      ]"
                    >
                      <ChevronRightIcon class="w-4 h-4" />
                    </button>
                    <div class="flex items-center gap-1 ml-2">
                      <input 
                        type="number" 
                        v-model.number="questionPageInput"
                        @keyup.enter="goToQuestionPageInput"
                        min="1"
                        :max="questionPagination.totalPages"
                        placeholder="Trang"
                        class="w-14 px-2 py-1 text-sm border border-gray-300 rounded focus:ring-primary-500 focus:border-primary-500"
                      />
                      <button @click="goToQuestionPageInput" class="px-2 py-1 text-sm bg-gray-100 hover:bg-gray-200 rounded border border-gray-300">&#272;i</button>
                    </div>
                  </div>
                  
                  <!-- Footer actions -->
                  <div class="flex justify-between">
                    <span class="text-sm text-gray-500">
                      Đã chọn: {{ selectedQuestions.length }} câu hỏi mới
                    </span>
                    <div class="flex gap-3">
                      <button type="button" @click="showQuestionModal = false" class="btn-secondary">
                        Hủy
                      </button>
                      <button type="button" @click="addSelectedQuestions" class="btn-primary" :disabled="selectedQuestions.length === 0">
                        Thêm câu hỏi
                      </button>
                    </div>
                  </div>
                </div>
              </DialogPanel>
            </TransitionChild>
          </div>
        </div>
      </Dialog>
    </TransitionRoot>

    <!-- New Question Modal - Batch Creation -->
    <TransitionRoot appear :show="showNewQuestionModal" as="template">
      <Dialog as="div" @close="closeNewQuestionModal" class="relative z-50" :initialFocus="batchModalFocusRef">
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
              <DialogPanel class="w-[1024px] max-w-[95vw] transform rounded-2xl bg-white shadow-xl transition-all">
                <!-- Header -->
                <div class="p-5 border-b border-gray-100">
                  <DialogTitle as="h3" class="text-lg font-semibold text-gray-900" ref="batchModalFocusRef">
                    Tạo nhiều câu hỏi
                  </DialogTitle>
                  <p class="text-sm text-gray-500 mt-1">Tạo nhanh nhiều câu hỏi cùng lúc và thêm vào bài thi</p>
                  
                  <!-- Settings Row - All in one line -->
                  <div class="mt-4 flex items-center gap-4">
                    <div class="flex items-center gap-2">
                      <label class="text-sm font-medium text-gray-700 whitespace-nowrap">Số lượng câu hỏi <span class="text-xs text-gray-500 font-normal">(tối đa 50)</span></label>
                      <div class="flex items-center gap-1">
                        <button 
                          type="button" 
                          @click="decreaseBatchQuestionCount"
                          :disabled="batchQuestionCount <= 1"
                          class="w-8 h-8 rounded-lg border border-gray-300 flex items-center justify-center hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                          <MinusIcon class="w-4 h-4" />
                        </button>
                        <input
                          v-model.number="batchQuestionCount"
                          type="number"
                          min="1"
                          max="50"
                          class="w-14 h-8 text-center border border-gray-300 rounded-lg text-sm focus:ring-2 focus:ring-primary-500 focus:border-primary-500 [appearance:textfield] [&::-webkit-outer-spin-button]:appearance-none [&::-webkit-inner-spin-button]:appearance-none"
                          @change="onBatchCountChange"
                        />
                        <button 
                          type="button" 
                          @click="increaseBatchQuestionCount"
                          :disabled="batchQuestionCount >= 50"
                          class="w-8 h-8 rounded-lg border border-gray-300 flex items-center justify-center hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                          <PlusIcon class="w-4 h-4" />
                        </button>
                      </div>
                    </div>
                    
                    <div class="flex items-center gap-2 flex-1">
                      <label class="text-sm font-medium text-gray-700 whitespace-nowrap">Danh mục chung</label>
                      <select v-model="batchCommonCategory" class="input-field-sm flex-1" @change="applyCommonCategory">
                        <option value="">Không có danh mục</option>
                        <option v-for="cat in categories" :key="cat.maDanhMuc" :value="cat.maDanhMuc">
                          {{ cat.tenDanhMuc }}
                        </option>
                      </select>
                    </div>

                    <!-- Progress indicator -->
                    <div class="flex-shrink-0 text-right">
                      <p class="text-sm text-gray-500">Hoàn thành</p>
                      <p class="text-lg font-semibold text-primary-600">
                        {{ completedBatchQuestions }}/{{ batchQuestionCount }}
                      </p>
                    </div>
                  </div>

                  <!-- Legend - moved here -->
                  <div class="mt-3 flex items-center gap-4 text-xs">
                    <div class="flex items-center gap-1.5">
                      <span class="w-4 h-4 rounded bg-green-100 border border-green-300"></span>
                      <span class="text-gray-600">Hoàn thành</span>
                    </div>
                    <div class="flex items-center gap-1.5">
                      <span class="w-4 h-4 rounded bg-yellow-100 border border-yellow-300"></span>
                      <span class="text-gray-600">Chưa đủ</span>
                    </div>
                    <div class="flex items-center gap-1.5">
                      <span class="w-4 h-4 rounded bg-white border border-gray-300"></span>
                      <span class="text-gray-600">Trống</span>
                    </div>
                  </div>
                </div>

                <!-- Main Content - 2 Columns -->
                <div class="flex" style="min-height: 500px; max-height: 70vh;">
                  <!-- Left Column - Question Navigator -->
                  <div class="w-48 flex-shrink-0 border-r border-gray-100 p-4 overflow-y-auto bg-gray-50">
                    <p class="text-xs font-medium text-gray-500 uppercase mb-3">Câu hỏi</p>
                    <div class="grid grid-cols-4 gap-2">
                      <button
                        v-for="index in batchQuestionCount"
                        :key="index"
                        type="button"
                        @click="selectBatchQuestion(index - 1)"
                        :class="[
                          'w-9 h-9 rounded-lg text-sm font-medium transition-all flex items-center justify-center relative',
                          currentBatchIndex === index - 1
                            ? 'bg-primary-600 text-white shadow-md ring-2 ring-primary-300'
                            : isBatchQuestionValid(index - 1)
                              ? 'bg-green-100 text-green-700 hover:bg-green-200'
                              : batchQuestions[index - 1]?.noiDungCauHoi
                                ? 'bg-yellow-100 text-yellow-700 hover:bg-yellow-200'
                                : 'bg-white text-gray-600 hover:bg-gray-100 border border-gray-200'
                        ]"
                      >
                        {{ index }}
                        <CheckIcon 
                          v-if="isBatchQuestionValid(index - 1) && currentBatchIndex !== index - 1" 
                          class="w-3 h-3 absolute -top-1 -right-1 text-green-600" 
                        />
                      </button>
                    </div>
                  </div>

                  <!-- Right Column - Question Editor -->
                  <div class="flex-1 min-w-0 p-5 overflow-y-auto">
                    <div v-if="currentBatchQuestion" class="space-y-4">
                      <!-- Question Header -->
                      <div class="flex items-center justify-between">
                        <h4 class="font-medium text-gray-900">
                          Câu {{ currentBatchIndex + 1 }}
                          <span v-if="isBatchQuestionValid(currentBatchIndex)" class="ml-2 text-green-600 text-sm">
                            ✓ Hoàn thành
                          </span>
                        </h4>
                        <div class="flex items-center gap-2">
                          <button
                            type="button"
                            @click="prevBatchQuestion"
                            :disabled="currentBatchIndex === 0"
                            class="p-1.5 rounded hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed"
                          >
                            <ChevronLeftIcon class="w-5 h-5" />
                          </button>
                          <button
                            type="button"
                            @click="nextBatchQuestion"
                            :disabled="currentBatchIndex === batchQuestionCount - 1"
                            class="p-1.5 rounded hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed"
                          >
                            <ChevronRightIcon class="w-5 h-5" />
                          </button>
                        </div>
                      </div>

                      <!-- Mức độ - Moved to top -->
                      <div>
                        <div>
                          <label class="block text-sm font-medium text-gray-700 mb-1">Mức độ</label>
                          <select v-model="currentBatchQuestion.mucDo" class="input-field-sm w-40">
                            <option value="De">Dễ</option>
                            <option value="TrungBinh">Trung bình</option>
                            <option value="Kho">Khó</option>
                          </select>
                        </div>
                      </div>

                      <!-- Nội dung câu hỏi -->
                      <div>
                        <label class="block text-sm font-medium text-gray-700 mb-1">
                          Nội dung câu hỏi <span class="text-red-500">*</span>
                        </label>
                        <textarea
                          v-model="currentBatchQuestion.noiDungCauHoi"
                          rows="1"
                          placeholder="Nhập nội dung câu hỏi"
                          class="textarea-auto"
                          @keydown.ctrl.enter="nextBatchQuestion"
                          @input="autoResizeTextarea"
                          ref="batchQuestionTextarea"
                        ></textarea>
                      </div>

                      <!-- Các lựa chọn -->
                      <div>
                        <label class="block text-sm font-medium text-gray-700 mb-2">
                          4 đáp án <span class="text-red-500">*</span>
                          <span class="text-xs text-gray-500 font-normal ml-2">(Bắt buộc điền đủ 4 đáp án)</span>
                        </label>
                        <div class="space-y-2">
                          <div
                            v-for="(option, index) in currentBatchQuestion.cacLuaChon"
                            :key="index"
                            :class="[
                              'flex items-center gap-2 p-2 rounded-lg transition-colors',
                              option.laLuaChonDung ? 'bg-green-50 ring-1 ring-green-200' : 'bg-gray-50'
                            ]"
                          >
                            <span class="w-6 h-6 rounded-full bg-gray-200 flex items-center justify-center text-xs font-medium text-gray-600 shrink-0">
                              {{ String.fromCharCode(65 + index) }}
                            </span>
                            <button
                              type="button"
                              @click="selectBatchCorrectAnswer(index)"
                              :class="[
                                'w-6 h-6 rounded-full border-2 flex items-center justify-center flex-shrink-0 transition-colors',
                                option.laLuaChonDung
                                  ? 'bg-green-500 border-green-500 text-white'
                                  : 'border-gray-300 hover:border-green-400'
                              ]"
                              title="Đánh dấu là đáp án đúng"
                            >
                              <CheckIcon v-if="option.laLuaChonDung" class="w-4 h-4" />
                            </button>
                            <textarea
                              v-model="option.noiDung"
                              rows="1"
                              required
                              :placeholder="'Đáp án ' + String.fromCharCode(65 + index) + ' (bắt buộc)'"
                              :class="[
                                'textarea-auto flex-1',
                                !option.noiDung.trim() && currentBatchQuestion.noiDungCauHoi.trim() ? 'border-red-300 focus:border-red-500 focus:ring-red-500' : ''
                              ]"
                              @input="autoResizeTextarea"
                              @keydown.enter.prevent="focusNextOption(index)"
                            ></textarea>
                          </div>
                        </div>
                        <p v-if="getBatchValidationError(currentBatchIndex)" class="text-xs text-red-500 mt-2">
                          {{ getBatchValidationError(currentBatchIndex) }}
                        </p>
                        <p v-else class="text-xs text-gray-500 mt-2">
                          Click vào vòng tròn để đánh dấu đáp án đúng • Ctrl+Enter để chuyển câu tiếp
                        </p>
                      </div>

                      <!-- Giải thích -->
                      <div>
                        <label class="block text-sm font-medium text-gray-700 mb-1">Giải thích (tùy chọn)</label>
                        <textarea
                          v-model="currentBatchQuestion.giaiThich"
                          rows="1"
                          placeholder="Giải thích đáp án"
                          class="textarea-auto"
                          @input="autoResizeTextarea"
                        ></textarea>
                      </div>
                    </div>
                  </div>
                </div>

                <!-- Footer -->
                <div class="p-4 border-t border-gray-100 flex justify-end items-center bg-gray-50 rounded-b-2xl">
                  <div class="flex gap-3">
                    <button type="button" @click="closeNewQuestionModal" class="btn-secondary">
                      Hủy
                    </button>
                    <button
                      type="button"
                      @click="createBatchQuestions"
                      :disabled="creatingQuestion || completedBatchQuestions < batchQuestionCount"
                      class="btn-primary"
                    >
                      <ArrowPathIcon v-if="creatingQuestion" class="w-5 h-5 mr-2 animate-spin" />
                      {{ creatingQuestion ? 'Đang tạo...' : `Tạo ${completedBatchQuestions}/${batchQuestionCount} câu hỏi` }}
                    </button>
                  </div>
                </div>
              </DialogPanel>
            </TransitionChild>
          </div>
        </div>
      </Dialog>
    </TransitionRoot>

    <!-- AI Import Modal -->
    <TransitionRoot appear :show="showAIImportModal" as="template">
      <Dialog as="div" @close="showAIImportModal = false" class="relative z-50">
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
              <DialogPanel class="w-full max-w-4xl transform rounded-2xl bg-white shadow-xl transition-all">
                <div class="p-6 border-b border-gray-100">
                  <DialogTitle as="h3" class="text-xl font-semibold text-gray-900 flex items-center">
                    <SparklesIcon class="w-7 h-7 text-primary-600 mr-2" />
                    Tạo câu hỏi bằng AI
                  </DialogTitle>
                  <p class="text-sm text-gray-500 mt-1">Trích xuất từ file hoặc tạo mới từ chủ đề</p>
                </div>

                <!-- Tab Navigation (only show in upload/generate step) -->
                <div v-if="aiImportStep === 'upload' || aiImportStep === 'generate'" class="border-b border-gray-200">
                  <nav class="flex -mb-px">
                    <button
                      type="button"
                      @click="aiImportStep = 'upload'"
                      :class="[
                        'flex-1 py-3 px-4 text-sm font-medium text-center border-b-2 transition-colors',
                        aiImportStep === 'upload'
                          ? 'border-primary-600 text-primary-600'
                          : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                      ]"
                    >
                      <DocumentArrowUpIcon class="w-5 h-5 inline mr-2" />
                      Trích xuất từ file
                    </button>
                    <button
                      type="button"
                      @click="aiImportStep = 'generate'"
                      :class="[
                        'flex-1 py-3 px-4 text-sm font-medium text-center border-b-2 transition-colors',
                        aiImportStep === 'generate'
                          ? 'border-primary-600 text-primary-600'
                          : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                      ]"
                    >
                      <SparklesIcon class="w-5 h-5 inline mr-2" />
                      Tạo từ chủ đề
                    </button>
                  </nav>
                </div>

                <!-- Tab 1: Upload File -->
                <div v-if="aiImportStep === 'upload'" class="p-8">
                  <!-- Category for extracted questions - REQUIRED -->
                  <div class="mb-6">
                    <label class="block text-sm font-medium text-gray-700 mb-2">
                      Danh mục <span class="text-red-500">*</span>
                    </label>
                    <select v-model="uploadCategory" class="input-field max-w-xs">
                      <option value="" disabled>-- Chọn danh mục --</option>
                      <option v-for="cat in categories" :key="cat.maDanhMuc" :value="cat.maDanhMuc">
                        {{ cat.tenDanhMuc }}
                      </option>
                    </select>
                    <p class="text-xs text-gray-500 mt-1">Vui lòng chọn danh mục để phân loại câu hỏi</p>
                  </div>
                  
                  <div
                    class="border-2 border-dashed border-gray-300 rounded-xl p-12 text-center hover:border-primary-400 transition-colors bg-gray-50"
                    @dragover.prevent
                    @drop.prevent="handleFileDrop"
                  >
                    <DocumentArrowUpIcon class="w-16 h-16 text-gray-400 mx-auto mb-4" />
                    <p class="text-lg text-gray-600 mb-3">Kéo thả file vào đây hoặc</p>
                    <label class="btn-primary cursor-pointer text-base px-6 py-3">
                      Chọn file
                      <input
                        type="file"
                        accept=".txt,.docx,.pdf"
                        class="hidden"
                        @change="handleFileSelect"
                      />
                    </label>
                    <p class="text-sm text-gray-500 mt-6">Hỗ trợ: .txt, .docx, .pdf (tối đa 5MB)</p>
                  </div>
                </div>

                <!-- Tab 2: Generate from Topic -->
                <div v-else-if="aiImportStep === 'generate'" class="p-8">
                  <div class="space-y-6">
                    <div>
                      <label class="block text-sm font-medium text-gray-700 mb-2">
                        Chủ đề / Nội dung <span class="text-red-500">*</span>
                      </label>
                      <textarea
                        v-model="generateForm.topic"
                        rows="5"
                        placeholder="Nhập chủ đề, đoạn văn bản, hoặc nội dung bạn muốn tạo câu hỏi...

Ví dụ:
- Lịch sử Việt Nam thời kỳ phong kiến
- Đoạn văn về định luật Newton
- Các công thức hóa học cơ bản..."
                        class="input-field text-base"
                      ></textarea>
                    </div>
                    
                    <div class="grid grid-cols-2 gap-4">
                      <div>
                        <label class="block text-sm font-medium text-gray-700 mb-2">
                          Số lượng câu hỏi
                        </label>
                        <select v-model.number="generateForm.numberOfQuestions" class="input-field">
                          <option :value="5">5 câu</option>
                          <option :value="10">10 câu</option>
                          <option :value="15">15 câu</option>
                          <option :value="20">20 câu</option>
                        </select>
                      </div>
                      <div>
                        <label class="block text-sm font-medium text-gray-700 mb-2">
                          Danh mục <span class="text-red-500">*</span>
                        </label>
                        <select v-model="generateForm.maDanhMuc" class="input-field">
                          <option value="" disabled>-- Chọn danh mục --</option>
                          <option v-for="cat in categories" :key="cat.maDanhMuc" :value="cat.maDanhMuc">
                            {{ cat.tenDanhMuc }}
                          </option>
                        </select>
                      </div>
                    </div>
                    
                    <p class="text-xs text-gray-500">
                      Vui lòng chọn danh mục để phân loại câu hỏi. AI sẽ tự động xác định độ khó cho từng câu.
                    </p>
                    
                    <button
                      type="button"
                      @click="generateFromTopic"
                      :disabled="!generateForm.topic || generateForm.numberOfQuestions < 1 || !generateForm.maDanhMuc"
                      class="btn-primary w-full py-3 text-base"
                    >
                      <SparklesIcon class="w-5 h-5 mr-2" />
                      Tạo câu hỏi bằng AI
                    </button>
                  </div>
                </div>

                <!-- Step 2: Processing -->
                <div v-else-if="aiImportStep === 'processing'" class="p-12 text-center">
                  <ArrowPathIcon class="w-16 h-16 text-primary-600 animate-spin mx-auto mb-4" />
                  <p class="text-lg font-medium text-gray-900">Đang xử lý...</p>
                  <p class="text-gray-500 mt-2">AI đang tạo câu hỏi, vui lòng đợi...</p>
                </div>

                <!-- Step 3: Select questions -->
                <div v-else-if="aiImportStep === 'select'" class="p-6">
                  <div class="flex items-center justify-between mb-4">
                    <p class="text-sm text-gray-600">
                      Đã tạo <span class="font-semibold text-primary-600">{{ aiExtractedQuestions.length }}</span> câu hỏi
                    </p>
                    <button
                      type="button"
                      @click="toggleSelectAll"
                      class="text-sm text-primary-600 hover:text-primary-700"
                    >
                      {{ aiSelectedQuestions.length === aiExtractedQuestions.length ? 'Bỏ chọn tất cả' : 'Chọn tất cả' }}
                    </button>
                  </div>
                  
                  <div class="max-h-96 overflow-y-auto space-y-3">
                    <label
                      v-for="(q, index) in aiExtractedQuestions"
                      :key="index"
                      class="flex items-start p-4 rounded-lg border border-gray-200 hover:border-primary-300 cursor-pointer"
                    >
                      <input
                        type="checkbox"
                        :value="q"
                        v-model="aiSelectedQuestions"
                        class="mt-1 w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
                      />
                      <div class="ml-3 flex-1">
                        <p class="text-gray-900 font-medium">{{ q.noiDungCauHoi }}</p>
                        <div class="mt-2 space-y-1">
                          <div
                            v-for="(opt, i) in q.cacLuaChon"
                            :key="i"
                            :class="[
                              'text-sm px-2 py-1 rounded',
                              opt.laLuaChonDung ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-600'
                            ]"
                          >
                            {{ opt.laLuaChonDung ? '✓' : '' }} {{ opt.noiDung }}
                          </div>
                        </div>
                      </div>
                    </label>
                  </div>
                </div>

                <div class="p-6 border-t border-gray-100 flex justify-between">
                  <button
                    type="button"
                    @click="closeAIImport"
                    class="btn-secondary"
                  >
                    {{ aiImportStep === 'select' ? 'Hủy' : 'Đóng' }}
                  </button>
                  <button
                    v-if="aiImportStep === 'select'"
                    type="button"
                    @click="addAIQuestions"
                    :disabled="aiSelectedQuestions.length === 0"
                    class="btn-primary"
                  >
                    Thêm {{ aiSelectedQuestions.length }} câu hỏi
                  </button>
                </div>
              </DialogPanel>
            </TransitionChild>
          </div>
        </div>
      </Dialog>
    </TransitionRoot>

    <!-- Question Detail/Edit Modal -->
    <TransitionRoot appear :show="showQuestionDetailModal" as="template">
      <Dialog as="div" @close="showQuestionDetailModal = false" class="relative z-50">
        <TransitionChild
          enter="ease-out duration-300"
          enter-from="opacity-0"
          enter-to="opacity-100"
          leave="ease-in duration-200"
          leave-from="opacity-100"
          leave-to="opacity-0"
        >
          <div class="fixed inset-0 bg-black/30" />
        </TransitionChild>

        <div class="fixed inset-0 overflow-y-auto">
          <div class="flex min-h-full items-center justify-center p-4">
            <TransitionChild
              enter="ease-out duration-300"
              enter-from="opacity-0 scale-95"
              enter-to="opacity-100 scale-100"
              leave="ease-in duration-200"
              leave-from="opacity-100 scale-100"
              leave-to="opacity-0 scale-95"
            >
              <DialogPanel class="w-[640px] max-w-[95vw] transform bg-white rounded-xl shadow-xl transition-all">
                <!-- Header - Thống nhất giữa view và edit -->
                <div class="p-5 border-b border-gray-100 flex items-center justify-between">
                  <DialogTitle class="text-lg font-semibold text-gray-900 flex items-center gap-2">
                    <QuestionMarkCircleIcon class="w-5 h-5 text-primary-600" />
                    {{ isEditingQuestion ? 'Chỉnh sửa câu hỏi' : 'Chi tiết câu hỏi' }}
                    <span class="text-sm font-normal text-gray-500">(Câu {{ editingQuestionIndex + 1 }})</span>
                  </DialogTitle>
                  <button
                    type="button"
                    @click="toggleEditMode"
                    :class="[
                      'px-3 py-1.5 rounded-lg text-sm font-medium transition-colors flex items-center gap-1.5',
                      isEditingQuestion 
                        ? 'bg-gray-100 text-gray-700 hover:bg-gray-200' 
                        : 'bg-primary-50 text-primary-700 hover:bg-primary-100'
                    ]"
                  >
                    <template v-if="isEditingQuestion">
                      <XMarkIcon class="w-4 h-4" />
                      Hủy chỉnh sửa
                    </template>
                    <template v-else>
                      <PencilIcon class="w-4 h-4" />
                      Chỉnh sửa
                    </template>
                  </button>
                </div>

                <!-- Body - Unified form -->
                <div class="p-6 overflow-y-auto" style="min-height: 340px; max-height: 65vh;" v-if="editingQuestion">
                  <div class="space-y-4">
                    <!-- Mức độ - Moved to top (chỉ hiển thị khi đang sửa) -->
                    <div v-if="isEditingQuestion">
                      <div>
                        <label class="block text-sm font-medium text-gray-700 mb-1">Mức độ</label>
                        <select v-model="editingQuestionData.mucDo" class="input-field-sm w-40">
                          <option value="De">Dễ</option>
                          <option value="TrungBinh">Trung bình</option>
                          <option value="Kho">Khó</option>
                        </select>
                      </div>
                    </div>

                    <!-- Nội dung câu hỏi -->
                    <div>
                      <label class="block text-sm font-medium text-gray-700 mb-1">
                        Nội dung câu hỏi <span v-if="isEditingQuestion" class="text-red-500">*</span>
                      </label>
                      <textarea
                        v-if="isEditingQuestion"
                        v-model="editingQuestionData.noiDungCauHoi"
                        rows="1"
                        class="textarea-auto"
                        placeholder="Nhập nội dung câu hỏi"
                        @input="autoResizeTextarea"
                        ref="editQuestionContentRef"
                      ></textarea>
                      <div 
                        v-else 
                        class="input-field-sm bg-gray-50 py-2 whitespace-pre-wrap break-words"
                      >{{ editingQuestion.noiDungCauHoi }}</div>
                    </div>

                    <!-- Các lựa chọn -->
                    <div>
                      <label class="block text-sm font-medium text-gray-700 mb-2">
                        Các lựa chọn (4 đáp án) <span v-if="isEditingQuestion" class="text-red-500">*</span>
                      </label>
                      
                      <!-- Options list -->
                      <div class="space-y-2">
                        <div
                          v-for="(option, index) in (isEditingQuestion ? editingQuestionData.cacLuaChon : editingQuestion.cacLuaChon)"
                          :key="index"
                          :class="[
                            'flex gap-2 p-2 rounded-lg',
                            getOptionIsCorrect(option) ? 'bg-green-50' : 'bg-gray-50'
                          ]"
                        >
                          <!-- Left side: Letter + Tick (fixed width, centered vertically) -->
                          <div class="flex items-center gap-2 self-center shrink-0">
                            <!-- Letter marker A, B, C, D -->
                            <span class="w-6 h-6 rounded-full bg-gray-200 flex items-center justify-center text-xs font-medium text-gray-600">
                              {{ String.fromCharCode(65 + index) }}
                            </span>
                            
                            <!-- Correct answer toggle button (edit mode) / indicator (view mode) -->
                            <button
                              v-if="isEditingQuestion"
                              type="button"
                              @click="toggleEditAnswer(index)"
                              :class="[
                                'w-6 h-6 rounded-full border-2 flex items-center justify-center transition-colors',
                                option.laDapAnDung
                                  ? 'bg-green-500 border-green-500 text-white'
                                  : 'border-gray-300 hover:border-green-400'
                              ]"
                              title="Đánh dấu là đáp án đúng"
                            >
                              <CheckIcon v-if="option.laDapAnDung" class="w-4 h-4" />
                            </button>
                            <span 
                              v-else
                              :class="[
                                'w-6 h-6 rounded-full flex items-center justify-center',
                                getOptionIsCorrect(option) ? 'bg-green-500 text-white' : 'border-2 border-gray-300'
                              ]"
                            >
                              <CheckIcon v-if="getOptionIsCorrect(option)" class="w-4 h-4" />
                            </span>
                          </div>
                          
                          <!-- Option content -->
                          <textarea
                            v-if="isEditingQuestion"
                            v-model="option.noiDungDapAn"
                            rows="1"
                            :placeholder="'Đáp án ' + String.fromCharCode(65 + index)"
                            class="textarea-auto flex-1 min-w-0"
                            @input="autoResizeTextarea"
                            :ref="el => { if (el) editOptionRefs[index] = el }"
                          ></textarea>
                          <div
                            v-else
                            class="input-field-sm flex-1 min-w-0 bg-transparent py-2 whitespace-pre-wrap break-all"
                          >{{ getOptionContent(option) }}</div>
                        </div>
                      </div>
                      
                      <p v-if="isEditingQuestion" class="text-xs text-gray-500 mt-2">
                        Click vào vòng tròn để đánh dấu đáp án đúng
                      </p>
                    </div>

                    <!-- Giải thích -->
                    <div v-if="isEditingQuestion || editingQuestion.giaiThich">
                      <label class="block text-sm font-medium text-gray-700 mb-1">
                        Giải thích {{ isEditingQuestion ? '(tùy chọn)' : '' }}
                      </label>
                      <textarea
                        v-if="isEditingQuestion"
                        v-model="editingQuestionData.giaiThich"
                        rows="1"
                        placeholder="Giải thích đáp án"
                        class="textarea-auto"
                        @input="autoResizeTextarea"
                        ref="editQuestionExplanationRef"
                      ></textarea>
                      <div 
                        v-else 
                        class="input-field-sm bg-gray-50 min-h-[38px] flex items-center"
                      >{{ editingQuestion.giaiThich }}</div>
                    </div>
                  </div>
                </div>

                <!-- Footer - Thống nhất layout -->
                <div class="p-4 border-t border-gray-100 flex justify-between items-center bg-gray-50 rounded-b-xl">
                  <button type="button" @click="showQuestionDetailModal = false" class="btn-secondary px-5">
                    Đóng
                  </button>
                  <button
                    v-if="isEditingQuestion"
                    type="button"
                    @click="saveEditedQuestion"
                    :disabled="savingQuestion"
                    class="btn-primary px-5 flex items-center gap-1.5"
                  >
                    <ArrowPathIcon v-if="savingQuestion" class="w-4 h-4 animate-spin" />
                    <CheckIcon v-else class="w-4 h-4" />
                    {{ savingQuestion ? 'Đang lưu...' : 'Lưu thay đổi' }}
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
import { ref, reactive, computed, onMounted, nextTick, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'vue-toastification'
import draggable from 'vuedraggable'
import {
  Dialog,
  DialogPanel,
  DialogTitle,
  TransitionRoot,
  TransitionChild
} from '@headlessui/vue'
import {
  PlusIcon,
  MinusIcon,
  QuestionMarkCircleIcon,
  Bars3Icon,
  XMarkIcon,
  MagnifyingGlassIcon,
  ArrowPathIcon,
  FolderIcon,
  CheckIcon,
  SparklesIcon,
  DocumentArrowUpIcon,
  PencilIcon,
  ChevronLeftIcon,
  ChevronRightIcon
} from '@heroicons/vue/24/outline'
import { quizService, questionService, categoryService, aiService } from '@/services'

const router = useRouter()
const toast = useToast()

const submitting = ref(false)
const showQuestionModal = ref(false)
const showNewQuestionModal = ref(false)
const showAIImportModal = ref(false)
const showQuestionDetailModal = ref(false)
const loadingQuestions = ref(false)
const creatingQuestion = ref(false)
const savingQuestion = ref(false)
const availableQuestions = ref([])
const selectedQuestions = ref([])
const categories = ref([])
const questionSearch = ref('')
const showAllQuestions = ref(false)

// Question detail/edit state
const editingQuestion = ref(null)
const editingQuestionIndex = ref(-1)
const isEditingQuestion = ref(false)
const editOptionRefs = ref([])
const editQuestionContentRef = ref(null)
const editQuestionExplanationRef = ref(null)
const editingQuestionData = reactive({
  noiDungCauHoi: '',
  giaiThich: '',
  loaiCauHoi: 'MotDapAn',
  mucDo: 'De',
  cacLuaChon: []
})

// AI Import state
const aiImportStep = ref('upload') // 'upload' | 'generate' | 'processing' | 'select'
const aiExtractedQuestions = ref([])
const aiSelectedQuestions = ref([])
const aiSessionId = ref(null)
const uploadCategory = ref('') // Category for file extraction

// Generate from topic form
const generateForm = reactive({
  topic: '',
  numberOfQuestions: 5,
  maDanhMuc: ''
})

const form = reactive({
  tieuDe: '',
  moTa: '',
  maDanhMuc: '',  // Danh mục bài thi
  thoiGianLamBai: 30,
  diemDat: null,
  cheDoCongKhai: 'RiengTu',
  matKhau: '',
  hienThiDapAnSauKhiNop: true,
  choPhepXemLai: true,
  cacCauHoi: []
})

// Toggle xem lại - đồng bộ cả 2 giá trị
const toggleXemLai = () => {
  form.choPhepXemLai = !form.choPhepXemLai
  form.hienThiDapAnSauKhiNop = form.choPhepXemLai
}

// Form tạo câu hỏi mới
const newQuestion = reactive({
  noiDungCauHoi: '',
  maDanhMuc: '',
  mucDo: 'De',
  loaiCauHoi: 'MotDapAn',
  giaiThich: '',
  congKhai: false,
  cacLuaChon: [
    { noiDung: '', laLuaChonDung: true },
    { noiDung: '', laLuaChonDung: false },
    { noiDung: '', laLuaChonDung: false },
    { noiDung: '', laLuaChonDung: false }
  ]
})

const resetNewQuestion = () => {
  newQuestion.noiDungCauHoi = ''
  newQuestion.maDanhMuc = ''
  newQuestion.mucDo = 'De'
  newQuestion.loaiCauHoi = 'MotDapAn'
  newQuestion.giaiThich = ''
  newQuestion.congKhai = false
  newQuestion.cacLuaChon = [
    { noiDung: '', laLuaChonDung: true },
    { noiDung: '', laLuaChonDung: false },
    { noiDung: '', laLuaChonDung: false },
    { noiDung: '', laLuaChonDung: false }
  ]
}

// ===== Batch Question Creation State =====
const batchQuestionCount = ref(1)
const batchCommonCategory = ref('')
const batchCommonDifficulty = ref('')
const currentBatchIndex = ref(0)
const batchQuestions = ref([])
const batchQuestionTextarea = ref(null)
const batchModalFocusRef = ref(null)

// Auto resize textarea - giữ min-height 38px
const autoResizeTextarea = (event) => {
  const textarea = event.target
  textarea.style.height = '38px'
  if (textarea.scrollHeight > 38) {
    textarea.style.height = textarea.scrollHeight + 'px'
  }
}

// Initialize batch questions array
const initBatchQuestions = () => {
  batchQuestions.value = Array.from({ length: batchQuestionCount.value }, () => createEmptyBatchQuestion())
  currentBatchIndex.value = 0
}

const createEmptyBatchQuestion = () => ({
  noiDungCauHoi: '',
  maDanhMuc: batchCommonCategory.value || '',
  mucDo: batchCommonDifficulty.value || 'De',
  loaiCauHoi: 'MotDapAn',
  giaiThich: '',
  congKhai: false,
  cacLuaChon: [
    { noiDung: '', laLuaChonDung: true },
    { noiDung: '', laLuaChonDung: false },
    { noiDung: '', laLuaChonDung: false },
    { noiDung: '', laLuaChonDung: false }
  ]
})

// Get current batch question
const currentBatchQuestion = computed(() => {
  return batchQuestions.value[currentBatchIndex.value]
})

// Check if a batch question is valid (complete) - requires all 4 options filled
const isBatchQuestionValid = (index) => {
  const q = batchQuestions.value[index]
  if (!q) return false
  
  const hasContent = q.noiDungCauHoi.trim() !== ''
  // Yêu cầu điền đủ 4 đáp án
  const allOptionsFilled = q.cacLuaChon.length === 4 && q.cacLuaChon.every(o => o.noiDung.trim() !== '')
  const hasCorrectAnswer = q.cacLuaChon.some(o => o.laLuaChonDung)
  
  return hasContent && allOptionsFilled && hasCorrectAnswer
}

// Get validation error message for a batch question
const getBatchValidationError = (index) => {
  const q = batchQuestions.value[index]
  if (!q) return ''
  
  // Only show errors if user has started filling the question
  if (!q.noiDungCauHoi.trim()) return ''
  
  const emptyOptions = q.cacLuaChon.filter(o => !o.noiDung.trim())
  if (emptyOptions.length > 0) {
    return `Còn ${emptyOptions.length} đáp án chưa điền. Vui lòng điền đủ 4 đáp án.`
  }
  
  if (!q.cacLuaChon.some(o => o.laLuaChonDung)) {
    return 'Chưa chọn đáp án đúng. Vui lòng đánh dấu ít nhất 1 đáp án đúng.'
  }
  
  return ''
}

// Count completed questions
const completedBatchQuestions = computed(() => {
  return batchQuestions.value.filter((_, index) => isBatchQuestionValid(index)).length
})

// Navigate between questions
const selectBatchQuestion = (index) => {
  currentBatchIndex.value = index
}

const prevBatchQuestion = () => {
  if (currentBatchIndex.value > 0) {
    currentBatchIndex.value--
  }
}

const nextBatchQuestion = () => {
  if (currentBatchIndex.value < batchQuestionCount.value - 1) {
    currentBatchIndex.value++
  }
}

// Change batch count
const increaseBatchQuestionCount = () => {
  if (batchQuestionCount.value < 50) {
    batchQuestionCount.value++
    batchQuestions.value.push(createEmptyBatchQuestion())
  }
}

const decreaseBatchQuestionCount = () => {
  if (batchQuestionCount.value > 1) {
    batchQuestionCount.value--
    batchQuestions.value.pop()
    if (currentBatchIndex.value >= batchQuestionCount.value) {
      currentBatchIndex.value = batchQuestionCount.value - 1
    }
  }
}

const onBatchCountChange = () => {
  const count = Math.max(1, Math.min(50, batchQuestionCount.value))
  batchQuestionCount.value = count
  
  while (batchQuestions.value.length < count) {
    batchQuestions.value.push(createEmptyBatchQuestion())
  }
  while (batchQuestions.value.length > count) {
    batchQuestions.value.pop()
  }
  
  if (currentBatchIndex.value >= count) {
    currentBatchIndex.value = count - 1
  }
}

// Apply common settings
const applyCommonCategory = () => {
  batchQuestions.value.forEach(q => {
    if (batchCommonCategory.value) {
      q.maDanhMuc = batchCommonCategory.value
    }
  })
}

const applyCommonDifficulty = () => {
  batchQuestions.value.forEach(q => {
    if (batchCommonDifficulty.value) {
      q.mucDo = batchCommonDifficulty.value
    }
  })
}

// Select correct answer for batch question
const selectBatchCorrectAnswer = (index) => {
  const q = currentBatchQuestion.value
  if (!q) return
  
  if (q.loaiCauHoi === 'MotDapAn') {
    q.cacLuaChon.forEach((opt, i) => {
      opt.laLuaChonDung = i === index
    })
  } else {
    q.cacLuaChon[index].laLuaChonDung = !q.cacLuaChon[index].laLuaChonDung
  }
}

// Handle question type change for batch
const onBatchQuestionTypeChange = () => {
  const q = currentBatchQuestion.value
  if (!q) return
  
  if (q.loaiCauHoi === 'MotDapAn') {
    let foundFirst = false
    q.cacLuaChon.forEach(opt => {
      if (opt.laLuaChonDung && !foundFirst) {
        foundFirst = true
      } else {
        opt.laLuaChonDung = false
      }
    })
    if (!foundFirst && q.cacLuaChon.length > 0) {
      q.cacLuaChon[0].laLuaChonDung = true
    }
  }
}

// Focus next option input
const focusNextOption = (index) => {
  if (index < 3) {
    // Focus next option input
    const inputs = document.querySelectorAll('.input-field')
    // Find and focus next
  }
}

// Close modal and reset
const closeNewQuestionModal = () => {
  showNewQuestionModal.value = false
  batchQuestionCount.value = 1
  batchCommonCategory.value = ''
  batchCommonDifficulty.value = ''
  currentBatchIndex.value = 0
  batchQuestions.value = []
}

// Open modal with proper initialization
const openNewQuestionModal = () => {
  batchQuestionCount.value = 1
  batchCommonCategory.value = ''
  batchCommonDifficulty.value = ''
  currentBatchIndex.value = 0
  initBatchQuestions()
  showNewQuestionModal.value = true
}

// Create all valid batch questions
const createBatchQuestions = async () => {
  const validQuestions = batchQuestions.value.filter((_, index) => isBatchQuestionValid(index))
  
  if (validQuestions.length === 0) {
    toast.error('Không có câu hỏi hợp lệ nào để tạo. Mỗi câu hỏi cần có nội dung, đủ 4 đáp án và ít nhất 1 đáp án đúng.')
    return
  }

  creatingQuestion.value = true
  let successCount = 0
  let failCount = 0

  try {
    for (const q of validQuestions) {
      // Sử dụng tất cả 4 đáp án
      const allOptions = q.cacLuaChon
      
      const data = {
        noiDungCauHoi: q.noiDungCauHoi,
        maDanhMuc: q.maDanhMuc || null,
        mucDo: q.mucDo,
        loaiCauHoi: q.loaiCauHoi,
        giaiThich: q.giaiThich || null,
        congKhai: q.congKhai,
        cacLuaChon: allOptions.map((o, i) => ({
          noiDungDapAn: o.noiDung,
          laDapAnDung: o.laLuaChonDung,
          thuTu: i
        }))
      }

      try {
        const response = await questionService.createQuestion(data)
        if (response.success) {
          // Add to quiz
          form.cacCauHoi.push({
            maCauHoi: response.data.maCauHoi,
            noiDungCauHoi: response.data.noiDungCauHoi,
            cacLuaChon: response.data.cacLuaChon,
            diem: 1,
            thuTu: form.cacCauHoi.length
          })
          successCount++
        } else {
          failCount++
        }
      } catch (error) {
        failCount++
        console.error('Failed to create question:', error)
      }
    }

    if (failCount > 0) {
      toast.warning(`${failCount} câu hỏi không thể tạo`)
    }
    
    closeNewQuestionModal()
  } catch (error) {
    toast.error('Lỗi khi tạo câu hỏi')
  } finally {
    creatingQuestion.value = false
  }
}

// Watch for modal open to initialize
watch(showNewQuestionModal, (newVal) => {
  if (newVal && batchQuestions.value.length === 0) {
    initBatchQuestions()
  }
})

// Xử lý khi thay đổi loại câu hỏi
const onQuestionTypeChange = () => {
  if (newQuestion.loaiCauHoi === 'MotDapAn') {
    // Nếu chuyển sang 1 đáp án, chỉ giữ lại 1 đáp án đúng đầu tiên
    let foundFirst = false
    newQuestion.cacLuaChon.forEach(opt => {
      if (opt.laLuaChonDung && !foundFirst) {
        foundFirst = true
      } else {
        opt.laLuaChonDung = false
      }
    })
    // Nếu không có đáp án đúng nào, chọn đáp án đầu tiên
    if (!foundFirst && newQuestion.cacLuaChon.length > 0) {
      newQuestion.cacLuaChon[0].laLuaChonDung = true
    }
  }
}

// Chọn đáp án đúng - xử lý theo loại câu hỏi
const selectCorrectAnswer = (index) => {
  if (newQuestion.loaiCauHoi === 'MotDapAn') {
    // Một đáp án: bỏ chọn tất cả, chọn cái được click
    newQuestion.cacLuaChon.forEach((opt, i) => {
      opt.laLuaChonDung = i === index
    })
  } else {
    // Nhiều đáp án: toggle
    newQuestion.cacLuaChon[index].laLuaChonDung = !newQuestion.cacLuaChon[index].laLuaChonDung
  }
}

const createAndAddQuestion = async () => {
  // Validate
  if (!newQuestion.noiDungCauHoi.trim()) {
    toast.error('Vui lòng nhập nội dung câu hỏi')
    return
  }
  
  // Kiểm tra phải điền đủ 4 đáp án
  const emptyOptions = newQuestion.cacLuaChon.filter(o => !o.noiDung.trim())
  if (emptyOptions.length > 0) {
    toast.error('Vui lòng điền đủ 4 đáp án, không được để trống ô nào')
    return
  }
  
  const hasCorrect = newQuestion.cacLuaChon.some(o => o.laLuaChonDung)
  if (!hasCorrect) {
    toast.error('Cần đánh dấu ít nhất 1 đáp án đúng')
    return
  }

  creatingQuestion.value = true
  try {
    const data = {
      noiDungCauHoi: newQuestion.noiDungCauHoi,
      maDanhMuc: newQuestion.maDanhMuc || null,
      mucDo: newQuestion.mucDo,
      loaiCauHoi: newQuestion.loaiCauHoi,
      giaiThich: newQuestion.giaiThich || null,
      congKhai: newQuestion.congKhai,
      cacLuaChon: newQuestion.cacLuaChon.map((o, i) => ({
        noiDungDapAn: o.noiDung,
        laDapAnDung: o.laLuaChonDung,
        thuTu: i
      }))
    }

    const response = await questionService.createQuestion(data)
    if (response.success) {
      // Thêm câu hỏi vừa tạo vào bài thi
      form.cacCauHoi.push({
        maCauHoi: response.data.maCauHoi,
        noiDungCauHoi: response.data.noiDungCauHoi,
        cacLuaChon: response.data.cacLuaChon,
        diem: 1,
        thuTu: form.cacCauHoi.length
      })
      
      showNewQuestionModal.value = false
      resetNewQuestion()
    } else {
      toast.error(response.message || 'Không thể tạo câu hỏi')
    }
  } catch (error) {
    toast.error(error.response?.data?.message || 'Không thể tạo câu hỏi')
  } finally {
    creatingQuestion.value = false
  }
}

const questionFilter = reactive({
  maDanhMuc: '',
  trangThaiChon: '', // '' | 'chuaChon' | 'daChon'
  pageSize: 10,
  pageNumber: 1
})

const questionPagination = reactive({
  currentPage: 1,
  totalPages: 1,
  totalCount: 0
})

const questionPageInput = ref(1)

// Computed: Câu hỏi hiển thị ở chế độ thu gọn (5 câu đầu)
const displayedQuestions = computed(() => {
  return form.cacCauHoi.slice(0, 5)
})

// Check if question is already in quiz
const isQuestionInQuiz = (maCauHoi) => {
  return form.cacCauHoi.some(q => q.maCauHoi === maCauHoi)
}

// Check if question is selected in modal
const isQuestionSelected = (maCauHoi) => {
  return selectedQuestions.value.some(q => q.maCauHoi === maCauHoi)
}

// Toggle question selection in modal (allow selecting even if already in quiz)
const toggleQuestionSelection = (question) => {
  const index = selectedQuestions.value.findIndex(q => q.maCauHoi === question.maCauHoi)
  if (index === -1) {
    // Add to selection - allow adding even if already in quiz
    selectedQuestions.value.push(question)
  } else {
    selectedQuestions.value.splice(index, 1)
  }
}

// Filtered questions for modal with sorting (selected first)
const filteredModalQuestions = computed(() => {
  let questions = [...availableQuestions.value]
  
  // Filter by selection status
  if (questionFilter.trangThaiChon === 'daChon') {
    questions = questions.filter(q => isQuestionInQuiz(q.maCauHoi))
  } else if (questionFilter.trangThaiChon === 'chuaChon') {
    questions = questions.filter(q => !isQuestionInQuiz(q.maCauHoi))
  }
  
  // Sort: questions in quiz first
  return questions.sort((a, b) => {
    const aInQuiz = isQuestionInQuiz(a.maCauHoi) ? 0 : 1
    const bInQuiz = isQuestionInQuiz(b.maCauHoi) ? 0 : 1
    return aInQuiz - bInQuiz
  })
})

const loadAvailableQuestions = async () => {
  loadingQuestions.value = true
  try {
    const response = await questionService.getQuestions({
      maDanhMuc: questionFilter.maDanhMuc,
      pageSize: questionFilter.pageSize,
      pageNumber: questionFilter.pageNumber,
      timKiem: questionSearch.value
    })
    // Keep all questions, we'll filter in computed
    availableQuestions.value = response.data || []
    
    // Update pagination
    if (response.pagination) {
      questionPagination.currentPage = response.pagination.currentPage
      questionPagination.totalPages = response.pagination.totalPages
      questionPagination.totalCount = response.pagination.totalCount
    }
  } catch (error) {
    console.error('Failed to load questions:', error)
  } finally {
    loadingQuestions.value = false
  }
}

const goToQuestionPage = (page) => {
  if (page < 1 || page > questionPagination.totalPages) return
  questionFilter.pageNumber = page
  questionPageInput.value = page
  loadAvailableQuestions()
}

const goToQuestionPageInput = () => {
  const page = parseInt(questionPageInput.value)
  if (page >= 1 && page <= questionPagination.totalPages) {
    goToQuestionPage(page)
  } else {
    questionPageInput.value = questionPagination.currentPage
  }
}

const resetAndLoadQuestions = () => {
  questionFilter.pageNumber = 1
  questionPageInput.value = 1
  loadAvailableQuestions()
}

const loadCategories = async () => {
  try {
    const response = await categoryService.getCategories()
    categories.value = response.data || []
  } catch (error) {
    console.error('Failed to load categories:', error)
  }
}

const addSelectedQuestions = () => {
  const count = selectedQuestions.value.length
  // Thêm theo thứ tự đã tick (selectedQuestions giữ thứ tự tick)
  selectedQuestions.value.forEach(q => {
    form.cacCauHoi.push({
      maCauHoi: q.maCauHoi,
      noiDungCauHoi: q.noiDungCauHoi,
      cacLuaChon: q.cacLuaChon,
      diem: 1,
      thuTu: form.cacCauHoi.length
    })
  })
  selectedQuestions.value = []
  showQuestionModal.value = false
}

// Shuffle questions randomly
const shuffleQuestions = () => {
  const array = [...form.cacCauHoi]
  for (let i = array.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [array[i], array[j]] = [array[j], array[i]]
  }
  form.cacCauHoi = array
}

// ===== Question Detail/Edit Functions =====
const openQuestionDetail = (question, index) => {
  editingQuestion.value = question
  editingQuestionIndex.value = index
  isEditingQuestion.value = false
  
  // Copy data for editing
  editingQuestionData.noiDungCauHoi = question.noiDungCauHoi
  editingQuestionData.giaiThich = question.giaiThich || ''
  editingQuestionData.loaiCauHoi = question.loaiCauHoi || 'MotDapAn'
  editingQuestionData.mucDo = question.mucDo || 'De'
  editingQuestionData.cacLuaChon = (question.cacLuaChon || []).map(opt => ({
    noiDungDapAn: opt.noiDungDapAn || opt.noiDung || '',
    laDapAnDung: opt.laDapAnDung || opt.laLuaChonDung || false,
    thuTu: opt.thuTu || 0
  }))
  
  showQuestionDetailModal.value = true
}

// Helper function to check if option is correct (handles both field names)
const getOptionIsCorrect = (option) => {
  return option.laDapAnDung || option.laLuaChonDung || false
}

// Helper function to get option content (handles both field names)
const getOptionContent = (option) => {
  return option.noiDungDapAn || option.noiDung || ''
}

// Toggle edit mode with proper state reset
const toggleEditMode = () => {
  if (isEditingQuestion.value) {
    // Switching from edit to view - reset editing data to original
    if (editingQuestion.value) {
      editingQuestionData.noiDungCauHoi = editingQuestion.value.noiDungCauHoi
      editingQuestionData.giaiThich = editingQuestion.value.giaiThich || ''
      editingQuestionData.loaiCauHoi = editingQuestion.value.loaiCauHoi || 'MotDapAn'
      editingQuestionData.mucDo = editingQuestion.value.mucDo || 'De'
      editingQuestionData.cacLuaChon = (editingQuestion.value.cacLuaChon || []).map(opt => ({
        noiDungDapAn: opt.noiDungDapAn || opt.noiDung || '',
        laDapAnDung: opt.laDapAnDung || opt.laLuaChonDung || false,
        thuTu: opt.thuTu || 0
      }))
    }
    isEditingQuestion.value = false
  } else {
    // Switching to edit mode
    isEditingQuestion.value = true
    // Auto-resize all textareas after DOM update
    nextTick(() => {
      // Resize question content textarea
      if (editQuestionContentRef.value) {
        editQuestionContentRef.value.style.height = 'auto'
        editQuestionContentRef.value.style.height = editQuestionContentRef.value.scrollHeight + 'px'
      }
      // Resize explanation textarea
      if (editQuestionExplanationRef.value) {
        editQuestionExplanationRef.value.style.height = 'auto'
        editQuestionExplanationRef.value.style.height = editQuestionExplanationRef.value.scrollHeight + 'px'
      }
      // Resize option textareas
      editOptionRefs.value.forEach(textarea => {
        if (textarea) {
          textarea.style.height = 'auto'
          textarea.style.height = textarea.scrollHeight + 'px'
        }
      })
    })
  }
}

// Toggle answer based on question type
const toggleEditAnswer = (index) => {
  const loaiCauHoi = editingQuestionData.loaiCauHoi || 'MotDapAn'
  
  if (loaiCauHoi === 'NhieuDapAn') {
    // Multiple answers allowed - just toggle
    editingQuestionData.cacLuaChon[index].laDapAnDung = !editingQuestionData.cacLuaChon[index].laDapAnDung
  } else {
    // Single answer - uncheck others first
    editingQuestionData.cacLuaChon.forEach((opt, i) => {
      opt.laDapAnDung = i === index
    })
  }
}

// Handle input on contenteditable option
const onOptionInput = (event, index) => {
  const text = event.target.innerText
  editingQuestionData.cacLuaChon[index].noiDungDapAn = text
}

// Handle blur on contenteditable option - ensure data is synced
const onOptionBlur = (event, index) => {
  const text = event.target.innerText
  editingQuestionData.cacLuaChon[index].noiDungDapAn = text
}

// Auto resize textarea - tự động mở rộng chiều cao khi nội dung dài
const autoResize = (event) => {
  const el = event.target
  el.style.height = 'auto'
  el.style.height = el.scrollHeight + 'px'
}

const saveEditedQuestion = async () => {
  // Validate
  if (!editingQuestionData.noiDungCauHoi.trim()) {
    toast.error('Vui lòng nhập nội dung câu hỏi')
    return
  }
  
  // Kiểm tra phải điền đủ 4 đáp án
  const emptyOptions = editingQuestionData.cacLuaChon.filter(o => !o.noiDungDapAn.trim())
  if (emptyOptions.length > 0) {
    toast.error('Vui lòng điền đủ 4 đáp án, không được để trống ô nào')
    return
  }
  
  const hasCorrect = editingQuestionData.cacLuaChon.some(o => o.laDapAnDung)
  if (!hasCorrect) {
    toast.error('Cần đánh dấu ít nhất 1 đáp án đúng')
    return
  }

  savingQuestion.value = true
  try {
    const maCauHoi = editingQuestion.value.maCauHoi
    
    const updateData = {
      noiDungCauHoi: editingQuestionData.noiDungCauHoi,
      giaiThich: editingQuestionData.giaiThich || null,
      loaiCauHoi: editingQuestionData.loaiCauHoi,
      mucDo: editingQuestionData.mucDo,
      cacLuaChon: editingQuestionData.cacLuaChon.map((o, i) => ({
        noiDungDapAn: o.noiDungDapAn,
        laDapAnDung: o.laDapAnDung,
        thuTu: i
      }))
    }

    const response = await questionService.updateQuestion(maCauHoi, updateData)
    if (response.success) {
      // Update in form.cacCauHoi
      const idx = editingQuestionIndex.value
      if (idx >= 0 && idx < form.cacCauHoi.length) {
        form.cacCauHoi[idx] = {
          ...form.cacCauHoi[idx],
          noiDungCauHoi: response.data.noiDungCauHoi,
          giaiThich: response.data.giaiThich,
          loaiCauHoi: response.data.loaiCauHoi,
          mucDo: response.data.mucDo,
          cacLuaChon: response.data.cacLuaChon
        }
      }
      
      // Update editingQuestion ref
      editingQuestion.value = {
        ...editingQuestion.value,
        noiDungCauHoi: response.data.noiDungCauHoi,
        giaiThich: response.data.giaiThich,
        loaiCauHoi: response.data.loaiCauHoi,
        mucDo: response.data.mucDo,
        cacLuaChon: response.data.cacLuaChon
      }
      
      isEditingQuestion.value = false
    } else {
      toast.error(response.message || 'Không thể cập nhật câu hỏi')
    }
  } catch (error) {
    toast.error(error.response?.data?.message || 'Không thể cập nhật câu hỏi')
  } finally {
    savingQuestion.value = false
  }
}

const removeQuestion = (index) => {
  form.cacCauHoi.splice(index, 1)
}

const handleSubmit = async () => {
  if (form.cacCauHoi.length === 0) {
    toast.error('Vui lòng thêm ít nhất 1 câu hỏi')
    return
  }

  submitting.value = true
  try {
    // Tính điểm mỗi câu: 10 / số câu hỏi
    const diemMoiCau = 10 / form.cacCauHoi.length
    
    const data = {
      ...form,
      maDanhMuc: form.maDanhMuc || null,  // Gửi null nếu không chọn danh mục
      cacCauHoi: form.cacCauHoi.map((q, index) => ({
        maCauHoi: q.maCauHoi,
        diem: diemMoiCau,
        thuTu: index
      }))
    }

    const response = await quizService.createQuiz(data)
    if (response.success) {
      router.push('/my-quizzes')
    } else {
      toast.error(response.message || 'Không thể tạo bài thi')
    }
  } catch (error) {
    toast.error(error.response?.data?.message || 'Không thể tạo bài thi')
  } finally {
    submitting.value = false
  }
}

// ===== AI Import Functions =====
const handleFileSelect = (event) => {
  const file = event.target.files[0]
  if (file) {
    processAIFile(file)
  }
}

const handleFileDrop = (event) => {
  const file = event.dataTransfer.files[0]
  if (file) {
    processAIFile(file)
  }
}

const processAIFile = async (file) => {
  // Validate category is selected
  if (!uploadCategory.value) {
    toast.error('Vui lòng chọn danh mục trước khi trích xuất')
    return
  }

  // Validate file size (5MB)
  if (file.size > 5 * 1024 * 1024) {
    toast.error('File quá lớn. Tối đa 5MB')
    return
  }
  
  // Validate file type
  const validTypes = ['.txt', '.docx', '.pdf']
  const ext = '.' + file.name.split('.').pop().toLowerCase()
  if (!validTypes.includes(ext)) {
    toast.error('Định dạng file không hỗ trợ. Chỉ chấp nhận: .txt, .docx, .pdf')
    return
  }

  aiImportStep.value = 'processing'
  
  try {
    const response = await aiService.extractQuestions(file)
    if (response.success && response.data) {
      // Transform API response - cacLuaChon is string[], dapAnDung is index
      const questions = response.data.cacCauHoi || []
      aiExtractedQuestions.value = questions.map(q => ({
        noiDungCauHoi: q.noiDungCauHoi,
        giaiThich: q.giaiThich || '',
        mucDo: q.mucDo || 'TrungBinh',
        maDanhMuc: uploadCategory.value || null,
        cacLuaChon: (q.cacLuaChon || []).map((opt, i) => ({
          noiDung: typeof opt === 'string' ? opt : (opt.noiDung || ''),
          laLuaChonDung: i === q.dapAnDung
        }))
      }))
      aiSessionId.value = response.data.maPhien
      aiSelectedQuestions.value = [...aiExtractedQuestions.value] // Select all by default
      aiImportStep.value = 'select'
      
      if (aiExtractedQuestions.value.length === 0) {
        toast.warning('Không tìm thấy câu hỏi nào trong file')
        aiImportStep.value = 'upload'
      }
    } else {
      toast.error(response.message || 'Không thể trích xuất câu hỏi')
      aiImportStep.value = 'upload'
    }
  } catch (error) {
    toast.error(error.response?.data?.message || 'Lỗi khi xử lý file')
    aiImportStep.value = 'upload'
  }
}

const toggleSelectAll = () => {
  if (aiSelectedQuestions.value.length === aiExtractedQuestions.value.length) {
    aiSelectedQuestions.value = []
  } else {
    aiSelectedQuestions.value = [...aiExtractedQuestions.value]
  }
}

// Generate questions from topic
const generateFromTopic = async () => {
  if (!generateForm.topic || generateForm.numberOfQuestions < 1) {
    toast.error('Vui lòng nhập chủ đề và số câu hỏi')
    return
  }

  aiImportStep.value = 'processing'
  
  try {
    const response = await aiService.generateFromTopic({
      topic: generateForm.topic,
      numberOfQuestions: Math.min(generateForm.numberOfQuestions, 20)
      // No difficulty - AI will decide for each question
    })

    if (response.success && response.data) {
      // Transform API response to match expected format
      // Backend returns: cacLuaChon as string[], dapAnDung as index
      const questions = response.data.cacCauHoi || []
      aiExtractedQuestions.value = questions.map(q => ({
        noiDungCauHoi: q.noiDungCauHoi,
        giaiThich: q.giaiThich || '',
        mucDo: q.mucDo || 'TrungBinh', // AI decides difficulty
        maDanhMuc: generateForm.maDanhMuc || null, // User-selected category
        // Transform cacLuaChon from string[] to object format
        cacLuaChon: (q.cacLuaChon || []).map((opt, i) => ({
          noiDung: typeof opt === 'string' ? opt : (opt.noiDung || ''),
          laLuaChonDung: i === q.dapAnDung
        }))
      }))
      aiSessionId.value = response.data.maPhien
      aiSelectedQuestions.value = [...aiExtractedQuestions.value]
      aiImportStep.value = 'select'

      if (aiExtractedQuestions.value.length === 0) {
        toast.warning('Không thể tạo câu hỏi từ chủ đề này')
        aiImportStep.value = 'generate'
      }
    } else {
      toast.error(response.message || 'Không thể tạo câu hỏi')
      aiImportStep.value = 'generate'
    }
  } catch (error) {
    console.error('Generate error:', error)
    toast.error(error.response?.data?.message || 'Lỗi khi tạo câu hỏi')
    aiImportStep.value = 'generate'
  }
}

const addAIQuestions = async () => {
  if (aiSelectedQuestions.value.length === 0) return

  try {
    // First, save questions to database
    const questionsToSave = aiSelectedQuestions.value.map(q => ({
      noiDungCauHoi: q.noiDungCauHoi,
      giaiThich: q.giaiThich || '',
      mucDo: q.mucDo || 'TrungBinh',
      loaiCauHoi: 'MotDapAn', // Fixed: Must be 'MotDapAn' or 'NhieuDapAn'
      congKhai: false,
      maDanhMuc: form.maDanhMuc || null,
      cacLuaChon: (q.cacLuaChon || []).map((opt, i) => ({
        noiDungDapAn: opt.noiDung || opt.noiDungDapAn || '',
        laDapAnDung: opt.laLuaChonDung || opt.laDapAnDung || false,
        thuTu: i
      }))
    }))

    // Save questions to database using bulk create
    const saveResult = await questionService.createBulkQuestions(questionsToSave)
    
    if (saveResult.success && saveResult.data?.length > 0) {
      // Add saved questions (with real IDs) to the quiz form
      saveResult.data.forEach(savedQuestion => {
        form.cacCauHoi.push({
          maCauHoi: savedQuestion.maCauHoi,
          noiDungCauHoi: savedQuestion.noiDungCauHoi,
          giaiThich: savedQuestion.giaiThich || '',
          mucDo: savedQuestion.mucDo || 'TrungBinh',
          cacLuaChon: savedQuestion.cacLuaChon || [],
          diem: 1,
          thuTu: form.cacCauHoi.length
        })
      })
      
      closeAIImport()
    } else {
      toast.error('Không thể lưu câu hỏi. Vui lòng thử lại.')
    }
  } catch (error) {
    console.error('Error saving AI questions:', error)
    toast.error(error.response?.data?.message || 'Lỗi khi lưu câu hỏi')
  }
}

const closeAIImport = () => {
  showAIImportModal.value = false
  aiImportStep.value = 'upload'
  aiExtractedQuestions.value = []
  aiSelectedQuestions.value = []
  aiSessionId.value = null
  uploadCategory.value = ''
  // Reset generate form
  generateForm.topic = ''
  generateForm.numberOfQuestions = 5
  generateForm.maDanhMuc = ''
}

onMounted(() => {
  loadCategories()
  loadAvailableQuestions()
})
</script>
