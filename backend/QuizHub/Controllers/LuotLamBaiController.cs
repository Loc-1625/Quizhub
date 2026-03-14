using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.SignalR;
using QuizHub.Models.DTOs.LuotLamBai;
using QuizHub.Services.Interfaces;

namespace QuizHub.Controllers
{
    /// <summary>
    /// Controller quản lý các lượt làm bài thi
    /// Áp dụng Concurrency Rate Limiting để giới hạn số người làm bài đồng thời
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("quiz")]
    public class LuotLamBaiController : ControllerBase
    {
        private readonly ILuotLamBaiService _luotLamBaiService;
        private readonly IHubContext<QuizHub.Hubs.QuizHub> _hubContext;

        public LuotLamBaiController(ILuotLamBaiService luotLamBaiService, IHubContext<QuizHub.Hubs.QuizHub> hubContext)
        {
            _luotLamBaiService = luotLamBaiService;
            _hubContext = hubContext;
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        private string? GetClientIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        /// <summary>
        /// Bắt đầu làm bài thi với SignalR notification
        /// </summary>
        [HttpPost("start")]
        [AllowAnonymous]
        public async Task<ActionResult<BaiThiSessionDto>> StartQuiz([FromBody] StartBaiThiDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Dữ liệu không hợp lệ",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            try
            {
                var userId = GetCurrentUserId();
                var session = await _luotLamBaiService.StartQuizAsync(dto, userId);

                // Gửi thông báo qua SignalR
                await _hubContext.Clients.All.SendAsync("NewParticipant", new
                {
                    baiThiId = dto.MaBaiThi,
                    participantName = dto.TenNguoiThamGia ?? "Ẩn danh",
                    timestamp = DateTime.UtcNow
                });

                return Ok(new
                {
                    success = true,
                    message = "Bắt đầu làm bài thành công",
                    data = session
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy thông tin session làm bài hiện tại
        /// </summary>
        [HttpGet("session/{luotLamBaiId}")]
        [AllowAnonymous]
        public async Task<ActionResult<BaiThiSessionDto>> GetSession(Guid luotLamBaiId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var session = await _luotLamBaiService.GetSessionAsync(luotLamBaiId, userId);

                if (session == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy session"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = session
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lưu câu trả lời (auto-save)
        /// </summary>
        [HttpPost("{luotLamBaiId}/answer")]
        [AllowAnonymous]
        public async Task<ActionResult> SaveAnswer(Guid luotLamBaiId, [FromBody] SubmitCauTraLoiDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Dữ liệu không hợp lệ"
                });
            }

            try
            {
                var userId = GetCurrentUserId();
                var result = await _luotLamBaiService.SaveAnswerAsync(luotLamBaiId, dto, userId);

                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy lượt làm bài hoặc câu hỏi"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Lưu câu trả lời thành công"
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Nộp bài thi(với SignalR notification)
        /// </summary>
        [HttpPost("{luotLamBaiId}/submit")]
        [AllowAnonymous]
        public async Task<ActionResult<KetQuaBaiThiDto>> SubmitQuiz(Guid luotLamBaiId, [FromBody] SubmitBaiThiDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Dữ liệu không hợp lệ"
                });
            }

            try
            {
                var userId = GetCurrentUserId();
                var result = await _luotLamBaiService.SubmitQuizAsync(luotLamBaiId, dto, userId);

                // ✅ THÊM: Notify qua SignalR
                await _hubContext.Clients.All.SendAsync("QuizCompleted", new
                {
                    baiThiId = result.MaBaiThi,
                    participantName = "User", // Hoặc lấy từ database
                    score = result.Diem,
                    timestamp = DateTime.UtcNow
                });

                return Ok(new
                {
                    success = true,
                    message = "Nộp bài thành công",
                    data = result
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Xem kết quả bài thi
        /// </summary>
        [HttpGet("{luotLamBaiId}/result")]
        [AllowAnonymous]
        public async Task<ActionResult<KetQuaBaiThiDto>> GetResult(Guid luotLamBaiId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _luotLamBaiService.GetResultAsync(luotLamBaiId, userId);

                if (result == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy kết quả"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        /// <summary>
        /// Lấy lịch sử làm bài của user hiện tại
        /// </summary>
        [HttpGet("history")]
        [Authorize]
        public async Task<ActionResult> GetHistory(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? sortBy = "ngayLamBai",
            [FromQuery] bool sortDesc = true)
        {
            var userId = GetCurrentUserId()!;
            var (items, totalCount) = await _luotLamBaiService.GetHistoryAsync(userId, pageNumber, pageSize, sortBy, sortDesc);

            return Ok(new
            {
                success = true,
                data = items,
                pagination = new
                {
                    currentPage = pageNumber,
                    pageSize,
                    totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                }
            });
        }

        /// <summary>
        /// Lấy thống kê làm bài của user hiện tại
        /// </summary>
        [HttpGet("my-stats")]
        [Authorize]
        public async Task<ActionResult> GetMyStats()
        {
            var userId = GetCurrentUserId()!;
            var stats = await _luotLamBaiService.GetUserStatsAsync(userId);

            return Ok(new
            {
                success = true,
                data = stats
            });
        }

        /// <summary>
        /// Tự động nộp các bài thi đã hết giờ (Background job)
        /// </summary>
        [HttpPost("auto-submit-expired")]
        [ApiExplorerSettings(IgnoreApi = true)] // Ẩn khỏi Swagger
        public async Task<ActionResult> AutoSubmitExpired()
        {
            await _luotLamBaiService.AutoSubmitExpiredQuizzesAsync();
            return Ok();
        }
    }
}