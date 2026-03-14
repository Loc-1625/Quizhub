using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizHub.Models.Entities
{
    /// <summary>
    /// Entity đại diện cho câu hỏi trong ngân hàng câu hỏi
    /// </summary>
    public class CauHoi
    {
        /// <summary>
        /// Mã định danh câu hỏi (Primary Key)
        /// </summary>
        [Key]
        public Guid MaCauHoi { get; set; } = Guid.NewGuid();

        /// <summary>
        /// ID người tạo câu hỏi
        /// </summary>
        [Required]
        public string NguoiTaoId { get; set; } = string.Empty;

        /// <summary>
        /// Mã danh mục chứa câu hỏi
        /// </summary>
        public Guid? MaDanhMuc { get; set; }

        /// <summary>
        /// Nội dung câu hỏi (hỗ trợ HTML/Markdown)
        /// </summary>
        [Required]
        public string NoiDungCauHoi { get; set; } = string.Empty;

        /// <summary>
        /// Giải thích đáp án đúng
        /// </summary>
        public string? GiaiThich { get; set; }

        /// <summary>
        /// Loại câu hỏi: MotDapAn, NhieuDapAn
        /// </summary>
        [MaxLength(50)]
        public string LoaiCauHoi { get; set; } = "MotDapAn";

        /// <summary>
        /// Mức độ khó: De, TrungBinh, Kho
        /// </summary>
        [MaxLength(20)]
        public string? MucDo { get; set; }

        /// <summary>
        /// Cho phép công khai câu hỏi
        /// </summary>
        public bool CongKhai { get; set; } = false;

        /// <summary>
        /// Nguồn nhập câu hỏi (AI, Manual, Import)
        /// </summary>
        [MaxLength(50)]
        public string? NguonNhap { get; set; }

        /// <summary>
        /// Đánh dấu đã xóa (soft delete)
        /// </summary>
        public bool DaXoa { get; set; } = false;

        /// <summary>
        /// Ngày tạo câu hỏi
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
        /// Danh mục chứa câu hỏi (Navigation Property)
        /// </summary>
        [ForeignKey("MaDanhMuc")]
        public virtual DanhMuc? DanhMuc { get; set; }

        /// <summary>
        /// Danh sách lựa chọn đáp án
        /// </summary>
        public virtual ICollection<LuaChonDapAn> CacLuaChon { get; set; } = new List<LuaChonDapAn>();

        /// <summary>
        /// Danh sách bài thi chứa câu hỏi này
        /// </summary>
        public virtual ICollection<CauHoiTrongBaiThi> CacBaiThiChuaCauHoiNay { get; set; } = new List<CauHoiTrongBaiThi>();
    }
}