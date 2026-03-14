using QuizHub.Models.DTOs.BaiThi;

namespace QuizHub.Models.DTOs.Auth
{
    /// <summary>
    /// DTO hi?n th? thông tin công khai (cho ng??i khác xem)
    /// </summary>
    public class PublicProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string? AnhDaiDien { get; set; }
        public string? TieuSu { get; set; }
        public DateTime NgayTao { get; set; }

        // Th?ng kê công khai
        public int TongBaiThiCongKhai { get; set; }
        public int TongLuotXem { get; set; }
        public double DiemDanhGiaTrungBinh { get; set; }
        public int TongDanhGia { get; set; }

        // Danh sách bài thi công khai g?n ?ây
        public List<BaiThiSummaryDto>? BaiThiNoiBat { get; set; }
    }
}
