using QuizHub.Models.DTOs.Auth;

namespace QuizHub.Services.Interfaces
{
    /// <summary>
    /// Interface quản lý profile người dùng
    /// </summary>
    public interface IUserProfileService
    {
        /// <summary>
        /// Lấy thông tin profile đầy đủ của user hiện tại
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Thông tin profile đầy đủ</returns>
        Task<UserProfileDto?> GetMyProfileAsync(string userId);

        /// <summary>
        /// Lấy thông tin công khai của một user (cho người khác xem)
        /// </summary>
        /// <param name="userId">ID người dùng cần xem</param>
        /// <returns>Thông tin công khai</returns>
        Task<PublicProfileDto?> GetPublicProfileAsync(string userId);

        /// <summary>
        /// Cập nhật thông tin profile
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="dto">Dữ liệu cập nhật</param>
        /// <returns>Thông tin profile đã cập nhật</returns>
        Task<UserProfileDto?> UpdateProfileAsync(string userId, UpdateProfileDto dto);

        /// <summary>
        /// Upload ảnh đại diện
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="file">File ảnh</param>
        /// <returns>Đường dẫn ảnh đại diện mới</returns>
        Task<string> UploadAvatarAsync(string userId, IFormFile file);

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="dto">Dữ liệu đổi mật khẩu</param>
        /// <returns>True nếu thành công</returns>
        Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto dto);

        /// <summary>
        /// Gửi email reset mật khẩu
        /// </summary>
        /// <param name="dto">Dữ liệu quên mật khẩu</param>
        /// <returns>True nếu thành công</returns>
        Task<bool> SendPasswordResetEmailAsync(ForgotPasswordDto dto);

        /// <summary>
        /// Reset mật khẩu với token
        /// </summary>
        /// <param name="dto">Dữ liệu reset mật khẩu</param>
        /// <returns>True nếu thành công</returns>
        Task<bool> ResetPasswordAsync(ResetPasswordDto dto);
    }
}
