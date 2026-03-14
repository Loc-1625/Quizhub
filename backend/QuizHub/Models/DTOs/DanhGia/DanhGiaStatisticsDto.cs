namespace QuizHub.Models.DTOs.DanhGia
{
    public class DanhGiaStatisticsDto
    {
        public Guid MaBaiThi { get; set; }
        public int TongDanhGia { get; set; }
        public double XepHangTrungBinh { get; set; }
        public int Sao5 { get; set; }
        public int Sao4 { get; set; }
        public int Sao3 { get; set; }
        public int Sao2 { get; set; }
        public int Sao1 { get; set; }
    }
}