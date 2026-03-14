using Microsoft.EntityFrameworkCore;
using QuizHub.Data;
using QuizHub.Models.DTOs.BaiThi;
using QuizHub.Models.Entities;
using QuizHub.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace QuizHub.Services.Implementations
{
    public class BaiThiService : IBaiThiService
    {
        private readonly QuizHubDbContext _context;

        public BaiThiService(QuizHubDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<BaiThiSummaryDto> Items, int TotalCount)> GetAllAsync(
            BaiThiFilterDto filter,
            string? userId)
        {
            var query = _context.BaiThi
                .Include(bt => bt.CacCauHoi)
                .Include(bt => bt.DanhMuc)
                .Where(bt => bt.DaXoa == false);

            // Filter: Chỉ lấy bài thi công khai/có mật khẩu HOẶC bài thi của user hiện tại
            if (!string.IsNullOrEmpty(userId))
            {
                if (filter.ChiLayCuaToi == true)
                {
                    query = query.Where(bt => bt.NguoiTaoId == userId);
                }
                else
                {
                    query = query.Where(bt => bt.CheDoCongKhai == "CongKhai" || 
                                              bt.CheDoCongKhai == "CoMatKhau" || 
                                              bt.NguoiTaoId == userId);
                }
            }
            else
            {
                // Anonymous user chỉ xem được public hoặc có mật khẩu
                query = query.Where(bt => bt.CheDoCongKhai == "CongKhai" || bt.CheDoCongKhai == "CoMatKhau");
            }

            // Filter: Theo chế độ công khai (cho phép filter đa chế độ với dấu phẩy)
            if (!string.IsNullOrEmpty(filter.CheDoCongKhai))
            {
                var modes = filter.CheDoCongKhai.Split(',').Select(m => m.Trim()).ToList();
                query = query.Where(bt => modes.Contains(bt.CheDoCongKhai));
            }

            // Filter: Theo trạng thái
            if (!string.IsNullOrEmpty(filter.TrangThai))
            {
                query = query.Where(bt => bt.TrangThai == filter.TrangThai);
            }

            // Filter: Theo danh mục
            if (filter.MaDanhMuc.HasValue)
            {
                query = query.Where(bt => bt.MaDanhMuc == filter.MaDanhMuc.Value);
            }

            // Search: Tìm kiếm theo tiêu đề (chỉ tìm theo tên bài thi)
            if (!string.IsNullOrEmpty(filter.TimKiem))
            {
                query = query.Where(bt => bt.TieuDe.Contains(filter.TimKiem));
            }

            // Sorting
            query = filter.SortBy?.ToLower() switch
            {
                "luotxem" => filter.SortOrder?.ToUpper() == "ASC"
                    ? query.OrderBy(bt => bt.LuotXem)
                    : query.OrderByDescending(bt => bt.LuotXem),
                "tongluotlambai" => filter.SortOrder?.ToUpper() == "ASC"
                    ? query.OrderBy(bt => bt.TongLuotLamBai)
                    : query.OrderByDescending(bt => bt.TongLuotLamBai),
                "xephangtrungbinh" => filter.SortOrder?.ToUpper() == "ASC"
                    ? query.OrderBy(bt => bt.XepHangTrungBinh ?? 0)
                            .ThenByDescending(bt => bt.NgayTao)
                    : query.OrderByDescending(bt => bt.XepHangTrungBinh ?? 0)
                            .ThenByDescending(bt => bt.NgayTao),
                _ => filter.SortOrder?.ToUpper() == "ASC"
                    ? query.OrderBy(bt => bt.NgayTao)
                    : query.OrderByDescending(bt => bt.NgayTao)
            };

            var totalCount = await query.CountAsync();

            // Load user data trước để tránh N+1 query
            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            // Lấy danh sách NguoiTaoId unique
            var creatorIds = items.Select(bt => bt.NguoiTaoId).Distinct().ToList();
            
            // Query tất cả users một lần duy nhất
            var creators = await _context.Users
                .Where(u => creatorIds.Contains(u.Id))
                .Select(u => new { u.Id, Name = u.HoTen ?? u.Email!, u.AnhDaiDien })
                .ToDictionaryAsync(u => u.Id, u => u);

            // Map kết quả
            var result = items.Select(bt => new BaiThiSummaryDto
            {
                MaBaiThi = bt.MaBaiThi,
                TieuDe = bt.TieuDe,
                MoTa = bt.MoTa,
                AnhBia = bt.AnhBia,
                ThoiGianLamBai = bt.ThoiGianLamBai,
                CheDoCongKhai = bt.CheDoCongKhai,
                TrangThai = bt.TrangThai,
                MaTruyCapDinhDanh = bt.MaTruyCapDinhDanh,
                SoCauHoi = bt.CacCauHoi.Count,
                LuotXem = bt.LuotXem,
                TongLuotLamBai = bt.TongLuotLamBai,
                XepHangTrungBinh = bt.XepHangTrungBinh,
                NgayTao = bt.NgayTao,
                TenNguoiTao = creators.ContainsKey(bt.NguoiTaoId) 
                    ? creators[bt.NguoiTaoId].Name
                    : "Unknown",
                AnhDaiDienNguoiTao = creators.ContainsKey(bt.NguoiTaoId) 
                    ? creators[bt.NguoiTaoId].AnhDaiDien
                    : null,
                MaDanhMuc = bt.MaDanhMuc,
                TenDanhMuc = bt.DanhMuc?.TenDanhMuc
            }).ToList();

            return (result, totalCount);
        }

        public async Task<BaiThiDto?> GetByIdAsync(Guid id, string? userId)
        {
            var baiThi = await _context.BaiThi
                .Include(bt => bt.CacCauHoi)
                    .ThenInclude(chtbt => chtbt.CauHoi)
                        .ThenInclude(ch => ch.CacLuaChon)
                .Include(bt => bt.DanhMuc)
                .Where(bt => bt.MaBaiThi == id && bt.DaXoa == false)
                .FirstOrDefaultAsync();

            if (baiThi == null)
                return null;

            // Kiểm tra quyền xem
            if (baiThi.CheDoCongKhai == "RiengTu" && baiThi.NguoiTaoId != userId)
                return null;

            // Tăng lượt xem bằng SQL atomic operation thay vì ++
            if (baiThi.NguoiTaoId != userId)
            {
                await _context.Database.ExecuteSqlInterpolatedAsync($@"
                    UPDATE BaiThi
                    SET LuotXem = LuotXem + 1
                    WHERE MaBaiThi = {id}
                ");
                
                // Refresh entity để có giá trị mới
                await _context.Entry(baiThi).ReloadAsync();
            }

            var nguoiTao = await _context.Users.FindAsync(baiThi.NguoiTaoId);

            return new BaiThiDto
            {
                MaBaiThi = baiThi.MaBaiThi,
                TieuDe = baiThi.TieuDe,
                MoTa = baiThi.MoTa,
                AnhBia = baiThi.AnhBia,
                ThoiGianLamBai = baiThi.ThoiGianLamBai,
                DiemDat = baiThi.DiemDat,
                CheDoCongKhai = baiThi.CheDoCongKhai,
                TrangThai = baiThi.TrangThai,
                HienThiDapAnSauKhiNop = baiThi.HienThiDapAnSauKhiNop,
                ChoPhepXemLai = baiThi.ChoPhepXemLai,
                MaTruyCapDinhDanh = baiThi.MaTruyCapDinhDanh,
                CoMatKhau = !string.IsNullOrEmpty(baiThi.MatKhau),
                LuotXem = baiThi.LuotXem,
                TongLuotLamBai = baiThi.TongLuotLamBai,
                XepHangTrungBinh = baiThi.XepHangTrungBinh,
                NgayTao = baiThi.NgayTao,
                NgayCapNhat = baiThi.NgayCapNhat,
                NguoiTaoId = baiThi.NguoiTaoId,
                TenNguoiTao = nguoiTao?.HoTen ?? nguoiTao?.Email ?? "Unknown",
                AnhDaiDienNguoiTao = nguoiTao?.AnhDaiDien,
                MaDanhMuc = baiThi.MaDanhMuc,
                TenDanhMuc = baiThi.DanhMuc?.TenDanhMuc,
                SoCauHoi = baiThi.CacCauHoi.Count,
                CacCauHoi = baiThi.CacCauHoi
                    .OrderBy(chtbt => chtbt.ThuTu)
                    .Select(chtbt => new CauHoiTrongBaiThiDetailDto
                    {
                        MaCauHoi = chtbt.MaCauHoi,
                        NoiDungCauHoi = chtbt.CauHoi.NoiDungCauHoi,
                        Diem = chtbt.Diem,
                        ThuTu = chtbt.ThuTu
                    })
                    .ToList()
            };
        }

        public async Task<BaiThiDto?> GetByAccessCodeAsync(string accessCode, string? userId)
        {
            var baiThi = await _context.BaiThi
                .FirstOrDefaultAsync(bt => bt.MaTruyCapDinhDanh == accessCode && bt.DaXoa == false);

            if (baiThi == null)
                return null;

            return await GetByIdAsync(baiThi.MaBaiThi, userId);
        }

        public async Task<BaiThiDto> CreateAsync(CreateBaiThiDto dto, string userId)
        {
            // Validate: Tất cả câu hỏi phải tồn tại và thuộc về user
            var cauHoiIds = dto.CacCauHoi.Select(ch => ch.MaCauHoi).ToList();
            var existingCauHoi = await _context.CauHoi
                .Where(ch => cauHoiIds.Contains(ch.MaCauHoi) &&
                            ch.DaXoa == false &&
                            (ch.NguoiTaoId == userId || ch.CongKhai == true))
                .ToListAsync();

            if (existingCauHoi.Count != cauHoiIds.Distinct().Count())
            {
                throw new InvalidOperationException("Một số câu hỏi không tồn tại hoặc bạn không có quyền sử dụng");
            }

            // Tạo mã truy cập duy nhất
            var accessCode = await GenerateUniqueAccessCode();

            // Hash mật khẩu nếu có
            string? hashedPassword = null;
            if (!string.IsNullOrEmpty(dto.MatKhau))
            {
                hashedPassword = HashPassword(dto.MatKhau);
            }

            var baiThi = new QuizHub.Models.Entities.BaiThi
            {
                TieuDe = dto.TieuDe,
                MoTa = dto.MoTa,
                AnhBia = dto.AnhBia,
                MaDanhMuc = dto.MaDanhMuc,
                ThoiGianLamBai = dto.ThoiGianLamBai,
                DiemDat = dto.DiemDat,
                CheDoCongKhai = dto.CheDoCongKhai,
                TrangThai = "XuatBan", // Tự động xuất bản khi tạo
                HienThiDapAnSauKhiNop = dto.HienThiDapAnSauKhiNop,
                ChoPhepXemLai = dto.ChoPhepXemLai,
                MaTruyCapDinhDanh = accessCode,
                MatKhau = hashedPassword,
                NguoiTaoId = userId,
                NgayTao = DateTime.UtcNow,
                NgayCapNhat = DateTime.UtcNow,
                DaXoa = false
            };

            // Thêm câu hỏi vào bài thi
            foreach (var cauHoiDto in dto.CacCauHoi)
            {
                baiThi.CacCauHoi.Add(new CauHoiTrongBaiThi
                {
                    MaCauHoi = cauHoiDto.MaCauHoi,
                    Diem = cauHoiDto.Diem,
                    ThuTu = cauHoiDto.ThuTu
                });
            }

            _context.BaiThi.Add(baiThi);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(baiThi.MaBaiThi, userId)
                ?? throw new Exception("Không thể lấy bài thi vừa tạo");
        }

        public async Task<BaiThiDto?> UpdateAsync(Guid id, UpdateBaiThiDto dto, string userId)
        {
            var baiThi = await _context.BaiThi
                .Include(bt => bt.CacCauHoi)
                .FirstOrDefaultAsync(bt => bt.MaBaiThi == id && bt.DaXoa == false);

            if (baiThi == null)
                return null;

            // Kiểm tra quyền sở hữu
            if (baiThi.NguoiTaoId != userId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền chỉnh sửa bài thi này");
            }

            // Không cho phép sửa nếu đã có người làm bài VÀ đang thay đổi câu hỏi
            if (baiThi.TongLuotLamBai > 0 && dto.CacCauHoi.Count > 0)
            {
                throw new InvalidOperationException("Không thể chỉnh sửa câu hỏi của bài thi đã có người làm bài");
            }

            // Chỉ validate câu hỏi nếu có thay đổi
            if (dto.CacCauHoi.Count > 0)
            {
                var cauHoiIds = dto.CacCauHoi.Select(ch => ch.MaCauHoi).ToList();
                var existingCauHoi = await _context.CauHoi
                    .Where(ch => cauHoiIds.Contains(ch.MaCauHoi) &&
                                ch.DaXoa == false &&
                                (ch.NguoiTaoId == userId || ch.CongKhai == true))
                    .ToListAsync();

                if (existingCauHoi.Count != cauHoiIds.Distinct().Count())
                {
                    throw new InvalidOperationException("Một số câu hỏi không tồn tại hoặc bạn không có quyền sử dụng");
                }
            }

            // Cập nhật thông tin
            baiThi.TieuDe = dto.TieuDe;
            baiThi.MoTa = dto.MoTa;
            baiThi.AnhBia = dto.AnhBia;
            baiThi.MaDanhMuc = dto.MaDanhMuc;
            baiThi.ThoiGianLamBai = dto.ThoiGianLamBai;
            baiThi.DiemDat = dto.DiemDat;
            baiThi.CheDoCongKhai = dto.CheDoCongKhai;
            baiThi.HienThiDapAnSauKhiNop = dto.HienThiDapAnSauKhiNop;
            baiThi.ChoPhepXemLai = dto.ChoPhepXemLai;
            baiThi.NgayCapNhat = DateTime.UtcNow;

            // Cập nhật mật khẩu nếu có
            if (!string.IsNullOrEmpty(dto.MatKhau))
            {
                baiThi.MatKhau = HashPassword(dto.MatKhau);
            }
            else
            {
                baiThi.MatKhau = null;
            }

            // Chỉ cập nhật câu hỏi nếu có gửi danh sách
            if (dto.CacCauHoi.Count > 0)
            {
                // Xóa câu hỏi cũ và thêm mới
                _context.CauHoiTrongBaiThi.RemoveRange(baiThi.CacCauHoi);
                baiThi.CacCauHoi.Clear();

                foreach (var cauHoiDto in dto.CacCauHoi)
                {
                    baiThi.CacCauHoi.Add(new CauHoiTrongBaiThi
                    {
                        MaCauHoi = cauHoiDto.MaCauHoi,
                        Diem = cauHoiDto.Diem,
                        ThuTu = cauHoiDto.ThuTu
                    });
                }
            }

            await _context.SaveChangesAsync();

            return await GetByIdAsync(id, userId);
        }

        public async Task<bool> DeleteAsync(Guid id, string userId)
        {
            var baiThi = await _context.BaiThi.FindAsync(id);

            if (baiThi == null || baiThi.DaXoa)
                return false;

            // Kiểm tra quyền sở hữu
            if (baiThi.NguoiTaoId != userId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xóa bài thi này");
            }

            // Cho phép xóa bài thi đã có người làm (soft delete)
            // Soft delete
            baiThi.DaXoa = true;
            baiThi.NgayCapNhat = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PublishAsync(Guid id, string userId)
        {
            var baiThi = await _context.BaiThi.FindAsync(id);

            if (baiThi == null || baiThi.DaXoa)
                return false;

            // Kiểm tra quyền sở hữu
            if (baiThi.NguoiTaoId != userId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xuất bản bài thi này");
            }

            // Kiểm tra bài thi có câu hỏi chưa
            var soCauHoi = await _context.CauHoiTrongBaiThi
                .CountAsync(chtbt => chtbt.MaBaiThi == id);

            if (soCauHoi == 0)
            {
                throw new InvalidOperationException("Không thể xuất bản bài thi chưa có câu hỏi");
            }

            baiThi.TrangThai = "XuatBan";
            baiThi.NgayCapNhat = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CloseAsync(Guid id, string userId)
        {
            var baiThi = await _context.BaiThi.FindAsync(id);

            if (baiThi == null || baiThi.DaXoa)
                return false;

            // Kiểm tra quyền sở hữu
            if (baiThi.NguoiTaoId != userId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền đóng bài thi này");
            }

            baiThi.TrangThai = "DaDong";
            baiThi.NgayCapNhat = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string?> ToggleVisibilityAsync(Guid id, string userId)
        {
            var baiThi = await _context.BaiThi.FindAsync(id);

            if (baiThi == null || baiThi.DaXoa)
                return null;

            // Kiểm tra quyền sở hữu
            if (baiThi.NguoiTaoId != userId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền thay đổi chế độ bài thi này");
            }

            // Toggle chế độ
            baiThi.CheDoCongKhai = baiThi.CheDoCongKhai == "CongKhai" ? "RiengTu" : "CongKhai";
            baiThi.NgayCapNhat = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return baiThi.CheDoCongKhai;
        }

        public async Task<BaiThiStatisticsDto?> GetStatisticsAsync(Guid id, string userId)
        {
            var baiThi = await _context.BaiThi.FindAsync(id);

            if (baiThi == null || baiThi.DaXoa)
                return null;

            // Kiểm tra quyền sở hữu
            if (baiThi.NguoiTaoId != userId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xem thống kê bài thi này");
            }

            var luotLamBai = await _context.LuotLamBai
                .Where(llb => llb.MaBaiThi == id)
                .ToListAsync();

            var luotHoanThanh = luotLamBai.Count(llb => llb.TrangThai == "HoanThanh");
            var diemList = luotLamBai.Where(llb => llb.Diem.HasValue).Select(llb => llb.Diem!.Value).ToList();

            var soNguoiDat = baiThi.DiemDat.HasValue
                ? diemList.Count(d => d >= baiThi.DiemDat.Value)
                : 0;

            var tyLeDat = luotHoanThanh > 0
                ? (decimal)soNguoiDat / luotHoanThanh * 100
                : 0;

            // Lấy top 10 người làm bài có điểm cao nhất từ database.
            var topLuotLamBaiTho = await _context.LuotLamBai
                .Where(llb => llb.MaBaiThi == id && llb.TrangThai == "HoanThanh")
                .OrderByDescending(llb => llb.Diem)
                .ThenBy(llb => llb.ThoiGianLamBaiThucTe)
                .Take(10)
                .Select(llb => new
                {
                    // Lấy các thuộc tính cần thiết
                    llb.TenNguoiThamGia,
                    llb.NguoiDungId,
                    llb.Diem,
                    llb.ThoiGianLamBaiThucTe,
                    llb.ThoiGianNopBai
                })
                .ToListAsync(); // Thực thi truy vấn và lấy dữ liệu về client

            // Tạo một danh sách người dùng ID cần tra cứu để tối ưu hóa
            var userIds = topLuotLamBaiTho
                .Where(llb => llb.NguoiDungId != null && llb.TenNguoiThamGia == null)
                .Select(llb => llb.NguoiDungId)
                .Distinct()
                .ToList();

            // Chỉ truy vấn database một lần để lấy thông tin tất cả user cần thiết
            var usersInfo = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.HoTen ?? u.Email!);

            // Ánh xạ kết quả cuối cùng
            var topNguoiLamBai = topLuotLamBaiTho
                .Select((llb, index) => new TopNguoiLamBaiDto
                {
                    TenNguoiThamGia = llb.TenNguoiThamGia ??
                                      (llb.NguoiDungId != null && usersInfo.ContainsKey(llb.NguoiDungId) ? usersInfo[llb.NguoiDungId] : "Ẩn danh"),
                    Diem = llb.Diem ?? 0,
                    ThoiGianLamBai = llb.ThoiGianLamBaiThucTe ?? 0,
                    ThoiGianNopBai = llb.ThoiGianNopBai ?? DateTime.UtcNow,
                    XepHang = index + 1 // Bây giờ có thể dùng index vì đang chạy trong bộ nhớ
                })
                .ToList();

            return new BaiThiStatisticsDto
            {
                MaBaiThi = id,
                TieuDe = baiThi.TieuDe,
                TongLuotLamBai = luotLamBai.Count,
                LuotHoanThanh = luotHoanThanh,
                LuotDangLam = luotLamBai.Count(llb => llb.TrangThai == "DangLam"),
                DiemTrungBinh = diemList.Any() ? diemList.Average() : null,
                DiemCaoNhat = diemList.Any() ? diemList.Max() : null,
                DiemThapNhat = diemList.Any() ? diemList.Min() : null,
                SoNguoiDat = soNguoiDat,
                TyLeDat = tyLeDat,
                TopNguoiLamBai = topNguoiLamBai
            };
        }

        public async Task<int> CountByUserAsync(string userId)
        {
            return await _context.BaiThi
                .CountAsync(bt => bt.NguoiTaoId == userId && bt.DaXoa == false);
        }

        public async Task<bool> VerifyPasswordAsync(Guid id, string password)
        {
            var baiThi = await _context.BaiThi
                .FirstOrDefaultAsync(bt => bt.MaBaiThi == id && bt.DaXoa == false);

            if (baiThi == null || baiThi.CheDoCongKhai != "CoMatKhau")
            {
                return false;
            }

            // Hash password đầu vào và so sánh với password đã lưu (đã được hash)
            var hashedInputPassword = HashPassword(password);
            return baiThi.MatKhau == hashedInputPassword;
        }

        // Helper Methods
        private async Task<string> GenerateUniqueAccessCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            string code;

            do
            {
                code = new string(Enumerable.Repeat(chars, 8)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            }
            while (await _context.BaiThi.AnyAsync(bt => bt.MaTruyCapDinhDanh == code));

            return code;
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        public async Task<(IEnumerable<NguoiLamBaiDto> Items, int TotalCount, ThongKeNguoiLamBaiDto ThongKe)> GetNguoiLamBaiAsync(
            Guid baiThiId, 
            NguoiLamBaiFilterDto filter, 
            string userId)
        {
            // Kiểm tra quyền sở hữu bài thi
            var baiThi = await _context.BaiThi
                .FirstOrDefaultAsync(bt => bt.MaBaiThi == baiThiId && bt.DaXoa == false);

            if (baiThi == null)
            {
                throw new InvalidOperationException("Không tìm thấy bài thi");
            }

            if (baiThi.NguoiTaoId != userId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xem danh sách người làm bài thi này");
            }

            var query = _context.LuotLamBai
                .Where(llb => llb.MaBaiThi == baiThiId);

            // Filter theo trạng thái
            if (!string.IsNullOrEmpty(filter.TrangThai))
            {
                query = query.Where(llb => llb.TrangThai == filter.TrangThai);
            }

            // Sorting
            query = filter.SortBy?.ToLower() switch
            {
                "diem" => filter.SortOrder?.ToUpper() == "ASC"
                    ? query.OrderBy(llb => llb.Diem ?? 0)
                    : query.OrderByDescending(llb => llb.Diem ?? 0),
                "thoigianlambai" => filter.SortOrder?.ToUpper() == "ASC"
                    ? query.OrderBy(llb => llb.ThoiGianLamBaiThucTe ?? 0)
                    : query.OrderByDescending(llb => llb.ThoiGianLamBaiThucTe ?? 0),
                _ => filter.SortOrder?.ToUpper() == "ASC"
                    ? query.OrderBy(llb => llb.ThoiGianBatDau)
                    : query.OrderByDescending(llb => llb.ThoiGianBatDau)
            };

            var totalCount = await query.CountAsync();

            // Lấy dữ liệu với phân trang
            var rawData = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(llb => new
                {
                    llb.MaLuotLamBai,
                    llb.MaBaiThi,
                    llb.NguoiDungId,
                    llb.TenNguoiThamGia,
                    llb.ThoiGianBatDau,
                    llb.ThoiGianNopBai,
                    llb.ThoiGianLamBaiThucTe,
                    llb.Diem,
                    llb.TongSoCauHoi,
                    llb.SoCauDung,
                    llb.SoCauSai,
                    llb.SoCauChuaLam,
                    llb.TrangThai
                })
                .ToListAsync();

            // Lấy thông tin user riêng
            var userIds = rawData
                .Where(llb => llb.NguoiDungId != null)
                .Select(llb => llb.NguoiDungId!)
                .Distinct()
                .ToList();

            var usersInfo = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.HoTen, u.Email, u.AnhDaiDien })
                .ToDictionaryAsync(u => u.Id);

            // Filter theo tên người làm (sau khi đã lấy users info)
            var filteredData = rawData.AsEnumerable();
            if (!string.IsNullOrEmpty(filter.TimKiem))
            {
                var searchTerm = filter.TimKiem.ToLower();
                filteredData = filteredData.Where(llb =>
                {
                    var tenNguoiLam = llb.TenNguoiThamGia;
                    if (llb.NguoiDungId != null && usersInfo.TryGetValue(llb.NguoiDungId, out var user))
                    {
                        tenNguoiLam = tenNguoiLam ?? user.HoTen ?? user.Email;
                        if (user.Email?.ToLower().Contains(searchTerm) == true)
                            return true;
                    }
                    return tenNguoiLam?.ToLower().Contains(searchTerm) == true;
                });
            }

            var items = filteredData.Select(llb =>
            {
                string tenNguoiLamBai = llb.TenNguoiThamGia ?? "Ẩn danh";
                string? email = null;
                string? anhDaiDien = null;

                if (llb.NguoiDungId != null && usersInfo.TryGetValue(llb.NguoiDungId, out var user))
                {
                    tenNguoiLamBai = llb.TenNguoiThamGia ?? user.HoTen ?? user.Email ?? "Ẩn danh";
                    email = user.Email;
                    anhDaiDien = user.AnhDaiDien;
                }

                return new NguoiLamBaiDto
                {
                    MaLuotLamBai = llb.MaLuotLamBai,
                    MaBaiThi = llb.MaBaiThi,
                    TieuDeBaiThi = baiThi.TieuDe,
                    NguoiDungId = llb.NguoiDungId,
                    TenNguoiLamBai = tenNguoiLamBai,
                    Email = email,
                    AnhDaiDien = anhDaiDien,
                    ThoiGianBatDau = llb.ThoiGianBatDau,
                    ThoiGianNopBai = llb.ThoiGianNopBai,
                    ThoiGianLamBaiThucTe = llb.ThoiGianLamBaiThucTe,
                    Diem = llb.Diem,
                    TongSoCauHoi = llb.TongSoCauHoi,
                    SoCauDung = llb.SoCauDung,
                    SoCauSai = llb.SoCauSai,
                    SoCauChuaLam = llb.SoCauChuaLam,
                    TrangThai = llb.TrangThai,
                    DaDat = baiThi.DiemDat.HasValue && llb.Diem.HasValue && llb.Diem >= baiThi.DiemDat
                };
            }).ToList();

            // Tính thống kê
            var allLuotLamBai = await _context.LuotLamBai
                .Where(llb => llb.MaBaiThi == baiThiId)
                .ToListAsync();

            var diemList = allLuotLamBai.Where(llb => llb.Diem.HasValue && llb.TrangThai == "HoanThanh").Select(llb => llb.Diem!.Value).ToList();

            var thongKe = new ThongKeNguoiLamBaiDto
            {
                TongSoNguoiLamBai = allLuotLamBai.Count,
                SoNguoiHoanThanh = allLuotLamBai.Count(llb => llb.TrangThai == "HoanThanh"),
                SoNguoiDangLam = allLuotLamBai.Count(llb => llb.TrangThai == "DangLam"),
                SoNguoiDat = baiThi.DiemDat.HasValue ? diemList.Count(d => d >= baiThi.DiemDat.Value) : 0,
                SoNguoiKhongDat = baiThi.DiemDat.HasValue ? diemList.Count(d => d < baiThi.DiemDat.Value) : 0,
                DiemTrungBinh = diemList.Any() ? Math.Round(diemList.Average(), 2) : null,
                DiemCaoNhat = diemList.Any() ? diemList.Max() : null,
                DiemThapNhat = diemList.Any() ? diemList.Min() : null
            };

            return (items, totalCount, thongKe);
        }

        public async Task<(IEnumerable<NguoiLamBaiDto> Items, int TotalCount, ThongKeNguoiLamBaiDto ThongKe)> GetAllNguoiLamBaiAsync(
            NguoiLamBaiFilterDto filter, 
            string userId)
        {
            // Lấy tất cả bài thi của user
            var baiThiList = await _context.BaiThi
                .Where(bt => bt.NguoiTaoId == userId && bt.DaXoa == false)
                .Select(bt => new { bt.MaBaiThi, bt.TieuDe, bt.DiemDat })
                .ToListAsync();

            var baiThiIds = baiThiList.Select(bt => bt.MaBaiThi).ToList();
            var baiThiDict = baiThiList.ToDictionary(bt => bt.MaBaiThi);

            if (!baiThiIds.Any())
            {
                return (new List<NguoiLamBaiDto>(), 0, new ThongKeNguoiLamBaiDto());
            }

            var query = _context.LuotLamBai
                .Where(llb => baiThiIds.Contains(llb.MaBaiThi));

            // Filter theo bài thi cụ thể (nếu có)
            if (filter.MaBaiThi.HasValue)
            {
                query = query.Where(llb => llb.MaBaiThi == filter.MaBaiThi.Value);
            }

            // Filter theo trạng thái
            if (!string.IsNullOrEmpty(filter.TrangThai))
            {
                query = query.Where(llb => llb.TrangThai == filter.TrangThai);
            }

            // Sorting
            query = filter.SortBy?.ToLower() switch
            {
                "diem" => filter.SortOrder?.ToUpper() == "ASC"
                    ? query.OrderBy(llb => llb.Diem ?? 0)
                    : query.OrderByDescending(llb => llb.Diem ?? 0),
                "thoigianlambai" => filter.SortOrder?.ToUpper() == "ASC"
                    ? query.OrderBy(llb => llb.ThoiGianLamBaiThucTe ?? 0)
                    : query.OrderByDescending(llb => llb.ThoiGianLamBaiThucTe ?? 0),
                _ => filter.SortOrder?.ToUpper() == "ASC"
                    ? query.OrderBy(llb => llb.ThoiGianBatDau)
                    : query.OrderByDescending(llb => llb.ThoiGianBatDau)
            };

            var totalCount = await query.CountAsync();

            // Lấy dữ liệu với phân trang
            var rawData = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(llb => new
                {
                    llb.MaLuotLamBai,
                    llb.MaBaiThi,
                    llb.NguoiDungId,
                    llb.TenNguoiThamGia,
                    llb.ThoiGianBatDau,
                    llb.ThoiGianNopBai,
                    llb.ThoiGianLamBaiThucTe,
                    llb.Diem,
                    llb.TongSoCauHoi,
                    llb.SoCauDung,
                    llb.SoCauSai,
                    llb.SoCauChuaLam,
                    llb.TrangThai
                })
                .ToListAsync();

            // Lấy thông tin user riêng
            var userIds = rawData
                .Where(llb => llb.NguoiDungId != null)
                .Select(llb => llb.NguoiDungId!)
                .Distinct()
                .ToList();

            var usersInfo = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.HoTen, u.Email, u.AnhDaiDien })
                .ToDictionaryAsync(u => u.Id);

            // Filter theo tên người làm, email (sau khi đã lấy users info)
            var filteredData = rawData.AsEnumerable();
            if (!string.IsNullOrEmpty(filter.TimKiem))
            {
                var searchTerm = filter.TimKiem.ToLower();
                filteredData = filteredData.Where(llb =>
                {
                    var tenNguoiLam = llb.TenNguoiThamGia;
                    if (llb.NguoiDungId != null && usersInfo.TryGetValue(llb.NguoiDungId, out var user))
                    {
                        tenNguoiLam = tenNguoiLam ?? user.HoTen ?? user.Email;
                        if (user.Email?.ToLower().Contains(searchTerm) == true)
                            return true;
                    }
                    return tenNguoiLam?.ToLower().Contains(searchTerm) == true;
                });
            }

            // Filter theo tên bài thi
            if (!string.IsNullOrEmpty(filter.TimKiemBaiThi))
            {
                var searchQuizTerm = filter.TimKiemBaiThi.ToLower();
                filteredData = filteredData.Where(llb =>
                    baiThiDict.TryGetValue(llb.MaBaiThi, out var baiThi) && 
                    baiThi.TieuDe.ToLower().Contains(searchQuizTerm)
                );
            }

            var items = filteredData.Select(llb =>
            {
                string tenNguoiLamBai = llb.TenNguoiThamGia ?? "Ẩn danh";
                string? email = null;
                string? anhDaiDien = null;
                string tieuDeBaiThi = "";
                int? diemDat = null;

                if (baiThiDict.TryGetValue(llb.MaBaiThi, out var baiThi))
                {
                    tieuDeBaiThi = baiThi.TieuDe;
                    diemDat = baiThi.DiemDat;
                }

                if (llb.NguoiDungId != null && usersInfo.TryGetValue(llb.NguoiDungId, out var user))
                {
                    tenNguoiLamBai = llb.TenNguoiThamGia ?? user.HoTen ?? user.Email ?? "Ẩn danh";
                    email = user.Email;
                    anhDaiDien = user.AnhDaiDien;
                }

                return new NguoiLamBaiDto
                {
                    MaLuotLamBai = llb.MaLuotLamBai,
                    MaBaiThi = llb.MaBaiThi,
                    TieuDeBaiThi = tieuDeBaiThi,
                    NguoiDungId = llb.NguoiDungId,
                    TenNguoiLamBai = tenNguoiLamBai,
                    Email = email,
                    AnhDaiDien = anhDaiDien,
                    ThoiGianBatDau = llb.ThoiGianBatDau,
                    ThoiGianNopBai = llb.ThoiGianNopBai,
                    ThoiGianLamBaiThucTe = llb.ThoiGianLamBaiThucTe,
                    Diem = llb.Diem,
                    TongSoCauHoi = llb.TongSoCauHoi,
                    SoCauDung = llb.SoCauDung,
                    SoCauSai = llb.SoCauSai,
                    SoCauChuaLam = llb.SoCauChuaLam,
                    TrangThai = llb.TrangThai,
                    DaDat = diemDat.HasValue && llb.Diem.HasValue && llb.Diem >= diemDat
                };
            }).ToList();

            // Tính thống kê tổng hợp cho tất cả bài thi
            var allLuotLamBai = await _context.LuotLamBai
                .Where(llb => baiThiIds.Contains(llb.MaBaiThi))
                .ToListAsync();

            var diemList = allLuotLamBai.Where(llb => llb.Diem.HasValue && llb.TrangThai == "HoanThanh").Select(llb => llb.Diem!.Value).ToList();
            var soNguoiDat = allLuotLamBai.Count(llb => 
                llb.TrangThai == "HoanThanh" && 
                baiThiDict.TryGetValue(llb.MaBaiThi, out var bt) &&
                bt.DiemDat.HasValue && 
                llb.Diem.HasValue && 
                llb.Diem >= bt.DiemDat);

            var thongKe = new ThongKeNguoiLamBaiDto
            {
                TongSoNguoiLamBai = allLuotLamBai.Count,
                SoNguoiHoanThanh = allLuotLamBai.Count(llb => llb.TrangThai == "HoanThanh"),
                SoNguoiDangLam = allLuotLamBai.Count(llb => llb.TrangThai == "DangLam"),
                SoNguoiDat = soNguoiDat,
                SoNguoiKhongDat = allLuotLamBai.Count(llb => llb.TrangThai == "HoanThanh") - soNguoiDat,
                DiemTrungBinh = diemList.Any() ? Math.Round(diemList.Average(), 2) : null,
                DiemCaoNhat = diemList.Any() ? diemList.Max() : null,
                DiemThapNhat = diemList.Any() ? diemList.Min() : null
            };

            return (items, totalCount, thongKe);
        }

        public async Task<List<CauHoiChiTietDto>?> GetQuestionsDetailAsync(Guid id, string userId)
        {
            var baiThi = await _context.BaiThi
                .Include(bt => bt.CacCauHoi)
                    .ThenInclude(chtbt => chtbt.CauHoi)
                        .ThenInclude(ch => ch.CacLuaChon)
                .Where(bt => bt.MaBaiThi == id && bt.DaXoa == false)
                .FirstOrDefaultAsync();

            if (baiThi == null)
                return null;

            // Chỉ chủ bài thi mới được xem chi tiết đáp án
            if (baiThi.NguoiTaoId != userId)
                return null;

            return baiThi.CacCauHoi
                .OrderBy(chtbt => chtbt.ThuTu)
                .Select(chtbt => new CauHoiChiTietDto
                {
                    MaCauHoi = chtbt.MaCauHoi,
                    NoiDungCauHoi = chtbt.CauHoi.NoiDungCauHoi,
                    GiaiThich = chtbt.CauHoi.GiaiThich,
                    LoaiCauHoi = chtbt.CauHoi.LoaiCauHoi,
                    MucDo = chtbt.CauHoi.MucDo,
                    Diem = chtbt.Diem,
                    ThuTu = chtbt.ThuTu,
                    CacLuaChon = chtbt.CauHoi.CacLuaChon
                        .OrderBy(lc => lc.ThuTu)
                        .Select(lc => new LuaChonChiTietDto
                        {
                            NoiDung = lc.NoiDungDapAn,
                            LaDapAnDung = lc.LaDapAnDung
                        })
                        .ToList()
                })
                .ToList();
        }
    }
}