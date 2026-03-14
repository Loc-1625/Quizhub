using QuizHub.Models.DTOs.BaiThi;

namespace QuizHub.Services.Interfaces
{
    /// <summary>
    /// Interface quản lý bài thi
    /// </summary>
    public interface IBaiThiService
    {
        /// <summary>
        /// Lấy danh sách bài thi có phân trang và lọc
        /// </summary>
        /// <param name="filter">Điều kiện lọc</param>
        /// <param name="userId">ID người dùng (tùy chọn)</param>
        /// <returns>Danh sách bài thi và tổng số</returns>
        Task<(IEnumerable<BaiThiSummaryDto> Items, int TotalCount)> GetAllAsync(BaiThiFilterDto filter, string? userId);

        /// <summary>
        /// Lấy bài thi theo ID
        /// </summary>
        /// <param name="id">Mã bài thi</param>
        /// <param name="userId">ID người dùng (tùy chọn)</param>
        /// <returns>Thông tin bài thi</returns>
        Task<BaiThiDto?> GetByIdAsync(Guid id, string? userId);

        /// <summary>
        /// Lấy bài thi theo mã truy cập
        /// </summary>
        /// <param name="accessCode">Mã truy cập</param>
        /// <param name="userId">ID người dùng (tùy chọn)</param>
        /// <returns>Thông tin bài thi</returns>
        Task<BaiThiDto?> GetByAccessCodeAsync(string accessCode, string? userId);

        /// <summary>
        /// Tạo bài thi mới
        /// </summary>
        /// <param name="dto">Dữ liệu tạo mới</param>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Bài thi đã tạo</returns>
        Task<BaiThiDto> CreateAsync(CreateBaiThiDto dto, string userId);

        /// <summary>
        /// Cập nhật bài thi
        /// </summary>
        /// <param name="id">Mã bài thi</param>
        /// <param name="dto">Dữ liệu cập nhật</param>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Bài thi đã cập nhật</returns>
        Task<BaiThiDto?> UpdateAsync(Guid id, UpdateBaiThiDto dto, string userId);

        /// <summary>
        /// Xóa bài thi
        /// </summary>
        /// <param name="id">Mã bài thi</param>
        /// <param name="userId">ID người dùng</param>
        /// <returns>True nếu xóa thành công</returns>
        Task<bool> DeleteAsync(Guid id, string userId);

        /// <summary>
        /// Xuất bản bài thi
        /// </summary>
        /// <param name="id">Mã bài thi</param>
        /// <param name="userId">ID người dùng</param>
        /// <returns>True nếu xuất bản thành công</returns>
        Task<bool> PublishAsync(Guid id, string userId);

        /// <summary>
        /// Đóng bài thi
        /// </summary>
        /// <param name="id">Mã bài thi</param>
        /// <param name="userId">ID người dùng</param>
        /// <returns>True nếu đóng thành công</returns>
        Task<bool> CloseAsync(Guid id, string userId);

        /// <summary>
        /// Chuyển đổi chế độ công khai/riêng tư
        /// </summary>
        /// <param name="id">Mã bài thi</param>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Chế độ mới (CongKhai/RiengTu) hoặc null nếu không tìm thấy</returns>
        Task<string?> ToggleVisibilityAsync(Guid id, string userId);

        /// <summary>
        /// Lấy thống kê bài thi
        /// </summary>
        /// <param name="id">Mã bài thi</param>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Thống kê bài thi</returns>
        Task<BaiThiStatisticsDto?> GetStatisticsAsync(Guid id, string userId);

        /// <summary>
        /// Đếm số bài thi của người dùng
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Số lượng bài thi</returns>
        Task<int> CountByUserAsync(string userId);

        /// <summary>
        /// Xác thực mật khẩu bài thi (cho chế độ CoMatKhau)
        /// </summary>
        /// <param name="id">Mã bài thi</param>
        /// <param name="password">Mật khẩu cần xác thực</param>
        /// <returns>True nếu mật khẩu đúng</returns>
        Task<bool> VerifyPasswordAsync(Guid id, string password);

        /// <summary>
        /// Lấy danh sách người đã làm bài thi của một bài thi cụ thể
        /// </summary>
        /// <param name="baiThiId">Mã bài thi</param>
        /// <param name="filter">Điều kiện lọc</param>
        /// <param name="userId">ID người dùng (chủ bài thi)</param>
        /// <returns>Danh sách người làm bài và tổng số</returns>
        Task<(IEnumerable<NguoiLamBaiDto> Items, int TotalCount, ThongKeNguoiLamBaiDto ThongKe)> GetNguoiLamBaiAsync(Guid baiThiId, NguoiLamBaiFilterDto filter, string userId);

        /// <summary>
        /// Lấy danh sách tất cả người đã làm bài thi của tất cả bài thi do user tạo
        /// </summary>
        /// <param name="filter">Điều kiện lọc</param>
        /// <param name="userId">ID người dùng (chủ bài thi)</param>
        /// <returns>Danh sách người làm bài và tổng số</returns>
        Task<(IEnumerable<NguoiLamBaiDto> Items, int TotalCount, ThongKeNguoiLamBaiDto ThongKe)> GetAllNguoiLamBaiAsync(NguoiLamBaiFilterDto filter, string userId);

        /// <summary>
        /// Lấy chi tiết câu hỏi và đáp án của bài thi (chỉ dành cho chủ bài thi)
        /// </summary>
        /// <param name="id">Mã bài thi</param>
        /// <param name="userId">ID người dùng (chủ bài thi)</param>
        /// <returns>Danh sách câu hỏi chi tiết</returns>
        Task<List<CauHoiChiTietDto>?> GetQuestionsDetailAsync(Guid id, string userId);
    }
}