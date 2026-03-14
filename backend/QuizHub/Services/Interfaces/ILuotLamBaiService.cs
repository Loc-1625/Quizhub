using QuizHub.Models.DTOs.LuotLamBai;

namespace QuizHub.Services.Interfaces
{
    /// <summary>
    /// Interface quản lý lượt làm bài
    /// </summary>
    public interface ILuotLamBaiService
    {
        /// <summary>
        /// Bắt đầu phíen làm bài thi
        /// </summary>
        /// <param name="dto">Dữ liệu bắt đầu quiz</param>
        /// <param name="userId">ID người dùng (tùy chọn cho khách)</param>
        /// <returns>Thông tin phíen làm bài</returns>
        Task<BaiThiSessionDto> StartQuizAsync(StartBaiThiDto dto, string? userId);

        /// <summary>
        /// Lấy thông tin phíen làm bài
        /// </summary>
        /// <param name="luotLamBaiId">Mã lượt làm bài</param>
        /// <param name="userId">ID người dùng (tùy chọn)</param>
        /// <returns>Thông tin phíen</returns>
        Task<BaiThiSessionDto?> GetSessionAsync(Guid luotLamBaiId, string? userId);

        /// <summary>
        /// Lưu câu trả lời
        /// </summary>
        /// <param name="luotLamBaiId">Mã lượt làm bài</param>
        /// <param name="dto">Dữ liệu câu trả lời</param>
        /// <param name="userId">ID người dùng (tùy chọn)</param>
        /// <returns>True nếu lưu thành công</returns>
        Task<bool> SaveAnswerAsync(Guid luotLamBaiId, SubmitCauTraLoiDto dto, string? userId);

        /// <summary>
        /// Nộp bài thi
        /// </summary>
        /// <param name="luotLamBaiId">Mã lượt làm bài</param>
        /// <param name="dto">Dữ liệu nộp bài</param>
        /// <param name="userId">ID người dùng (tùy chọn)</param>
        /// <returns>Kết quả bài thi</returns>
        Task<KetQuaBaiThiDto> SubmitQuizAsync(Guid luotLamBaiId, SubmitBaiThiDto dto, string? userId);

        /// <summary>
        /// Lấy kết quả bài thi
        /// </summary>
        /// <param name="luotLamBaiId">Mã lượt làm bài</param>
        /// <param name="userId">ID người dùng (tùy chọn)</param>
        /// <returns>Kết quả bài thi</returns>
        Task<KetQuaBaiThiDto?> GetResultAsync(Guid luotLamBaiId, string? userId);

        /// <summary>
        /// Lấy lịch sử làm bài của người dùng
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="pageNumber">Số trang</param>
        /// <param name="pageSize">Kích thước trang</param>
        /// <param name="sortBy">Trường sắp xếp (ngayLamBai, diem)</param>
        /// <param name="sortDesc">Sắp xếp giảm dần</param>
        /// <returns>Danh sách lượt làm bài và tổng số</returns>
        Task<(IEnumerable<LuotLamBaiDto> Items, int TotalCount)> GetHistoryAsync(string userId, int pageNumber, int pageSize, string? sortBy = "ngayLamBai", bool sortDesc = true);

        /// <summary>
        /// Tự động nộp các bài thi đã hết thời gian
        /// </summary>
        /// <returns>True nếu có bài được nộp</returns>
        Task<bool> AutoSubmitExpiredQuizzesAsync();

        /// <summary>
        /// Lấy thống kê làm bài của người dùng
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Thống kê làm bài</returns>
        Task<UserQuizStatsDto> GetUserStatsAsync(string userId);
    }
}