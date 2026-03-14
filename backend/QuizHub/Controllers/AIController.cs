using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using QuizHub.Models.DTOs.AI;
using QuizHub.Services.Interfaces;
using System.Security.Claims;

namespace QuizHub.Controllers
{
    /// <summary>
    /// Controller xử lý các tính năng AI (Gemini)
    /// Áp dụng Token Bucket Rate Limiting để kiểm soát chi phí API
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("ai")]
    public class AIController : ControllerBase
    {
        private readonly IGeminiAIService _aiService;

        /// <summary>
        /// Khởi tạo AIController
        /// </summary>
        /// <param name="aiService">Service xử lý AI</param>
        public AIController(IGeminiAIService aiService)
        {
            _aiService = aiService;
        }

        /// <summary>
        /// Lấy ID người dùng hiện tại từ JWT token
        /// </summary>
        /// <returns>ID người dùng</returns>
        /// <exception cref="UnauthorizedAccessException">Khi không thể xác thực</exception>
        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("Không thể xác minh người dùng");
        }

        /// <summary>
        /// [UC-13] Upload file và trích xuất câu hỏi bằng AI
        /// Hỗ trợ định dạng: .docx, .pdf, .txt
        /// </summary>
        /// <param name="file">File cần trích xuất câu hỏi</param>
        /// <returns>Kết quả trích xuất với danh sách câu hỏi</returns>
        [HttpPost("extract-questions")]
        [DisableRequestSizeLimit]
        public async Task<ActionResult> ExtractQuestions(IFormFile file)
        {
            // Kiểm tra file hợp lệ
            if (file == null || file.Length == 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "File không hợp lệ"
                });
            }

            try
            {
                var userId = GetCurrentUserId();
                var result = await _aiService.ExtractQuestionsFromFileAsync(file, userId);

                return Ok(new
                {
                    success = true,
                    message = $"Trích xuất thành công {result.SoCauHoiTrichXuat} câu hỏi",
                    data = result
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
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi xử lý file",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy kết quả trích xuất theo mã phiên
        /// </summary>
        /// <param name="maPhien">Mã phiên trích xuất</param>
        /// <returns>Chi tiết kết quả trích xuất</returns>
        [HttpGet("sessions/{maPhien}")]
        public async Task<ActionResult> GetExtractionResult(Guid maPhien)
        {
            var userId = GetCurrentUserId();
            var result = await _aiService.GetExtractionResultAsync(maPhien, userId);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy phiên trích xuất"
                });
            }

            return Ok(new
            {
                success = true,
                data = result
            });
        }

        /// <summary>
        /// [UC-13] Import các câu hỏi đã trích xuất vào database
        /// </summary>
        /// <param name="dto">Dữ liệu import gồm mã phiên và danh sách câu hỏi được chọn</param>
        /// <returns>Số lượng câu hỏi đã import thành công</returns>
        [HttpPost("import-questions")]
        public async Task<ActionResult> ImportQuestions([FromBody] ImportQuestionDto dto)
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
                var importCount = await _aiService.ImportQuestionsAsync(dto, userId);

                return Ok(new
                {
                    success = true,
                    message = $"Đã import thành công {importCount} câu hỏi",
                    data = new { soLuongCauHoiNhap = importCount }
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
        /// Lấy lịch sử các phiên import của người dùng
        /// </summary>
        /// <param name="pageNumber">Số trang (mặc định: 1)</param>
        /// <param name="pageSize">Số lượng mỗi trang (mặc định: 20)</param>
        /// <returns>Danh sách phiên import có phân trang</returns>
        [HttpGet("history")]
        public async Task<ActionResult> GetImportHistory(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var userId = GetCurrentUserId();
            var (items, totalCount) = await _aiService.GetImportHistoryAsync(userId, pageNumber, pageSize);

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
        /// Xóa phiên import
        /// </summary>
        /// <param name="maPhien">Mã phiên cần xóa</param>
        /// <returns>Kết quả thao tác</returns>
        [HttpDelete("sessions/{maPhien}")]
        public async Task<ActionResult> DeleteSession(Guid maPhien)
        {
            var userId = GetCurrentUserId();
            var result = await _aiService.DeleteSessionAsync(maPhien, userId);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy phiên import"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Xóa phiên import thành công"
            });
        }


        [HttpGet("test")]
        [DisableRateLimiting]
        public async Task<ActionResult> TestAI()
        {
            try
            {
                var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                var apiKey = config["GeminiAI:ApiKey"] ?? "NOT FOUND";
                var model = config["GeminiAI:Model"] ?? "NOT FOUND";

                var googleAI = new Mscc.GenerativeAI.GoogleAI(apiKey);
                var geminiModel = googleAI.GenerativeModel(model: model);
                var response = await geminiModel.GenerateContent("Say hello in Vietnamese");

                return Ok(new
                {
                    success = true,
                    apiKeyPrefix = apiKey.Length > 10 ? apiKey.Substring(0, 10) + "..." : "TOO SHORT",
                    modelName = model,
                    testResponse = response?.Text ?? "NO RESPONSE"
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    success = false,
                    error = ex.Message,
                    innerError = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Tạo câu hỏi từ mô tả chủ đề bằng AI
        /// </summary>
        /// <param name="dto">Dữ liệu chủ đề và số lượng câu hỏi</param>
        /// <returns>Danh sách câu hỏi được tạo</returns>
        [HttpPost("generate-questions")]
        public async Task<ActionResult> GenerateQuestions([FromBody] GenerateFromTopicDto dto)
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
                var result = await _aiService.GenerateQuestionsFromTopicAsync(
                    dto.Topic,
                    dto.NumberOfQuestions,
                    dto.Difficulty ?? "TrungBinh",
                    userId
                );

                if (result.TrangThai == "ThatBai")
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = result.ThongBaoLoi ?? "Không thể tạo câu hỏi"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = $"Đã tạo thành công {result.SoCauHoiTao} câu hỏi",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi tạo câu hỏi",
                    error = ex.Message
                });
            }
        }
    }
}
