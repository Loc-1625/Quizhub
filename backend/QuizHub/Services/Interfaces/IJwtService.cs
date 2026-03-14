using QuizHub.Models.Entities;

namespace QuizHub.Services.Interfaces
{
    /// <summary>
    /// Interface cho JWT Service
    /// Cung cấp các phương thức liên quan đến JWT token
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Tạo JWT token cho người dùng
        /// </summary>
        /// <param name="user">Thông tin người dùng</param>
        /// <returns>JWT token string</returns>
        Task<string> GenerateJwtToken(NguoiDung user);
    }
}