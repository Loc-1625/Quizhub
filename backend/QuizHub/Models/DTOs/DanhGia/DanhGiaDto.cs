namespace QuizHub.Models.DTOs.DanhGia
{
    public class DanhGiaDto
    {
        public Guid MaDanhGia { get; set; }
        public Guid MaBaiThi { get; set; }
        public string TenBaiThi { get; set; } = string.Empty;
        public string NguoiDungId { get; set; } = string.Empty;
        public string TenNguoiDung { get; set; } = string.Empty;
        public string? AnhDaiDien { get; set; }
        public int XepHang { get; set; }
        public string? BinhLuan { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime NgayCapNhat { get; set; }
    }
}