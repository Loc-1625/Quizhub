using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuizHub.Data;
using QuizHub.Models.DTOs.Admin;
using QuizHub.Models.DTOs.CauHoi;
using QuizHub.Models.Entities;
using QuizHub.Services.Interfaces;

namespace QuizHub.Services.Implementations
{
  public class AdminService : IAdminService
  {
    private readonly QuizHubDbContext _context;
    private readonly UserManager<NguoiDung> _userManager;

    public AdminService(QuizHubDbContext context, UserManager<NguoiDung> userManager)
    {
      _context = context;
      _userManager = userManager;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
      var now = DateTime.UtcNow;
      var today = now.Date;
      var sevenDaysAgo = now.AddDays(-7);
      var thirtyDaysAgo = now.AddDays(-30);

      var stats = new DashboardStatsDto
      {
        // Người dùng
        TongNguoiDung = await _context.Users.CountAsync(),
        NguoiDungMoiHomNay = await _context.Users.CountAsync(u => u.NgayTao >= today),
        NguoiDungMoi7Ngay = await _context.Users.CountAsync(u => u.NgayTao >= sevenDaysAgo),
        NguoiDungMoi30Ngay = await _context.Users.CountAsync(u => u.NgayTao >= thirtyDaysAgo),

        // Bài thi
        TongBaiThi = await _context.BaiThi.CountAsync(bt => !bt.DaXoa),
        BaiThiMoiHomNay = await _context.BaiThi.CountAsync(bt => !bt.DaXoa && bt.NgayTao >= today),
        BaiThiMoi7Ngay = await _context.BaiThi.CountAsync(bt => !bt.DaXoa && bt.NgayTao >= sevenDaysAgo),
        BaiThiMoi30Ngay = await _context.BaiThi.CountAsync(bt => !bt.DaXoa && bt.NgayTao >= thirtyDaysAgo),

        // Câu hỏi
        TongCauHoi = await _context.CauHoi.CountAsync(ch => !ch.DaXoa),
        CauHoiMoiHomNay = await _context.CauHoi.CountAsync(ch => !ch.DaXoa && ch.NgayTao >= today),
        CauHoiMoi7Ngay = await _context.CauHoi.CountAsync(ch => !ch.DaXoa && ch.NgayTao >= sevenDaysAgo),
        CauHoiMoi30Ngay = await _context.CauHoi.CountAsync(ch => !ch.DaXoa && ch.NgayTao >= thirtyDaysAgo),

        // Lượt làm bài
        TongLuotLamBai = await _context.LuotLamBai.CountAsync(),
        LuotLamBaiHomNay = await _context.LuotLamBai.CountAsync(l => l.ThoiGianBatDau >= today),
        LuotLamBai7Ngay = await _context.LuotLamBai.CountAsync(l => l.ThoiGianBatDau >= sevenDaysAgo),
        LuotLamBai30Ngay = await _context.LuotLamBai.CountAsync(l => l.ThoiGianBatDau >= thirtyDaysAgo),

        // Báo cáo
        BaoCaoChoDuyet = await _context.BaoCao.CountAsync(bc => bc.TrangThai == "ChoDuyet"),
        BaoCaoDangXuLy = await _context.BaoCao.CountAsync(bc => bc.TrangThai == "DangXuLy"),

        // Top bài thi
        TopBaiThi = await _context.BaiThi
       .Where(bt => !bt.DaXoa)
            .OrderByDescending(bt => bt.TongLuotLamBai)
     .Take(5)
               .Select(bt => new TopBaiThiDto
               {
                 MaBaiThi = bt.MaBaiThi,
                 TieuDe = bt.TieuDe,
                 NguoiTao = _context.Users.Where(u => u.Id == bt.NguoiTaoId).Select(u => u.HoTen).FirstOrDefault() ?? "Unknown",
                 TongLuotLamBai = bt.TongLuotLamBai,
                 DiemTrungBinh = bt.DiemTrungBinh
               })
     .ToListAsync(),

        // Top người tạo
        TopNguoiTao = await _context.Users
              .Select(u => new TopNguoiTaoDto
              {
                NguoiDungId = u.Id,
                HoTen = u.HoTen ?? "Unknown",
                SoBaiThiTao = _context.BaiThi.Count(bt => bt.NguoiTaoId == u.Id && !bt.DaXoa),
                SoCauHoiTao = _context.CauHoi.Count(ch => ch.NguoiTaoId == u.Id && !ch.DaXoa),
                TongLuotLamBai = _context.BaiThi.Where(bt => bt.NguoiTaoId == u.Id).Sum(bt => bt.TongLuotLamBai)
              })
        .Where(u => u.SoBaiThiTao > 0 || u.SoCauHoiTao > 0)
       .OrderByDescending(u => u.TongLuotLamBai)
              .Take(5)
           .ToListAsync()
      };

      return stats;
    }

    public async Task<(IEnumerable<UserManagementDto> Items, int TotalCount)> GetUsersAsync(
string? searchTerm = null,
    bool? activeStatus = null,
    string? role = null,
        int pageNumber = 1,
        int pageSize = 20)
    {
      var query = _context.Users.AsQueryable();

      // Filter
      if (!string.IsNullOrEmpty(searchTerm))
      {
        query = query.Where(u =>
  u.UserName!.Contains(searchTerm) ||
         u.Email!.Contains(searchTerm) ||
  (u.HoTen != null && u.HoTen.Contains(searchTerm)));
      }

      if (activeStatus.HasValue)
      {
        query = query.Where(u => u.TrangThaiKichHoat == activeStatus.Value);
      }

      if (!string.IsNullOrEmpty(role))
      {
        // Tìm RoleId dựa trên tên Role (Admin/User)
        var targetRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == role);
        if (targetRole != null)
        {
          // Lọc những user có UserId nằm trong bảng UserRoles ứng với RoleId này
          query = query.Where(u => _context.UserRoles
              .Any(ur => ur.UserId == u.Id && ur.RoleId == targetRole.Id));
        }
      }

      var totalCount = await query.CountAsync();

      var users = await query
            .OrderByDescending(u => u.NgayTao)
        .Skip((pageNumber - 1) * pageSize)
      .Take(pageSize)
                 .ToListAsync();

      var dtos = new List<UserManagementDto>();
      foreach (var user in users)
      {
        var roles = await _userManager.GetRolesAsync(user);
        dtos.Add(new UserManagementDto
        {
          Id = user.Id,
          UserName = user.UserName,
          Email = user.Email,
          HoTen = user.HoTen,
          AnhDaiDien = user.AnhDaiDien,
          NgayTao = user.NgayTao,
          LanDangNhapCuoi = user.LanDangNhapCuoi,
          TrangThaiKichHoat = user.TrangThaiKichHoat,
          Roles = roles.ToList(),
          SoBaiThiTao = await _context.BaiThi.CountAsync(bt => bt.NguoiTaoId == user.Id && !bt.DaXoa),
          SoCauHoiTao = await _context.CauHoi.CountAsync(ch => ch.NguoiTaoId == user.Id && !ch.DaXoa),
          SoLuotLamBai = await _context.LuotLamBai.CountAsync(l => l.NguoiDungId == user.Id)
        });
      }

      return (dtos, totalCount);
    }

    public async Task<UserManagementDto?> GetUserByIdAsync(string userId)
    {
      var user = await _context.Users.FindAsync(userId);
      if (user == null)
        return null;

      var roles = await _userManager.GetRolesAsync(user);

      return new UserManagementDto
      {
        Id = user.Id,
        UserName = user.UserName,
        Email = user.Email,
        HoTen = user.HoTen,
        AnhDaiDien = user.AnhDaiDien,
        NgayTao = user.NgayTao,
        LanDangNhapCuoi = user.LanDangNhapCuoi,
        TrangThaiKichHoat = user.TrangThaiKichHoat,
        Roles = roles.ToList(),
        SoBaiThiTao = await _context.BaiThi.CountAsync(bt => bt.NguoiTaoId == user.Id && !bt.DaXoa),
        SoCauHoiTao = await _context.CauHoi.CountAsync(ch => ch.NguoiTaoId == user.Id && !ch.DaXoa),
        SoLuotLamBai = await _context.LuotLamBai.CountAsync(l => l.NguoiDungId == user.Id)
      };
    }

    public async Task<bool> ToggleUserStatusAsync(string userId)
    {
      var user = await _context.Users.FindAsync(userId);
      if (user == null)
        return false;

      user.TrangThaiKichHoat = !user.TrangThaiKichHoat;
      await _context.SaveChangesAsync();

      return true;
    }

    public async Task<(bool Success, string Message)> UpdateUserRoleAsync(string userId, string role)
    {
      var user = await _context.Users.FindAsync(userId);
      if (user == null)
        return (false, "Không tìm thấy người dùng");

      // Validate role
      if (role != "Admin" && role != "User")
        return (false, "Vai trò không hợp lệ");

      // Get current roles
      var currentRoles = await _userManager.GetRolesAsync(user);

      // Remove all current roles
      if (currentRoles.Any())
      {
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
      }

      // Add new role
      var result = await _userManager.AddToRoleAsync(user, role);
      if (!result.Succeeded)
      {
        return (false, "Không thể cập nhật vai trò: " + string.Join(", ", result.Errors.Select(e => e.Description)));
      }

      return (true, "Cập nhật vai trò thành công");
    }

    public async Task<(bool Success, string Message)> DeleteUserAsync(string userId)
    {
      var user = await _context.Users.FindAsync(userId);
      if (user == null)
        return (false, "Không tìm thấy người dùng");

      // Kiểm tra xem user có phải là Admin không
      var roles = await _userManager.GetRolesAsync(user);
      if (roles.Contains("Admin"))
      {
        // Đếm số admin còn lại
        var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole != null)
        {
          var adminCount = await _context.UserRoles.CountAsync(ur => ur.RoleId == adminRole.Id);
          if (adminCount <= 1)
          {
            return (false, "Không thể xóa Admin cuối cùng của hệ thống");
          }
        }
      }

      // Xóa các dữ liệu liên quan (soft delete hoặc chuyển ownership)
      // Xóa lượt làm bài của user
      var luotLamBais = await _context.LuotLamBai.Where(l => l.NguoiDungId == userId).ToListAsync();
      _context.LuotLamBai.RemoveRange(luotLamBais);

      // Xóa user
      var result = await _userManager.DeleteAsync(user);
      if (!result.Succeeded)
      {
        return (false, "Không thể xóa người dùng: " + string.Join(", ", result.Errors.Select(e => e.Description)));
      }

      return (true, "Xóa người dùng thành công");
    }

    public async Task<(IEnumerable<ContentManagementDto> Items, int TotalCount)> GetAllContentAsync(
    string? loaiNoiDung = null,
    string? searchTerm = null,
    string? tenNguoiTao = null,
    bool? daXoa = null,
    string? sortBy = "NgayTao",
    string? sortOrder = "DESC",
    Guid? maDanhMuc = null,
    int pageNumber = 1,
    int pageSize = 20)
    {
      if (loaiNoiDung == "BaiThi")
      {
        var query = _context.BaiThi.AsQueryable();

        // 1. Filter
        if (!string.IsNullOrEmpty(searchTerm))
          query = query.Where(bt => bt.TieuDe.Contains(searchTerm));

        if (!string.IsNullOrEmpty(tenNguoiTao))
          query = query.Where(bt =>
          _context.Users.Where(u => u.Id == bt.NguoiTaoId)
            .Select(u => u.HoTen)
            .FirstOrDefault()
            .Contains(tenNguoiTao));

        if (daXoa.HasValue)
          query = query.Where(bt => bt.DaXoa == daXoa.Value);

        if (maDanhMuc.HasValue)
          query = query.Where(bt => bt.MaDanhMuc == maDanhMuc.Value);

        // Sort (Sắp xếp ngay trên Query)
        bool isDesc = sortOrder?.ToUpper() == "DESC";
        query = sortBy switch
        {
          "TongLuotLamBai" => isDesc ? query.OrderByDescending(b => b.TongLuotLamBai) : query.OrderBy(b => b.TongLuotLamBai),
          "XepHangTrungBinh" => isDesc ? query.OrderByDescending(b => b.XepHangTrungBinh) : query.OrderBy(b => b.XepHangTrungBinh),
          _ => isDesc ? query.OrderByDescending(b => b.NgayTao) : query.OrderBy(b => b.NgayTao)
        };

        // Đếm tổng số bản ghi thỏa mãn điều kiện
        var totalCount = await query.CountAsync();

        // Lấy trang hiện tại và map sang DTO
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(bt => new ContentManagementDto
            {
              Id = bt.MaBaiThi,
              LoaiNoiDung = "BaiThi",
              TieuDe = bt.TieuDe,
              NguoiTaoId = bt.NguoiTaoId,
              // Lấy tên người tạo bằng Subquery (Nhanh hơn join nhiều lần)
              TenNguoiTao = _context.Users.Where(u => u.Id == bt.NguoiTaoId).Select(u => u.HoTen).FirstOrDefault() ?? "Unknown",
              NgayTao = bt.NgayTao,
              TrangThai = bt.TrangThai,
              DaXoa = bt.DaXoa,
              CheDoCongKhai = bt.CheDoCongKhai,
              TongLuotLamBai = bt.TongLuotLamBai,
              XepHangTrungBinh = bt.XepHangTrungBinh,
              DanhMuc = bt.DanhMuc != null ? bt.DanhMuc.TenDanhMuc : null
            })
            .ToListAsync();

        return (items, totalCount);
      }

      else if (loaiNoiDung == "CauHoi")
      {
        var query = _context.CauHoi.AsQueryable();

        // Filter
        if (!string.IsNullOrEmpty(searchTerm))
          query = query.Where(ch => ch.NoiDungCauHoi.Contains(searchTerm));

        if (!string.IsNullOrEmpty(tenNguoiTao))
          query = query.Where(bt =>
          _context.Users.Where(u => u.Id == bt.NguoiTaoId)
            .Select(u => u.HoTen)
            .FirstOrDefault()
            .Contains(tenNguoiTao));

        if (daXoa.HasValue)
          query = query.Where(ch => ch.DaXoa == daXoa.Value);

        if (maDanhMuc.HasValue)
        {
          query = query.Where(ch => ch.MaDanhMuc == maDanhMuc.Value);
        }

        // Sort
        bool isDesc = sortOrder?.ToUpper() == "DESC";
        // Câu hỏi thường chỉ sort theo ngày tạo
        query = isDesc ? query.OrderByDescending(c => c.NgayTao) : query.OrderBy(c => c.NgayTao);

        // Count
        var totalCount = await query.CountAsync();

        // Paging & Projection
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(ch => new ContentManagementDto
            {
              Id = ch.MaCauHoi,
              LoaiNoiDung = "CauHoi",
              // Cắt ngắn nội dung nếu quá dài
              TieuDe = ch.NoiDungCauHoi.Length > 100 ? ch.NoiDungCauHoi.Substring(0, 100) + "..." : ch.NoiDungCauHoi,
              NguoiTaoId = ch.NguoiTaoId,
              TenNguoiTao = _context.Users.Where(u => u.Id == ch.NguoiTaoId).Select(u => u.HoTen).FirstOrDefault() ?? "Unknown",
              NgayTao = ch.NgayTao,
              // Logic hiển thị trạng thái
              TrangThai = ch.DaXoa ? "DaXoa" : (ch.CongKhai ? "CongKhai" : "RiengTu"),
              DaXoa = ch.DaXoa,
              DanhMuc = ch.MaDanhMuc != null ? ch.DanhMuc.TenDanhMuc : null,
            })
            .ToListAsync();

        return (items, totalCount);
      }

      // Trường hợp không truyền gì cả
      return (new List<ContentManagementDto>(), 0);
    }

    public async Task<CauHoiDto?> GetQuestionAndAnswerByAdminAsync(Guid id)
    {
      return await _context.CauHoi
          .Include(ch => ch.DanhMuc)
          .Include(ch => ch.CacLuaChon)
          .Where(ch => ch.MaCauHoi == id && ch.DaXoa == false)
          .Select(ch => new CauHoiDto
          {
            MaCauHoi = ch.MaCauHoi,
            NoiDungCauHoi = ch.NoiDungCauHoi,
            GiaiThich = ch.GiaiThich,
            MaDanhMuc = ch.MaDanhMuc,
            TenDanhMuc = ch.DanhMuc != null ? ch.DanhMuc.TenDanhMuc : null,
            MucDo = ch.MucDo,
            LoaiCauHoi = ch.LoaiCauHoi,
            CongKhai = ch.CongKhai,
            NguonNhap = ch.NguonNhap,
            NgayTao = ch.NgayTao,
            NgayCapNhat = ch.NgayCapNhat,
            CacLuaChon = ch.CacLuaChon
                  .OrderBy(lc => lc.ThuTu)
                  .Select(lc => new LuaChonDapAnDto
                  {
                    MaLuaChon = lc.MaLuaChon,
                    NoiDungDapAn = lc.NoiDungDapAn,
                    LaDapAnDung = lc.LaDapAnDung,
                    ThuTu = lc.ThuTu
                  })
                  .ToList()
          })
          .FirstOrDefaultAsync();
    }

    public async Task<bool> DeleteContentAsync(string loaiNoiDung, Guid id, bool hardDelete = false)
    {
      if (loaiNoiDung == "BaiThi")
      {
        var baiThi = await _context.BaiThi.FindAsync(id);
        if (baiThi == null)
          return false;

        if (hardDelete)
        {
          _context.BaiThi.Remove(baiThi);
        }
        else
        {
          baiThi.DaXoa = true;
        }
      }
      else if (loaiNoiDung == "CauHoi")
      {
        var cauHoi = await _context.CauHoi.FindAsync(id);
        if (cauHoi == null)
          return false;

        if (hardDelete)
        {
          _context.CauHoi.Remove(cauHoi);
        }
        else
        {
          cauHoi.DaXoa = true;
        }
      }
      else
      {
        return false;
      }

      await _context.SaveChangesAsync();
      return true;
    }

    public async Task<bool> RestoreContentAsync(string loaiNoiDung, Guid id)
    {
      if (loaiNoiDung == "BaiThi")
      {
        var baiThi = await _context.BaiThi.FindAsync(id);
        if (baiThi == null)
          return false;

        baiThi.DaXoa = false;
      }
      else if (loaiNoiDung == "CauHoi")
      {
        var cauHoi = await _context.CauHoi.FindAsync(id);
        if (cauHoi == null)
          return false;

        cauHoi.DaXoa = false;
      }
      else
      {
        return false;
      }

      await _context.SaveChangesAsync();
      return true;
    }

    public async Task<int> RecalculateAllRatingsAsync()
    {
      // Lấy tất cả bài thi
      var baiThiList = await _context.BaiThi.ToListAsync();
      int count = 0;

      foreach (var baiThi in baiThiList)
      {
        // Tính điểm trung bình từ bảng DanhGia
        var avgRating = await _context.DanhGia
          .Where(dg => dg.MaBaiThi == baiThi.MaBaiThi)
          .AverageAsync(dg => (decimal?)dg.XepHang);

        baiThi.XepHangTrungBinh = avgRating ?? 0;
        count++;
      }

      await _context.SaveChangesAsync();
      return count;
    }
  }
}
