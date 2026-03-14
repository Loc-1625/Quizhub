using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTOs.Auth
{
    /// <summary>
    /// DTO ?? ??i m?t kh?u (d�nh cho user ?� ??ng nh?p)
    /// </summary>
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "M?t kh?u hi?n t?i l� b?t bu?c")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [MinLength(8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", 
            ErrorMessage = "Mật khẩu phải chứa ít nhất 1 chữ hoa, 1 chữ thường và 1 số")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "X�c nh?n m?t kh?u l� b?t bu?c")]
        [Compare("NewPassword", ErrorMessage = "M?t kh?u x�c nh?n kh�ng kh?p")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
