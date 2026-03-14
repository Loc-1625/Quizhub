using QuizHub.Models.DTOs.DanhGia;

namespace QuizHub.Services.Interfaces
{
    /// <summary>
    /// Interface quản lý đánh giá bài thi
    /// </summary>
    public interface IDanhGiaService
    {
        /// <summary>
        /// Lấy danh sách đánh giá của bài thi
        /// </summary>
        /// <param name="baiThiId">Mã bài thi</param>
        /// <param name="pageNumber">Số trang</param>
        /// <param name="pageSize">Kích thước trang</param>
        /// <returns>Danh sách đánh giá và tổng số</returns>
        Task<(IEnumerable<DanhGiaDto> Items, int TotalCount)> GetByBaiThiAsync(Guid baiThiId, int pageNumber, int pageSize);

        /// <summary>
        /// Lấy đánh giá theo ID
        /// </summary>
        /// <param name="id">Mã đánh giá</param>
        /// <returns>Thông tin đánh giá</returns>
        Task<DanhGiaDto?> GetByIdAsync(Guid id);

        /// <summary>
        /// Lấy đánh giá của user cho bài thi
        /// </summary>
        /// <param name="baiThiId">Mã bài thi</param>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Thông tin đánh giá</returns>
        Task<DanhGiaDto?> GetUserReviewAsync(Guid baiThiId, string userId);

        /// <summary>
        /// Tạo đánh giá mới
        /// </summary>
        /// <param name="dto">Dữ liệu tạo đánh giá</param>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Đánh giá đã tạo</returns>
        Task<DanhGiaDto> CreateAsync(CreateDanhGiaDto dto, string userId);

        /// <summary>
        /// Cập nhật đánh giá
        /// </summary>
        /// <param name="id">Mã đánh giá</param>
        /// <param name="dto">Dữ liệu cập nhật</param>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Đánh giá đã cập nhật</returns>
        Task<DanhGiaDto?> UpdateAsync(Guid id, UpdateDanhGiaDto dto, string userId);

        /// <summary>
        /// Xóa đánh giá
        /// </summary>
        /// <param name="id">Mã đánh giá</param>
        /// <param name="userId">ID người dùng</param>
        /// <returns>True nếu xóa thành công</returns>
        Task<bool> DeleteAsync(Guid id, string userId);

        /// <summary>
        /// Lấy thống kê đánh giá của bài thi
        /// </summary>
        /// <param name="baiThiId">Mã bài thi</param>
        /// <returns>Thống kê đánh giá</returns>
        Task<DanhGiaStatisticsDto?> GetStatisticsAsync(Guid baiThiId);
    }
}