namespace QuizHub.Services.Interfaces
{
    /// <summary>
    /// Interface cho Email Service
    /// Cung cấp các phương thức gửi email
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Gửi email
        /// </summary>
        /// <param name="to">Địa chỉ email người nhận</param>
        /// <param name="subject">Tiêu đề email</param>
        /// <param name="htmlBody">Nội dung HTML</param>
        /// <param name="plainTextBody">Nội dung text thuần (tùy chọn)</param>
        /// <returns>True nếu gửi thành công</returns>
        Task<bool> SendEmailAsync(string to, string subject, string htmlBody, string? plainTextBody = null);

        /// <summary>
        /// Gửi email reset password
        /// </summary>
        /// <param name="toEmail">Địa chỉ email người nhận</param>
        /// <param name="toName">Tên người nhận</param>
        /// <param name="resetLink">Link reset mật khẩu</param>
        /// <returns>True nếu gửi thành công</returns>
        Task<bool> SendPasswordResetEmailAsync(string toEmail, string toName, string resetLink);

        /// <summary>
        /// Gửi email chào mừng khi đăng ký
        /// </summary>
        /// <param name="toEmail">Địa chỉ email người nhận</param>
        /// <param name="userName">Tên người dùng</param>
        /// <returns>True nếu gửi thành công</returns>
        Task<bool> SendWelcomeEmailAsync(string toEmail, string userName);

        /// <summary>
        /// Gửi email xác nhận email (nếu implement email confirmation)
        /// </summary>
        /// <param name="toEmail">Địa chỉ email người nhận</param>
        /// <param name="userName">Tên người dùng</param>
        /// <param name="confirmationLink">Link xác nhận</param>
        /// <returns>True nếu gửi thành công</returns>
        Task<bool> SendEmailConfirmationAsync(string toEmail, string userName, string confirmationLink);
    }
}
