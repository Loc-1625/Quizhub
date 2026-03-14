using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizHub.Models.Entities
{
    public class LuaChonDapAn
    {
        [Key]
        public Guid MaLuaChon { get; set; } = Guid.NewGuid();

        [Required]
        public Guid MaCauHoi { get; set; }

        [Required]
        [MaxLength(1000)]
        public string NoiDungDapAn { get; set; } = string.Empty;

        public bool LaDapAnDung { get; set; } = false;

        public int ThuTu { get; set; }

        // Navigation properties
        [ForeignKey("MaCauHoi")]
        public virtual CauHoi CauHoi { get; set; } = null!;
    }
}