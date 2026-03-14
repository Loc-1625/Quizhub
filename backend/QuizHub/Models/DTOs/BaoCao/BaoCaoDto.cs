namespace QuizHub.Models.DTOs.BaoCao
{
    public class BaoCaoDto
    {
        public Guid MaBaoCao { get; set; }
     public string NguoiBaoCaoId { get; set; } = string.Empty;
        public string TenNguoiBaoCao { get; set; } = string.Empty;
    public string LoaiDoiTuong { get; set; } = string.Empty;
        public Guid MaDoiTuong { get; set; }
    public string TenDoiTuong { get; set; } = string.Empty; // Tên bài thi ho?c n?i dung câu h?i
   public string LyDo { get; set; } = string.Empty;
        public string? MoTa { get; set; }
   public string TrangThai { get; set; } = string.Empty; // "ChoDuyet", "DangXuLy", "DaXuLy", "TuChoi"
        public string? NguoiXuLyId { get; set; }
        public string? TenNguoiXuLy { get; set; }
        public DateTime? ThoiGianXuLy { get; set; }
 public string? KetQuaXuLy { get; set; }
        public DateTime NgayTao { get; set; }
    }
}
