namespace QuizHub.Models.DTOs.LuotLamBai
{
    public class BaiThiSessionDto
    {
        public Guid MaLuotLamBai { get; set; }
        public Guid MaBaiThi { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public int ThoiGianLamBai { get; set; } // Phút
        public DateTime ThoiGianBatDau { get; set; }
        public DateTime ThoiGianKetThuc { get; set; }
        public int ThoiGianConLai { get; set; } // Giây
        public List<CauHoiInSessionDto> CacCauHoi { get; set; } = new();
    }

    public class CauHoiInSessionDto
    {
        public Guid MaCauHoi { get; set; }
        public string NoiDungCauHoi { get; set; } = string.Empty;
        public int ThuTu { get; set; }
        public decimal Diem { get; set; }
        public List<LuaChonInSessionDto> CacLuaChon { get; set; } = new();
        public Guid? DaChon { get; set; } // MaLuaChon đã chọn (nếu có)
    }

    public class LuaChonInSessionDto
    {
        public Guid MaLuaChon { get; set; }
        public string NoiDungDapAn { get; set; } = string.Empty;
        public int ThuTu { get; set; }
    }
}