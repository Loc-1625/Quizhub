namespace QuizHub.Models.DTOs.CauHoi
{
    public class CauHoiDto
    {
        public Guid MaCauHoi { get; set; }
        public string NoiDungCauHoi { get; set; } = string.Empty;
        public string? GiaiThich { get; set; }
        public Guid? MaDanhMuc { get; set; }
        public string? TenDanhMuc { get; set; }
        public string? MucDo { get; set; }
        public string LoaiCauHoi { get; set; } = "MotDapAn";
        public bool CongKhai { get; set; }
        public string? NguonNhap { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime NgayCapNhat { get; set; }
        public List<LuaChonDapAnDto> CacLuaChon { get; set; } = new();
    }
}