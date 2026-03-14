using QuizHub.Models.DTOs.DanhMuc;

namespace QuizHub.Services.Interfaces
{
    /// <summary>
    /// Interface quản lý danh mục câu hỏi
    /// </summary>
    public interface IDanhMucService
    {
        /// <summary>
        /// Lấy tất cả danh mục
        /// </summary>
        /// <returns>Danh sách danh mục</returns>
        Task<IEnumerable<DanhMucDto>> GetAllAsync();

        /// <summary>
        /// Lấy danh mục theo ID
        /// </summary>
        /// <param name="id">Mã danh mục</param>
        /// <returns>Thông tin danh mục</returns>
        Task<DanhMucDto?> GetByIdAsync(Guid id);

        /// <summary>
        /// Tạo danh mục mới
        /// </summary>
        /// <param name="dto">Dữ liệu tạo mới</param>
        /// <returns>Danh mục đã tạo</returns>
        Task<DanhMucDto> CreateAsync(CreateDanhMucDto dto);

        /// <summary>
        /// Cập nhật danh mục
        /// </summary>
        /// <param name="id">Mã danh mục</param>
        /// <param name="dto">Dữ liệu cập nhật</param>
        /// <returns>Danh mục đã cập nhật</returns>
        Task<DanhMucDto?> UpdateAsync(Guid id, UpdateDanhMucDto dto);

        /// <summary>
        /// Xóa danh mục
        /// </summary>
        /// <param name="id">Mã danh mục</param>
        /// <returns>True nếu xóa thành công</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Kiểm tra danh mục có tồn tại không
        /// </summary>
        /// <param name="id">Mã danh mục</param>
        /// <returns>True nếu tồn tại</returns>
        Task<bool> ExistsAsync(Guid id);
    }
}