namespace QuizHub.Models.DTOs.BaiThi
{
    public class BaiThiSummaryDto
    {
        public Guid MaBaiThi { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public string? AnhBia { get; set; }
        public int ThoiGianLamBai { get; set; }
        public string CheDoCongKhai { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty;
        public string MaTruyCapDinhDanh { get; set; } = string.Empty;
        public int SoCauHoi { get; set; }
        public int LuotXem { get; set; }
        public int TongLuotLamBai { get; set; }
        public decimal? DiemTrungBinh { get; set; }
        public decimal? XepHangTrungBinh { get; set; }
        public DateTime NgayTao { get; set; }
        public string TenNguoiTao { get; set; } = string.Empty;
        public string? AnhDaiDienNguoiTao { get; set; } = null;
        
        // Thông tin danh mục
        public Guid? MaDanhMuc { get; set; }
        public string? TenDanhMuc { get; set; }
    }
}