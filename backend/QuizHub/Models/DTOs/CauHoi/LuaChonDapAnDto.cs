using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTOs.CauHoi
{
    public class LuaChonDapAnDto
    {
        public Guid? MaLuaChon { get; set; } // Null khi tạo mới

        [Required(ErrorMessage = "Nội dung đáp án là bắt buộc")]
        [MaxLength(1000, ErrorMessage = "Nội dung đáp án không được vượt quá 1000 ký tự")]
        public string NoiDungDapAn { get; set; } = string.Empty;

        public bool LaDapAnDung { get; set; }

        [Range(0, 3, ErrorMessage = "Thứ tự phải từ 0 đến 3 (A, B, C, D)")]
        public int ThuTu { get; set; } // 0=A, 1=B, 2=C, 3=D
    }
}