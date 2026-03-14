namespace QuizHub.Models.DTOs.BaiThi
{
    /// <summary>
    /// DTO hiển thị thông tin người đã làm bài thi
    /// </summary>
    public class NguoiLamBaiDto
    {
        public Guid MaLuotLamBai { get; set; }
        public Guid MaBaiThi { get; set; }
        public string TieuDeBaiThi { get; set; } = string.Empty;
        public string? NguoiDungId { get; set; }
        public string TenNguoiLamBai { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? AnhDaiDien { get; set; }
        public DateTime ThoiGianBatDau { get; set; }
        public DateTime? ThoiGianNopBai { get; set; }
        public int? ThoiGianLamBaiThucTe { get; set; } // Giây
        public decimal? Diem { get; set; }
        public int TongSoCauHoi { get; set; }
        public int SoCauDung { get; set; }
        public int SoCauSai { get; set; }
        public int SoCauChuaLam { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public bool DaDat { get; set; } // Đã đạt điểm yêu cầu hay chưa
    }

    /// <summary>
    /// DTO filter cho danh sách người làm bài
    /// </summary>
    public class NguoiLamBaiFilterDto
    {
        public Guid? MaBaiThi { get; set; }
        public string? TimKiem { get; set; } // Tìm theo tên người làm, email
        public string? TimKiemBaiThi { get; set; } // Tìm theo tên bài thi
        public string? TrangThai { get; set; } // DangLam, DaNop, HoanThanh, TuDongNop
        public string? SortBy { get; set; } = "ThoiGianBatDau"; // ThoiGianBatDau, Diem, TenNguoiLamBai
        public string? SortOrder { get; set; } = "DESC"; // ASC, DESC
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// DTO tổng hợp thống kê người làm bài
    /// </summary>
    public class ThongKeNguoiLamBaiDto
    {
        public int TongSoNguoiLamBai { get; set; }
        public int SoNguoiHoanThanh { get; set; }
        public int SoNguoiDangLam { get; set; }
        public int SoNguoiDat { get; set; }
        public int SoNguoiKhongDat { get; set; }
        public decimal? DiemTrungBinh { get; set; }
        public decimal? DiemCaoNhat { get; set; }
        public decimal? DiemThapNhat { get; set; }
    }
}
