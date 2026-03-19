import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

// Lazy loading components
const Home = () => import('@/views/Home.vue')
const Login = () => import('@/views/auth/Login.vue')
const Register = () => import('@/views/auth/Register.vue')
const ConfirmEmail = () => import('@/views/auth/ConfirmEmail.vue')
const ForgotPassword = () => import('@/views/auth/ForgotPassword.vue')
const ResetPassword = () => import('@/views/auth/ResetPassword.vue')
const GoogleCallback = () => import('@/views/auth/GoogleCallback.vue')
const Dashboard = () => import('@/views/Dashboard.vue')
const Profile = () => import('@/views/Profile.vue')
const PublicProfile = () => import('@/views/PublicProfile.vue')

// Quiz routes
const QuizExplore = () => import('@/views/quiz/QuizExplore.vue')
const QuizDetail = () => import('@/views/quiz/QuizDetail.vue')
const QuizTake = () => import('@/views/quiz/QuizTake.vue')
const QuizResult = () => import('@/views/quiz/QuizResult.vue')
const QuizCreate = () => import('@/views/quiz/QuizCreate.vue')
const QuizEdit = () => import('@/views/quiz/QuizEdit.vue')
const QuizManage = () => import('@/views/quiz/QuizManage.vue')
const QuizParticipants = () => import('@/views/quiz/QuizParticipants.vue')

// Question routes
const QuestionBank = () => import('@/views/questions/QuestionBank.vue')
const QuestionCreate = () => import('@/views/questions/QuestionCreate.vue')
const QuestionEdit = () => import('@/views/questions/QuestionEdit.vue')
const QuestionImport = () => import('@/views/questions/QuestionImport.vue')

// History routes
const History = () => import('@/views/History.vue')

// Admin routes
const AdminDashboard = () => import('@/views/admin/AdminDashboard.vue')
const AdminUsers = () => import('@/views/admin/AdminUsers.vue')
const AdminContent = () => import('@/views/admin/AdminContent.vue')
const AdminCategories = () => import('@/views/admin/AdminCategories.vue')
// const AdminReports = () => import('@/views/admin/AdminReports.vue') // Tạm ẩn

const routes = [
  {
    path: '/',
    name: 'home',
    component: Home,
    meta: { title: 'Trang chủ' }
  },
  
  // Auth routes
  {
    path: '/login',
    name: 'login',
    component: Login,
    meta: { title: 'Đăng nhập', guest: true }
  },
  {
    path: '/register',
    name: 'register',
    component: Register,
    meta: { title: 'Đăng ký', guest: true }
  },
  {
    path: '/confirm-email',
    name: 'confirm-email',
    component: ConfirmEmail,
    meta: { title: 'Xác nhận email', guest: true }
  },
  {
    path: '/forgot-password',
    name: 'forgot-password',
    component: ForgotPassword,
    meta: { title: 'Quên mật khẩu', guest: true }
  },
  {
    path: '/reset-password',
    name: 'reset-password',
    component: ResetPassword,
    meta: { title: 'Đặt lại mật khẩu', guest: true }
  },
  {
    path: '/auth/google-callback',
    name: 'google-callback',
    component: GoogleCallback,
    meta: { title: 'Đăng nhập Google' }
  },
  
  // User routes
  {
    path: '/dashboard',
    name: 'dashboard',
    component: Dashboard,
    meta: { title: 'Dashboard', requiresAuth: true }
  },
  {
    path: '/profile',
    name: 'profile',
    component: Profile,
    meta: { title: 'Hồ sơ cá nhân', requiresAuth: true }
  },
  {
    path: '/user/:userId',
    name: 'public-profile',
    component: PublicProfile,
    meta: { title: 'Hồ sơ người dùng' }
  },
  
  // Quiz routes
  {
    path: '/explore',
    name: 'explore',
    component: QuizExplore,
    meta: { title: 'Khám phá bài thi' }
  },
  {
    path: '/quiz/:id',
    name: 'quiz-detail',
    component: QuizDetail,
    meta: { title: 'Chi tiết bài thi' }
  },
  {
    path: '/quiz/:id/take',
    name: 'quiz-take',
    component: QuizTake,
    meta: { title: 'Làm bài thi' }
  },
  {
    path: '/quiz/:id/result/:luotLamBaiId',
    name: 'quiz-result',
    component: QuizResult,
    meta: { title: 'Kết quả bài thi' }
  },
  {
    path: '/quiz/create',
    name: 'quiz-create',
    component: QuizCreate,
    meta: { title: 'Tạo bài thi mới', requiresAuth: true }
  },
  {
    path: '/quiz/:id/edit',
    name: 'quiz-edit',
    component: QuizEdit,
    meta: { title: 'Chỉnh sửa bài thi', requiresAuth: true }
  },
  {
    path: '/my-quizzes',
    name: 'quiz-manage',
    component: QuizManage,
    meta: { title: 'Quản lý bài thi', requiresAuth: true }
  },
  {
    path: '/quiz/:id/participants',
    name: 'quiz-participants',
    component: QuizParticipants,
    meta: { title: 'Người tham gia', requiresAuth: true }
  },
  {
    path: '/my-quizzes/participants',
    name: 'quiz-participants-all',
    component: QuizParticipants,
    meta: { title: 'Tất cả người tham gia', requiresAuth: true }
  },
  
  // Question routes
  {
    path: '/questions',
    name: 'question-bank',
    component: QuestionBank,
    meta: { title: 'Ngân hàng câu hỏi', requiresAuth: true }
  },
  {
    path: '/questions/create',
    name: 'question-create',
    component: QuestionCreate,
    meta: { title: 'Tạo câu hỏi mới', requiresAuth: true }
  },
  {
    path: '/questions/:id/edit',
    name: 'question-edit',
    component: QuestionEdit,
    meta: { title: 'Chỉnh sửa câu hỏi', requiresAuth: true }
  },
  {
    path: '/questions/import',
    name: 'question-import',
    component: QuestionImport,
    meta: { title: 'Import câu hỏi bằng AI', requiresAuth: true }
  },
  
  // History
  {
    path: '/history',
    name: 'history',
    component: History,
    meta: { title: 'Lịch sử làm bài', requiresAuth: true }
  },
  
  // Admin routes
  {
    path: '/admin',
    name: 'admin-dashboard',
    component: AdminDashboard,
    meta: { title: 'Quản trị hệ thống', requiresAuth: true, requiresAdmin: true }
  },
  {
    path: '/admin/users',
    name: 'admin-users',
    component: AdminUsers,
    meta: { title: 'Quản lý người dùng', requiresAuth: true, requiresAdmin: true }
  },
  {
    path: '/admin/content',
    name: 'admin-content',
    component: AdminContent,
    meta: { title: 'Quản lý nội dung', requiresAuth: true, requiresAdmin: true }
  },
  {
    path: '/admin/categories',
    name: 'admin-categories',
    component: AdminCategories,
    meta: { title: 'Quản lý danh mục', requiresAuth: true, requiresAdmin: true }
  },
  // Route admin-reports tạm ẩn
  // {
  //   path: '/admin/reports',
  //   name: 'admin-reports',
  //   component: AdminReports,
  //   meta: { title: 'Xử lý báo cáo', requiresAuth: true, requiresAdmin: true }
  // },
  
  // Join quiz by code
  {
    path: '/join/:code',
    name: 'join-quiz',
    redirect: to => {
      return { name: 'quiz-detail', params: { id: 'code' }, query: { code: to.params.code } }
    }
  },
  
  // 404
  {
    path: '/:pathMatch(.*)*',
    name: 'not-found',
    component: () => import('@/views/NotFound.vue'),
    meta: { title: 'Không tìm thấy trang' }
  }
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
  scrollBehavior(to, from, savedPosition) {
    if (savedPosition) {
      return savedPosition
    } else {
      return { top: 0 }
    }
  }
})

// Navigation guards
router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()
  
  // Update page title
  document.title = to.meta.title ? `${to.meta.title} | QuizHub` : 'QuizHub'
  
  // Check if route requires authentication
  if (to.meta.requiresAuth && !authStore.isLoggedIn) {
    next({ name: 'login', query: { redirect: to.fullPath } })
    return
  }
  
  // Check if route requires admin
  if (to.meta.requiresAdmin && !authStore.isAdmin) {
    next({ name: 'home' })
    return
  }
  
  // Redirect logged in users away from guest pages
  if (to.meta.guest && authStore.isLoggedIn) {
    next({ name: 'dashboard' })
    return
  }
  
  next()
})

export default router
