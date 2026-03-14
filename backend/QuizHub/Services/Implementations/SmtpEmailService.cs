using System.Net;
using System.Net.Mail;
using QuizHub.Services.Interfaces;

namespace QuizHub.Services.Implementations
{
    /// <summary>
    /// Email Service sử dụng SMTP (Gmail/Outlook)
    /// </summary>
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpEmailService> _logger;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly bool _enableSsl;

        public SmtpEmailService(
            IConfiguration configuration,
            ILogger<SmtpEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Load SMTP settings từ appsettings.json
            var smtpSettings = _configuration.GetSection("EmailSettings:Smtp");
            _smtpHost = smtpSettings["Host"] ?? throw new ArgumentNullException("SMTP Host not configured");
            _smtpPort = int.Parse(smtpSettings["Port"] ?? "587");
            _smtpUsername = smtpSettings["Username"] ?? throw new ArgumentNullException("SMTP Username not configured");
            _smtpPassword = smtpSettings["Password"] ?? throw new ArgumentNullException("SMTP Password not configured");
            _fromEmail = smtpSettings["FromEmail"] ?? _smtpUsername;
            _fromName = smtpSettings["FromName"] ?? "QuizHub";
            _enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");
        }

        public async Task<bool> SendEmailAsync(
            string to,
            string subject,
            string htmlBody,
            string? plainTextBody = null)
        {
            try
            {
                using var client = new SmtpClient(_smtpHost, _smtpPort)
                {
                    EnableSsl = _enableSsl,
                    Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                    Timeout = 30000 // 30 seconds
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_fromEmail, _fromName),
                    Subject = subject,
                    IsBodyHtml = true,
                    Body = htmlBody
                };

                mailMessage.To.Add(to);

                // Thêm plain text alternative nếu có
                if (!string.IsNullOrEmpty(plainTextBody))
                {
                    var plainView = AlternateView.CreateAlternateViewFromString(
                        plainTextBody,
                        null,
                        "text/plain");
                    mailMessage.AlternateViews.Add(plainView);
                }

                await client.SendMailAsync(mailMessage);

                _logger.LogInformation("Email sent successfully to {To}", to);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}. Subject: {Subject}", to, subject);
                return false;
            }
        }

        public async Task<bool> SendPasswordResetEmailAsync(
            string toEmail,
            string toName,
            string resetLink)
        {
            var subject = "[QuizHub] Yêu cầu đặt lại mật khẩu";

            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 30px; border-radius: 0 0 5px 5px; }}
        .button {{ display: inline-block; padding: 12px 30px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ margin-top: 20px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #777; }}
        .warning {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 10px; margin: 15px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔐 QuizHub - Đặt lại mật khẩu</h1>
        </div>
        <div class='content'>
            <h2>Xin chào {toName},</h2>
            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản QuizHub của bạn.</p>
            
            <p>Vui lòng nhấn vào nút bên dưới để đặt lại mật khẩu:</p>
            
            <div style='text-align: center;'>
                <a href='{resetLink}' class='button'>Đặt lại mật khẩu</a>
            </div>
            
            <p>Hoặc copy link sau vào trình duyệt:</p>
            <p style='background-color: #f0f0f0; padding: 10px; word-break: break-all; font-size: 12px;'>
                {resetLink}
            </p>
            
            <div class='warning'>
                ⚠️ <strong>Lưu ý quan trọng:</strong>
                <ul>
                    <li>Link này sẽ hết hạn sau <strong>24 giờ</strong></li>
                    <li>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này</li>
                    <li>Không chia sẻ link này với bất kỳ ai</li>
                </ul>
            </div>
            
            <p>Nếu bạn gặp vấn đề, vui lòng liên hệ với chúng tôi qua email: support@quizhub.com</p>
        </div>
        <div class='footer'>
            <p>Email này được gửi tự động từ hệ thống QuizHub.</p>
            <p>&copy; 2024 QuizHub. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

            var plainTextBody = $@"
QuizHub - Đặt lại mật khẩu

Xin chào {toName},

Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản QuizHub của bạn.

Vui lòng truy cập link sau để đặt lại mật khẩu:
{resetLink}

LƯU Ý:
- Link này sẽ hết hạn sau 24 giờ
- Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này
- Không chia sẻ link này với bất kỳ ai

Nếu bạn gặp vấn đề, vui lòng liên hệ: support@quizhub.com

---
© 2024 QuizHub. All rights reserved.
";

            return await SendEmailAsync(toEmail, subject, htmlBody, plainTextBody);
        }

        public async Task<bool> SendWelcomeEmailAsync(string toEmail, string userName)
        {
            var subject = "🎉 Chào mừng bạn đến với QuizHub!";

            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 30px; border-radius: 0 0 5px 5px; }}
        .feature {{ background-color: white; padding: 15px; margin: 10px 0; border-left: 4px solid #4CAF50; }}
        .button {{ display: inline-block; padding: 12px 30px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Chào mừng đến với QuizHub!</h1>
        </div>
        <div class='content'>
            <h2>Xin chào {userName},</h2>
            <p>Cảm ơn bạn đã đăng ký tài khoản tại <strong>QuizHub</strong> - Nền tảng tạo và chia sẻ đề thi trắc nghiệm thông minh!</p>
            
            <h3>✨ Bạn có thể làm gì với QuizHub?</h3>
            
            <div class='feature'>
                📝 <strong>Tạo đề thi dễ dàng:</strong> Sử dụng AI để import câu hỏi từ file
            </div>
            
            <div class='feature'>
                🎯 <strong>Làm bài trực tuyến:</strong> Hệ thống real-time với đồng hồ đếm ngược
            </div>
            
            <div class='feature'>
                🤝 <strong>Chia sẻ cộng đồng:</strong> Chia sẻ bài thi của bạn với mọi người
            </div>
            
            <div class='feature'>
                📊 <strong>Thống kê chi tiết:</strong> Xem kết quả và phân tích điểm số
            </div>
            
            <p style='margin-top: 30px;'>Chúc bạn có trải nghiệm tuyệt vời!</p>
            <p><strong>QuizHub Team</strong></p>
        </div>
    </div>
</body>
</html>";

            return await SendEmailAsync(toEmail, subject, htmlBody);
        }

        public async Task<bool> SendEmailConfirmationAsync(
            string toEmail,
            string userName,
            string confirmationLink)
        {
            var subject = "[QuizHub] Xác nhận địa chỉ email";

            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 30px; border-radius: 0 0 5px 5px; }}
        .button {{ display: inline-block; padding: 12px 30px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✉️ Xác nhận email</h1>
        </div>
        <div class='content'>
            <h2>Xin chào {userName},</h2>
            <p>Vui lòng xác nhận địa chỉ email của bạn bằng cách nhấn vào nút bên dưới:</p>
            
            <div style='text-align: center;'>
                <a href='{confirmationLink}' class='button'>Xác nhận email</a>
            </div>
            
            <p>Link này sẽ hết hạn sau 48 giờ.</p>
        </div>
    </div>
</body>
</html>";

            return await SendEmailAsync(toEmail, subject, htmlBody);
        }
    }
}
