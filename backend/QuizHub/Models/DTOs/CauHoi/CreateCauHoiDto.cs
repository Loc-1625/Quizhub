using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.DTOs.CauHoi
{
    public class CreateCauHoiDto
    {
        [Required(ErrorMessage = "Nội dung câu hỏi là bắt buộc")]
        public string NoiDungCauHoi { get; set; } = string.Empty;

        public string? GiaiThich { get; set; }

        public Guid? MaDanhMuc { get; set; }

        [RegularExpression("^(De|TrungBinh|Kho)$", ErrorMessage = "Mức độ phải là: De, TrungBinh hoặc Kho")]
        public string? MucDo { get; set; }

        [RegularExpression("^(MotDapAn|NhieuDapAn)$", ErrorMessage = "Loại câu hỏi phải là: MotDapAn hoặc NhieuDapAn")]
        public string LoaiCauHoi { get; set; } = "MotDapAn";

        public bool CongKhai { get; set; } = false;

        [Required(ErrorMessage = "Phải có ít nhất 2 đáp án")]
        [MinLength(2, ErrorMessage = "Phải có ít nhất 2 đáp án")]
        [MaxLength(4, ErrorMessage = "Tối đa 4 đáp án")]
        public List<LuaChonDapAnDto> CacLuaChon { get; set; } = new();
    }
}