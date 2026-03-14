using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizHub.Models.Entities
{
    public class CauTraLoi
    {
        [Key]
        public Guid MaCauTraLoi { get; set; } = Guid.NewGuid();

        [Required]
        public Guid MaLuotLamBai { get; set; }

        [Required]
        public Guid MaCauHoi { get; set; }

        public Guid? MaLuaChonDaChon { get; set; }

        public bool LaDapAnDung { get; set; } = false;

        public DateTime ThoiGianTraLoi { get; set; } = DateTime.UtcNow;

        public int? ThoiGianSuDung { get; set; }

        // Navigation properties
        [ForeignKey("MaLuotLamBai")]
        public virtual LuotLamBai LuotLamBai { get; set; } = null!;

        [ForeignKey("MaCauHoi")]
        public virtual CauHoi CauHoi { get; set; } = null!;

        [ForeignKey("MaLuaChonDaChon")]
        public virtual LuaChonDapAn? LuaChonDaChon { get; set; }
    }
}