using System.Diagnostics;

namespace QuizHub.Middleware
{
    /// <summary>
    /// Middleware để log tất cả requests và response time
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestId = context.TraceIdentifier;

            // Log request
            _logger.LogInformation(
                "Request started: {Method} {Path} - RequestId: {RequestId} - User: {User}",
                context.Request.Method,
                context.Request.Path,
                requestId,
                context.User.Identity?.Name ?? "Anonymous");

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                var logLevel = context.Response.StatusCode >= 500 
                    ? LogLevel.Error 
                    : context.Response.StatusCode >= 400 
                        ? LogLevel.Warning 
                        : LogLevel.Information;

                _logger.Log(
                    logLevel,
                    "Request completed: {Method} {Path} - StatusCode: {StatusCode} - Duration: {Duration}ms - RequestId: {RequestId}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    requestId);

                // Log cảnh báo nếu request chậm (> 3 giây)
                if (stopwatch.ElapsedMilliseconds > 3000)
                {
                    _logger.LogWarning(
                        "Slow request detected: {Method} {Path} took {Duration}ms",
                        context.Request.Method,
                        context.Request.Path,
                        stopwatch.ElapsedMilliseconds);
                }
            }
        }
    }

    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}
