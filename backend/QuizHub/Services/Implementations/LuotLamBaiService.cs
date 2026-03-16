using Microsoft.EntityFrameworkCore;
using QuizHub.Data;
using QuizHub.Models.DTOs.LuotLamBai;
using QuizHub.Models.Entities;
using QuizHub.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace QuizHub.Services.Implementations
{
    public class LuotLamBaiService : ILuotLamBaiService
    {
        private readonly QuizHubDbContext _context;

        public LuotLamBaiService(QuizHubDbContext context)
        {
            _context = context;
        }

        public async Task<BaiThiSessionDto> StartQuizAsync(StartBaiThiDto dto, string? userId)
        {
            // Lấy thông tin bài thi
            var baiThi = await _context.BaiThi
                .Include(bt => bt.CacCauHoi)
                    .ThenInclude(chtbt => chtbt.CauHoi)
                        .ThenInclude(ch => ch.CacLuaChon)
                .FirstOrDefaultAsync(bt => bt.MaBaiThi == dto.MaBaiThi && bt.DaXoa == false);

            if (baiThi == null)
            {
                throw new InvalidOperationException("Không tìm thấy bài thi");
            }

            // Kiểm tra trạng thái bài thi
            if (baiThi.TrangThai != "XuatBan")
            {
                throw new InvalidOperationException("Bài thi chưa được xuất bản hoặc đã đóng");
            }

            // Kiểm tra quyền truy cập (public/private)
            if (baiThi.CheDoCongKhai == "RiengTu" && baiThi.NguoiTaoId != userId)
            {
                // Kiểm tra mật khẩu nếu có
                if (!string.IsNullOrEmpty(baiThi.MatKhau))
                {
                    if (string.IsNullOrEmpty(dto.MatKhau))
                    {
                        throw new UnauthorizedAccessException("Bài thi này yêu cầu mật khẩu");
                    }

                    var hashedPassword = HashPassword(dto.MatKhau);
                    if (hashedPassword != baiThi.MatKhau)
                    {
                        throw new UnauthorizedAccessException("Mật khẩu không đúng");
                    }
                }
            }

            // Kiểm tra xem user đã làm bài này chưa (nếu không cho phép làm lại)
            if (!string.IsNullOrEmpty(userId))
            {
                var existingAttempt = await _context.LuotLamBai
                    .FirstOrDefaultAsync(llb => llb.MaBaiThi == dto.MaBaiThi &&
                                                llb.NguoiDungId == userId &&
                                                llb.TrangThai == "DangLam");

                if (existingAttempt != null)
                {
                    // Trả về session đang làm dở
                    return await GetSessionAsync(existingAttempt.MaLuotLamBai, userId)
                        ?? throw new Exception("Không thể lấy session hiện tại");
                }
            }

            // Tạo lượt làm bài mới
            var thoiGianBatDau = DateTime.UtcNow;
            var thoiGianKetThuc = thoiGianBatDau.AddMinutes(baiThi.ThoiGianLamBai);

            var luotLamBai = new QuizHub.Models.Entities.LuotLamBai
            {
                MaBaiThi = dto.MaBaiThi,
                NguoiDungId = userId,
                TenNguoiThamGia = dto.TenNguoiThamGia,
                ThoiGianBatDau = thoiGianBatDau,
                ThoiGianKetThuc = thoiGianKetThuc,
                TongSoCauHoi = baiThi.CacCauHoi.Count,
                TrangThai = "DangLam",
                DiaChiIP = null // Sẽ set từ controller
            };

            _context.LuotLamBai.Add(luotLamBai);

            // Tạo các câu trả lời trống
            foreach (var cauHoiTrongBaiThi in baiThi.CacCauHoi)
            {
                _context.CauTraLoi.Add(new CauTraLoi
                {
                    MaLuotLamBai = luotLamBai.MaLuotLamBai,
                    MaCauHoi = cauHoiTrongBaiThi.MaCauHoi,
                    MaLuaChonDaChon = null,
                    LaDapAnDung = false
                });
            }

            await _context.SaveChangesAsync();

            // Trả về session
            return new BaiThiSessionDto
            {
                MaLuotLamBai = luotLamBai.MaLuotLamBai,
                MaBaiThi = baiThi.MaBaiThi,
                TieuDe = baiThi.TieuDe,
                MoTa = baiThi.MoTa,
                ThoiGianLamBai = baiThi.ThoiGianLamBai,
                ThoiGianBatDau = thoiGianBatDau,
                ThoiGianKetThuc = thoiGianKetThuc,
                ThoiGianConLai = (int)(thoiGianKetThuc - DateTime.UtcNow).TotalSeconds,
                CacCauHoi = baiThi.CacCauHoi
                    .OrderBy(chtbt => chtbt.ThuTu)
                    .Select(chtbt => new CauHoiInSessionDto
                    {
                        MaCauHoi = chtbt.MaCauHoi,
                        NoiDungCauHoi = chtbt.CauHoi.NoiDungCauHoi,
                        ThuTu = chtbt.ThuTu,
                        Diem = chtbt.Diem,
                        CacLuaChon = chtbt.CauHoi.CacLuaChon
                            .OrderBy(lc => lc.ThuTu)
                            .Select(lc => new LuaChonInSessionDto
                            {
                                MaLuaChon = lc.MaLuaChon,
                                NoiDungDapAn = lc.NoiDungDapAn,
                                ThuTu = lc.ThuTu
                            })
                            .ToList(),
                        DaChon = null
                    })
                    .ToList()
            };
        }

        public async Task<BaiThiSessionDto?> GetSessionAsync(Guid luotLamBaiId, string? userId)
        {
            var luotLamBai = await _context.LuotLamBai
                .Include(llb => llb.BaiThi)
                    .ThenInclude(bt => bt.CacCauHoi)
                        .ThenInclude(chtbt => chtbt.CauHoi)
                            .ThenInclude(ch => ch.CacLuaChon)
                .Include(llb => llb.CacCauTraLoi)
                .FirstOrDefaultAsync(llb => llb.MaLuotLamBai == luotLamBaiId);

            if (luotLamBai == null)
                return null;

            // Kiểm tra quyền truy cập
            if (luotLamBai.NguoiDungId != userId && !string.IsNullOrEmpty(luotLamBai.NguoiDungId))
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xem session này");
            }

            // Kiểm tra đã nộp bài chưa
            if (luotLamBai.TrangThai != "DangLam")
            {
                throw new InvalidOperationException("Session này đã kết thúc");
            }

            // Kiểm tra hết giờ chưa
            var thoiGianConLai = (int)(luotLamBai.ThoiGianKetThuc!.Value - DateTime.UtcNow).TotalSeconds;
            if (thoiGianConLai <= 0)
            {
                // Tự động nộp bài
                await AutoSubmitAsync(luotLamBaiId);
                throw new InvalidOperationException("Đã hết thời gian làm bài");
            }

            return new BaiThiSessionDto
            {
                MaLuotLamBai = luotLamBai.MaLuotLamBai,
                MaBaiThi = luotLamBai.MaBaiThi,
                TieuDe = luotLamBai.BaiThi.TieuDe,
                MoTa = luotLamBai.BaiThi.MoTa,
                ThoiGianLamBai = luotLamBai.BaiThi.ThoiGianLamBai,
                ThoiGianBatDau = luotLamBai.ThoiGianBatDau,
                ThoiGianKetThuc = luotLamBai.ThoiGianKetThuc!.Value,
                ThoiGianConLai = thoiGianConLai,
                CacCauHoi = luotLamBai.BaiThi.CacCauHoi
                    .OrderBy(chtbt => chtbt.ThuTu)
                    .Select(chtbt => new CauHoiInSessionDto
                    {
                        MaCauHoi = chtbt.MaCauHoi,
                        NoiDungCauHoi = chtbt.CauHoi.NoiDungCauHoi,
                        ThuTu = chtbt.ThuTu,
                        Diem = chtbt.Diem,
                        CacLuaChon = chtbt.CauHoi.CacLuaChon
                            .OrderBy(lc => lc.ThuTu)
                            .Select(lc => new LuaChonInSessionDto
                            {
                                MaLuaChon = lc.MaLuaChon,
                                NoiDungDapAn = lc.NoiDungDapAn,
                                ThuTu = lc.ThuTu
                            })
                            .ToList(),
                        DaChon = luotLamBai.CacCauTraLoi
                            .FirstOrDefault(ctl => ctl.MaCauHoi == chtbt.MaCauHoi)?.MaLuaChonDaChon
                    })
                    .ToList()
            };
        }

        public async Task<bool> SaveAnswerAsync(Guid luotLamBaiId, SubmitCauTraLoiDto dto, string? userId)
        {
            var luotLamBai = await _context.LuotLamBai
                .Include(llb => llb.CacCauTraLoi)
                .FirstOrDefaultAsync(llb => llb.MaLuotLamBai == luotLamBaiId);

            if (luotLamBai == null)
                return false;

            // Kiểm tra quyền
            if (luotLamBai.NguoiDungId != userId && !string.IsNullOrEmpty(luotLamBai.NguoiDungId))
            {
                throw new UnauthorizedAccessException();
            }

            // Kiểm tra trạng thái
            if (luotLamBai.TrangThai != "DangLam")
            {
                throw new InvalidOperationException("Session đã kết thúc");
            }

            // Kiểm tra hết giờ
            if (luotLamBai.ThoiGianKetThuc!.Value <= DateTime.UtcNow)
            {
                await AutoSubmitAsync(luotLamBaiId);
                throw new InvalidOperationException("Đã hết thời gian");
            }

            // Tìm câu trả lời
            var cauTraLoi = luotLamBai.CacCauTraLoi
                .FirstOrDefault(ctl => ctl.MaCauHoi == dto.MaCauHoi);

            if (cauTraLoi == null)
                return false;

            // Kiểm tra đáp án có đúng không
            bool laDapAnDung = false;
            if (dto.MaLuaChonDaChon.HasValue)
            {
                var luaChon = await _context.LuaChonDapAn
                    .FirstOrDefaultAsync(lc => lc.MaLuaChon == dto.MaLuaChonDaChon.Value);
                laDapAnDung = luaChon?.LaDapAnDung ?? false;
            }

            // Cập nhật câu trả lời
            cauTraLoi.MaLuaChonDaChon = dto.MaLuaChonDaChon;
            cauTraLoi.LaDapAnDung = laDapAnDung;
            cauTraLoi.ThoiGianTraLoi = DateTime.UtcNow;
            cauTraLoi.ThoiGianSuDung = dto.ThoiGianSuDung;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<KetQuaBaiThiDto> SubmitQuizAsync(Guid luotLamBaiId, SubmitBaiThiDto dto, string? userId)
        {
            // Sử dụng ExecutionStrategy để tương thích với EnableRetryOnFailure
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var luotLamBai = await _context.LuotLamBai
                        .Include(llb => llb.BaiThi)
                        .Include(llb => llb.CacCauTraLoi)
                        .FirstOrDefaultAsync(llb => llb.MaLuotLamBai == luotLamBaiId);

                    if (luotLamBai == null)
                    {
                        throw new InvalidOperationException("Không tìm thấy lượt làm bài");
                    }

                    // Kiểm tra quyền
                    if (luotLamBai.NguoiDungId != userId && !string.IsNullOrEmpty(luotLamBai.NguoiDungId))
                    {
                        throw new UnauthorizedAccessException();
                    }

                    // Kiểm tra đã nộp chưa
                    if (luotLamBai.TrangThai != "DangLam")
                    {
                        throw new InvalidOperationException("Bài thi đã được nộp");
                    }

                    //  Lưu tất cả câu trả lời trong memory trước
                    foreach (var answer in dto.CacCauTraLoi)
                    {
                        var cauTraLoi = luotLamBai.CacCauTraLoi
                            .FirstOrDefault(ctl => ctl.MaCauHoi == answer.MaCauHoi);

                        if (cauTraLoi != null)
                        {
                            bool laDapAnDung = false;
                            if (answer.MaLuaChonDaChon.HasValue)
                            {
                                var luaChon = await _context.LuaChonDapAn
                                    .FirstOrDefaultAsync(lc => lc.MaLuaChon == answer.MaLuaChonDaChon.Value);
                                laDapAnDung = luaChon?.LaDapAnDung ?? false;
                            }

                            cauTraLoi.MaLuaChonDaChon = answer.MaLuaChonDaChon;
                            cauTraLoi.LaDapAnDung = laDapAnDung;
                            cauTraLoi.ThoiGianTraLoi = DateTime.UtcNow;
                            cauTraLoi.ThoiGianSuDung = answer.ThoiGianSuDung;
                        }
                    }

                    // Tính điểm
                    var soCauDung = luotLamBai.CacCauTraLoi.Count(ctl => ctl.LaDapAnDung);
                    var soCauSai = luotLamBai.CacCauTraLoi.Count(ctl => !ctl.LaDapAnDung && ctl.MaLuaChonDaChon != null);
                    var soCauChuaLam = luotLamBai.CacCauTraLoi.Count(ctl => ctl.MaLuaChonDaChon == null);

                    var diem = luotLamBai.TongSoCauHoi > 0
                        ? (decimal)soCauDung / luotLamBai.TongSoCauHoi * 10
                        : 0;

                    var thoiGianNopBai = DateTime.UtcNow;
                    var thoiGianLamBaiThucTe = (int)(thoiGianNopBai - luotLamBai.ThoiGianBatDau).TotalSeconds;

                    // Cập nhật lượt làm bài
                    luotLamBai.Diem = Math.Round(diem, 2);
                    luotLamBai.SoCauDung = soCauDung;
                    luotLamBai.SoCauSai = soCauSai;
                    luotLamBai.SoCauChuaLam = soCauChuaLam;
                    luotLamBai.ThoiGianNopBai = thoiGianNopBai;
                    luotLamBai.ThoiGianLamBaiThucTe = thoiGianLamBaiThucTe;
                    luotLamBai.TrangThai = "HoanThanh";

                    //  Lưu tất cả changes trong 1 transaction
                    await _context.SaveChangesAsync();

                    // Cập nhật thống kê bài thi (atomic operation)
                    await UpdateQuizStatisticsAsync(luotLamBai.MaBaiThi);

                    //  Commit transaction
                    await transaction.CommitAsync();

                    return await GetResultAsync(luotLamBaiId, userId)
                        ?? throw new Exception("Không thể lấy kết quả");
                }
                catch
                {
                    //  Rollback nếu có lỗi
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<KetQuaBaiThiDto?> GetResultAsync(Guid luotLamBaiId, string? userId)
        {
            var luotLamBai = await _context.LuotLamBai
                .Include(llb => llb.BaiThi)
                    .ThenInclude(bt => bt.CacCauHoi)
                        .ThenInclude(chtbt => chtbt.CauHoi)
                            .ThenInclude(ch => ch.CacLuaChon)
                .Include(llb => llb.CacCauTraLoi)
                .FirstOrDefaultAsync(llb => llb.MaLuotLamBai == luotLamBaiId);

            if (luotLamBai == null)
                return null;

            // Kiểm tra quyền
            if (luotLamBai.NguoiDungId != userId && !string.IsNullOrEmpty(luotLamBai.NguoiDungId))
            {
                // Cho phép xem nếu là chủ bài thi
                var isOwner = luotLamBai.BaiThi.NguoiTaoId == userId;
                if (!isOwner)
                {
                    throw new UnauthorizedAccessException();
                }
            }

            var datYeuCau = luotLamBai.BaiThi.DiemDat.HasValue && luotLamBai.Diem >= luotLamBai.BaiThi.DiemDat;

            List<CauTraLoiChiTietDto>? chiTietCauTraLoi = null;

            // Chỉ hiển thị chi tiết nếu cho phép xem lại
            if (luotLamBai.BaiThi.ChoPhepXemLai)
            {
                chiTietCauTraLoi = luotLamBai.BaiThi.CacCauHoi
                    .OrderBy(chtbt => chtbt.ThuTu)
                    .Select(chtbt =>
                    {
                        var cauTraLoi = luotLamBai.CacCauTraLoi.First(ctl => ctl.MaCauHoi == chtbt.MaCauHoi);
                        var dapAnDung = chtbt.CauHoi.CacLuaChon.FirstOrDefault(lc => lc.LaDapAnDung);

                        return new CauTraLoiChiTietDto
                        {
                            MaCauHoi = chtbt.MaCauHoi,
                            NoiDungCauHoi = chtbt.CauHoi.NoiDungCauHoi,
                            GiaiThich = luotLamBai.BaiThi.HienThiDapAnSauKhiNop ? chtbt.CauHoi.GiaiThich : null,
                            ThuTu = chtbt.ThuTu,
                            MaLuaChonDaChon = cauTraLoi.MaLuaChonDaChon,
                            MaDapAnDung = dapAnDung?.MaLuaChon,
                            LaDapAnDung = cauTraLoi.LaDapAnDung,
                            CacLuaChon = chtbt.CauHoi.CacLuaChon
                                .OrderBy(lc => lc.ThuTu)
                                .Select(lc => new LuaChonChiTietDto
                                {
                                    MaLuaChon = lc.MaLuaChon,
                                    NoiDungDapAn = lc.NoiDungDapAn,
                                    LaDapAnDung = luotLamBai.BaiThi.HienThiDapAnSauKhiNop && lc.LaDapAnDung,
                                    ThuTu = lc.ThuTu
                                })
                                .ToList()
                        };
                    })
                    .ToList();
            }

            return new KetQuaBaiThiDto
            {
                MaLuotLamBai = luotLamBai.MaLuotLamBai,
                MaBaiThi = luotLamBai.MaBaiThi,
                TieuDeBaiThi = luotLamBai.BaiThi.TieuDe,
                Diem = luotLamBai.Diem ?? 0,
                TongSoCauHoi = luotLamBai.TongSoCauHoi,
                SoCauDung = luotLamBai.SoCauDung,
                SoCauSai = luotLamBai.SoCauSai,
                SoCauChuaLam = luotLamBai.SoCauChuaLam,
                ThoiGianLamBaiThucTe = luotLamBai.ThoiGianLamBaiThucTe ?? 0,
                ThoiGianNopBai = luotLamBai.ThoiGianNopBai ?? DateTime.UtcNow,
                DatYeuCau = datYeuCau,
                DiemDat = luotLamBai.BaiThi.DiemDat,
                ChoPhepXemLai = luotLamBai.BaiThi.ChoPhepXemLai,
                ChiTietCauTraLoi = chiTietCauTraLoi
            };
        }

        public async Task<(IEnumerable<LuotLamBaiDto> Items, int TotalCount)> GetHistoryAsync(
            string userId,
            int pageNumber,
            int pageSize,
            string? sortBy = "ngayLamBai",
            bool sortDesc = true)
        {
            var baseQuery = _context.LuotLamBai
                .Include(llb => llb.BaiThi)
                .Where(llb => llb.NguoiDungId == userId);

            // Apply sorting
            IOrderedQueryable<LuotLamBai> query;
            switch (sortBy?.ToLower())
            {
                case "diem":
                    // Khi sắp xếp theo điểm, nếu bằng điểm thì sắp xếp theo thời gian gần nhất
                    query = sortDesc
                        ? baseQuery.OrderByDescending(llb => llb.Diem ?? 0).ThenByDescending(llb => llb.ThoiGianBatDau)
                        : baseQuery.OrderBy(llb => llb.Diem ?? 0).ThenByDescending(llb => llb.ThoiGianBatDau);
                    break;
                case "ngaylambai":
                default:
                    query = sortDesc
                        ? baseQuery.OrderByDescending(llb => llb.ThoiGianBatDau)
                        : baseQuery.OrderBy(llb => llb.ThoiGianBatDau);
                    break;
            }

            var totalCount = await baseQuery.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(llb => new LuotLamBaiDto
                {
                    MaLuotLamBai = llb.MaLuotLamBai,
                    MaBaiThi = llb.MaBaiThi,
                    TieuDeBaiThi = llb.BaiThi.TieuDe,
                    TenNguoiThamGia = llb.TenNguoiThamGia,
                    ThoiGianBatDau = llb.ThoiGianBatDau,
                    ThoiGianKetThuc = llb.ThoiGianKetThuc,
                    ThoiGianNopBai = llb.ThoiGianNopBai,
                    ThoiGianLamBaiThucTe = llb.ThoiGianLamBaiThucTe,
                    Diem = llb.Diem,
                    TongSoCauHoi = llb.TongSoCauHoi,
                    SoCauDung = llb.SoCauDung,
                    SoCauSai = llb.SoCauSai,
                    SoCauChuaLam = llb.SoCauChuaLam,
                    TrangThai = llb.TrangThai,
                    ThoiGianConLai = llb.TrangThai == "DangLam" && llb.ThoiGianKetThuc.HasValue
                        ? Math.Max(0, (int)(llb.ThoiGianKetThuc.Value - DateTime.UtcNow).TotalSeconds)
                        : 0,
                    ChoPhepXemLai = llb.BaiThi.ChoPhepXemLai
                })
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> AutoSubmitExpiredQuizzesAsync()
        {
            var expiredQuizzes = await _context.LuotLamBai
                .Where(llb => llb.TrangThai == "DangLam" &&
                             llb.ThoiGianKetThuc.HasValue &&
                             llb.ThoiGianKetThuc.Value <= DateTime.UtcNow)
                .ToListAsync();

            foreach (var luotLamBai in expiredQuizzes)
            {
                await AutoSubmitAsync(luotLamBai.MaLuotLamBai);
            }

            return true;
        }

        // Helper Methods
        private async Task AutoSubmitAsync(Guid luotLamBaiId)
        {
            var luotLamBai = await _context.LuotLamBai
                .Include(llb => llb.CacCauTraLoi)
                .FirstOrDefaultAsync(llb => llb.MaLuotLamBai == luotLamBaiId);

            if (luotLamBai == null || luotLamBai.TrangThai != "DangLam")
                return;

            // Tính điểm
            var soCauDung = luotLamBai.CacCauTraLoi.Count(ctl => ctl.LaDapAnDung);
            var soCauSai = luotLamBai.CacCauTraLoi.Count(ctl => !ctl.LaDapAnDung && ctl.MaLuaChonDaChon != null);
            var soCauChuaLam = luotLamBai.CacCauTraLoi.Count(ctl => ctl.MaLuaChonDaChon == null);

            var diem = luotLamBai.TongSoCauHoi > 0
                ? (decimal)soCauDung / luotLamBai.TongSoCauHoi * 10
                : 0;

            var now = DateTime.UtcNow;
            var thoiGianNopBai = luotLamBai.ThoiGianKetThuc.HasValue && luotLamBai.ThoiGianKetThuc.Value < now
                ? luotLamBai.ThoiGianKetThuc.Value
                : now;
            var thoiGianLamBaiThucTe = (int)(thoiGianNopBai - luotLamBai.ThoiGianBatDau).TotalSeconds;

            // Cập nhật
            luotLamBai.Diem = Math.Round(diem, 2);
            luotLamBai.SoCauDung = soCauDung;
            luotLamBai.SoCauSai = soCauSai;
            luotLamBai.SoCauChuaLam = soCauChuaLam;
            luotLamBai.ThoiGianNopBai = thoiGianNopBai;
            luotLamBai.ThoiGianLamBaiThucTe = thoiGianLamBaiThucTe;
            luotLamBai.TrangThai = "TuDongNop";

            await UpdateQuizStatisticsAsync(luotLamBai.MaBaiThi);
            await _context.SaveChangesAsync();
        }

        private async Task UpdateQuizStatisticsAsync(Guid maBaiThi)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE BaiThi
                SET TongLuotLamBai = (
                        SELECT COUNT(*)
                        FROM LuotLamBai
                        WHERE MaBaiThi = {maBaiThi}
                          AND TrangThai IN ('HoanThanh', 'TuDongNop')
                    ),
                    DiemTrungBinh = (
                        SELECT AVG(CAST(XepHang AS DECIMAL(5,2)))
                        FROM DanhGia
                        WHERE MaBaiThi = {maBaiThi}
                    )
                WHERE MaBaiThi = {maBaiThi}
            ");
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        public async Task<UserQuizStatsDto> GetUserStatsAsync(string userId)
        {
            var completedAttempts = await _context.LuotLamBai
                .Where(llb => llb.NguoiDungId == userId &&
                             (llb.TrangThai == "HoanThanh" || llb.TrangThai == "TuDongNop"))
                .ToListAsync();

            if (!completedAttempts.Any())
            {
                return new UserQuizStatsDto
                {
                    TongLuotLam = 0,
                    DiemTrungBinh = 0,
                    DiemCaoNhat = 0,
                    DiemThapNhat = 0,
                    TyLeDat = 0,
                    TongCauDung = 0,
                    TongCauSai = 0,
                    TongThoiGianLam = 0
                };
            }

            var tongLuotLam = completedAttempts.Count;
            var diemList = completedAttempts.Where(a => a.Diem.HasValue).Select(a => a.Diem!.Value).ToList();
            var diemTrungBinh = diemList.Any() ? diemList.Average() : 0;
            var diemCaoNhat = diemList.Any() ? diemList.Max() : 0;
            var diemThapNhat = diemList.Any() ? diemList.Min() : 0;
            var soDat = completedAttempts.Count(a => a.Diem >= 5);
            var tyLeDat = tongLuotLam > 0 ? (int)Math.Round((double)soDat / tongLuotLam * 100) : 0;
            var tongCauDung = completedAttempts.Sum(a => a.SoCauDung);
            var tongCauSai = completedAttempts.Sum(a => a.SoCauSai);
            var tongThoiGianLam = completedAttempts.Sum(a => a.ThoiGianLamBaiThucTe ?? 0);

            return new UserQuizStatsDto
            {
                TongLuotLam = tongLuotLam,
                DiemTrungBinh = Math.Round(diemTrungBinh, 1),
                DiemCaoNhat = Math.Round(diemCaoNhat, 1),
                DiemThapNhat = Math.Round(diemThapNhat, 1),
                TyLeDat = tyLeDat,
                TongCauDung = tongCauDung,
                TongCauSai = tongCauSai,
                TongThoiGianLam = tongThoiGianLam
            };
        }
    }
}