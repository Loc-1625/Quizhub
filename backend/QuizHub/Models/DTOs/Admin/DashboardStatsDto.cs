namespace QuizHub.Models.DTOs.Admin
{
    public class DashboardStatsDto
{
     public int TongNguoiDung { get; set; }
        public int NguoiDungMoiHomNay { get; set; }
        public int NguoiDungMoi7Ngay { get; set; }
        public int NguoiDungMoi30Ngay { get; set; }
    public int TongBaiThi { get; set; }
        public int BaiThiMoiHomNay { get; set; }
        public int BaiThiMoi7Ngay { get; set; }
        public int BaiThiMoi30Ngay { get; set; }
        public int TongCauHoi { get; set; }
        public int CauHoiMoiHomNay { get; set; }
        public int CauHoiMoi7Ngay { get; set; }
        public int CauHoiMoi30Ngay { get; set; }
        public int TongLuotLamBai { get; set; }
        public int LuotLamBaiHomNay { get; set; }
        public int LuotLamBai7Ngay { get; set; }
        public int LuotLamBai30Ngay { get; set; }
        public int BaoCaoChoDuyet { get; set; }
        public int BaoCaoDangXuLy { get; set; }
        
        // Top Statistics
        public List<TopBaiThiDto>? TopBaiThi { get; set; }
        public List<TopNguoiTaoDto>? TopNguoiTao { get; set; }
    }

    public class TopBaiThiDto
 {
        public Guid MaBaiThi { get; set; }
        public string TieuDe { get; set; } = string.Empty;
    public string NguoiTao { get; set; } = string.Empty;
        public int TongLuotLamBai { get; set; }
        public decimal? DiemTrungBinh { get; set; }
    }

    public class TopNguoiTaoDto
    {
        public string NguoiDungId { get; set; } = string.Empty;
      public string HoTen { get; set; } = string.Empty;
        public int SoBaiThiTao { get; set; }
        public int SoCauHoiTao { get; set; }
  public int TongLuotLamBai { get; set; }
    }
}
