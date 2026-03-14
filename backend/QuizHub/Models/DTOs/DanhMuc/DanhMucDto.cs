namespace QuizHub.Models.DTOs.DanhMuc
{
    public class DanhMucDto
    {
        public Guid MaDanhMuc { get; set; }
        public string TenDanhMuc { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public string DuongDan { get; set; } = string.Empty;
        public string? HinhAnh { get; set; }
        public DateTime NgayTao { get; set; }
        public int SoCauHoi { get; set; } // Số câu hỏi thuộc danh mục này
    }
}