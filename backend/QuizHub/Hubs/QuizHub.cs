using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace QuizHub.Hubs
{
    /// <summary>
    /// SignalR Hub xử lý realtime communication cho bài thi
    /// Hỗ trợ countdown, thông báo, và đồng bộ trạng thái
    /// </summary>
    public class QuizHub : Hub
    {
        private readonly ILogger<QuizHub> _logger;

        /// <summary>
        /// Dictionary lưu thông tin: connectionId -> (luotLamBaiId, userId)
        /// </summary>
        private static readonly ConcurrentDictionary<string, QuizConnection> _connections = new();

        /// <summary>
        /// Dictionary lưu: luotLamBaiId -> danh sách connectionIds
        /// </summary>
        private static readonly ConcurrentDictionary<Guid, HashSet<string>> _quizRooms = new();

        /// <summary>
        /// Khởi tạo QuizHub với logger
        /// </summary>
        /// <param name="logger">Logger instance</param>
        public QuizHub(ILogger<QuizHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Xử lý khi client kết nối
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            _logger.LogDebug("Client connected: {ConnectionId}", Context.ConnectionId);
        }

        /// <summary>
        /// Xử lý khi client ngắt kết nối 
        /// </summary>
        /// <param name="exception">Exception nếu có lỗi</param>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Xóa connection khi disconnect
            if (_connections.TryRemove(Context.ConnectionId, out var connection))
            {
                if (_quizRooms.TryGetValue(connection.LuotLamBaiId, out var connectionIds))
                {
                    connectionIds.Remove(Context.ConnectionId);
                    if (connectionIds.Count == 0)
                    {
                        _quizRooms.TryRemove(connection.LuotLamBaiId, out _);
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
            _logger.LogDebug("Client disconnected: {ConnectionId}", Context.ConnectionId);
        }

        /// <summary>
        /// Client join vào room làm bài thi
        /// </summary>
        public async Task JoinQuizRoom(string luotLamBaiId, string? userId)
        {
            if (!Guid.TryParse(luotLamBaiId, out var luotLamBaiGuid))
            {
                await Clients.Caller.SendAsync("Error", "Invalid luotLamBaiId");
                return;
            }

            // Lưu connection info
            var connection = new QuizConnection
            {
                ConnectionId = Context.ConnectionId,
                LuotLamBaiId = luotLamBaiGuid,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            };

            _connections[Context.ConnectionId] = connection;

            // Thêm vào room
            if (!_quizRooms.ContainsKey(luotLamBaiGuid))
            {
                _quizRooms[luotLamBaiGuid] = new HashSet<string>();
            }
            _quizRooms[luotLamBaiGuid].Add(Context.ConnectionId);

            await Groups.AddToGroupAsync(Context.ConnectionId, luotLamBaiId);

            await Clients.Caller.SendAsync("JoinedQuizRoom", new
            {
                luotLamBaiId,
                message = "Joined quiz room successfully",
                timestamp = DateTime.UtcNow
            });

            _logger.LogDebug("Client {ConnectionId} joined quiz room {RoomId}", Context.ConnectionId, luotLamBaiId);
        }

        /// <summary>
        /// Client leave room
        /// </summary>
        public async Task LeaveQuizRoom(string luotLamBaiId)
        {
            if (!Guid.TryParse(luotLamBaiId, out var luotLamBaiGuid))
            {
                return;
            }

            if (_quizRooms.TryGetValue(luotLamBaiGuid, out var connectionIds))
            {
                connectionIds.Remove(Context.ConnectionId);
            }

            _connections.TryRemove(Context.ConnectionId, out _);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, luotLamBaiId);

            await Clients.Caller.SendAsync("LeftQuizRoom", new
            {
                luotLamBaiId,
                message = "Left quiz room successfully"
            });
        }

        /// <summary>
        /// Server gửi countdown mỗi giây
        /// </summary>
        public async Task StartCountdown(string luotLamBaiId, int totalSeconds)
        {
            for (int remaining = totalSeconds; remaining >= 0; remaining--)
            {
                await Clients.Group(luotLamBaiId).SendAsync("CountdownUpdate", new
                {
                    luotLamBaiId,
                    remainingSeconds = remaining,
                    remainingMinutes = remaining / 60,
                    remainingSecondsInMinute = remaining % 60,
                    timestamp = DateTime.UtcNow
                });

                if (remaining == 0)
                {
                    // Hết giờ - tự động nộp bài
                    await Clients.Group(luotLamBaiId).SendAsync("TimeExpired", new
                    {
                        luotLamBaiId,
                        message = "Đã hết thời gian làm bài. Hệ thống sẽ tự động nộp bài.",
                        timestamp = DateTime.UtcNow
                    });
                    break;
                }

                // Cảnh báo khi còn 5 phút
                if (remaining == 300)
                {
                    await Clients.Group(luotLamBaiId).SendAsync("TimeWarning", new
                    {
                        luotLamBaiId,
                        message = "Còn 5 phút!",
                        remainingSeconds = remaining
                    });
                }

                // Cảnh báo khi còn 1 phút
                if (remaining == 60)
                {
                    await Clients.Group(luotLamBaiId).SendAsync("TimeWarning", new
                    {
                        luotLamBaiId,
                        message = "Còn 1 phút!",
                        remainingSeconds = remaining
                    });
                }

                await Task.Delay(1000); // Đợi 1 giây
            }
        }

        /// <summary>
        /// Notify khi có người mới tham gia bài thi
        /// </summary>
        public async Task NotifyNewParticipant(string baiThiId, string participantName)
        {
            await Clients.All.SendAsync("NewParticipant", new
            {
                baiThiId,
                participantName,
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Notify khi có người hoàn thành bài thi
        /// </summary>
        public async Task NotifyQuizCompleted(string baiThiId, string participantName, decimal score)
        {
            await Clients.All.SendAsync("QuizCompleted", new
            {
                baiThiId,
                participantName,
                score,
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Ping để giữ connection alive
        /// </summary>
        public async Task Ping()
        {
            await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
        }

        /// <summary>
        /// Lấy số người đang làm bài
        /// </summary>
        public async Task GetActiveParticipants(string luotLamBaiId)
        {
            if (Guid.TryParse(luotLamBaiId, out var luotLamBaiGuid))
            {
                var count = _quizRooms.TryGetValue(luotLamBaiGuid, out var connectionIds)
                    ? connectionIds.Count
                    : 0;

                await Clients.Caller.SendAsync("ActiveParticipantsCount", new
                {
                    luotLamBaiId,
                    count,
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }

    /// <summary>
    /// Helper class lưu thông tin kết nối
    /// </summary>
    public class QuizConnection
    {
        /// <summary>
        /// ID kết nối SignalR
        /// </summary>
        public string ConnectionId { get; set; } = string.Empty;

        /// <summary>
        /// Mã lượt làm bài đang tham gia
        /// </summary>
        public Guid LuotLamBaiId { get; set; }

        /// <summary>
        /// ID người dùng (null nếu là khách)
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Thời điểm tham gia room
        /// </summary>
        public DateTime JoinedAt { get; set; }
    }
}