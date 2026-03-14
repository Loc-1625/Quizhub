using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using QuizHub.Models.Entities;
using QuizHub.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace QuizHub.Services.Implementations
{
    /// <summary>
    /// Service xử lý JWT token
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<NguoiDung> _userManager;

        /// <summary>
        /// Khởi tạo JwtService
        /// </summary>
        /// <param name="configuration">Configuration để đọc JWT settings</param>
        /// <param name="userManager">UserManager để lấy roles</param>
        public JwtService(IConfiguration configuration, UserManager<NguoiDung> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }

        /// <summary>
        /// Tạo JWT token cho người dùng
        /// </summary>
        /// <param name="user">Thông tin người dùng</param>
        /// <returns>JWT token string</returns>
        public async Task<string> GenerateJwtToken(NguoiDung user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

            var userRoles = await _userManager.GetRolesAsync(user);

            // Tạo danh sách claims cho token
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.HoTen ?? user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Thêm roles vào claims
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Cấu hình token descriptor
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpirationMinutes"])),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(secretKey),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}