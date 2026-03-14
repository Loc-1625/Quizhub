namespace QuizHub.Models.DTOs.LuotLamBai
{
    public class UserQuizStatsDto
    {
        public int TongLuotLam { get; set; }
        public decimal DiemTrungBinh { get; set; }
        public decimal DiemCaoNhat { get; set; }
        public decimal DiemThapNhat { get; set; }
        public int TyLeDat { get; set; } // Phần trăm
        public int TongCauDung { get; set; }
        public int TongCauSai { get; set; }
        public int TongThoiGianLam { get; set; } // Giây
    }
}
