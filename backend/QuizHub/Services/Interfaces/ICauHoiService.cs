using QuizHub.Models.DTOs.CauHoi;

namespace QuizHub.Services.Interfaces
{
    /// <summary>
    /// Interface quản lý câu hỏi
    /// </summary>
    public interface ICauHoiService
    {
        /// <summary>
        /// Lấy danh sách câu hỏi có phân trang và lọc
        /// </summary>
        /// <param name="filter">Điều kiện lọc</param>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Danh sách câu hỏi và tổng số</returns>
        Task<(IEnumerable<CauHoiDto> Items, int TotalCount)> GetAllAsync(CauHoiFilterDto filter, string userId);

        /// <summary>
        /// Lấy câu hỏi theo ID
        /// </summary>
        /// <param name="id">Mã câu hỏi</param>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Thông tin câu hỏi</returns>
        Task<CauHoiDto?> GetByIdAsync(Guid id, string userId);

        /// <summary>
        /// Tạo câu hỏi mới
        /// </summary>
        /// <param name="dto">Dữ liệu tạo mới</param>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Câu hỏi đã tạo</returns>
        Task<CauHoiDto> CreateAsync(CreateCauHoiDto dto, string userId);

        /// <summary>
        /// Cập nhật câu hỏi
        /// </summary>
        /// <param name="id">Mã câu hỏi</param>
        /// <param name="dto">Dữ liệu cập nhật</param>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Câu hỏi đã cập nhật</returns>
        Task<CauHoiDto?> UpdateAsync(Guid id, UpdateCauHoiDto dto, string userId);

        /// <summary>
        /// Xóa câu hỏi
        /// </summary>
        /// <param name="id">Mã câu hỏi</param>
        /// <param name="userId">ID người dùng</param>
        /// <returns>True nếu xóa thành công</returns>
        Task<bool> DeleteAsync(Guid id, string userId);

        /// <summary>
        /// Đếm số câu hỏi của người dùng
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Số lượng câu hỏi</returns>
        Task<int> CountByUserAsync(string userId);

        /// <summary>
        /// Đếm số câu hỏi trong danh mục
        /// </summary>
        /// <param name="danhMucId">Mã danh mục</param>
        /// <returns>Số lượng câu hỏi</returns>
        Task<int> CountByDanhMucAsync(Guid danhMucId);

        /// <summary>
        /// Lấy thống kê câu hỏi theo mức độ
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Thống kê số lượng theo mức độ</returns>
        Task<object> GetStatsAsync(string userId);
    }
}