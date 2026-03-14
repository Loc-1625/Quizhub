using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTOs.Auth
{
    /// <summary>
    /// DTO để cập nhật thông tin profile
    /// </summary>
    public class UpdateProfileDto
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [MaxLength(200, ErrorMessage = "Họ tên không được vượt quá 200 ký tự")]
        public string HoTen { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "URL ảnh đại diện không được vượt quá 500 ký tự")]
        public string? AnhDaiDien { get; set; }

        [RegularExpression(@"^[0-9]{9,15}$", ErrorMessage = "Số điện thoại không hợp lệ (9-15 chữ số)")]
        public string? SoDienThoai { get; set; }
    }
}
