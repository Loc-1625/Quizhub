using QuizHub.Models.DTOs.AI;

namespace QuizHub.Services.Interfaces
{
    /// <summary>
    /// Interface cho dịch vụ tích hợp Gemini AI
    /// </summary>
    public interface IGeminiAIService
    {
        /// <summary>
        /// Upload file và trích xuất câu hỏi
        /// </summary>
        /// <param name="file">File cần trích xuất</param>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Kết quả trích xuất</returns>
        Task<ExtractionResultDto> ExtractQuestionsFromFileAsync(IFormFile file, string userId);

        /// <summary>
        /// Lấy kết quả trích xuất theo phiên
        /// </summary>
        /// <param name="maPhien">Mã phiên</param>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Kết quả trích xuất</returns>
        Task<ExtractionResultDto?> GetExtractionResultAsync(Guid maPhien, string userId);

        /// <summary>
        /// Import các câu hỏi đã chọn vào database
        /// </summary>
        /// <param name="dto">Dữ liệu import</param>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Số câu hỏi đã import</returns>
        Task<int> ImportQuestionsAsync(ImportQuestionDto dto, string userId);

        /// <summary>
        /// Lấy lịch sử các phiên import của user
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="pageNumber">Số trang</param>
        /// <param name="pageSize">Kích thước trang</param>
        /// <returns>Danh sách phiên và tổng số</returns>
        Task<(IEnumerable<PhienNhapDuLieuDto> Items, int TotalCount)> GetImportHistoryAsync(
            string userId,
            int pageNumber = 1,
            int pageSize = 20);

        /// <summary>
        /// Xóa phiên import
        /// </summary>
        /// <param name="maPhien">Mã phiên</param>
        /// <param name="userId">ID người dùng</param>
        /// <returns>True nếu xóa thành công</returns>
        Task<bool> DeleteSessionAsync(Guid maPhien, string userId);

        /// <summary>
        /// Tạo câu hỏi từ mô tả chủ đề bằng AI
        /// </summary>
        /// <param name="topic">Chủ đề/mô tả</param>
        /// <param name="numberOfQuestions">Số lượng câu hỏi cần tạo</param>
        /// <param name="difficulty">Độ khó (De/TrungBinh/Kho)</param>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Danh sách câu hỏi được tạo</returns>
        Task<GenerateResultDto> GenerateQuestionsFromTopicAsync(string topic, int numberOfQuestions, string difficulty, string userId);
    }
}
