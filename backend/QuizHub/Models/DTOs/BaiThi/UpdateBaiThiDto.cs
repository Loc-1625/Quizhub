using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTOs.BaiThi
{
    public class UpdateBaiThiDto
    {
        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [MaxLength(500, ErrorMessage = "Tiêu đề không được vượt quá 500 ký tự")]
        public string TieuDe { get; set; } = string.Empty;

        public string? MoTa { get; set; }

        [MaxLength(500)]
        public string? AnhBia { get; set; }

        /// <summary>
        /// Mã danh mục bài thi (tùy chọn)
        /// </summary>
        public Guid? MaDanhMuc { get; set; }

        [Required(ErrorMessage = "Thời gian làm bài là bắt buộc")]
        [Range(1, 300, ErrorMessage = "Thời gian làm bài phải từ 1 đến 300 phút")]
        public int ThoiGianLamBai { get; set; }

        [Range(0, 100, ErrorMessage = "Điểm đạt phải từ 0 đến 100")]
        public int? DiemDat { get; set; }

        [Required]
        [RegularExpression("^(RiengTu|CongKhai|CoMatKhau)$", ErrorMessage = "Chế độ phải là: RiengTu, CongKhai hoặc CoMatKhau")]
        public string CheDoCongKhai { get; set; } = "RiengTu";

        public bool HienThiDapAnSauKhiNop { get; set; } = true;

        public bool ChoPhepXemLai { get; set; } = true;

        public string? MatKhau { get; set; }

        // Có thể rỗng nếu chỉ update thông tin cơ bản (không update câu hỏi)
        public List<CauHoiTrongBaiThiDto> CacCauHoi { get; set; } = new();
    }
}