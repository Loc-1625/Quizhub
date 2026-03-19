using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuizHub.Data;
using QuizHub.Models.DTOs.Auth;
using QuizHub.Models.DTOs.BaiThi;
using QuizHub.Models.Entities;
using QuizHub.Services.Interfaces;

namespace QuizHub.Services.Implementations
{
    public class UserProfileService : IUserProfileService
    {
        private readonly QuizHubDbContext _context;
        private readonly UserManager<NguoiDung> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<UserProfileService> _logger;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public UserProfileService(
            QuizHubDbContext context,
            UserManager<NguoiDung> userManager,
            IWebHostEnvironment environment,
            ILogger<UserProfileService> logger,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _logger = logger;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<UserProfileDto?> GetMyProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);

            // Thống kê
            var tongBaiThi = await _context.BaiThi
                .CountAsync(bt => bt.NguoiTaoId == userId && bt.DaXoa == false);

            var tongCauHoi = await _context.CauHoi
                .CountAsync(ch => ch.NguoiTaoId == userId && ch.DaXoa == false);

            var tongLuotLamBai = await _context.LuotLamBai
                .CountAsync(llb => llb.NguoiDungId == userId);

            return new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email!,
                HoTen = user.HoTen ?? "",
                AnhDaiDien = user.AnhDaiDien,
                TieuSu = user.TieuSu,
                SoDienThoai = user.PhoneNumber,
                DiaChi = user.DiaChi,
                ProfileCongKhai = user.ProfileCongKhai,
                NgayTao = user.NgayTao,
                LanDangNhapCuoi = user.LanDangNhapCuoi,
                Roles = roles.ToList(),
                TongBaiThiDaTao = tongBaiThi,
                TongCauHoiDaTao = tongCauHoi,
                TongLuotLamBai = tongLuotLamBai
            };
        }

        public async Task<PublicProfileDto?> GetPublicProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.ProfileCongKhai) return null;

            // Thống kê công khai
            var baiThiCongKhai = await _context.BaiThi
                .Where(bt => bt.NguoiTaoId == userId && 
                            bt.DaXoa == false && 
                            bt.CheDoCongKhai == "CongKhai" &&
                            bt.TrangThai == "XuatBan")
                .ToListAsync();

            var tongLuotXem = baiThiCongKhai.Sum(bt => bt.LuotXem);

            // Điểm đánh giá trung bình
            var danhGiaIds = baiThiCongKhai.Select(bt => bt.MaBaiThi).ToList();
            var danhGias = await _context.DanhGia
                .Where(dg => danhGiaIds.Contains(dg.MaBaiThi))
                .ToListAsync();

            var diemTrungBinh = danhGias.Any() ? danhGias.Average(dg => dg.XepHang) : 0;
            var tongDanhGia = danhGias.Count;

            // Bài thi nổi bật (top 5 theo lượt xem)
            var baiThiNoiBat = baiThiCongKhai
                .OrderByDescending(bt => bt.LuotXem)
                .Take(5)
                .Select(bt => new BaiThiSummaryDto
                {
                    MaBaiThi = bt.MaBaiThi,
                    TieuDe = bt.TieuDe,
                    MoTa = bt.MoTa,
                    AnhBia = bt.AnhBia,
                    SoCauHoi = bt.CacCauHoi.Count,
                    ThoiGianLamBai = bt.ThoiGianLamBai,
                    LuotXem = bt.LuotXem,
                    NgayTao = bt.NgayTao,
                    TenNguoiTao = user.HoTen ?? "?n danh"
                })
                .ToList();

            return new PublicProfileDto
            {
                Id = user.Id,
                HoTen = user.HoTen ?? "",
                AnhDaiDien = user.AnhDaiDien,
                TieuSu = user.TieuSu,
                NgayTao = user.NgayTao,
                TongBaiThiCongKhai = baiThiCongKhai.Count,
                TongLuotXem = tongLuotXem,
                DiemDanhGiaTrungBinh = Math.Round(diemTrungBinh, 1),
                TongDanhGia = tongDanhGia,
                BaiThiNoiBat = baiThiNoiBat
            };
        }

        public async Task<UserProfileDto?> UpdateProfileAsync(string userId, UpdateProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            user.HoTen = dto.HoTen;
            // Chỉ update AnhDaiDien nếu có giá trị mới (không ghi đè nếu null)
            if (!string.IsNullOrEmpty(dto.AnhDaiDien))
            {
                user.AnhDaiDien = dto.AnhDaiDien;
            }
            user.PhoneNumber = dto.SoDienThoai;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            return await GetMyProfileAsync(userId);
        }

        public async Task<string> UploadAvatarAsync(string userId, IFormFile file)
        {
            // Validate file
            if (file == null || file.Length == 0)
                throw new InvalidOperationException("File không hợp lệ");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                throw new InvalidOperationException("Chỉ chấp nhận file ảnh: .jpg, .jpeg, .png, .gif");

            if (file.Length > 5 * 1024 * 1024) // 5MB
                throw new InvalidOperationException("Kích thước file không được vượt quá 5MB");

            // Tạo tên file unique
            var fileName = $"{userId}_{Guid.NewGuid()}{extension}";
            
            // Xử lý WebRootPath có thể null
            var webRootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            var uploadsFolder = Path.Combine(webRootPath, "uploads", "avatars");

            // Tạo thư mục nếu chưa tồn tại
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, fileName);

            // Lưu file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Cập nhật database
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                // Xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(user.AnhDaiDien))
                {
                    var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
                    var oldFilePath = Path.Combine(webRoot, user.AnhDaiDien.TrimStart('/'));
                    if (File.Exists(oldFilePath))
                    {
                        try { File.Delete(oldFilePath); } catch { }
                    }
                }

                user.AnhDaiDien = $"/uploads/avatars/{fileName}";
                await _userManager.UpdateAsync(user);
            }

            return $"/uploads/avatars/{fileName}";
        }

        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.ChangePasswordAsync(
                user, 
                dto.CurrentPassword, 
                dto.NewPassword);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            _logger.LogInformation("User {UserId} changed password successfully", userId);
            return true;
        }

        public async Task<bool> SendPasswordResetEmailAsync(ForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                // Không tiết lộ rằng email không tồn tại (bảo mật)
                _logger.LogWarning("Password reset requested for non-existent email: {Email}", dto.Email);
                return true; // Vẫn trả về true
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // Encode token thành Base64 URL-safe để tránh mất ký tự đặc biệt
            var tokenBytes = System.Text.Encoding.UTF8.GetBytes(token);
            var encodedToken = Convert.ToBase64String(tokenBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");

            var frontendBaseUrl = _configuration["Frontend:BaseUrl"];
            if (string.IsNullOrWhiteSpace(frontendBaseUrl))
            {
                frontendBaseUrl = _configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()?.FirstOrDefault()
                    ?? "http://localhost:5173";
            }

            // Tạo reset link theo cấu hình để chạy đúng ở localhost/Docker và production.
            var resetLink =
                $"{frontendBaseUrl.TrimEnd('/')}/reset-password?email={Uri.EscapeDataString(dto.Email)}&token={encodedToken}";

            if (_environment.IsDevelopment())
            {
                _logger.LogInformation("=== PASSWORD RESET ===");
                _logger.LogInformation("Email: {Email}", dto.Email);
                _logger.LogInformation("Link: {Link}", resetLink);
                _logger.LogInformation("======================");
            }

            // Gửi email
            var emailSent = await _emailService.SendPasswordResetEmailAsync(
                dto.Email,
                user.HoTen ?? "User",
                resetLink
            );

            if (!emailSent)
            {
                _logger.LogError("Failed to send password reset email to {Email}", dto.Email);
            }

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                throw new InvalidOperationException("Yêu cầu reset mật khẩu không hợp lệ");
            }

            // Decode token từ Base64 URL-safe
            var encodedToken = dto.Token
                .Replace("-", "+")
                .Replace("_", "/");
            
            // Add padding if necessary
            switch (encodedToken.Length % 4)
            {
                case 2: encodedToken += "=="; break;
                case 3: encodedToken += "="; break;
            }
            
            string token;
            try 
            {
                var tokenBytes = Convert.FromBase64String(encodedToken);
                token = System.Text.Encoding.UTF8.GetString(tokenBytes);
            }
            catch
            {
                // Fallback: try using token directly (for backwards compatibility)
                token = dto.Token;
            }

            var result = await _userManager.ResetPasswordAsync(
                user, 
                token, 
                dto.NewPassword);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            _logger.LogInformation("User {UserId} reset password successfully", user.Id);
            return true;
        }
    }
}
