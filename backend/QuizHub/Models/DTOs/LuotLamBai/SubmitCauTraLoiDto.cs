using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTOs.LuotLamBai
{
    public class SubmitCauTraLoiDto
    {
        [Required]
        public Guid MaCauHoi { get; set; }

        public Guid? MaLuaChonDaChon { get; set; } // NULL nếu bỏ qua câu

        public int? ThoiGianSuDung { get; set; } // Giây
    }
}