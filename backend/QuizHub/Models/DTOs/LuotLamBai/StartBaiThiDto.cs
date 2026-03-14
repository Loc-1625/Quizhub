using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTOs.LuotLamBai
{
    public class StartBaiThiDto
    {
        [Required(ErrorMessage = "Mã bài thi là bắt buộc")]
        public Guid MaBaiThi { get; set; }

        public string? TenNguoiThamGia { get; set; } // Cho trường hợp anonymous

        public string? MatKhau { get; set; } // Nếu bài thi có mật khẩu
    }
}