using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTOs.BaoCao
{
    public class CreateBaoCaoDto
    {
  [Required(ErrorMessage = "Lo?i ??i t??ng là b?t bu?c")]
        [MaxLength(20, ErrorMessage = "Lo?i ??i t??ng không ???c v??t quá 20 ký t?")]
        public string LoaiDoiTuong { get; set; } = string.Empty; // "BaiThi", "CauHoi"

        [Required(ErrorMessage = "Mã ??i t??ng là b?t bu?c")]
        public Guid MaDoiTuong { get; set; }

        [Required(ErrorMessage = "Lý do báo cáo là b?t bu?c")]
        [MaxLength(50, ErrorMessage = "Lý do không ???c v??t quá 50 ký t?")]
        public string LyDo { get; set; } = string.Empty; // "NoiDungKhongPhuHop", "SaiSot", "Spam", etc.

        [MaxLength(1000, ErrorMessage = "Mô t? không ???c v??t quá 1000 ký t?")]
 public string? MoTa { get; set; }
    }
}
