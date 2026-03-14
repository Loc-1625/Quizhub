using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizHub.Models.Entities
{
    public class CauHoiTrongBaiThi
    {
        [Key]
        public Guid MaCauHoiTrongBaiThi { get; set; } = Guid.NewGuid();

        [Required]
        public Guid MaBaiThi { get; set; }

        [Required]
        public Guid MaCauHoi { get; set; }

        public int ThuTu { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal Diem { get; set; } = 1;

        // Navigation properties
        [ForeignKey("MaBaiThi")]
        public virtual BaiThi BaiThi { get; set; } = null!;

        [ForeignKey("MaCauHoi")]
        public virtual CauHoi CauHoi { get; set; } = null!;
    }
}