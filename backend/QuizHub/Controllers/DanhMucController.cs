using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizHub.Models.DTOs.DanhMuc;
using QuizHub.Services.Interfaces;

namespace QuizHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DanhMucController : ControllerBase
    {
        private readonly IDanhMucService _danhMucService;

        public DanhMucController(IDanhMucService danhMucService)
        {
            _danhMucService = danhMucService;
        }

        /// <summary>
        /// Lấy danh sách tất cả danh mục
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DanhMucDto>>> GetAll()
        {
            var danhMucs = await _danhMucService.GetAllAsync();
            return Ok(new
            {
                success = true,
                data = danhMucs
            });
        }

        /// <summary>
        /// Lấy chi tiết một danh mục theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<DanhMucDto>> GetById(Guid id)
        {
            var danhMuc = await _danhMucService.GetByIdAsync(id);

            if (danhMuc == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy danh mục"
                });
            }

            return Ok(new
            {
                success = true,
                data = danhMuc
            });
        }

        /// <summary>
        /// Tạo danh mục mới (Chỉ Admin)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<DanhMucDto>> Create([FromBody] CreateDanhMucDto dto)
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

            var danhMuc = await _danhMucService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = danhMuc.MaDanhMuc },
                new
                {
                    success = true,
                    message = "Tạo danh mục thành công",
                    data = danhMuc
                });
        }

        /// <summary>
        /// Cập nhật danh mục (Chỉ Admin)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<DanhMucDto>> Update(Guid id, [FromBody] UpdateDanhMucDto dto)
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

            var danhMuc = await _danhMucService.UpdateAsync(id, dto);

            if (danhMuc == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy danh mục"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Cập nhật danh mục thành công",
                data = danhMuc
            });
        }

        /// <summary>
        /// Xóa danh mục (Chỉ Admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _danhMucService.DeleteAsync(id);

                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy danh mục"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Xóa danh mục thành công"
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
    }
}