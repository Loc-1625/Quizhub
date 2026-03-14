namespace QuizHub.Models.DTOs.Admin
{
    public class UserManagementDto
    {
        public string Id { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? HoTen { get; set; }
        public string? AnhDaiDien { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime? LanDangNhapCuoi { get; set; }
        public bool TrangThaiKichHoat { get; set; }
        public List<string> Roles { get; set; } = new();

        // Statistics
        public int SoBaiThiTao { get; set; }
        public int SoCauHoiTao { get; set; }
        public int SoLuotLamBai { get; set; }
    }
}
