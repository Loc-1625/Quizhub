using QuizHub.Models.DTOs.BaoCao;

namespace QuizHub.Services.Interfaces
{
    /// <summary>
    /// Interface quản lý báo cáo vi phạm
    /// </summary>
    public interface IBaoCaoService
    {
        /// <summary>
        /// Tạo báo cáo mới
        /// </summary>
        /// <param name="dto">Dữ liệu tạo báo cáo</param>
        /// <param name="nguoiBaoCaoId">ID người báo cáo</param>
        /// <returns>Báo cáo đã tạo</returns>
        Task<BaoCaoDto> CreateAsync(CreateBaoCaoDto dto, string nguoiBaoCaoId);

        /// <summary>
        /// Lấy danh sách tất cả báo cáo (Admin)
        /// </summary>
        /// <param name="trangThai">Trạng thái lọc (tùy chọn)</param>
        /// <param name="loaiDoiTuong">Loại đối tượng lọc (tùy chọn)</param>
        /// <param name="pageNumber">Số trang</param>
        /// <param name="pageSize">Kích thước trang</param>
        /// <returns>Danh sách báo cáo và tổng số</returns>
        Task<(IEnumerable<BaoCaoDto> Items, int TotalCount)> GetAllAsync(
            string? trangThai = null,
            string? loaiDoiTuong = null,
            int pageNumber = 1,
            int pageSize = 20);

        /// <summary>
        /// Lấy chi tiết một báo cáo
        /// </summary>
        /// <param name="id">Mã báo cáo</param>
        /// <returns>Thông tin báo cáo</returns>
        Task<BaoCaoDto?> GetByIdAsync(Guid id);

        /// <summary>
        /// Xử lý báo cáo (Admin)
        /// </summary>
        /// <param name="id">Mã báo cáo</param>
        /// <param name="dto">Dữ liệu xử lý</param>
        /// <param name="nguoiXuLyId">ID người xử lý</param>
        /// <returns>Báo cáo đã xử lý</returns>
        Task<BaoCaoDto?> ResolveAsync(Guid id, XuLyBaoCaoDto dto, string nguoiXuLyId);

        /// <summary>
        /// Xóa báo cáo
        /// </summary>
        /// <param name="id">Mã báo cáo</param>
        /// <returns>True nếu xóa thành công</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Lấy danh sách báo cáo của user
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="pageNumber">Số trang</param>
        /// <param name="pageSize">Kích thước trang</param>
        /// <returns>Danh sách báo cáo và tổng số</returns>
        Task<(IEnumerable<BaoCaoDto> Items, int TotalCount)> GetByUserAsync(
            string userId,
            int pageNumber = 1,
            int pageSize = 20);
    }
}
