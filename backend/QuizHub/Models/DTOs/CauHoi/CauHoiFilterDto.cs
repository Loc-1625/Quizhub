namespace QuizHub.Models.DTOs.CauHoi
{
    public class CauHoiFilterDto
    {
        public Guid? MaDanhMuc { get; set; }
        public string? MucDo { get; set; }
        public string? LoaiCauHoi { get; set; } // MotDapAn, NhieuDapAn, DienKhuyet
        public string? TimKiem { get; set; } // Tìm theo nội dung câu hỏi
        public bool? ChiLayCongKhai { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}