using Microsoft.EntityFrameworkCore;
using QuizHub.Data;
using QuizHub.Models.DTOs.DanhGia;
using QuizHub.Models.Entities;
using QuizHub.Services.Interfaces;

namespace QuizHub.Services.Implementations
{
    public class DanhGiaService : IDanhGiaService
    {
        private readonly QuizHubDbContext _context;

        public DanhGiaService(QuizHubDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<DanhGiaDto> Items, int TotalCount)> GetByBaiThiAsync(
            Guid baiThiId,
            int pageNumber,
            int pageSize)
        {
            var query = _context.DanhGia
                .Where(dg => dg.MaBaiThi == baiThiId)
                .OrderByDescending(dg => dg.NgayTao);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(dg => new DanhGiaDto
                {
                    MaDanhGia = dg.MaDanhGia,
                    MaBaiThi = dg.MaBaiThi,
                    TenBaiThi = _context.BaiThi
                        .Where(bt => bt.MaBaiThi == dg.MaBaiThi)
                        .Select(bt => bt.TieuDe)
                        .FirstOrDefault() ?? "",
                    NguoiDungId = dg.NguoiDungId,
                    TenNguoiDung = _context.Users
                        .Where(u => u.Id == dg.NguoiDungId)
                        .Select(u => u.HoTen ?? u.Email!)
                        .FirstOrDefault() ?? "Unknown",
                    AnhDaiDien = _context.Users
                        .Where(u => u.Id == dg.NguoiDungId)
                        .Select(u => u.AnhDaiDien)
                        .FirstOrDefault(),
                    XepHang = dg.XepHang,
                    BinhLuan = dg.BinhLuan,
                    NgayTao = dg.NgayTao,
                    NgayCapNhat = dg.NgayCapNhat
                })
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<DanhGiaDto?> GetByIdAsync(Guid id)
        {
            return await _context.DanhGia
                .Where(dg => dg.MaDanhGia == id)
                .Select(dg => new DanhGiaDto
                {
                    MaDanhGia = dg.MaDanhGia,
                    MaBaiThi = dg.MaBaiThi,
                    TenBaiThi = _context.BaiThi
                        .Where(bt => bt.MaBaiThi == dg.MaBaiThi)
                        .Select(bt => bt.TieuDe)
                        .FirstOrDefault() ?? "",
                    NguoiDungId = dg.NguoiDungId,
                    TenNguoiDung = _context.Users
                        .Where(u => u.Id == dg.NguoiDungId)
                        .Select(u => u.HoTen ?? u.Email!)
                        .FirstOrDefault() ?? "Unknown",
                    AnhDaiDien = _context.Users
                        .Where(u => u.Id == dg.NguoiDungId)
                        .Select(u => u.AnhDaiDien)
                        .FirstOrDefault(),
                    XepHang = dg.XepHang,
                    BinhLuan = dg.BinhLuan,
                    NgayTao = dg.NgayTao,
                    NgayCapNhat = dg.NgayCapNhat
                })
                .FirstOrDefaultAsync();
        }

        public async Task<DanhGiaDto?> GetUserReviewAsync(Guid baiThiId, string userId)
        {
            return await _context.DanhGia
                .Where(dg => dg.MaBaiThi == baiThiId && dg.NguoiDungId == userId)
                .Select(dg => new DanhGiaDto
                {
                    MaDanhGia = dg.MaDanhGia,
                    MaBaiThi = dg.MaBaiThi,
                    TenBaiThi = _context.BaiThi
                        .Where(bt => bt.MaBaiThi == dg.MaBaiThi)
                        .Select(bt => bt.TieuDe)
                        .FirstOrDefault() ?? "",
                    NguoiDungId = dg.NguoiDungId,
                    TenNguoiDung = _context.Users
                        .Where(u => u.Id == dg.NguoiDungId)
                        .Select(u => u.HoTen ?? u.Email!)
                        .FirstOrDefault() ?? "Unknown",
                    XepHang = dg.XepHang,
                    BinhLuan = dg.BinhLuan,
                    NgayTao = dg.NgayTao,
                    NgayCapNhat = dg.NgayCapNhat
                })
                .FirstOrDefaultAsync();
        }

        public async Task<DanhGiaDto> CreateAsync(CreateDanhGiaDto dto, string userId)
        {
            // Kiểm tra bài thi tồn tại
            var baiThi = await _context.BaiThi.FindAsync(dto.MaBaiThi);
            if (baiThi == null || baiThi.DaXoa)
            {
                throw new InvalidOperationException("Không tìm thấy bài thi");
            }

            // Kiểm tra đã làm bài chưa
            var hasCompleted = await _context.LuotLamBai
                .AnyAsync(llb => llb.MaBaiThi == dto.MaBaiThi &&
                                llb.NguoiDungId == userId &&
                                (llb.TrangThai == "HoanThanh" || llb.TrangThai == "TuDongNop"));

            if (!hasCompleted)
            {
                throw new InvalidOperationException("Bạn phải hoàn thành bài thi trước khi đánh giá");
            }

            // Cho phép đánh giá nhiều lần - tạo đánh giá mới mỗi lần
            var danhGia = new QuizHub.Models.Entities.DanhGia
            {
                MaBaiThi = dto.MaBaiThi,
                NguoiDungId = userId,
                XepHang = dto.XepHang,
                BinhLuan = dto.BinhLuan,
                NgayTao = DateTime.UtcNow,
                NgayCapNhat = DateTime.UtcNow
            };

            _context.DanhGia.Add(danhGia);
            await _context.SaveChangesAsync();

            // Cập nhật điểm trung bình của bài thi
            await UpdateBaiThiDanhGiaTrungBinhAsync(dto.MaBaiThi);

            return await GetByIdAsync(danhGia.MaDanhGia)
                ?? throw new Exception("Không thể lấy đánh giá vừa tạo");
        }

        private async Task UpdateBaiThiDanhGiaTrungBinhAsync(Guid maBaiThi)
        {
            var avgRating = await _context.DanhGia
                .Where(dg => dg.MaBaiThi == maBaiThi)
                .AverageAsync(dg => (decimal?)dg.XepHang);

            var baiThi = await _context.BaiThi.FindAsync(maBaiThi);
            if (baiThi != null)
            {
                baiThi.XepHangTrungBinh = avgRating;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<DanhGiaDto?> UpdateAsync(Guid id, UpdateDanhGiaDto dto, string userId)
        {
            var danhGia = await _context.DanhGia.FindAsync(id);

            if (danhGia == null)
                return null;

            // Kiểm tra quyền sở hữu
            if (danhGia.NguoiDungId != userId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền chỉnh sửa đánh giá này");
            }

            danhGia.XepHang = dto.XepHang;
            danhGia.BinhLuan = dto.BinhLuan;
            danhGia.NgayCapNhat = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Cập nhật điểm trung bình của bài thi
            await UpdateBaiThiDanhGiaTrungBinhAsync(danhGia.MaBaiThi);

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id, string userId)
        {
            var danhGia = await _context.DanhGia.FindAsync(id);

            if (danhGia == null)
                return false;

            // Kiểm tra quyền sở hữu
            if (danhGia.NguoiDungId != userId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xóa đánh giá này");
            }

            var maBaiThi = danhGia.MaBaiThi;
            _context.DanhGia.Remove(danhGia);
            await _context.SaveChangesAsync();

            // Cập nhật điểm trung bình của bài thi
            await UpdateBaiThiDanhGiaTrungBinhAsync(maBaiThi);

            return true;
        }

        public async Task<DanhGiaStatisticsDto?> GetStatisticsAsync(Guid baiThiId)
        {
            var danhGias = await _context.DanhGia
                .Where(dg => dg.MaBaiThi == baiThiId)
                .ToListAsync();

            if (!danhGias.Any())
                return null;

            return new DanhGiaStatisticsDto
            {
                MaBaiThi = baiThiId,
                TongDanhGia = danhGias.Count,
                XepHangTrungBinh = danhGias.Average(dg => dg.XepHang),
                Sao5 = danhGias.Count(dg => dg.XepHang == 5),
                Sao4 = danhGias.Count(dg => dg.XepHang == 4),
                Sao3 = danhGias.Count(dg => dg.XepHang == 3),
                Sao2 = danhGias.Count(dg => dg.XepHang == 2),
                Sao1 = danhGias.Count(dg => dg.XepHang == 1)
            };
        }
    }
}