using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizHub.Models.Entities
{
    /// <summary>
    /// Entity đại diện cho một lượt làm bài thi
    /// </summary>
    public class LuotLamBai
    {
        /// <summary>
        /// Mã định danh lượt làm bài (Primary Key)
        /// </summary>
        [Key]
        public Guid MaLuotLamBai { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Mã bài thi đang làm
        /// </summary>
        [Required]
        public Guid MaBaiThi { get; set; }

        /// <summary>
        /// ID người dùng (null nếu là khách)
        /// </summary>
        public string? NguoiDungId { get; set; }

        /// <summary>
        /// Tên người tham gia (cho khách)
        /// </summary>
        [MaxLength(200)]
        public string? TenNguoiThamGia { get; set; }

        /// <summary>
        /// Thời điểm bắt đầu làm bài
        /// </summary>
        public DateTime ThoiGianBatDau { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Thời điểm hết hạn (deadline)
        /// </summary>
        public DateTime? ThoiGianKetThuc { get; set; }

        /// <summary>
        /// Thời điểm nộp bài thực tế
        /// </summary>
        public DateTime? ThoiGianNopBai { get; set; }

        /// <summary>
        /// Thời gian làm bài thực tế (giây)
        /// </summary>
        public int? ThoiGianLamBaiThucTe { get; set; }

        /// <summary>
        /// Điểm số (0-100)
        /// </summary>
        public decimal? Diem { get; set; }

        /// <summary>
        /// Tổng số câu hỏi trong bài
        /// </summary>
        public int TongSoCauHoi { get; set; }

        /// <summary>
        /// Số câu trả lời đúng
        /// </summary>
        public int SoCauDung { get; set; } = 0;

        /// <summary>
        /// Số câu trả lời sai
        /// </summary>
        public int SoCauSai { get; set; } = 0;

        /// <summary>
        /// Số câu chưa làm
        /// </summary>
        public int SoCauChuaLam { get; set; } = 0;

        /// <summary>
        /// Trạng thái: DangLam, HoanThanh, HetGio, DaHuy
        /// </summary>
        [MaxLength(20)]
        public string TrangThai { get; set; } = "DangLam";

        /// <summary>
        /// Địa chỉ IP của người làm bài
        /// </summary>
        [MaxLength(50)]
        public string? DiaChiIP { get; set; }

        /// <summary>
        /// Thông tin trình duyệt (User Agent)
        /// </summary>
        [MaxLength(500)]
        public string? ThongTinTrinhDuyet { get; set; }

        /// <summary>
        /// Bài thi đang làm (Navigation Property)
        /// </summary>
        [ForeignKey("MaBaiThi")]
        public virtual BaiThi BaiThi { get; set; } = null!;

        /// <summary>
        /// Danh sách câu trả lời
        /// </summary>
        public virtual ICollection<CauTraLoi> CacCauTraLoi { get; set; } = new List<CauTraLoi>();
    }
}