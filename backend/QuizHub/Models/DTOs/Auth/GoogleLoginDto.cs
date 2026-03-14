namespace QuizHub.Models.DTOs.Auth
{
    public class GoogleLoginDto
    {
        public string AccessToken { get; set; } = string.Empty;
    }

     public class GoogleUserInfoDto
    {
        public string sub { get; set; }      // ID riêng của Google
        public string name { get; set; }     // Họ tên
        public string email { get; set; }    // Email
        public string picture { get; set; }  // Link ảnh avatar
        public bool email_verified { get; set; }
    }
}