using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTOs.BaiThi
{
    /// <summary>
    /// DTO để xác thực mật khẩu bài thi
    /// </summary>
    public class VerifyPasswordDto
    {
        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public string MatKhau { get; set; } = string.Empty;
    }
}
