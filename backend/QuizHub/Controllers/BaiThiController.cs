using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using QuizHub.Models.DTOs.BaiThi;
using QuizHub.Services.Interfaces;
using System.Security.Claims;

namespace QuizHub.Controllers
{
    /// <summary>
    /// Controller quản lý bài thi
    /// Áp dụng Concurrency Rate Limiting cho các endpoint làm bài
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BaiThiController : ControllerBase
    {
        private readonly IBaiThiService _baiThiService;

        public BaiThiController(IBaiThiService baiThiService)
        {
            _baiThiService = baiThiService;
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        /// <summary>
        /// Lấy danh sách bài thi với filter và phân trang
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> GetAll([FromQuery] BaiThiFilterDto filter)
        {
            var userId = GetCurrentUserId();
            var (items, totalCount) = await _baiThiService.GetAllAsync(filter, userId);

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
        /// Lấy danh sách bài thi công khai (Trang chủ)
        /// </summary>
        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<ActionResult> GetPublicQuizzes(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? timKiem = null,
            [FromQuery] Guid? maDanhMuc = null,
            [FromQuery] string? sortBy = "NgayTao",
            [FromQuery] string? sortOrder = "DESC")
        {
            var filter = new BaiThiFilterDto
            {
                CheDoCongKhai = "CongKhai,CoMatKhau", 
                TrangThai = "XuatBan", 
                TimKiem = timKiem,
                MaDanhMuc = maDanhMuc,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            var (items, totalCount) = await _baiThiService.GetAllAsync(filter, null);

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
        /// Lấy chi tiết bài thi theo ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetById(Guid id)
        {
            var userId = GetCurrentUserId();
            var baiThi = await _baiThiService.GetByIdAsync(id, userId);

            if (baiThi == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy bài thi hoặc bạn không có quyền xem"
                });
            }

            return Ok(new
            {
                success = true,
                data = baiThi
            });
        }

        /// <summary>
        /// Lấy bài thi qua mã truy cập
        /// </summary>
        [HttpGet("code/{accessCode}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetByAccessCode(string accessCode)
        {
            var userId = GetCurrentUserId();
            var baiThi = await _baiThiService.GetByAccessCodeAsync(accessCode, userId);

            if (baiThi == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy bài thi với mã này"
                });
            }

            return Ok(new
            {
                success = true,
                data = baiThi
            });
        }

        /// <summary>
        /// Lấy chi tiết câu hỏi và đáp án của bài thi (chỉ dành cho chủ bài thi)
        /// </summary>
        [HttpGet("{id}/questions-detail")]
        [Authorize]
        public async Task<ActionResult> GetQuestionsDetail(Guid id)
        {
            var userId = GetCurrentUserId()!;
            var questions = await _baiThiService.GetQuestionsDetailAsync(id, userId);

            if (questions == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy bài thi hoặc bạn không có quyền xem"
                });
            }

            return Ok(new
            {
                success = true,
                data = questions
            });
        }

        /// <summary>
        /// Xác thực mật khẩu bài thi (cho chế độ CoMatKhau)
        /// </summary>
        [HttpPost("{id}/verify-password")]
        [AllowAnonymous]
        public async Task<ActionResult> VerifyPassword(Guid id, [FromBody] VerifyPasswordDto dto)
        {
            var result = await _baiThiService.VerifyPasswordAsync(id, dto.MatKhau);

            if (!result)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Mật khẩu không đúng"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Mật khẩu chính xác"
            });
        }

        /// <summary>
        /// Tạo bài thi mới
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Create([FromBody] CreateBaiThiDto dto)
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
                var userId = GetCurrentUserId()!;
                var baiThi = await _baiThiService.CreateAsync(dto, userId);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = baiThi.MaBaiThi },
                    new
                    {
                        success = true,
                        message = "Tạo bài thi thành công",
                        data = baiThi
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
        /// Cập nhật bài thi
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult> Update(Guid id, [FromBody] UpdateBaiThiDto dto)
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
                var userId = GetCurrentUserId()!;
                var baiThi = await _baiThiService.UpdateAsync(id, dto, userId);

                if (baiThi == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy bài thi"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật bài thi thành công",
                    data = baiThi
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
        /// Xóa bài thi (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId()!;
                var result = await _baiThiService.DeleteAsync(id, userId);

                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy bài thi"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Xóa bài thi thành công"
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
        /// Chuyển đổi chế độ công khai/riêng tư
        /// </summary>
        [HttpPatch("{id}/toggle-visibility")]
        [Authorize]
        public async Task<ActionResult> ToggleVisibility(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId()!;
                var result = await _baiThiService.ToggleVisibilityAsync(id, userId);

                if (result == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy bài thi"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = $"Đã chuyển sang chế độ {(result == "CongKhai" ? "công khai" : "riêng tư")}",
                    data = new { cheDoCongKhai = result }
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        /// <summary>
        /// Xuất bản bài thi (Chuyển từ Nháp sang Xuất bản)
        /// </summary>
        [HttpPost("{id}/publish")]
        [Authorize]
        public async Task<ActionResult> Publish(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId()!;
                var result = await _baiThiService.PublishAsync(id, userId);

                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy bài thi"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Xuất bản bài thi thành công"
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
        /// Đóng bài thi (Không cho phép làm bài nữa)
        /// </summary>
        [HttpPost("{id}/close")]
        [Authorize]
        public async Task<ActionResult> Close(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId()!;
                var result = await _baiThiService.CloseAsync(id, userId);

                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy bài thi"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Đóng bài thi thành công"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        /// <summary>
        /// Lấy thống kê bài thi (Chỉ chủ sở hữu)
        /// </summary>
        [HttpGet("{id}/statistics")]
        [Authorize]
        public async Task<ActionResult> GetStatistics(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId()!;
                var statistics = await _baiThiService.GetStatisticsAsync(id, userId);

                if (statistics == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy bài thi"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = statistics
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        /// <summary>
        /// Đếm số bài thi của user hiện tại
        /// </summary>
        [HttpGet("count")]
        [Authorize]
        public async Task<ActionResult> GetCount()
        {
            var userId = GetCurrentUserId()!;
            var count = await _baiThiService.CountByUserAsync(userId);

            return Ok(new
            {
                success = true,
                data = new { count }
            });
        }

        /// <summary>
        /// Lấy bài thi của tôi
        /// </summary>
        [HttpGet("my-quizzes")]
        [Authorize]
        public async Task<ActionResult> GetMyQuizzes([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var userId = GetCurrentUserId()!;
            var filter = new BaiThiFilterDto
            {
                ChiLayCuaToi = true,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = "NgayTao",
                SortOrder = "DESC"
            };

            var (items, totalCount) = await _baiThiService.GetAllAsync(filter, userId);

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
        /// Lấy danh sách người đã làm một bài thi cụ thể (Chỉ chủ sở hữu)
        /// </summary>
        [HttpGet("{id}/participants")]
        [Authorize]
        public async Task<ActionResult> GetParticipants(
            Guid id,
            [FromQuery] string? timKiem = null,
            [FromQuery] string? trangThai = null,
            [FromQuery] string? sortBy = "ThoiGianBatDau",
            [FromQuery] string? sortOrder = "DESC",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = GetCurrentUserId()!;
                var filter = new BaiThiFilterDto
                {
                    TimKiem = timKiem,
                    TrangThai = trangThai,
                    SortBy = sortBy,
                    SortOrder = sortOrder,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var nguoiLamBaiFilter = new NguoiLamBaiFilterDto
                {
                    TimKiem = timKiem,
                    TrangThai = trangThai,
                    SortBy = sortBy,
                    SortOrder = sortOrder,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var (items, totalCount, thongKe) = await _baiThiService.GetNguoiLamBaiAsync(id, nguoiLamBaiFilter, userId);

                return Ok(new
                {
                    success = true,
                    data = items,
                    thongKe,
                    pagination = new
                    {
                        currentPage = pageNumber,
                        pageSize,
                        totalCount,
                        totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                    }
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả người đã làm bài thi của tất cả bài thi do user tạo
        /// </summary>
        [HttpGet("all-participants")]
        [Authorize]
        public async Task<ActionResult> GetAllParticipants(
            [FromQuery] Guid? maBaiThi = null,
            [FromQuery] string? timKiem = null,
            [FromQuery] string? timKiemBaiThi = null,
            [FromQuery] string? trangThai = null,
            [FromQuery] string? sortBy = "ThoiGianBatDau",
            [FromQuery] string? sortOrder = "DESC",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var userId = GetCurrentUserId()!;
            var filter = new NguoiLamBaiFilterDto
            {
                MaBaiThi = maBaiThi,
                TimKiem = timKiem,
                TimKiemBaiThi = timKiemBaiThi,
                TrangThai = trangThai,
                SortBy = sortBy,
                SortOrder = sortOrder,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var (items, totalCount, thongKe) = await _baiThiService.GetAllNguoiLamBaiAsync(filter, userId);

            return Ok(new
            {
                success = true,
                data = items,
                thongKe,
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