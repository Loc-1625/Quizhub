using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using QuizHub.Models.DTOs.Auth;
using QuizHub.Models.Entities;
using QuizHub.Services.Interfaces;
using Google.Apis.Auth;

namespace QuizHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<NguoiDung> _userManager;
        private readonly SignInManager<NguoiDung> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly IRecaptchaService _recaptchaService;
        private readonly IUserProfileService _profileService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<NguoiDung> userManager,
            SignInManager<NguoiDung> signInManager,
            IJwtService jwtService,
            IRecaptchaService recaptchaService,
            IUserProfileService profileService,
            IEmailService emailService,
            ILogger<AuthController> logger,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _recaptchaService = recaptchaService;
            _profileService = profileService;
            _emailService = emailService;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Đăng ký tài khoản mới
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ"
                });
            }

            var captchaPassed = await _recaptchaService.VerifyTokenAsync(
                model.CaptchaToken,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                expectedAction: "register");

            if (!captchaPassed)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Xác minh captcha thất bại. Vui lòng thử lại."
                });
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Email đã được sử dụng"
                });
            }

            var user = new NguoiDung
            {
                UserName = model.Email,
                Email = model.Email,
                HoTen = model.HoTen,
                NgayTao = DateTime.UtcNow,
                TrangThaiKichHoat = true,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                });
            }

            // Thêm role mặc định
            await _userManager.AddToRoleAsync(user, "User");

            var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var frontendBaseUrl = _configuration["Frontend:BaseUrl"];
            if (string.IsNullOrWhiteSpace(frontendBaseUrl))
            {
                frontendBaseUrl = _configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()?.FirstOrDefault()
                    ?? "http://localhost:5173";
            }

            var confirmationLink =
                $"{frontendBaseUrl.TrimEnd('/')}/confirm-email?userId={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(confirmationToken)}";

            var emailSent = await _emailService.SendEmailConfirmationAsync(
                user.Email!,
                user.HoTen ?? "User",
                confirmationLink);

            if (!emailSent)
            {
                await _userManager.DeleteAsync(user);
                return StatusCode(StatusCodes.Status500InternalServerError, new AuthResponseDto
                {
                    Success = false,
                    Message = "Không thể gửi email xác nhận. Vui lòng thử lại sau."
                });
            }

            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "Đăng ký thành công. Vui lòng kiểm tra email để xác nhận tài khoản trước khi đăng nhập."
            });
        }

        [HttpGet("confirm-email")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Link xác nhận không hợp lệ."
                });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Tài khoản không tồn tại hoặc đã bị xóa."
                });
            }

            if (user.EmailConfirmed)
            {
                return Ok(new AuthResponseDto
                {
                    Success = true,
                    Message = "Email đã được xác nhận trước đó. Bạn có thể đăng nhập."
                });
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Xác nhận email thất bại hoặc link đã hết hạn."
                });
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendWelcomeEmailAsync(user.Email!, user.HoTen ?? "User");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send welcome email to {Email}", user.Email);
                }
            });

            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "Xác nhận email thành công. Bạn có thể đăng nhập ngay bây giờ."
            });
        }

        /// <summary>
        /// Đăng nhập
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ"
                });
            }

            var captchaPassed = await _recaptchaService.VerifyTokenAsync(
                model.CaptchaToken,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                expectedAction: "login");

            if (!captchaPassed)
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Xác minh captcha thất bại. Vui lòng thử lại."
                });
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Email hoặc mật khẩu không đúng"
                });
            }

            // Kiểm tra tài khoản có bị khóa bởi admin không
            if (!user.TrangThaiKichHoat)
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên."
                });
            }

            if (!user.EmailConfirmed)
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Tài khoản chưa xác nhận email. Vui lòng kiểm tra hộp thư và xác nhận trước khi đăng nhập."
                });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    return Unauthorized(new AuthResponseDto
                    {
                        Success = false,
                        Message = "Tài khoản đã bị khóa do đăng nhập sai quá nhiều lần"
                    });
                }

                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Email hoặc mật khẩu không đúng"
                });
            }

            // Cập nhật thời gian đăng nhập
            user.LanDangNhapCuoi = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var token = await _jwtService.GenerateJwtToken(user);
            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "Đăng nhập thành công",
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(1),
                User = new UserInfoDto
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    HoTen = user.HoTen ?? "",
                    AnhDaiDien = user.AnhDaiDien,
                    SoDienThoai = user.PhoneNumber,
                    Roles = roles.ToList()
                }
            });
        }

        /// <summary>
        /// Lấy thông tin user hiện tại
        /// </summary>
        [HttpGet("me")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<ActionResult<UserInfoDto>> GetCurrentUser()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var hasPassword = await _userManager.HasPasswordAsync(user);

            return Ok(new UserInfoDto
            {
                Id = user.Id,
                Email = user.Email!,
                HoTen = user.HoTen ?? "",
                HasPassword = hasPassword,
                AnhDaiDien = user.AnhDaiDien,
                SoDienThoai = user.PhoneNumber,
                Roles = roles.ToList()
            });
        }

        /// <summary>
        /// Đăng nhập bằng Google (SPA Flow)
        /// Nhận Token từ Frontend -> Xác thực -> Trả về JWT
        /// </summary>
        [HttpPost("google-login")]
        public async Task<ActionResult<AuthResponseDto>> GoogleLogin([FromBody] GoogleLoginDto request)
        {
            try
            {
                // 1. Dùng AccessToken để gọi sang Google lấy thông tin User
                using var httpClient = new HttpClient();
                // Gọi API UserInfo của Google
                var googleResponse = await httpClient.GetAsync($"https://www.googleapis.com/oauth2/v3/userinfo?access_token={request.AccessToken}");

                if (!googleResponse.IsSuccessStatusCode)
                {
                    return BadRequest(new AuthResponseDto { Success = false, Message = "Token Google không hợp lệ hoặc đã hết hạn." });
                }

                // 2. Đọc thông tin trả về
                var googleUser = await googleResponse.Content.ReadFromJsonAsync<GoogleUserInfoDto>();

                if (googleUser == null) return BadRequest("Không lấy được thông tin người dùng.");

                // 3. Logic Đăng ký/Đăng nhập 
                var user = await _userManager.FindByEmailAsync(googleUser.email);

                if (user == null)
                {
                    user = new NguoiDung
                    {
                        UserName = googleUser.email,
                        Email = googleUser.email,
                        HoTen = googleUser.name,
                        AnhDaiDien = googleUser.picture,
                        EmailConfirmed = true,
                        TrangThaiKichHoat = true,
                        NgayTao = DateTime.UtcNow,
                        LanDangNhapCuoi = DateTime.UtcNow
                    };

                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                    {
                        return BadRequest(new AuthResponseDto { Success = false, Message = "Lỗi tạo user." });
                    }
                    await _userManager.AddToRoleAsync(user, "User");
                }
                else
                {
                    // User cũ -> Cập nhật avatar/tên mới nhất từ Google
                    user.AnhDaiDien = googleUser.picture;
                    user.HoTen = googleUser.name;
                    await _userManager.UpdateAsync(user);
                }

                // 4. Tạo JWT Token
                var token = await _jwtService.GenerateJwtToken(user);
                var roles = await _userManager.GetRolesAsync(user);

                return Ok(new AuthResponseDto
                {
                    Success = true,
                    Message = "Đăng nhập Google thành công",
                    Token = token,
                    User = new UserInfoDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        HoTen = user.HoTen,
                        AnhDaiDien = user.AnhDaiDien,
                        Roles = roles.ToList()
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi Google Login");
                return StatusCode(500, new AuthResponseDto { Success = false, Message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        /// <summary>
        /// Liên kết tài khoản hiện tại với Google
        /// </summary>
        [HttpPost("link-google")]
        [Authorize]
        public async Task<IActionResult> LinkGoogleAccount()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null)
            {
                return NotFound();
            }

            var redirectUrl = Url.Action(nameof(LinkGoogleCallback), "Auth");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl, userId);

            return Challenge(properties, "Google");
        }

        [HttpGet("link-google-callback")]
        [Authorize]
        public async Task<IActionResult> LinkGoogleCallback()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null)
            {
                return NotFound();
            }

            var info = await _signInManager.GetExternalLoginInfoAsync(userId);
            if (info == null)
            {
                return BadRequest("Không thể lấy thông tin từ Google");
            }

            var result = await _userManager.AddLoginAsync(user, info);

            if (result.Succeeded)
            {
                return Ok(new { message = "Liên kết Google thành công" });
            }

            return BadRequest(new { message = "Không thể liên kết Google" });
        }

        /// <summary>
        /// [UC-04b] Lấy thông tin profile đầy đủ của user hiện tại
        /// </summary>
        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<UserProfileDto>> GetMyProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var profile = await _profileService.GetMyProfileAsync(userId);

            if (profile == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy profile" });
            }

            return Ok(new { success = true, data = profile });
        }

        /// <summary>
        /// [UC-04c] Lấy thông tin công khai của một user khác
        /// </summary>
        [HttpGet("profile/{userId}")]
        [AllowAnonymous]
        public async Task<ActionResult<PublicProfileDto>> GetPublicProfile(string userId)
        {
            var profile = await _profileService.GetPublicProfileAsync(userId);

            if (profile == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy profile hoặc profile không công khai"
                });
            }

            return Ok(new { success = true, data = profile });
        }

        /// <summary>
        /// [UC-04d] Cập nhật thông tin profile
        /// </summary>
        [HttpPut("profile")]
        [Authorize]
        public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Dữ liệu không hợp lệ",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
                var profile = await _profileService.UpdateProfileAsync(userId, dto);

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật profile thành công",
                    data = profile
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// [UC-04e] Upload ảnh đại diện
        /// </summary>
        [HttpPost("profile/avatar")]
        [Authorize]
        public async Task<ActionResult> UploadAvatar(IFormFile file)
        {
            if (file == null)
            {
                return BadRequest(new { success = false, message = "Vui lòng chọn file" });
            }

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
                var avatarUrl = await _profileService.UploadAvatarAsync(userId, file);

                return Ok(new
                {
                    success = true,
                    message = "Upload ảnh đại diện thành công",
                    data = new { avatarUrl }
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// [UC-04f] Đổi mật khẩu
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Dữ liệu không hợp lệ",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
                await _profileService.ChangePasswordAsync(userId, dto);

                return Ok(new
                {
                    success = true,
                    message = "Đổi mật khẩu thành công"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// [UC-04g] Quên mật khẩu - Gửi email reset
        /// </summary>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Email không hợp lệ"
                });
            }

            await _profileService.SendPasswordResetEmailAsync(dto);

            return Ok(new
            {
                success = true,
                message = "Nếu email tồn tại, link reset mật khẩu đã được gửi đến email của bạn"
            });
        }

        /// <summary>
        /// [UC-04h] Reset mật khẩu với token
        /// </summary>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Dữ liệu không hợp lệ",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            try
            {
                await _profileService.ResetPasswordAsync(dto);

                return Ok(new
                {
                    success = true,
                    message = "Reset mật khẩu thành công. Vui lòng đăng nhập với mật khẩu mới"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}