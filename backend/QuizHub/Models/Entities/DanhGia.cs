using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizHub.Models.Entities
{
    public class DanhGia
    {
        [Key]
        public Guid MaDanhGia { get; set; } = Guid.NewGuid();

        [Required]
        public Guid MaBaiThi { get; set; }

        [Required]
        public string NguoiDungId { get; set; } = string.Empty;

        [Range(1, 5)]
        public int XepHang { get; set; }

        public string? BinhLuan { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.UtcNow;
        public DateTime NgayCapNhat { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("MaBaiThi")]
        public virtual BaiThi BaiThi { get; set; } = null!;
    }
}