namespace QuizHub.Models.DTOs.Auth
{
    /// <summary>
    /// DTO hi?n th? th�ng tin profile ??y ?? (cho ch�nh user)
    /// </summary>
    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string? AnhDaiDien { get; set; }
        public string? TieuSu { get; set; }
        public string? SoDienThoai { get; set; }
        public string? DiaChi { get; set; }
        public bool ProfileCongKhai { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime? LanDangNhapCuoi { get; set; }
        public List<string> Roles { get; set; } = new();

        // Th?ng k�
        public int TongBaiThiDaTao { get; set; }
        public int TongCauHoiDaTao { get; set; }
        public int TongLuotLamBai { get; set; }
    }
}
