using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizHub.Models.DTOs.CauHoi;
using QuizHub.Services.Interfaces;
using System.Security.Claims;

namespace QuizHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CauHoiController : ControllerBase
    {
        private readonly ICauHoiService _cauHoiService;

        public CauHoiController(ICauHoiService cauHoiService)
        {
            _cauHoiService = cauHoiService;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("Không thể xác định người dùng");
        }

        /// <summary>
        /// Lấy danh sách câu hỏi với filter và phân trang
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetAll([FromQuery] CauHoiFilterDto filter)
        {
            var userId = GetCurrentUserId();
            var (items, totalCount) = await _cauHoiService.GetAllAsync(filter, userId);

            return Ok(new
            {
                success = true,
                data = items,
                pagination = new
                {
                    currentPage = filter.PageNumber,
                    pageSize = filter.PageSize,
                    totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
                }
            });
        }

        /// <summary>
        /// Lấy chi tiết một câu hỏi
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(Guid id)
        {
            var userId = GetCurrentUserId();
            var cauHoi = await _cauHoiService.GetByIdAsync(id, userId);

            if (cauHoi == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy câu hỏi hoặc bạn không có quyền xem"
                });
            }

            return Ok(new
            {
                success = true,
                data = cauHoi
            });
        }

        /// <summary>
        /// Tạo câu hỏi mới
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreateCauHoiDto dto)
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
                var cauHoi = await _cauHoiService.CreateAsync(dto, userId);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = cauHoi.MaCauHoi },
                    new
                    {
                        success = true,
                        message = "Tạo câu hỏi thành công",
                        data = cauHoi
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
        /// Cập nhật câu hỏi
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, [FromBody] UpdateCauHoiDto dto)
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
                var cauHoi = await _cauHoiService.UpdateAsync(id, dto, userId);

                if (cauHoi == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy câu hỏi"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật câu hỏi thành công",
                    data = cauHoi
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
        /// Xóa câu hỏi (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _cauHoiService.DeleteAsync(id, userId);

                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy câu hỏi"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Xóa câu hỏi thành công"
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
        /// Đếm số câu hỏi của user hiện tại
        /// </summary>
        [HttpGet("count")]
        public async Task<ActionResult> GetCount()
        {
            var userId = GetCurrentUserId();
            var count = await _cauHoiService.CountByUserAsync(userId);

            return Ok(new
            {
                success = true,
                data = new { count }
            });
        }

        /// <summary>
        /// Lấy thống kê câu hỏi theo mức độ
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult> GetStats()
        {
            var userId = GetCurrentUserId();
            var stats = await _cauHoiService.GetStatsAsync(userId);

            return Ok(new
            {
                success = true,
                data = stats
            });
        }

        /// <summary>
        /// Lấy câu hỏi theo danh mục
        /// </summary>
        [HttpGet("danh-muc/{danhMucId}")]
        public async Task<ActionResult> GetByDanhMuc(Guid danhMucId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var userId = GetCurrentUserId();
            var filter = new CauHoiFilterDto
            {
                MaDanhMuc = danhMucId,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var (items, totalCount) = await _cauHoiService.GetAllAsync(filter, userId);

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
    }
}