namespace QuizHub.Models.DTOs.Auth
{
    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public DateTime? Expiration { get; set; }
        public UserInfoDto? User { get; set; }
    }

    public class UserInfoDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string? AnhDaiDien { get; set; }
        public string? SoDienThoai { get; set; }
        public List<string> Roles { get; set; } = new();
        public bool HasPassword { get; set; }
    }
}