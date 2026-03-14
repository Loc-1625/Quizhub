using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTOs.Auth
{
    /// <summary>
    /// DTO ?? yêu c?u reset m?t kh?u
    /// </summary>
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Email là b?t bu?c")]
        [EmailAddress(ErrorMessage = "Email không h?p l?")]
        public string Email { get; set; } = string.Empty;
    }
}
