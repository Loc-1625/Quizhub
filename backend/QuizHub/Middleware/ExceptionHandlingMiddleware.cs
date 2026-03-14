using System.Net;
using System.Text.Json;

namespace QuizHub.Middleware
{
    /// <summary>
    /// Global exception handling middleware để xử lý tất cả exceptions
    /// và trả về response thống nhất cho client
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var traceId = context.TraceIdentifier;
            
            // Log exception với context
            _logger.LogError(exception, 
                "Unhandled exception occurred. TraceId: {TraceId}, Path: {Path}, Method: {Method}",
                traceId, 
                context.Request.Path, 
                context.Request.Method);

            // Xác định response dựa trên loại exception
            var (statusCode, message, errorCode) = exception switch
            {
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Bạn không có quyền truy cập tài nguyên này.", "UNAUTHORIZED"),
                KeyNotFoundException => (HttpStatusCode.NotFound, "Không tìm thấy tài nguyên yêu cầu.", "NOT_FOUND"),
                ArgumentException argEx => (HttpStatusCode.BadRequest, argEx.Message, "INVALID_ARGUMENT"),
                InvalidOperationException invEx => (HttpStatusCode.BadRequest, invEx.Message, "INVALID_OPERATION"),
                OperationCanceledException => (HttpStatusCode.RequestTimeout, "Yêu cầu đã bị timeout.", "TIMEOUT"),
                _ => (HttpStatusCode.InternalServerError, "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.", "INTERNAL_ERROR")
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new ErrorResponse
            {
                Success = false,
                StatusCode = (int)statusCode,
                Message = message,
                ErrorCode = errorCode,
                TraceId = traceId,
                Timestamp = DateTime.UtcNow
            };

            // Thêm stack trace trong development
            if (_env.IsDevelopment())
            {
                response.Details = exception.ToString();
            }

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }
    }

    /// <summary>
    /// Response model cho errors
    /// </summary>
    public class ErrorResponse
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        public string TraceId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? Details { get; set; }
    }

    /// <summary>
    /// Extension method để đăng ký middleware
    /// </summary>
    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
