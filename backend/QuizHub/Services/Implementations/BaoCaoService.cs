using Microsoft.EntityFrameworkCore;
using QuizHub.Data;
using QuizHub.Models.DTOs.BaoCao;
using QuizHub.Models.Entities;
using QuizHub.Services.Interfaces;

namespace QuizHub.Services.Implementations
{
  public class BaoCaoService : IBaoCaoService
  {
    private readonly QuizHubDbContext _context;

    public BaoCaoService(QuizHubDbContext context)
    {
      _context = context;
    }

    public async Task<BaoCaoDto> CreateAsync(CreateBaoCaoDto dto, string nguoiBaoCaoId)
    {
      // Kiểm tra đối tượng có tồn tại không
      if (dto.LoaiDoiTuong == "BaiThi")
      {
        var baiThiExists = await _context.BaiThi.AnyAsync(bt => bt.MaBaiThi == dto.MaDoiTuong && !bt.DaXoa);
        if (!baiThiExists)
          throw new InvalidOperationException("Bài thi không tồn tại");
      }
      else if (dto.LoaiDoiTuong == "CauHoi")
      {
        var cauHoiExists = await _context.CauHoi.AnyAsync(ch => ch.MaCauHoi == dto.MaDoiTuong && !ch.DaXoa);
        if (!cauHoiExists)
          throw new InvalidOperationException("Câu hỏi không tồn tại");
      }

      var baoCao = new BaoCao
      {
        NguoiBaoCaoId = nguoiBaoCaoId,
        LoaiDoiTuong = dto.LoaiDoiTuong,
        MaDoiTuong = dto.MaDoiTuong,
        LyDo = dto.LyDo,
        MoTa = dto.MoTa,
        TrangThai = "ChoDuyet"
      };

      _context.BaoCao.Add(baoCao);
      await _context.SaveChangesAsync();

      return await MapToDto(baoCao);
    }

    public async Task<(IEnumerable<BaoCaoDto> Items, int TotalCount)> GetAllAsync(
          string? trangThai = null,
      string? loaiDoiTuong = null,
          int pageNumber = 1,
int pageSize = 20)
    {
      var query = _context.BaoCao.AsQueryable();

      // Filter
      if (!string.IsNullOrEmpty(trangThai))
        query = query.Where(bc => bc.TrangThai == trangThai);

      if (!string.IsNullOrEmpty(loaiDoiTuong))
        query = query.Where(bc => bc.LoaiDoiTuong == loaiDoiTuong);

      var totalCount = await query.CountAsync();

      // Pagination v� l?y d? li?u
      var items = await query
.OrderByDescending(bc => bc.NgayTao)
   .Skip((pageNumber - 1) * pageSize)
   .Take(pageSize)
.ToListAsync();

      var dtos = new List<BaoCaoDto>();
      foreach (var item in items)
      {
        dtos.Add(await MapToDto(item));
      }

      return (dtos, totalCount);
    }

    public async Task<BaoCaoDto?> GetByIdAsync(Guid id)
    {
      var baoCao = await _context.BaoCao.FindAsync(id);
      if (baoCao == null)
        return null;

      return await MapToDto(baoCao);
    }

    public async Task<BaoCaoDto?> ResolveAsync(Guid id, XuLyBaoCaoDto dto, string nguoiXuLyId)
    {
      var baoCao = await _context.BaoCao.FindAsync(id);
      if (baoCao == null)
        return null;

      baoCao.TrangThai = dto.TrangThai;
      baoCao.KetQuaXuLy = dto.KetQuaXuLy;
      baoCao.NguoiXuLyId = nguoiXuLyId;
      baoCao.ThoiGianXuLy = DateTime.UtcNow;

      await _context.SaveChangesAsync();

      return await MapToDto(baoCao);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
      var baoCao = await _context.BaoCao.FindAsync(id);
      if (baoCao == null)
        return false;

      _context.BaoCao.Remove(baoCao);
      await _context.SaveChangesAsync();

      return true;
    }

    public async Task<(IEnumerable<BaoCaoDto> Items, int TotalCount)> GetByUserAsync(
   string userId,
 int pageNumber = 1,
 int pageSize = 20)
    {
      var query = _context.BaoCao.Where(bc => bc.NguoiBaoCaoId == userId);

      var totalCount = await query.CountAsync();

      var items = await query
 .OrderByDescending(bc => bc.NgayTao)
   .Skip((pageNumber - 1) * pageSize)
.Take(pageSize)
.ToListAsync();

      var dtos = new List<BaoCaoDto>();
      foreach (var item in items)
      {
        dtos.Add(await MapToDto(item));
      }

      return (dtos, totalCount);
    }

    private async Task<BaoCaoDto> MapToDto(BaoCao baoCao)
    {
      var dto = new BaoCaoDto
      {
        MaBaoCao = baoCao.MaBaoCao,
        NguoiBaoCaoId = baoCao.NguoiBaoCaoId,
        LoaiDoiTuong = baoCao.LoaiDoiTuong,
        MaDoiTuong = baoCao.MaDoiTuong,
        LyDo = baoCao.LyDo,
        MoTa = baoCao.MoTa,
        TrangThai = baoCao.TrangThai,
        NguoiXuLyId = baoCao.NguoiXuLyId,
        ThoiGianXuLy = baoCao.ThoiGianXuLy,
        KetQuaXuLy = baoCao.KetQuaXuLy,
        NgayTao = baoCao.NgayTao
      };

      // Lấy tên người báo cáo
      var nguoiBaoCao = await _context.Users.FindAsync(baoCao.NguoiBaoCaoId);
      dto.TenNguoiBaoCao = nguoiBaoCao?.HoTen ?? "Unknown";

      // Lấy tên người xử lý (nếu có)
      if (!string.IsNullOrEmpty(baoCao.NguoiXuLyId))
      {
        var nguoiXuLy = await _context.Users.FindAsync(baoCao.NguoiXuLyId);
        dto.TenNguoiXuLy = nguoiXuLy?.HoTen;
      }

      // Lấy tên đối tượng
      if (baoCao.LoaiDoiTuong == "BaiThi")
      {
        var baiThi = await _context.BaiThi.FindAsync(baoCao.MaDoiTuong);
        dto.TenDoiTuong = baiThi?.TieuDe ?? "B�i thi kh�ng t?n t?i";
      }
      else if (baoCao.LoaiDoiTuong == "CauHoi")
      {
        var cauHoi = await _context.CauHoi.FindAsync(baoCao.MaDoiTuong);
        dto.TenDoiTuong = cauHoi?.NoiDungCauHoi?.Substring(0, Math.Min(50, cauHoi.NoiDungCauHoi.Length)) + "..." ?? "C�u h?i kh�ng t?n t?i";
      }

      return dto;
    }
  }
}
