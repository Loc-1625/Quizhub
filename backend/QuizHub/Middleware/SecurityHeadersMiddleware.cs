namespace QuizHub.Middleware
{
    /// <summary>
    /// Middleware để thêm security headers vào response
    /// Giúp bảo vệ chống các tấn công phổ biến
    /// </summary>
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Ngăn chặn clickjacking
            context.Response.Headers.Append("X-Frame-Options", "DENY");

            // Ngăn chặn XSS attacks
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

            // Ngăn chặn MIME type sniffing
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

            // Referrer policy
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

            // Content Security Policy (cấu hình cơ bản)
            context.Response.Headers.Append("Content-Security-Policy", 
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                "style-src 'self' 'unsafe-inline'; " +
                "img-src 'self' data: https:; " +
                "font-src 'self'; " +
                "connect-src 'self' wss: https:;");

            // Permissions Policy
            context.Response.Headers.Append("Permissions-Policy", 
                "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");

            await _next(context);
        }
    }

    public static class SecurityHeadersMiddlewareExtensions
    {
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SecurityHeadersMiddleware>();
        }
    }
}
