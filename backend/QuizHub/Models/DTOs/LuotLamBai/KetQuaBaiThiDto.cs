namespace QuizHub.Models.DTOs.LuotLamBai
{
    public class KetQuaBaiThiDto
    {
        public Guid MaLuotLamBai { get; set; }
        public Guid MaBaiThi { get; set; }
        public string TieuDeBaiThi { get; set; } = string.Empty;
        public decimal Diem { get; set; }
        public int TongSoCauHoi { get; set; }
        public int SoCauDung { get; set; }
        public int SoCauSai { get; set; }
        public int SoCauChuaLam { get; set; }
        public int ThoiGianLamBaiThucTe { get; set; } // Giây
        public DateTime ThoiGianNopBai { get; set; }
        public bool DatYeuCau { get; set; }
        public int? DiemDat { get; set; }
        public bool ChoPhepXemLai { get; set; }
        public List<CauTraLoiChiTietDto>? ChiTietCauTraLoi { get; set; }
    }

    public class CauTraLoiChiTietDto
    {
        public Guid MaCauHoi { get; set; }
        public string NoiDungCauHoi { get; set; } = string.Empty;
        public string? GiaiThich { get; set; }
        public int ThuTu { get; set; }
        public Guid? MaLuaChonDaChon { get; set; }
        public Guid? MaDapAnDung { get; set; }
        public bool LaDapAnDung { get; set; }
        public List<LuaChonChiTietDto> CacLuaChon { get; set; } = new();
    }

    public class LuaChonChiTietDto
    {
        public Guid MaLuaChon { get; set; }
        public string NoiDungDapAn { get; set; } = string.Empty;
        public bool LaDapAnDung { get; set; }
        public int ThuTu { get; set; }
    }
}