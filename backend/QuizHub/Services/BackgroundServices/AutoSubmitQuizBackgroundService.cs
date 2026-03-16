using QuizHub.Services.Interfaces;

namespace QuizHub.Services.BackgroundServices
{
    public class AutoSubmitQuizBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AutoSubmitQuizBackgroundService> _logger;

        public AutoSubmitQuizBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<AutoSubmitQuizBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Auto Submit Quiz Background Service đã khởi động");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var luotLamBaiService = scope.ServiceProvider
                            .GetRequiredService<ILuotLamBaiService>();

                        await luotLamBaiService.AutoSubmitExpiredQuizzesAsync();
                    }

                    // Chạy mỗi 1 giây để nộp bài gần như ngay khi hết giờ
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi tự động nộp bài thi hết giờ");
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
            }

            _logger.LogInformation("Auto Submit Quiz Background Service đã dừng");
        }
    }
}