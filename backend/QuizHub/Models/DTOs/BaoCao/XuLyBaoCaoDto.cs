using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTOs.BaoCao
{
    public class XuLyBaoCaoDto
    {
        [Required(ErrorMessage = "Tr?ng thái là b?t bu?c")]
        [MaxLength(20)]
      public string TrangThai { get; set; } = string.Empty; // "DaXuLy", "TuChoi"

        [Required(ErrorMessage = "K?t qu? x? lý là b?t bu?c")]
     [MaxLength(1000, ErrorMessage = "K?t qu? x? lý không ???c v??t quá 1000 ký t?")]
   public string KetQuaXuLy { get; set; } = string.Empty;
    }
}
