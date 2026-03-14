namespace QuizHub.Models.DTOs.BaiThi
{
    public class BaiThiFilterDto
    {
        public string? TimKiem { get; set; }
        public Guid? MaDanhMuc { get; set; } // Lọc theo danh mục
        public string? CheDoCongKhai { get; set; } // "RiengTu", "CongKhai", "CoMatKhau"
        public string? TrangThai { get; set; } // "Nhap", "XuatBan", "DaDong"
        public bool? ChiLayCuaToi { get; set; } // true = chỉ lấy bài thi của user hiện tại
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "NgayTao"; // NgayTao, LuotXem, TongLuotLamBai, DiemTrungBinh
        public string? SortOrder { get; set; } = "DESC"; // ASC, DESC
    }
}