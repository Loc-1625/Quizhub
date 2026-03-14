using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizHub.Models.DTOs.DanhGia;
using QuizHub.Services.Interfaces;
using System.Security.Claims;

namespace QuizHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DanhGiaController : ControllerBase
    {
        private readonly IDanhGiaService _danhGiaService;

        public DanhGiaController(IDanhGiaService danhGiaService)
        {
            _danhGiaService = danhGiaService;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("Không thể xác định người dùng");
        }

        /// <summary>
        /// Lấy danh sách đánh giá của một bài thi
        /// </summary>
        [HttpGet("bai-thi/{baiThiId}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetByBaiThi(
            Guid baiThiId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var (items, totalCount) = await _danhGiaService.GetByBaiThiAsync(baiThiId, pageNumber, pageSize);

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
        /// Lấy thống kê đánh giá của bài thi
        /// </summary>
        [HttpGet("bai-thi/{baiThiId}/statistics")]
        [AllowAnonymous]
        public async Task<ActionResult> GetStatistics(Guid baiThiId)
        {
            var statistics = await _danhGiaService.GetStatisticsAsync(baiThiId);

            if (statistics == null)
            {
                return Ok(new
                {
                    success = true,
                    data = new DanhGiaStatisticsDto
                    {
                        MaBaiThi = baiThiId,
                        TongDanhGia = 0,
                        XepHangTrungBinh = 0,
                        Sao5 = 0,
                        Sao4 = 0,
                        Sao3 = 0,
                        Sao2 = 0,
                        Sao1 = 0
                    }
                });
            }

            return Ok(new
            {
                success = true,
                data = statistics
            });
        }

        /// <summary>
        /// Lấy đánh giá của user hiện tại cho bài thi
        /// </summary>
        [HttpGet("bai-thi/{baiThiId}/my-review")]
        [Authorize]
        public async Task<ActionResult> GetMyReview(Guid baiThiId)
        {
            var userId = GetCurrentUserId();
            var review = await _danhGiaService.GetUserReviewAsync(baiThiId, userId);

            if (review == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Bạn chưa đánh giá bài thi này"
                });
            }

            return Ok(new
            {
                success = true,
                data = review
            });
        }

        /// <summary>
        /// Tạo đánh giá mới
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Create([FromBody] CreateDanhGiaDto dto)
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
                var danhGia = await _danhGiaService.CreateAsync(dto, userId);

                return CreatedAtAction(
                    nameof(GetByBaiThi),
                    new { baiThiId = dto.MaBaiThi },
                    new
                    {
                        success = true,
                        message = "Đánh giá thành công",
                        data = danhGia
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
        /// Cập nhật đánh giá
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult> Update(Guid id, [FromBody] UpdateDanhGiaDto dto)
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
                var danhGia = await _danhGiaService.UpdateAsync(id, dto, userId);

                if (danhGia == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy đánh giá"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật đánh giá thành công",
                    data = danhGia
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        /// <summary>
        /// Xóa đánh giá
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _danhGiaService.DeleteAsync(id, userId);

                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy đánh giá"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Xóa đánh giá thành công"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }
    }
}