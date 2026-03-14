namespace QuizHub.Models.DTOs.BaiThi
{
    public class BaiThiDto
    {
        public Guid MaBaiThi { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public string? AnhBia { get; set; }
        public int ThoiGianLamBai { get; set; }
        public int? DiemDat { get; set; }
        public string CheDoCongKhai { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty;
        public bool HienThiDapAnSauKhiNop { get; set; }
        public bool ChoPhepXemLai { get; set; }
        public string MaTruyCapDinhDanh { get; set; } = string.Empty;
        public bool CoMatKhau { get; set; }
        public int LuotXem { get; set; }
        public int TongLuotLamBai { get; set; }
        public decimal? DiemTrungBinh { get; set; }
        public decimal? XepHangTrungBinh { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime NgayCapNhat { get; set; }

        // Thông tin người tạo
        public string NguoiTaoId { get; set; } = string.Empty;
        public string TenNguoiTao { get; set; } = string.Empty;
        public string? AnhDaiDienNguoiTao { get; set; } = null;

        // Thông tin danh mục
        public Guid? MaDanhMuc { get; set; }
        public string? TenDanhMuc { get; set; }

        // Danh sách câu hỏi
        public int SoCauHoi { get; set; }
        public List<CauHoiTrongBaiThiDetailDto>? CacCauHoi { get; set; }
    }

    public class CauHoiTrongBaiThiDetailDto
    {
        public Guid MaCauHoi { get; set; }
        public string NoiDungCauHoi { get; set; } = string.Empty;
        public decimal Diem { get; set; }
        public int ThuTu { get; set; }
    }

    // DTO cho xem chi tiết câu hỏi và đáp án của bài thi (dành cho chủ bài thi)
    public class CauHoiChiTietDto
    {
        public Guid MaCauHoi { get; set; }
        public string NoiDungCauHoi { get; set; } = string.Empty;
        public string? GiaiThich { get; set; }
        public string LoaiCauHoi { get; set; } = "MotDapAn";
        public string MucDo { get; set; } = "De";
        public decimal Diem { get; set; }
        public int ThuTu { get; set; }
        public List<LuaChonChiTietDto> CacLuaChon { get; set; } = new();
    }

    public class LuaChonChiTietDto
    {
        public string NoiDung { get; set; } = string.Empty;
        public bool LaDapAnDung { get; set; }
    }
}