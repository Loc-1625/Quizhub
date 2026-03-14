using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizHub.Models.Entities
{
    /// <summary>
    /// Entity đại diện cho bài thi/quiz
    /// </summary>
    public class BaiThi
    {
        /// <summary>
        /// Mã định danh bài thi (Primary Key)
        /// </summary>
        [Key]
        public Guid MaBaiThi { get; set; } = Guid.NewGuid();

        /// <summary>
        /// ID người tạo bài thi
        /// </summary>
        [Required]
        public string NguoiTaoId { get; set; } = string.Empty;

        /// <summary>
        /// Mã danh mục bài thi (Foreign Key)
        /// </summary>
        public Guid? MaDanhMuc { get; set; }

        /// <summary>
        /// Tiêu đề bài thi
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string TieuDe { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả chi tiết về bài thi
        /// </summary>
        public string? MoTa { get; set; }

        /// <summary>
        /// Đường dẫn ảnh bìa bài thi
        /// </summary>
        [MaxLength(500)]
        public string? AnhBia { get; set; }

        /// <summary>
        /// Thời gian làm bài (phút)
        /// </summary>
        public int ThoiGianLamBai { get; set; }

        /// <summary>
        /// Điểm đạt tối thiểu (%)
        /// </summary>
        public int? DiemDat { get; set; }

        /// <summary>
        /// Chế độ công khai: CongKhai, RiengTu, CoMatKhau
        /// </summary>
        [MaxLength(20)]
        public string CheDoCongKhai { get; set; } = "RiengTu";

        /// <summary>
        /// Trạng thái: Nhap, XuatBan, DaDong
        /// </summary>
        [MaxLength(20)]
        public string TrangThai { get; set; } = "Nhap";

        /// <summary>
        /// Cho phép hiển thị đáp án sau khi nộp bài
        /// </summary>
        public bool HienThiDapAnSauKhiNop { get; set; } = true;

        /// <summary>
        /// Cho phép xem lại bài làm
        /// </summary>
        public bool ChoPhepXemLai { get; set; } = true;

        /// <summary>
        /// Mã truy cập định danh (dùng để chia sẻ)
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string MaTruyCapDinhDanh { get; set; } = string.Empty;

        /// <summary>
        /// Mật khẩu bảo vệ (nếu CheDoCongKhai = CoMatKhau)
        /// </summary>
        [MaxLength(500)]
        public string? MatKhau { get; set; }

        /// <summary>
        /// Số lượt xem
        /// </summary>
        public int LuotXem { get; set; } = 0;

        /// <summary>
        /// Tổng số lượt làm bài
        /// </summary>
        public int TongLuotLamBai { get; set; } = 0;

        /// <summary>
        /// Điểm trung bình của tất cả lượt làm bài
        /// </summary>
        public decimal? DiemTrungBinh { get; set; }

        /// <summary>
        /// Xếp hạng trung bình
        /// </summary>
        public decimal? XepHangTrungBinh { get; set; }

        /// <summary>
        /// Đánh dấu đã xóa (soft delete)
        /// </summary>
        public bool DaXoa { get; set; } = false;

        /// <summary>
        /// Ngày tạo bài thi
        /// </summary>
        public DateTime NgayTao { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Ngày cập nhật gần nhất
        /// </summary>
        public DateTime NgayCapNhat { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// ID người cập nhật gần nhất
        /// </summary>
        public string? NguoiCapNhatId { get; set; }

        /// <summary>
        /// Danh sách câu hỏi trong bài thi
        /// </summary>
        public virtual ICollection<CauHoiTrongBaiThi> CacCauHoi { get; set; } = new List<CauHoiTrongBaiThi>();

        /// <summary>
        /// Danh sách lượt làm bài
        /// </summary>
        public virtual ICollection<LuotLamBai> CacLuotLamBai { get; set; } = new List<LuotLamBai>();

        /// <summary>
        /// Danh sách đánh giá
        /// </summary>
        public virtual ICollection<DanhGia> CacDanhGia { get; set; } = new List<DanhGia>();

        /// <summary>
        /// Navigation property cho danh mục
        /// </summary>
        [ForeignKey("MaDanhMuc")]
        public virtual DanhMuc? DanhMuc { get; set; }
    }
}