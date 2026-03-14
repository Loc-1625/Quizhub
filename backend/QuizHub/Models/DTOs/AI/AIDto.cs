using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTOs.AI
{
    public class ExtractedQuestionDto
    {
        public string NoiDungCauHoi { get; set; } = string.Empty;
        public List<string> CacLuaChon { get; set; } = new();
        public int DapAnDung { get; set; } // Index c?a ?�p �n ?�ng (0-3)
        public string? GiaiThich { get; set; }
        public string? MucDo { get; set; }
    }

    public class ExtractionResultDto
    {
        public Guid MaPhien { get; set; }
        public string TenTepTin { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty; // "ThanhCong", "ThatBai", "DangXuLy"
        public int SoCauHoiTrichXuat { get; set; }
        public List<ExtractedQuestionDto> CacCauHoi { get; set; } = new();
        public string? ThongBaoLoi { get; set; }
        public int? ThoiGianXuLy { get; set; } // milliseconds
    }

    public class ImportQuestionDto
    {
        [Required]
        public Guid MaPhien { get; set; }

        [Required]
        public List<int> IndexCauHoi { get; set; } = new(); // Danh s�ch index c?a c�u h?i mu?n import

        public Guid? MaDanhMuc { get; set; }
        public bool CongKhai { get; set; } = false;
    }

    public class PhienNhapDuLieuDto
    {
        public Guid MaPhien { get; set; }
        public string TenTepTin { get; set; } = string.Empty;
        public string LoaiTepTin { get; set; } = string.Empty;
        public long KichThuocTepTin { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public int SoCauHoiTrichXuat { get; set; }
        public int SoCauHoiNhap { get; set; }
        public string? ThongBaoLoi { get; set; }
        public string? MoHinhAI { get; set; }
        public int? ThoiGianXuLy { get; set; }
        public DateTime NgayTao { get; set; }
    }

    public class GenerateFromTopicDto
    {
        [Required(ErrorMessage = "Chủ đề là bắt buộc")]
        [MinLength(5, ErrorMessage = "Chủ đề phải có ít nhất 5 ký tự")]
        [MaxLength(500, ErrorMessage = "Chủ đề không được vượt quá 500 ký tự")]
        public string Topic { get; set; } = string.Empty;

        [Range(1, 20, ErrorMessage = "Số lượng câu hỏi phải từ 1 đến 20")]
        public int NumberOfQuestions { get; set; } = 5;

        // Difficulty is optional - AI will determine appropriate difficulty for each question
        public string? Difficulty { get; set; }
    }

    public class GenerateResultDto
    {
        public Guid MaPhien { get; set; }
        public string ChuDe { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty;
        public int SoCauHoiTao { get; set; }
        public List<ExtractedQuestionDto> CacCauHoi { get; set; } = new();
        public string? ThongBaoLoi { get; set; }
        public int? ThoiGianXuLy { get; set; }
    }
}
