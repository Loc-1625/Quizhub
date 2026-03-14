using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTOs.DanhGia
{
    public class UpdateDanhGiaDto
    {
        [Required(ErrorMessage = "Xếp hạng là bắt buộc")]
        [Range(1, 5, ErrorMessage = "Xếp hạng phải từ 1 đến 5 sao")]
        public int XepHang { get; set; }

        [MaxLength(2000, ErrorMessage = "Bình luận không được vượt quá 2000 ký tự")]
        public string? BinhLuan { get; set; }
    }
}