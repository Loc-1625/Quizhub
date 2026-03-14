using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTOs.LuotLamBai
{
    public class SubmitBaiThiDto
    {
        [Required]
        public List<SubmitCauTraLoiDto> CacCauTraLoi { get; set; } = new();
    }
}