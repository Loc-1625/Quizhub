using Microsoft.AspNetCore.Identity;

namespace QuizHub.Models.Entities
{
    /// <summary>
    /// Entity đại diện cho người dùng trong hệ thống
    /// Kế thừa từ IdentityUser để sử dụng ASP.NET Identity
    /// </summary>
    public class NguoiDung : IdentityUser
    {
        /// <summary>
        /// Họ tên đầy đủ của người dùng
        /// </summary>
        public string? HoTen { get; set; }

        /// <summary>
        /// Đường dẫn ảnh đại diện
        /// </summary>
        public string? AnhDaiDien { get; set; }
        
        /// <summary>
        /// Tiểu sử/giới thiệu của người dùng
        /// </summary>
        public string? TieuSu { get; set; }

        /// <summary>
        /// Địa chỉ của người dùng
        /// </summary>
        public string? DiaChi { get; set; }

        /// <summary>
        /// Cho phép hiển thị profile công khai
        /// </summary>
        public bool ProfileCongKhai { get; set; } = true;
        
        /// <summary>
        /// Ngày tạo tài khoản
        /// </summary>
        public DateTime NgayTao { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Ngày cập nhật thông tin gần nhất
        /// </summary>
        public DateTime? NgayCapNhat { get; set; }

        /// <summary>
        /// ID người thực hiện cập nhật
        /// </summary>
        public string? CapNhatBoi { get; set; }

        /// <summary>
        /// Thời điểm đăng nhập gần nhất
        /// </summary>
        public DateTime? LanDangNhapCuoi { get; set; }

        /// <summary>
        /// Trạng thái kích hoạt tài khoản
        /// </summary>
        public bool TrangThaiKichHoat { get; set; } = true;

        /// <summary>
        /// Danh sách câu hỏi do người dùng tạo
        /// </summary>
        public virtual ICollection<CauHoi> CacCauHoiDaTao { get; set; } = new List<CauHoi>();

        /// <summary>
        /// Danh sách bài thi do người dùng tạo
        /// </summary>
        public virtual ICollection<BaiThi> CacBaiThiDaTao { get; set; } = new List<BaiThi>();

        /// <summary>
        /// Danh sách lượt làm bài của người dùng
        /// </summary>
        public virtual ICollection<LuotLamBai> CacLuotLamBai { get; set; } = new List<LuotLamBai>();
    }
}