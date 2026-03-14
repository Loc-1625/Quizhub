# QuizHub Frontend

Giao diện người dùng Vue.js cho hệ thống QuizHub - Nền tảng thi trắc nghiệm trực tuyến.

## 🚀 Tính năng

### Người dùng
- **Đăng ký / Đăng nhập**: Email hoặc Google OAuth
- **Dashboard cá nhân**: Thống kê hoạt động, bài thi gần đây
- **Khám phá bài thi**: Tìm kiếm, lọc theo danh mục, độ khó
- **Làm bài thi**: Giao diện làm bài với đồng hồ đếm ngược, real-time SignalR
- **Xem kết quả**: Chi tiết điểm số, đáp án, phân tích
- **Lịch sử làm bài**: Xem lại các lượt làm bài

### Người tạo nội dung
- **Quản lý bài thi**: Tạo, sửa, xóa bài thi
- **Ngân hàng câu hỏi**: Quản lý câu hỏi cá nhân
- **AI Import**: Tạo câu hỏi tự động bằng Gemini AI
- **Thống kê**: Xem báo cáo về bài thi đã tạo

### Admin
- **Dashboard**: Tổng quan hệ thống
- **Quản lý người dùng**: CRUD, khóa/mở tài khoản
- **Quản lý nội dung**: Duyệt bài thi, câu hỏi
- **Quản lý danh mục**: Phân loại bài thi
- **Báo cáo**: Xử lý report từ người dùng

## 🛠️ Công nghệ

- **Vue 3.4** - Composition API
- **Vite 5** - Build tool
- **Tailwind CSS 3.4** - Styling
- **Pinia 2** - State management
- **Vue Router 4** - Routing
- **Axios** - HTTP client
- **SignalR** - Real-time communication
- **Chart.js** - Biểu đồ
- **Vue Toastification** - Notifications
- **Heroicons** - Icons

## 📦 Cài đặt

```bash
# Clone repository
cd quizhub-frontend

# Cài đặt dependencies
npm install

# Tạo file .env từ template
cp .env.example .env

# Chạy development server
npm run dev
```

## ⚙️ Cấu hình

Tạo file `.env` với các biến:

```env
# API Backend URL
VITE_API_URL=http://localhost:5000/api

# SignalR Hub URL
VITE_SIGNALR_URL=http://localhost:5000/quizhub

# Google OAuth Client ID (optional)
VITE_GOOGLE_CLIENT_ID=your-google-client-id
```

## 🏃‍♂️ Chạy ứng dụng

```bash
# Development mode
npm run dev

# Build production
npm run build

# Preview production build
npm run preview

# Lint code
npm run lint
```

## 📁 Cấu trúc thư mục

```
src/
├── assets/           # CSS, images
├── components/       # Vue components
│   ├── layout/       # Navbar, Footer
│   └── common/       # Shared components
├── router/           # Vue Router config
├── services/         # API services
│   ├── api.js        # Axios instance
│   ├── signalr.js    # SignalR service
│   └── index.js      # Service exports
├── stores/           # Pinia stores
│   ├── auth.js       # Authentication
│   └── quiz.js       # Quiz state
├── views/            # Page components
│   ├── auth/         # Login, Register...
│   ├── quiz/         # Quiz pages
│   ├── questions/    # Question management
│   └── admin/        # Admin pages
├── App.vue           # Root component
└── main.js           # Entry point
```

## 🔗 API Endpoints

Frontend kết nối với các API endpoints:

| Module | Endpoints |
|--------|-----------|
| Auth | `/api/Auth/*` - Đăng nhập, đăng ký, profile |
| BaiThi | `/api/BaiThi/*` - CRUD bài thi |
| CauHoi | `/api/CauHoi/*` - CRUD câu hỏi |
| LuotLamBai | `/api/LuotLamBai/*` - Lượt làm bài |
| DanhMuc | `/api/DanhMuc/*` - Danh mục |
| DanhGia | `/api/DanhGia/*` - Đánh giá |
| BaoCao | `/api/BaoCao/*` - Báo cáo |
| AI | `/api/AI/*` - Gemini AI |
| Admin | `/api/Admin/*` - Quản trị |

## 🎨 Theming

Màu sắc chính được định nghĩa trong `tailwind.config.js`:

- **Primary**: Blue (#3B82F6)
- **Secondary**: Purple (#8B5CF6)
- **Success**: Green
- **Warning**: Yellow
- **Danger**: Red

## 📱 Responsive

- Mobile-first design
- Breakpoints: sm (640px), md (768px), lg (1024px), xl (1280px)
- Touch-friendly UI elements

## 🔒 Authentication

- JWT tokens stored in localStorage
- Auto refresh token
- Route guards cho protected pages
- Role-based access control (User/Admin)

## 🚀 Deployment

### Build production:

```bash
npm run build
```

### Output: `dist/` folder

### Deploy options:
- **Nginx**: Copy `dist/` to web root
- **Vercel**: Connect repo, auto deploy
- **Netlify**: Drag & drop `dist/` folder

### Nginx config example:

```nginx
server {
    listen 80;
    server_name quizhub.com;
    root /var/www/quizhub/dist;
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }

    location /api {
        proxy_pass http://localhost:5000;
    }
}
```

## 📄 License

MIT License

## 👨‍💻 Tác giả

QuizHub Team
