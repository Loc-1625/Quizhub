using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QuizHub.Data;
using QuizHub.Models.Entities;

namespace QuizHub.Services.BackgroundServices
{
    public class QuizCountdownBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<Hubs.QuizHub> _hubContext;
        private readonly IMemoryCache _cache;
        private readonly ILogger<QuizCountdownBackgroundService> _logger;
        private static readonly TimeSpan CacheExpiration = TimeSpan.FromSeconds(10);

        public QuizCountdownBackgroundService(
            IServiceProvider serviceProvider,
            IHubContext<Hubs.QuizHub> hubContext,
            IMemoryCache cache,
            ILogger<QuizCountdownBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
            _cache = cache;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Quiz Countdown Background Service đang khởi động...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Chỉ query database mỗi 10 giây thay vì mỗi 1 giây
                    if (!_cache.TryGetValue("ActiveSessions", out List<ActiveSessionDto>? activeSessions))
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var context = scope.ServiceProvider.GetRequiredService<QuizHubDbContext>();

                        // Chỉ lấy dữ liệu cần thiết
                        activeSessions = await context.LuotLamBai
                            .Where(llb => llb.TrangThai == "DangLam" && llb.ThoiGianKetThuc.HasValue)
                            .Select(llb => new ActiveSessionDto
                            {
                                MaLuotLamBai = llb.MaLuotLamBai,
                                ThoiGianKetThuc = llb.ThoiGianKetThuc!.Value
                            })
                            .ToListAsync(stoppingToken);

                        // Cache kết quả trong 10 giây
                        _cache.Set("ActiveSessions", activeSessions, CacheExpiration);
                        
                        _logger.LogDebug($"Loaded {activeSessions.Count} active sessions from database");
                    }

                    // Xử lý countdown từ cached data
                    if (activeSessions != null && activeSessions.Any())
                    {
                        var now = DateTime.UtcNow;
                        var expiredSessions = new List<Guid>();

                        foreach (var session in activeSessions)
                        {
                            var remainingSeconds = (int)(session.ThoiGianKetThuc - now).TotalSeconds;

                            if (remainingSeconds <= 0)
                            {
                                // Đánh dấu session hết hạn
                                expiredSessions.Add(session.MaLuotLamBai);
                                
                                await _hubContext.Clients.Group(session.MaLuotLamBai.ToString())
                                    .SendAsync("TimeExpired", new
                                    {
                                        luotLamBaiId = session.MaLuotLamBai,
                                        message = "Đã hết thời gian làm bài!",
                                        timestamp = now
                                    }, stoppingToken);
                            }
                            else
                            {
                                // Gửi countdown update
                                await _hubContext.Clients.Group(session.MaLuotLamBai.ToString())
                                    .SendAsync("CountdownUpdate", new
                                    {
                                        luotLamBaiId = session.MaLuotLamBai,
                                        remainingSeconds,
                                        remainingMinutes = remainingSeconds / 60,
                                        remainingSecondsInMinute = remainingSeconds % 60,
                                        timestamp = now
                                    }, stoppingToken);

                                // Cảnh báo đặc biệt
                                if (remainingSeconds == 300 || remainingSeconds == 60 || remainingSeconds == 10)
                                {
                                    await _hubContext.Clients.Group(session.MaLuotLamBai.ToString())
                                        .SendAsync("TimeWarning", new
                                        {
                                            luotLamBaiId = session.MaLuotLamBai,
                                            message = $"Còn {remainingSeconds / 60} phút {remainingSeconds % 60} giây!",
                                            remainingSeconds
                                        }, stoppingToken);
                                }
                            }
                        }

                        // Xóa expired sessions khỏi cache
                        if (expiredSessions.Any())
                        {
                            activeSessions.RemoveAll(s => expiredSessions.Contains(s.MaLuotLamBai));
                            _cache.Set("ActiveSessions", activeSessions, CacheExpiration);
                        }
                    }

                    // Chờ 1 giây trước khi lặp lại
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi trong Quiz Countdown Background Service");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }

            _logger.LogInformation("Quiz Countdown Background Service đã dừng");
        }
    }

    // DTO để cache session data
    public class ActiveSessionDto
    {
        public Guid MaLuotLamBai { get; set; }
        public DateTime ThoiGianKetThuc { get; set; }
    }
}