using Microsoft.EntityFrameworkCore;
using QuizHub.Data;
using QuizHub.Models.DTOs.DanhMuc;
using QuizHub.Models.Entities;
using QuizHub.Services.Interfaces;
using System.Text.RegularExpressions;

namespace QuizHub.Services.Implementations
{
    public class DanhMucService : IDanhMucService
    {
        private readonly QuizHubDbContext _context;

        public DanhMucService(QuizHubDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DanhMucDto>> GetAllAsync()
        {
            return await _context.DanhMuc
                .Select(dm => new DanhMucDto
                {
                    MaDanhMuc = dm.MaDanhMuc,
                    TenDanhMuc = dm.TenDanhMuc,
                    MoTa = dm.MoTa,
                    DuongDan = dm.DuongDan,
                    HinhAnh = dm.HinhAnh,
                    NgayTao = dm.NgayTao,
                    SoCauHoi = dm.CacCauHoi.Count(ch => ch.DaXoa == false)
                })
                .OrderBy(dm => dm.TenDanhMuc)
                .ToListAsync();
        }

        public async Task<DanhMucDto?> GetByIdAsync(Guid id)
        {
            return await _context.DanhMuc
                .Where(dm => dm.MaDanhMuc == id)
                .Select(dm => new DanhMucDto
                {
                    MaDanhMuc = dm.MaDanhMuc,
                    TenDanhMuc = dm.TenDanhMuc,
                    MoTa = dm.MoTa,
                    DuongDan = dm.DuongDan,
                    HinhAnh = dm.HinhAnh,
                    NgayTao = dm.NgayTao,
                    SoCauHoi = dm.CacCauHoi.Count(ch => ch.DaXoa == false)
                })
                .FirstOrDefaultAsync();
        }

        public async Task<DanhMucDto> CreateAsync(CreateDanhMucDto dto)
        {
            var danhMuc = new QuizHub.Models.Entities.DanhMuc
            {
                TenDanhMuc = dto.TenDanhMuc,
                MoTa = dto.MoTa,
                DuongDan = GenerateSlug(dto.TenDanhMuc),
                HinhAnh = dto.HinhAnh,
                NgayTao = DateTime.UtcNow,
                NgayCapNhat = DateTime.UtcNow
            };

            _context.DanhMuc.Add(danhMuc);
            await _context.SaveChangesAsync();

            return new DanhMucDto
            {
                MaDanhMuc = danhMuc.MaDanhMuc,
                TenDanhMuc = danhMuc.TenDanhMuc,
                MoTa = danhMuc.MoTa,
                DuongDan = danhMuc.DuongDan,
                HinhAnh = danhMuc.HinhAnh,
                NgayTao = danhMuc.NgayTao,
                SoCauHoi = 0
            };
        }

        public async Task<DanhMucDto?> UpdateAsync(Guid id, UpdateDanhMucDto dto)
        {
            var danhMuc = await _context.DanhMuc.FindAsync(id);
            if (danhMuc == null)
                return null;

            danhMuc.TenDanhMuc = dto.TenDanhMuc;
            danhMuc.MoTa = dto.MoTa;
            danhMuc.DuongDan = GenerateSlug(dto.TenDanhMuc);
            danhMuc.HinhAnh = dto.HinhAnh;
            danhMuc.NgayCapNhat = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new DanhMucDto
            {
                MaDanhMuc = danhMuc.MaDanhMuc,
                TenDanhMuc = danhMuc.TenDanhMuc,
                MoTa = danhMuc.MoTa,
                DuongDan = danhMuc.DuongDan,
                HinhAnh = danhMuc.HinhAnh,
                NgayTao = danhMuc.NgayTao,
                SoCauHoi = await _context.CauHoi.CountAsync(ch => ch.MaDanhMuc == id && ch.DaXoa == false)
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var danhMuc = await _context.DanhMuc.FindAsync(id);
            if (danhMuc == null)
                return false;

            // Kiểm tra xem có câu hỏi nào đang dùng danh mục này không
            var hasQuestions = await _context.CauHoi
                .AnyAsync(ch => ch.MaDanhMuc == id && ch.DaXoa == false);

            if (hasQuestions)
            {
                throw new InvalidOperationException(
                    "Không thể xóa danh mục này vì đang có câu hỏi sử dụng. Vui lòng xóa hoặc chuyển các câu hỏi sang danh mục khác trước.");
            }

            _context.DanhMuc.Remove(danhMuc);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.DanhMuc.AnyAsync(dm => dm.MaDanhMuc == id);
        }

        // Helper: Tạo slug từ tên danh mục
        private string GenerateSlug(string text)
        {
            // Chuyển về lowercase
            text = text.ToLowerInvariant();

            // Xóa dấu tiếng Việt
            text = RemoveVietnameseTones(text);

            // Thay thế khoảng trắng và ký tự đặc biệt bằng dấu gạch ngang
            text = Regex.Replace(text, @"[^a-z0-9\s-]", "");
            text = Regex.Replace(text, @"\s+", "-");
            text = Regex.Replace(text, @"-+", "-");

            return text.Trim('-');
        }

        private string RemoveVietnameseTones(string text)
        {
            string[] vietnameseSigns = new string[]
            {
                "aAeEoOuUiIdDyY",
                "áàạảãâấầậẩẫăắằặẳẵ",
                "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
                "éèẹẻẽêếềệểễ",
                "ÉÈẸẺẼÊẾỀỆỂỄ",
                "óòọỏõôốồộổỗơớờợởỡ",
                "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
                "úùụủũưứừựửữ",
                "ÚÙỤỦŨƯỨỪỰỬỮ",
                "íìịỉĩ",
                "ÍÌỊỈĨ",
                "đ",
                "Đ",
                "ýỳỵỷỹ",
                "ÝỲỴỶỸ"
            };

            for (int i = 1; i < vietnameseSigns.Length; i++)
            {
                for (int j = 0; j < vietnameseSigns[i].Length; j++)
                {
                    text = text.Replace(vietnameseSigns[i][j], vietnameseSigns[0][i - 1]);
                }
            }

            return text;
        }
    }
}