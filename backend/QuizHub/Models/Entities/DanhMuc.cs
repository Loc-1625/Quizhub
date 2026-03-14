using System.ComponentModel.DataAnnotations;

namespace QuizHub.Models.Entities
{
    /// <summary>
    /// Entity đại diện cho danh mục câu hỏi
    /// </summary>
    public class DanhMuc
    {
        /// <summary>
        /// Mã định danh danh mục (Primary Key)
        /// </summary>
        [Key]
        public Guid MaDanhMuc { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Tên danh mục
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string TenDanhMuc { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả danh mục
        /// </summary>
        [MaxLength(1000)]
        public string? MoTa { get; set; }

        /// <summary>
        /// Đường dẫn URL-friendly (slug)
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string DuongDan { get; set; } = string.Empty;

        /// <summary>
        /// Đường dẫn hình ảnh đại diện
        /// </summary>
        [MaxLength(500)]
        public string? HinhAnh { get; set; }

        /// <summary>
        /// Ngày tạo danh mục
        /// </summary>
        public DateTime NgayTao { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Ngày cập nhật gần nhất
        /// </summary>
        public DateTime NgayCapNhat { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Danh sách câu hỏi thuộc danh mục
        /// </summary>
        public virtual ICollection<CauHoi> CacCauHoi { get; set; } = new List<CauHoi>();
    }
}