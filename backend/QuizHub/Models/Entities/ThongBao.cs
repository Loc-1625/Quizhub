using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.Entities
{
    public class ThongBao
    {
        [Key]
        public Guid MaThongBao { get; set; } = Guid.NewGuid();

        [Required]
        public string NguoiDungId { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LoaiThongBao { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string TieuDe { get; set; } = string.Empty;

        [Required]
        public string NoiDung { get; set; } = string.Empty;

        public bool DaDoc { get; set; } = false;

        public Guid? MaDoiTuongLienQuan { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    }
}