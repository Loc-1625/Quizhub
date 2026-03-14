namespace QuizHub.Models.DTOs.LuotLamBai
{
    public class LuotLamBaiDto
    {
        public Guid MaLuotLamBai { get; set; }
        public Guid MaBaiThi { get; set; }
        public string TieuDeBaiThi { get; set; } = string.Empty;
        public string? TenNguoiThamGia { get; set; }
        public DateTime ThoiGianBatDau { get; set; }
        public DateTime? ThoiGianKetThuc { get; set; }
        public DateTime? ThoiGianNopBai { get; set; }
        public int? ThoiGianLamBaiThucTe { get; set; } // Giây
        public decimal? Diem { get; set; }
        public int TongSoCauHoi { get; set; }
        public int SoCauDung { get; set; }
        public int SoCauSai { get; set; }
        public int SoCauChuaLam { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public int ThoiGianConLai { get; set; } // Giây còn lại
        public bool ChoPhepXemLai { get; set; }
    }
}