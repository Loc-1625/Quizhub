using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.Entities
{
    public class BaoCao
    {
        [Key]
        public Guid MaBaoCao { get; set; } = Guid.NewGuid();

        [Required]
        public string NguoiBaoCaoId { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string LoaiDoiTuong { get; set; } = string.Empty;

        [Required]
        public Guid MaDoiTuong { get; set; }

        [Required]
        [MaxLength(50)]
        public string LyDo { get; set; } = string.Empty;

        public string? MoTa { get; set; }

        [MaxLength(20)]
        public string TrangThai { get; set; } = "ChoDuyet";

        public string? NguoiXuLyId { get; set; }

        public DateTime? ThoiGianXuLy { get; set; }

        public string? KetQuaXuLy { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    }
}