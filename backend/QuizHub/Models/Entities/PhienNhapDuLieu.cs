using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.Entities
{
    public class PhienNhapDuLieu
    {
        [Key]
        public Guid MaPhien { get; set; } = Guid.NewGuid();

        [Required]
        public string NguoiDungId { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string TenTepTin { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string LoaiTepTin { get; set; } = string.Empty;

        public long KichThuocTepTin { get; set; }

        [MaxLength(20)]
        public string TrangThai { get; set; } = "DangXuLy";

        public int SoCauHoiTrichXuat { get; set; } = 0;

        public int SoCauHoiNhap { get; set; } = 0;

        public string? ThongBaoLoi { get; set; }

        [MaxLength(50)]
        public string? MoHinhAI { get; set; }

        public int? ThoiGianXuLy { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    }
}