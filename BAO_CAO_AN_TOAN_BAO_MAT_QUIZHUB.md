# BÁO CÁO PHÂN TÍCH CÁC BIỆN PHÁP AN TOÀN VÀ BẢO MẬT THÔNG TIN

## 1. Giới thiệu
Hệ thống QuizHub là một ứng dụng Fullstack gồm Frontend Vue.js, Backend ASP.NET Core, CSDL Microsoft SQL Server và Entity Framework Core Migration. Báo cáo này phân tích các biện pháp bảo mật đã được triển khai trong mã nguồn, đánh giá hiệu quả phòng thủ trước các nhóm tấn công phổ biến, đồng thời nêu các điểm cần cải thiện để nâng cao mức độ an toàn thông tin.

## 2. Mô hình kiến trúc và bề mặt tấn công

### 2.1. Thành phần kiến trúc
- Frontend (Vue.js): gửi yêu cầu API, lưu trạng thái đăng nhập và token.
- Backend (ASP.NET Core): xác thực, phân quyền, xử lý nghiệp vụ, middleware bảo mật, SignalR realtime.
- Database (SQL Server): lưu dữ liệu nghiệp vụ, người dùng, quyền, lịch sử làm bài.
- ORM (Entity Framework Core): truy vấn dữ liệu và migration.

### 2.2. Bề mặt tấn công chính
- API xác thực và quản trị.
- Endpoint upload file, reset mật khẩu, AI import/extract.
- Kênh realtime SignalR.
- Cấu hình ứng dụng và secret.
- Cơ chế lưu token phía client.

## 3. Các công nghệ, kỹ thuật và nguyên tắc bảo mật đã áp dụng

### 3.1. Xác thực và quản lý danh tính
- ASP.NET Core Identity được cấu hình với chính sách mật khẩu, khóa tài khoản khi đăng nhập sai nhiều lần, email duy nhất.
- JWT Bearer Authentication cho API stateless.
- Token reset/password token có thời hạn sống giới hạn.

Nguyên tắc bảo mật áp dụng:
- Authentication mạnh theo chuẩn framework.
- Giảm brute-force qua lockout.
- Giới hạn thời gian hiệu lực token nhạy cảm.

### 3.2. Phân quyền truy cập (Authorization)
- Dùng Authorize và Authorize theo Role (Admin) ở controller/endpoint.
- Tách endpoint công khai và endpoint yêu cầu xác thực bằng AllowAnonymous/Authorize.

Nguyên tắc bảo mật áp dụng:
- Principle of Least Privilege.
- Role-Based Access Control (RBAC).

### 3.3. Bảo vệ API trước lạm dụng và tấn công từ chối dịch vụ
- Cấu hình ASP.NET Core Rate Limiter đa chiến lược:
  - Global limiter (sliding window).
  - Policy auth cho login/register.
  - Policy ai dạng token-bucket.
  - Policy quiz dạng concurrency limiter.
- Trả về Retry-After và thông điệp chuẩn cho trạng thái 429.

Nguyên tắc bảo mật áp dụng:
- Availability protection.
- Abuse prevention.

### 3.4. Security Headers và transport security
- Tự thêm các header bảo mật: X-Frame-Options, X-Content-Type-Options, Referrer-Policy, CSP, Permissions-Policy.
- Production bật HSTS.
- Bắt buộc chuyển hướng HTTPS.

Nguyên tắc bảo mật áp dụng:
- Defense in Depth ở lớp HTTP.
- Secure by default cho kênh truyền.

### 3.5. Kiểm soát CORS theo môi trường
- Development cho phép một số localhost origin cụ thể.
- Production dùng danh sách origin cho phép tường minh.

Nguyên tắc bảo mật áp dụng:
- Same-origin boundary enforcement.
- Giảm nguy cơ truy cập API từ nguồn không tin cậy.

### 3.6. Validation dữ liệu đầu vào
- Tích hợp FluentValidation cho DTO xác thực và DTO nghiệp vụ.
- Kết hợp ModelState validation ở controller.

Nguyên tắc bảo mật áp dụng:
- Input validation.
- Fail-fast khi dữ liệu bất hợp lệ.

### 3.7. Bảo vệ dữ liệu và thao tác CSDL
- EF Core giảm rủi ro SQL Injection khi dùng LINQ và truy vấn tham số hóa.
- Có sử dụng ExecuteSqlInterpolatedAsync (nội suy tham số an toàn theo cơ chế EF Core).

Nguyên tắc bảo mật áp dụng:
- Parameterized query.
- Data integrity trong cập nhật thống kê/atomic update.

### 3.8. Middleware xử lý lỗi và logging tập trung
- Global exception middleware chuẩn hóa response lỗi.
- Log có trace id, đường dẫn, phương thức.
- Chỉ trả chi tiết stack trace ở môi trường development.

Nguyên tắc bảo mật áp dụng:
- Controlled error disclosure.
- Auditability và forensic support.

### 3.9. Bảo mật file upload
- Kiểm tra phần mở rộng file ảnh cho avatar.
- Giới hạn kích thước file.
- Đặt tên file sinh ngẫu nhiên (GUID) trước khi lưu.

Nguyên tắc bảo mật áp dụng:
- Input/content restriction.
- File handling hygiene.

### 3.10. Frontend security practice
- Gắn Authorization Bearer token qua interceptor cho mọi request cần xác thực.
- Có xử lý 401 để thu hồi phiên phía client.
- Vue template mặc định escape nội dung khi render interpolation thông thường.

Nguyên tắc bảo mật áp dụng:
- Session consistency.
- Cơ chế tự phục hồi phiên và giảm token sai hạn.

## 4. Cách áp dụng cụ thể trong dự án (ASP.NET Core + Vue.js + SQL Server)

### 4.1. Backend ASP.NET Core
- Program.cs cấu hình Identity, JWT, RateLimiter, CORS, middleware pipeline và HTTPS/HSTS.
- AuthController triển khai đăng ký/đăng nhập/đổi mật khẩu/reset mật khẩu với kiểm tra trạng thái tài khoản.
- Controller quản trị áp dụng Role Admin ở cấp controller.
- DbInitializer seed role mặc định và tài khoản admin mặc định khi chưa tồn tại.

### 4.2. Database SQL Server + EF Core
- Dùng Migration để đồng bộ schema nhất quán, giảm thao tác SQL thủ công.
- Truy vấn nghiệp vụ chủ yếu bằng LINQ/EF.
- Một số cập nhật hiệu năng dùng ExecuteSqlInterpolatedAsync để giữ tính tham số hóa.

### 4.3. Frontend Vue.js
- Axios interceptor gắn Bearer token vào header Authorization.
- Trạng thái auth đang được persist trong localStorage qua pinia-plugin-persistedstate.
- Điều hướng về login khi token không hợp lệ (401).

## 5. Phân tích khả năng phòng chống các loại tấn công

### 5.1. SQL Injection
Biện pháp đang có:
- EF Core + LINQ (mặc định parameterized).
- ExecuteSqlInterpolatedAsync thay vì nối chuỗi SQL thủ công.

Hiệu quả:
- Giảm mạnh nguy cơ SQL Injection ở lớp truy vấn ứng dụng.

Giới hạn:
- Nếu trong tương lai dùng ExecuteSqlRaw và nối chuỗi trực tiếp thì rủi ro sẽ tăng cao.

### 5.2. Brute-force và credential stuffing
Biện pháp đang có:
- Identity lockout sau số lần đăng nhập sai nhất định.
- Rate limit policy riêng cho auth endpoint.

Hiệu quả:
- Giảm tốc độ thử mật khẩu tự động.
- Tăng chi phí tấn công dò tài khoản.

### 5.3. Authentication bypass / Privilege escalation
Biện pháp đang có:
- JWT có validate issuer/audience/lifetime/signing key.
- Authorize và Authorize(Roles = "Admin") cho tài nguyên nhạy cảm.

Hiệu quả:
- Hạn chế truy cập trái phép khi thiếu token hợp lệ hoặc thiếu role.

Giới hạn:
- Cần kiểm thử phân quyền định kỳ để tránh lỗi logic nghiệp vụ theo từng endpoint.

### 5.4. XSS (Cross-Site Scripting)
Biện pháp đang có:
- Vue mặc định escape output khi dùng interpolation thông thường.
- Security headers gồm CSP, X-XSS-Protection, X-Content-Type-Options.

Hiệu quả:
- Giảm nguy cơ thực thi script không mong muốn từ dữ liệu đầu vào.

Giới hạn quan trọng:
- CSP hiện vẫn cho phép unsafe-inline và unsafe-eval, làm suy giảm hiệu lực chống XSS hiện đại.
- Token lưu localStorage có thể bị đánh cắp nếu XSS xảy ra.

### 5.5. CSRF (Cross-Site Request Forgery)
Biện pháp đang có:
- API dùng Authorization Bearer token trong header, không dựa vào cookie session mặc định.

Hiệu quả:
- Mô hình token-header giảm đáng kể bề mặt CSRF truyền thống.

Giới hạn:
- Nếu tương lai chuyển sang cookie auth hoặc thêm endpoint dùng cookie, cần bật anti-forgery token và SameSite/HttpOnly/Secure.

### 5.6. Clickjacking
Biện pháp đang có:
- X-Frame-Options: DENY.

Hiệu quả:
- Ngăn trang bị nhúng iframe từ domain khác để lừa thao tác người dùng.

### 5.7. DoS/Abuse API
Biện pháp đang có:
- Global rate limit + policy theo nghiệp vụ (auth/ai/quiz/public).
- Concurrency limiter cho quiz.

Hiệu quả:
- Giảm nguy cơ bùng nổ request và lạm dụng tài nguyên backend.

### 5.8. Information disclosure
Biện pháp đang có:
- Exception middleware chuẩn hóa lỗi.
- Chỉ trả stack trace ở development.

Hiệu quả:
- Hạn chế rò rỉ chi tiết hệ thống khi xảy ra lỗi runtime.

Giới hạn:
- Một số endpoint test/debug có khả năng trả chi tiết lỗi quá mức nếu bật công khai.

## 6. Đối chiếu bằng chứng từ mã nguồn
- Rate limiting cấu hình tại Program: AddRateLimiter và các policy.
- Identity/JWT/CORS/HTTPS/HSTS trong Program pipeline.
- Security headers tại middleware SecurityHeadersMiddleware.
- RBAC Admin ở AdminController với Authorize role.
- Lockout login tại AuthController khi CheckPasswordSignInAsync với lockoutOnFailure.
- Validation bằng FluentValidation ở Validators/Auth.
- Upload avatar có giới hạn định dạng/kích thước ở UserProfileService.
- Persist token localStorage ở frontend store auth.

## 7. Các điểm rủi ro còn tồn tại và kiến nghị cải thiện

### 7.1. Secret nhạy cảm đang lưu trực tiếp trong appsettings
Quan sát:
- JWT SecretKey, Google ClientSecret, Gemini ApiKey, SMTP Password xuất hiện trực tiếp trong file cấu hình.

Rủi ro:
- Lộ bí mật khi push git, chia sẻ source hoặc log/backup sai cách.

Khuyến nghị:
- Chuyển sang Secret Manager (dev), biến môi trường/Key Vault (prod).
- Rotate toàn bộ secret đã lộ.

### 7.2. Tài khoản admin mặc định seed với mật khẩu cứng
Quan sát:
- Seed admin dùng mật khẩu cố định.

Rủi ro:
- Dễ bị khai thác nếu email/mật khẩu mẫu bị đoán hoặc rò rỉ.

Khuyến nghị:
- Bắt buộc đổi mật khẩu khi đăng nhập lần đầu.
- Seed bằng mật khẩu sinh ngẫu nhiên từ env var, không hard-code.

### 7.3. Mã hóa mật khẩu truy cập bài thi dùng SHA-256 không salt
Quan sát:
- HashPassword cho mật khẩu bài thi dùng SHA-256 trực tiếp.

Rủi ro:
- Chống brute-force yếu hơn các thuật toán password hashing chuyên dụng.

Khuyến nghị:
- Dùng PasswordHasher, PBKDF2, BCrypt hoặc Argon2.

### 7.4. Chính sách CSP còn nới lỏng
Quan sát:
- CSP cho phép unsafe-inline và unsafe-eval.

Rủi ro:
- Giảm hiệu quả phòng chống XSS nâng cao.

Khuyến nghị:
- Siết CSP bằng nonce/hash cho script và loại bỏ unsafe-eval.

### 7.5. Token lưu ở localStorage
Quan sát:
- Auth state persist user/token vào localStorage.

Rủi ro:
- Khi bị XSS, token có thể bị lấy cắp.

Khuyến nghị:
- Cân nhắc chuyển sang HttpOnly Secure SameSite cookie + CSRF token.
- Nếu giữ localStorage, cần tăng cường CSP và kiểm soát chặt dữ liệu hiển thị.

### 7.6. Endpoint test AI có dấu hiệu lộ thông tin nội bộ
Quan sát:
- Endpoint test trả stack trace và thông tin cấu hình API key prefix.

Rủi ro:
- Tăng nguy cơ reconnaissance cho attacker.

Khuyến nghị:
- Loại bỏ endpoint debug khỏi production hoặc bảo vệ bằng role nội bộ + feature flag.

## 8. Kết luận
Hệ thống đã triển khai nhiều lớp bảo mật quan trọng theo hướng hiện đại gồm Identity + JWT, RBAC, rate limiting đa chính sách, security headers, kiểm soát CORS, validation đầu vào, xử lý lỗi tập trung và cơ chế upload có kiểm tra. Các biện pháp này giúp giảm đáng kể rủi ro từ SQL Injection, brute-force, truy cập trái phép, clickjacking và lạm dụng API.

Tuy nhiên, mức độ an toàn tổng thể vẫn bị ảnh hưởng bởi các điểm yếu có tính vận hành và cấu hình như lộ secret trong appsettings, mật khẩu admin mặc định hard-code, CSP còn nới lỏng, lưu token trong localStorage và endpoint debug nhạy cảm. Nếu xử lý triệt để các điểm này, hệ thống sẽ đạt mức trưởng thành bảo mật cao hơn và phù hợp hơn với môi trường production thực tế.

## 9. Phụ lục: Gợi ý checklist cải tiến nhanh
- Di chuyển toàn bộ secret sang biến môi trường/secret vault và xoay vòng khóa.
- Loại bỏ mật khẩu admin cứng; bắt đổi mật khẩu ngay lần đăng nhập đầu.
- Thay SHA-256 cho mật khẩu bài thi bằng thuật toán password hashing mạnh.
- Siết CSP và kiểm thử XSS định kỳ.
- Rà soát endpoint test/debug, tắt hoàn toàn trên production.
- Bổ sung kiểm thử bảo mật tự động: SAST, dependency scan, secret scan.
- Thiết lập quy trình quản lý log bảo mật và cảnh báo bất thường.
