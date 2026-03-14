using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTOs.AI
{
    public class UploadFileDto
 {
        [Required(ErrorMessage = "File là b?t bu?c")]
  public IFormFile File { get; set; } = null!;

  [MaxLength(200)]
     public string? TenPhien { get; set; }
    }
}
