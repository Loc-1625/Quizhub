namespace QuizHub.Models.DTOs.BaiThi
{
    public class BaiThiStatisticsDto
    {
        public Guid MaBaiThi { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public int TongLuotLamBai { get; set; }
        public int LuotHoanThanh { get; set; }
        public int LuotDangLam { get; set; }
        public decimal? DiemTrungBinh { get; set; }
        public decimal? DiemCaoNhat { get; set; }
        public decimal? DiemThapNhat { get; set; }
        public int SoNguoiDat { get; set; }
        public decimal TyLeDat { get; set; }
        public List<TopNguoiLamBaiDto> TopNguoiLamBai { get; set; } = new();
    }

    public class TopNguoiLamBaiDto
    {
        public string TenNguoiThamGia { get; set; } = string.Empty;
        public decimal Diem { get; set; }
        public int ThoiGianLamBai { get; set; } // Giây
        public DateTime ThoiGianNopBai { get; set; }
        public int XepHang { get; set; }
    }
}