namespace QuizHub.Models.DTOs.Admin
{
  public class ContentManagementDto
  {
    public Guid Id { get; set; }
    public string LoaiNoiDung { get; set; } = string.Empty; // "BaiThi" or "CauHoi"
    public string TieuDe { get; set; } = string.Empty;
    public string NguoiTaoId { get; set; } = string.Empty;
    public string TenNguoiTao { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public bool DaXoa { get; set; }
    public decimal? XepHangTrungBinh { get; set; }

    // B�i Thi specific
    public string? CheDoCongKhai { get; set; }
    public int? TongLuotLamBai { get; set; }

    // C�u H?i specific
    public string? DanhMuc { get; set; }
    public bool? CongKhai { get; set; }
  }
}
