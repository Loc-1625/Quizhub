using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizHub.Models.DTOs.BaoCao;
using QuizHub.Services.Interfaces;
using System.Security.Claims;

namespace QuizHub.Controllers
{
    /// <summary>
    /// Controller quản lý báo cáo vi phạm
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BaoCaoController : ControllerBase
    {
        private readonly IBaoCaoService _baoCaoService;
        public BaoCaoController(IBaoCaoService baoCaoService)
        {
            _baoCaoService = baoCaoService;
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
        /// [UC-18] Tạo báo cáo mới
        /// </summary>
        /// <param name="dto">Dữ liệu báo cáo</param>
        /// <returns>Thông tin báo cáo đã tạo</returns>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Create([FromBody] CreateBaoCaoDto dto)
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
                var baoCao = await _baoCaoService.CreateAsync(dto, userId);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = baoCao.MaBaoCao },
                    new
                    {
                        success = true,
                        message = "Gửi báo cáo thành công",
                        data = baoCao
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
        /// [AD-06] Lấy danh sách tất cả báo cáo (Chỉ Admin)
        /// </summary>
        /// <param name="trangThai">Lọc theo trạng thái</param>
        /// <param name="loaiDoiTuong">Lọc theo loại đối tượng</param>
        /// <param name="pageNumber">Số trang</param>
        /// <param name="pageSize">Số lượng mỗi trang</param>
        /// <returns>Danh sách báo cáo có phân trang</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetAll(
            [FromQuery] string? trangThai = null,
            [FromQuery] string? loaiDoiTuong = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var (items, totalCount) = await _baoCaoService.GetAllAsync(
                trangThai,
                loaiDoiTuong,
                pageNumber,
                pageSize);

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
        /// Lấy chi tiết một báo cáo (Chỉ Admin)
        /// </summary>
        /// <param name="id">Mã báo cáo</param>
        /// <returns>Chi tiết báo cáo</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetById(Guid id)
        {
            var baoCao = await _baoCaoService.GetByIdAsync(id);

            if (baoCao == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy báo cáo"
                });
            }

            return Ok(new
            {
                success = true,
                data = baoCao
            });
        }

        /// <summary>
        /// [AD-06] Xử lý báo cáo (Chỉ Admin)
        /// </summary>
        /// <param name="id">Mã báo cáo</param>
        /// <param name="dto">Dữ liệu xử lý</param>
        /// <returns>Kết quả xử lý</returns>
        [HttpPut("{id}/resolve")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Resolve(Guid id, [FromBody] XuLyBaoCaoDto dto)
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

            var adminId = GetCurrentUserId();
            var baoCao = await _baoCaoService.ResolveAsync(id, dto, adminId);

            if (baoCao == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy báo cáo"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Xử lý báo cáo thành công",
                data = baoCao
            });
        }

        /// <summary>
        /// Xóa báo cáo (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var result = await _baoCaoService.DeleteAsync(id);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy báo cáo"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Xóa báo cáo thành công"
            });
        }

        /// <summary>
        /// Lấy danh sách báo cáo của user hiện tại
        /// </summary>
        /// <param name="pageNumber">Số trang</param>
        /// <param name="pageSize">Số lượng mỗi trang</param>
        /// <returns>Danh sách báo cáo có phân trang</returns>
        [HttpGet("my-reports")]
        [Authorize]
        public async Task<ActionResult> GetMyReports(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var userId = GetCurrentUserId();
            var (items, totalCount) = await _baoCaoService.GetByUserAsync(userId, pageNumber, pageSize);

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
