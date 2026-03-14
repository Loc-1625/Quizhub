using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTOs.BaiThi
{
    public class CauHoiTrongBaiThiDto
    {
        [Required(ErrorMessage = "Mã câu hỏi là bắt buộc")]
        public Guid MaCauHoi { get; set; }

        [Range(0.01, 100, ErrorMessage = "Điểm phải từ 0.01 đến 100")]
        public decimal Diem { get; set; } = 1;

        public int ThuTu { get; set; }
    }
}