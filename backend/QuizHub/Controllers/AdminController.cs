using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizHub.Models.DTOs.Admin;
using QuizHub.Services.Implementations;
using QuizHub.Services.Interfaces;

namespace QuizHub.Controllers
{
    /// <summary>
    /// Controller quản trị hệ thống
    /// Chỉ Admin mới có quyền truy cập
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        /// <summary>
        /// Khởi tạo AdminController
        /// </summary>
        /// <param name="adminService">Service quản trị</param>
        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        /// <summary>
        /// [AD-02] Dashboard tổng quan
        /// Lấy thống kê tổng quan về hệ thống
        /// </summary>
        /// <returns>Thống kê dashboard</returns>
        [HttpGet("dashboard")]
        public async Task<ActionResult> GetDashboard()
        {
            var stats = await _adminService.GetDashboardStatsAsync();

            return Ok(new
            {
                success = true,
                data = stats
            });
        }

        /// <summary>
        /// [AD-03] Lấy danh sách người dùng
        /// </summary>
        /// <param name="searchTerm">Từ khóa tìm kiếm</param>
        /// <param name="trangThaiKichHoat">Trạng thái kích hoạt</param>
        /// <param name="pageNumber">Số trang</param>
        /// <param name="pageSize">Số lượng mỗi trang</param>
        /// <returns>Danh sách người dùng có phân trang</returns>
        [HttpGet("users")]
        public async Task<ActionResult> GetUsers(
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? activeStatus = null,
            [FromQuery] string? role = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            bool? isActive = null;
            if (!string.IsNullOrEmpty(activeStatus))
            {
                if (activeStatus == "HoatDong") isActive = true;
                else if (activeStatus == "BiKhoa") isActive = false;
            }

            var (items, totalCount) = await _adminService.GetUsersAsync(
                searchTerm,
                isActive,
                role,
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
        /// [AD-03] Lấy chi tiết một người dùng
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Thông tin chi tiết người dùng</returns>
        [HttpGet("users/{userId}")]
        public async Task<ActionResult> GetUserById(string userId)
        {
            var user = await _adminService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy người dùng"
                });
            }

            return Ok(new
            {
                success = true,
                data = user
            });
        }

        /// <summary>
        /// [AD-03] Kích hoạt/Vô hiệu hóa tài khoản
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Kết quả thao tác</returns>
        [HttpPut("users/{userId}/toggle-status")]
        public async Task<ActionResult> ToggleUserStatus(string userId)
        {
            var result = await _adminService.ToggleUserStatusAsync(userId);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy người dùng"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Thay đổi trạng thái người dùng thành công"
            });
        }

        /// <summary>
        /// [AD-03] Cập nhật vai trò người dùng
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="request">Vai trò mới</param>
        /// <returns>Kết quả thao tác</returns>
        [HttpPut("users/{userId}/role")]
        public async Task<ActionResult> UpdateUserRole(string userId, [FromBody] UpdateRoleRequest request)
        {
            // Không cho phép tự thay đổi vai trò của chính mình
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == currentUserId)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Bạn không thể thay đổi vai trò của chính mình"
                });
            }

            var result = await _adminService.UpdateUserRoleAsync(userId, request.Role);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                message = "Cập nhật vai trò thành công"
            });
        }

        /// <summary>
        /// [AD-03] Xóa người dùng
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Kết quả thao tác</returns>
        [HttpDelete("users/{userId}")]
        public async Task<ActionResult> DeleteUser(string userId)
        {
            // Không cho phép tự xóa chính mình
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == currentUserId)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Bạn không thể xóa chính mình"
                });
            }

            var result = await _adminService.DeleteUserAsync(userId);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                message = "Xóa người dùng thành công"
            });
        }

        /// <summary>
        /// [AD-04] Lấy danh sách tất cả nội dung
        /// </summary>
        /// <param name="loaiNoiDung">Loại nội dung: "BaiThi" hoặc "CauHoi"</param>
        /// <param name="searchTerm">Từ khóa tìm kiếm</param>
        /// <param name="daXoa">Lọc theo trạng thái xóa</param>
        /// <param name="pageNumber">Số trang</param>
        /// <param name="pageSize">Số lượng mỗi trang</param>
        /// <returns>Danh sách nội dung có phân trang</returns>
        [HttpGet("content")]
        public async Task<ActionResult> GetAllContent(
            [FromQuery] string? loaiNoiDung = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? tenNguoiTao = null,
            [FromQuery] bool? daXoa = null,
            [FromQuery] string? sortBy = "NgayTao",
            [FromQuery] string? sortOrder = "DESC",
            [FromQuery] Guid? maDanhMuc = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var (items, totalCount) = await _adminService.GetAllContentAsync(
                loaiNoiDung,
                searchTerm,
                tenNguoiTao,
                daXoa,
                sortBy,
                sortOrder,
                maDanhMuc,
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

        [HttpGet("Answer/{questionId}")]
        public async Task<IActionResult> GetDapAnCauHoiByAdmin(Guid questionId)
        {
            var result = await _adminService.GetQuestionAndAnswerByAdminAsync(questionId);
            if (result == null) return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// [AD-04] Xóa nội dung vi phạm
        /// </summary>
        /// <param name="loaiNoiDung">Loại nội dung: "BaiThi" hoặc "CauHoi"</param>
        /// <param name="id">ID nội dung</param>
        /// <param name="hardDelete">True = xóa vĩnh viễn, False = xóa mềm</param>
        /// <returns>Kết quả thao tác</returns>
        [HttpDelete("content/{loaiNoiDung}/{id}")]
        public async Task<ActionResult> DeleteContent(string loaiNoiDung, Guid id, [FromQuery] bool hardDelete = false)
        {
            // Kiểm tra loại nội dung hợp lệ
            if (loaiNoiDung != "BaiThi" && loaiNoiDung != "CauHoi")
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Loại nội dung không hợp lệ. Chỉ chấp nhận 'BaiThi' hoặc 'CauHoi'"
                });
            }

            var result = await _adminService.DeleteContentAsync(loaiNoiDung, id, hardDelete);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy nội dung"
                });
            }

            return Ok(new
            {
                success = true,
                message = hardDelete ? "Xóa vĩnh viễn thành công" : "Xóa mềm thành công"
            });
        }

        /// <summary>
        /// Recalculate điểm trung bình đánh giá cho tất cả bài thi
        /// </summary>
        [HttpPost("recalculate-ratings")]
        public async Task<ActionResult> RecalculateRatings()
        {
            var count = await _adminService.RecalculateAllRatingsAsync();

            return Ok(new
            {
                success = true,
                message = $"Đã cập nhật điểm trung bình cho {count} bài thi"
            });
        }

        /// <summary>
        /// [AD-04] Khôi phục nội dung đã xóa
        /// </summary>
        /// <param name="loaiNoiDung">Loại nội dung: "BaiThi" hoặc "CauHoi"</param>
        /// <param name="id">ID nội dung</param>
        /// <returns>Kết quả thao tác</returns>
        [HttpPut("content/{loaiNoiDung}/{id}/restore")]
        public async Task<ActionResult> RestoreContent(string loaiNoiDung, Guid id)
        {
            // Kiểm tra loại nội dung hợp lệ
            if (loaiNoiDung != "BaiThi" && loaiNoiDung != "CauHoi")
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Loại nội dung không hợp lệ. Chỉ chấp nhận 'BaiThi' hoặc 'CauHoi'"
                });
            }

            var result = await _adminService.RestoreContentAsync(loaiNoiDung, id);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy nội dung"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Khôi phục nội dung thành công"
            });
        }
    }
}
