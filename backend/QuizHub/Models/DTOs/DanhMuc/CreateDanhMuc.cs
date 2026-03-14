using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTOs.DanhMuc
{
    public class CreateDanhMucDto
    {
        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [MaxLength(200, ErrorMessage = "Tên danh mục không được vượt quá 200 ký tự")]
        public string TenDanhMuc { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? MoTa { get; set; }

        [MaxLength(500)]
        public string? HinhAnh { get; set; }
    }
}