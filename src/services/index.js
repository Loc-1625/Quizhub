import api from './api'

export const authService = {
  // Login
  async login(data) {
    const response = await api.post('/Auth/login', data)
    return response.data
  },

  // Register
  async register(data) {
    const response = await api.post('/Auth/register', data)
    return response.data
  },

  // Google login
  async googleLogin(data) {
    const response = await api.post('/Auth/google', data)
    return response.data
  },

  // Forgot password
  async forgotPassword(email) {
    const response = await api.post('/Auth/forgot-password', { email })
    return response.data
  },

  // Reset password
  async resetPassword(data) {
    const response = await api.post('/Auth/reset-password', data)
    return response.data
  },

  // Get profile
  async getProfile() {
    const response = await api.get('/Auth/profile')
    return response.data
  },

  // Update profile
  async updateProfile(data) {
    const response = await api.put('/Auth/profile', data)
    return response.data
  },

  // Change password
  async changePassword(data) {
    const response = await api.post('/Auth/change-password', data)
    return response.data
  },

  // Upload avatar
  async uploadAvatar(formData) {
    const response = await api.post('/Auth/profile/avatar', formData, {
      headers: { 
        'Content-Type': 'multipart/form-data'
      }
    })
    return response.data
  },

  // Refresh token
  async refreshToken(refreshToken) {
    const response = await api.post('/Auth/refresh-token', { refreshToken })
    return response.data
  }
}

export const quizService = {
  // Public quizzes with filters
  async getPublicQuizzes(params = {}) {
    const response = await api.get('/BaiThi/public', { params })
    return response.data
  },

  // Get all quizzes with filter
  async getQuizzes(filter = {}) {
    const response = await api.get('/BaiThi', { params: filter })
    return response.data
  },

  // Get quiz by ID
  async getQuizById(id) {
    const response = await api.get(`/BaiThi/${id}`)
    return response.data
  },

  // Get quiz by access code
  async getQuizByCode(code) {
    const response = await api.get(`/BaiThi/code/${code}`)
    return response.data
  },

  // Verify quiz password (for CoMatKhau mode)
  async verifyPassword(id, password) {
    const response = await api.post(`/BaiThi/${id}/verify-password`, { matKhau: password })
    return response.data
  },

  // Create quiz
  async createQuiz(data) {
    const response = await api.post('/BaiThi', data)
    return response.data
  },

  // Update quiz
  async updateQuiz(id, data) {
    const response = await api.put(`/BaiThi/${id}`, data)
    return response.data
  },

  // Delete quiz
  async deleteQuiz(id) {
    const response = await api.delete(`/BaiThi/${id}`)
    return response.data
  },

  // Toggle quiz visibility (công khai/riêng tư)
  async toggleVisibility(id) {
    const response = await api.patch(`/BaiThi/${id}/toggle-visibility`)
    return response.data
  },

  // Toggle quiz status (legacy - kept for compatibility)
  async toggleStatus(id) {
    const response = await api.put(`/BaiThi/${id}/toggle-status`)
    return response.data
  },

  // Get quiz statistics
  async getStatistics(id) {
    const response = await api.get(`/BaiThi/${id}/statistics`)
    return response.data
  },

  // Get quiz questions detail (for quiz owner only)
  async getQuestionsDetail(id) {
    const response = await api.get(`/BaiThi/${id}/questions-detail`)
    return response.data
  },

  // Get participants of a specific quiz
  async getParticipants(quizId, params = {}) {
    const response = await api.get(`/BaiThi/${quizId}/participants`, { params })
    return response.data
  },

  // Get all participants across all quizzes created by user
  async getAllParticipants(params = {}) {
    const response = await api.get('/BaiThi/all-participants', { params })
    return response.data
  },

  // Start quiz
  async startQuiz(data) {
    const response = await api.post('/LuotLamBai/start', data)
    return response.data
  },

  // Get quiz session
  async getSession(luotLamBaiId) {
    const response = await api.get(`/LuotLamBai/session/${luotLamBaiId}`)
    return response.data
  },

  // Save answer
  async saveAnswer(luotLamBaiId, data) {
    const response = await api.post(`/LuotLamBai/${luotLamBaiId}/answer`, data)
    return response.data
  },

  // Submit quiz
  async submitQuiz(luotLamBaiId, data) {
    const response = await api.post(`/LuotLamBai/${luotLamBaiId}/submit`, data)
    return response.data
  },

  // Get result
  async getResult(luotLamBaiId) {
    const response = await api.get(`/LuotLamBai/${luotLamBaiId}/result`)
    return response.data
  },

  // Get history
  async getHistory(pageNumber = 1, pageSize = 20) {
    const response = await api.get('/LuotLamBai/history', {
      params: { pageNumber, pageSize }
    })
    return response.data
  }
}

export const questionService = {
  // Get all questions
  async getQuestions(filter = {}) {
    const response = await api.get('/CauHoi', { params: filter })
    return response.data
  },

  // Get question stats (count by difficulty)
  async getStats() {
    const response = await api.get('/CauHoi/stats')
    return response.data
  },

  // Get question by ID
  async getQuestionById(id) {
    const response = await api.get(`/CauHoi/${id}`)
    return response.data
  },

  // Create question
  async createQuestion(data) {
    const response = await api.post('/CauHoi', data)
    return response.data
  },

  // Update question
  async updateQuestion(id, data) {
    const response = await api.put(`/CauHoi/${id}`, data)
    return response.data
  },

  // Delete question
  async deleteQuestion(id) {
    const response = await api.delete(`/CauHoi/${id}`)
    return response.data
  },

  // Create multiple questions at once
  async createBulkQuestions(questions) {
    // Create questions one by one and collect results
    const results = []
    for (const q of questions) {
      try {
        const response = await api.post('/CauHoi', q)
        if (response.data.success) {
          results.push(response.data.data)
        }
      } catch (error) {
        console.error('Failed to create question:', error)
      }
    }
    return {
      success: results.length > 0,
      data: results,
      message: `Đã tạo ${results.length}/${questions.length} câu hỏi`
    }
  },

  // Get questions count
  async getCount() {
    const response = await api.get('/CauHoi/count')
    return response.data
  },

  // Get questions by category
  async getByCategory(categoryId, pageNumber = 1, pageSize = 20) {
    const response = await api.get(`/CauHoi/danh-muc/${categoryId}`, {
      params: { pageNumber, pageSize }
    })
    return response.data
  },

  // Get question answer detail for admin
  async getQuestionAnswerByAdmin(id) {
    const response = await api.get(`/Admin/Answer/${id}`)
    return response.data
  }
}

export const categoryService = {
  // Get all categories
  async getCategories() {
    const response = await api.get('/DanhMuc')
    return response.data
  },

  // Get category by ID
  async getCategoryById(id) {
    const response = await api.get(`/DanhMuc/${id}`)
    return response.data
  },

  // Create category (Admin)
  async createCategory(data) {
    const response = await api.post('/DanhMuc', data)
    return response.data
  },

  // Update category (Admin)
  async updateCategory(id, data) {
    const response = await api.put(`/DanhMuc/${id}`, data)
    return response.data
  },

  // Delete category (Admin)
  async deleteCategory(id) {
    const response = await api.delete(`/DanhMuc/${id}`)
    return response.data
  }
}

export const aiService = {
  // Extract questions from file
  async extractQuestions(file) {
    const formData = new FormData()
    formData.append('file', file)
    
    // Debug log
    console.log('Uploading file:', file.name, file.type, file.size)
    
    const response = await api.post('/AI/extract-questions', formData, {
      headers: { 
        'Content-Type': 'multipart/form-data'
      },
      timeout: 120000 // 2 minutes timeout for AI processing
    })
    return response.data
  },

  // Generate questions from text content (using generate-questions endpoint)
  async generateQuestions({ noiDung, soLuong, mucDo, maDanhMuc }) {
    // Call the proper generate-questions endpoint
    const response = await api.post('/AI/generate-questions', {
      topic: noiDung,
      numberOfQuestions: soLuong || 5,
      difficulty: mucDo || 'TrungBinh'
    }, {
      timeout: 120000 // 2 minutes timeout for AI processing
    })
    
    console.log('AI Response:', response.data) // Debug log
    
    // Transform response to match expected format
    if (response.data.success && response.data.data?.cacCauHoi) {
      return {
        success: true,
        data: response.data.data.cacCauHoi.map(q => ({
          noiDungCauHoi: q.noiDungCauHoi,
          giaiThich: q.giaiThich,
          mucDo: mucDo || q.mucDo || 'TrungBinh',
          maDanhMuc: maDanhMuc,
          congKhai: false,
          // Backend returns cacLuaChon as string[] and dapAnDung as index
          cacLuaChon: (q.cacLuaChon || []).map((optText, i) => ({
            noiDungDapAn: typeof optText === 'string' ? optText : (optText.noiDung || optText.noiDungDapAn || ''),
            laDapAnDung: i === q.dapAnDung,
            thuTu: i
          }))
        }))
      }
    }
    
    return response.data
  },

  // Get extraction result
  async getExtractionResult(sessionId) {
    const response = await api.get(`/AI/sessions/${sessionId}`)
    return response.data
  },

  // Import questions
  async importQuestions(data) {
    const response = await api.post('/AI/import-questions', data)
    return response.data
  },

  // Get import history
  async getImportHistory(pageNumber = 1, pageSize = 10) {
    const response = await api.get('/AI/sessions', {
      params: { pageNumber, pageSize }
    })
    return response.data
  },

  // Generate questions from topic
  async generateFromTopic({ topic, numberOfQuestions, difficulty }) {
    const response = await api.post('/AI/generate-questions', {
      topic,
      numberOfQuestions,
      difficulty
    }, {
      timeout: 300000 // 5 minutes timeout for AI processing
    })
    return response.data
  },

  // Extract questions from file (with options)
  async extractFromFile(file, options = {}) {
    const formData = new FormData()
    formData.append('file', file)
    
    // Debug log
    console.log('Uploading file:', file.name, file.type, file.size)
    
    const response = await api.post('/AI/extract-questions', formData, {
      headers: { 
        'Content-Type': 'multipart/form-data'
      },
      timeout: 300000 // 5 minutes timeout for AI processing
    })
    return response.data
  }
}

export const reviewService = {
  // Get reviews for a quiz
  async getReviews(baiThiId, pageNumber = 1, pageSize = 10) {
    const response = await api.get(`/DanhGia/bai-thi/${baiThiId}`, {
      params: { pageNumber, pageSize }
    })
    return response.data
  },

  // Get review statistics
  async getStatistics(baiThiId) {
    const response = await api.get(`/DanhGia/bai-thi/${baiThiId}/statistics`)
    return response.data
  },

  // Get my review
  async getMyReview(baiThiId) {
    const response = await api.get(`/DanhGia/bai-thi/${baiThiId}/my-review`)
    return response.data
  },

  // Create review
  async createReview(data) {
    const response = await api.post('/DanhGia', data)
    return response.data
  },

  // Update review
  async updateReview(id, data) {
    const response = await api.put(`/DanhGia/${id}`, data)
    return response.data
  },

  // Delete review
  async deleteReview(id) {
    const response = await api.delete(`/DanhGia/${id}`)
    return response.data
  }
}

export const reportService = {
  // Create report
  async createReport(data) {
    const response = await api.post('/BaoCao', data)
    return response.data
  },

  // Get reports (Admin)
  async getReports(params = {}) {
    const response = await api.get('/BaoCao', { params })
    return response.data
  },

  // Handle report (Admin)
  async handleReport(id, data) {
    const response = await api.put(`/BaoCao/${id}/xu-ly`, data)
    return response.data
  },

  // Get admin stats (calls /Admin/dashboard)
  async getAdminStats() {
    const response = await api.get('/Admin/dashboard')
    return response.data
  },

  // Get top quizzes (from dashboard stats)
  async getTopQuizzes(params = {}) {
    const response = await api.get('/Admin/dashboard')
    // Extract topBaiThi from dashboard response
    if (response.data.success && response.data.data?.topBaiThi) {
      const limit = params.limit || 5
      return {
        success: true,
        data: response.data.data.topBaiThi.slice(0, limit).map(q => ({
          maBaiThi: q.maBaiThi,
          tieuDe: q.tieuDe,
          tenNguoiTao: q.nguoiTao,
          tongLuotLamBai: q.tongLuotLamBai,
          diemTrungBinh: q.diemTrungBinh
        }))
      }
    }
    return { success: false, data: [] }
  }
}

export const adminService = {
  // Get dashboard stats
  async getDashboard() {
    const response = await api.get('/Admin/dashboard')
    return response.data
  },

  // Get users
  async getUsers(params = {}) {
    const response = await api.get('/Admin/users', { params })
    return response.data
  },

  // Get user by ID
  async getUserById(userId) {
    const response = await api.get(`/Admin/users/${userId}`)
    return response.data
  },

  // Create user
  async createUser(data) {
    const response = await api.post('/Admin/users', data)
    return response.data
  },

  // Update user
  async updateUser(userId, data) {
    const response = await api.put(`/Admin/users/${userId}`, data)
    return response.data
  },

  // Delete user
  async deleteUser(userId) {
    const response = await api.delete(`/Admin/users/${userId}`)
    return response.data
  },

  // Toggle user status
  async toggleUserStatus(userId) {
    const response = await api.put(`/Admin/users/${userId}/toggle-status`)
    return response.data
  },

  // Update user role
  async updateUserRole(userId, role) {
    const response = await api.put(`/Admin/users/${userId}/role`, { role })
    return response.data
  },

  // Get recent users
  async getRecentUsers(params = {}) {
    const { limit, ...rest } = params
    const response = await api.get('/Admin/users', { 
      params: { ...rest, pageSize: limit || 5, sortBy: 'NgayTao', sortOrder: 'DESC' }
    })
    return response.data
  },

  // Get all quizzes (admin)
  async getAllQuizzes(params = {}) {
    const response = await api.get('/BaiThi', { params })
    return response.data
  },

  // Get all questions (admin)
  async getAllQuestions(params = {}) {
    const response = await api.get('/CauHoi', { params })
    return response.data
  },

  // Get all content
  async getContent(params = {}) {
    const response = await api.get('/Admin/content', { params })
    return response.data
  },

  async getAllContent(params = {}) {
    const response = await api.get('/Admin/content', { params })
    return response.data
  },

  // Delete content
  async deleteContent(type, id, hardDelete = false) {
    const response = await api.delete(`/Admin/content/${type}/${id}`, {
      params: { hardDelete }
    })
    return response.data
  },

  // Restore content
  async restoreContent(type, id) {
    const response = await api.put(`/Admin/content/${type}/${id}/restore`)
    return response.data
  }
}

export const attemptService = {
  // Start quiz
  async startQuiz(quizId, data = {}) {
    const response = await api.post('/LuotLamBai/start', { maBaiThi: quizId, ...data })
    return response.data
  },

  // Get session
  async getSession(attemptId) {
    const response = await api.get(`/LuotLamBai/session/${attemptId}`)
    return response.data
  },

  // Save answer
  async saveAnswer(attemptId, data) {
    const response = await api.post(`/LuotLamBai/${attemptId}/answer`, data)
    return response.data
  },

  // Submit quiz
  async submitQuiz(attemptId) {
    const response = await api.post(`/LuotLamBai/${attemptId}/submit`)
    return response.data
  },

  // Get result
  async getResult(attemptId) {
    const response = await api.get(`/LuotLamBai/${attemptId}/result`)
    return response.data
  },

  // Get my attempts
  async getMyAttempts(params = {}) {
    const response = await api.get('/LuotLamBai/history', { params })
    return response.data
  },

  // Get my stats
  async getMyStats() {
    const response = await api.get('/LuotLamBai/my-stats')
    return response.data
  },

  // Get attempts by quiz
  async getAttemptsByQuiz(quizId, params = {}) {
    const response = await api.get(`/LuotLamBai/quiz/${quizId}`, { params })
    return response.data
  }
}
