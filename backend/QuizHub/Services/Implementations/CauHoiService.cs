using Microsoft.EntityFrameworkCore;
using QuizHub.Data;
using QuizHub.Models.DTOs.CauHoi;
using QuizHub.Models.Entities;
using QuizHub.Services.Interfaces;

namespace QuizHub.Services.Implementations
{
    public class CauHoiService : ICauHoiService
    {
        private readonly QuizHubDbContext _context;

        public CauHoiService(QuizHubDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<CauHoiDto> Items, int TotalCount)> GetAllAsync(
            CauHoiFilterDto filter,
            string userId)
        {
            var query = _context.CauHoi
                .Include(ch => ch.DanhMuc)
                .Include(ch => ch.CacLuaChon)
                .Where(ch => ch.DaXoa == false);

            // Filter: Chỉ lấy câu hỏi của user hiện tại (trừ khi là câu hỏi công khai)
            query = query.Where(ch => ch.NguoiTaoId == userId || ch.CongKhai == true);

            // Filter: Theo danh mục
            if (filter.MaDanhMuc.HasValue)
            {
                query = query.Where(ch => ch.MaDanhMuc == filter.MaDanhMuc);
            }

            // Filter: Theo mức độ
            if (!string.IsNullOrEmpty(filter.MucDo))
            {
                query = query.Where(ch => ch.MucDo == filter.MucDo);
            }

            // Filter: Theo loại câu hỏi
            if (!string.IsNullOrEmpty(filter.LoaiCauHoi))
            {
                query = query.Where(ch => ch.LoaiCauHoi == filter.LoaiCauHoi);
            }

            // Filter: Chỉ lấy công khai
            if (filter.ChiLayCongKhai.HasValue && filter.ChiLayCongKhai.Value)
            {
                query = query.Where(ch => ch.CongKhai == true);
            }

            // Search: Tìm kiếm theo nội dung câu hỏi
            if (!string.IsNullOrEmpty(filter.TimKiem))
            {
                query = query.Where(ch => ch.NoiDungCauHoi.Contains(filter.TimKiem));
            }

            // Đếm tổng số
            var totalCount = await query.CountAsync();

            // Phân trang
            var items = await query
                .OrderByDescending(ch => ch.NgayTao)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
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
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<CauHoiDto?> GetByIdAsync(Guid id, string userId)
        {
            return await _context.CauHoi
                .Include(ch => ch.DanhMuc)
                .Include(ch => ch.CacLuaChon)
                .Where(ch => ch.MaCauHoi == id && ch.DaXoa == false)
                .Where(ch => ch.NguoiTaoId == userId || ch.CongKhai == true) // Chỉ xem được nếu là chủ sở hữu hoặc công khai
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

        public async Task<CauHoiDto> CreateAsync(CreateCauHoiDto dto, string userId)
        {
            // Validate: Phải có ít nhất 1 đáp án đúng
            if (!dto.CacLuaChon.Any(lc => lc.LaDapAnDung))
            {
                throw new InvalidOperationException("Phải có ít nhất 1 đáp án đúng");
            }

            // Validate: Thứ tự đáp án phải unique
            var thuTuList = dto.CacLuaChon.Select(lc => lc.ThuTu).ToList();
            if (thuTuList.Distinct().Count() != thuTuList.Count)
            {
                throw new InvalidOperationException("Thứ tự các đáp án không được trùng lặp");
            }

            // Tạo câu hỏi
            var cauHoi = new QuizHub.Models.Entities.CauHoi
            {
                NoiDungCauHoi = dto.NoiDungCauHoi,
                GiaiThich = dto.GiaiThich,
                MaDanhMuc = dto.MaDanhMuc,
                MucDo = dto.MucDo,
                LoaiCauHoi = dto.LoaiCauHoi,
                CongKhai = dto.CongKhai,
                NguonNhap = "ThuCong",
                NguoiTaoId = userId,
                NgayTao = DateTime.UtcNow,
                NgayCapNhat = DateTime.UtcNow,
                DaXoa = false
            };

            // Thêm các lựa chọn đáp án
            foreach (var luaChonDto in dto.CacLuaChon)
            {
                cauHoi.CacLuaChon.Add(new LuaChonDapAn
                {
                    NoiDungDapAn = luaChonDto.NoiDungDapAn,
                    LaDapAnDung = luaChonDto.LaDapAnDung,
                    ThuTu = luaChonDto.ThuTu
                });
            }

            _context.CauHoi.Add(cauHoi);
            await _context.SaveChangesAsync();

            // Lấy lại để include DanhMuc
            return await GetByIdAsync(cauHoi.MaCauHoi, userId)
                ?? throw new Exception("Không thể lấy câu hỏi vừa tạo");
        }


        //Xóa toàn bộ các lựa chọn cũ rồi thêm mới
        public async Task<CauHoiDto?> UpdateAsync(Guid id, UpdateCauHoiDto dto, string userId)
        {
            // Sử dụng ExecutionStrategy để tương thích với EnableRetryOnFailure
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var cauHoi = await _context.CauHoi
                        .Include(ch => ch.CacLuaChon)
                        .FirstOrDefaultAsync(ch => ch.MaCauHoi == id && ch.DaXoa == false);

                    if (cauHoi == null)
                        return null;

                    // Kiểm tra quyền sở hữu
                    if (cauHoi.NguoiTaoId != userId)
                    {
                        throw new UnauthorizedAccessException("Bạn không có quyền chỉnh sửa câu hỏi này");
                    }

                    // 1. KIỂM TRA: Câu hỏi này đã có ai trả lời chưa?
                    // Kiểm tra trong bảng CauTraLoi (đây là bảng lưu lịch sử làm bài)
                    bool daDuocSuDung = await _context.CauTraLoi.AnyAsync(ctl => ctl.MaCauHoi == id);

                    if (!daDuocSuDung)
                    {
                        // Validate: Phải có ít nhất 1 đáp án đúng
                        if (!dto.CacLuaChon.Any(lc => lc.LaDapAnDung))
                        {
                            throw new InvalidOperationException("Phải có ít nhất 1 đáp án đúng");
                        }

                        // Validate: Thứ tự đáp án phải unique
                        var thuTuList = dto.CacLuaChon.Select(lc => lc.ThuTu).ToList();
                        if (thuTuList.Distinct().Count() != thuTuList.Count)
                        {
                            throw new InvalidOperationException("Thứ tự các đáp án không được trùng lặp");
                        }

                        // Cập nhật thông tin câu hỏi
                        cauHoi.NoiDungCauHoi = dto.NoiDungCauHoi;
                        cauHoi.GiaiThich = dto.GiaiThich;
                        cauHoi.MaDanhMuc = dto.MaDanhMuc;
                        cauHoi.MucDo = dto.MucDo;
                        cauHoi.LoaiCauHoi = dto.LoaiCauHoi;
                        cauHoi.CongKhai = dto.CongKhai;
                        cauHoi.NgayCapNhat = DateTime.UtcNow;
                        cauHoi.NguoiCapNhatId = userId;

                        // Xóa và thêm trong cùng 1 transaction
                        _context.LuaChonDapAn.RemoveRange(cauHoi.CacLuaChon);

                        // Thêm các lựa chọn mới
                        foreach (var luaChonDto in dto.CacLuaChon)
                        {
                            _context.LuaChonDapAn.Add(new LuaChonDapAn
                            {
                                MaCauHoi = id,
                                NoiDungDapAn = luaChonDto.NoiDungDapAn,
                                LaDapAnDung = luaChonDto.LaDapAnDung,
                                ThuTu = luaChonDto.ThuTu
                            });
                        }

                        //Lưu tất cả changes trong 1 transaction
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return await GetByIdAsync(id, userId);
                    }
                    else
                    {
                        // === TRƯỜNG HỢP 2: ĐÃ CÓ NGƯỜI LÀM -> TẠO BẢN SAO MỚI (CLONE) ===

                        // A. Tạo câu hỏi mới (ID mới)
                        var newCauHoi = new CauHoi
                        {
                            MaCauHoi = Guid.NewGuid(), // ID Mới tinh
                            NguoiTaoId = userId,
                            MaDanhMuc = dto.MaDanhMuc,
                            NoiDungCauHoi = dto.NoiDungCauHoi, // Nội dung mới
                            GiaiThich = dto.GiaiThich,
                            LoaiCauHoi = dto.LoaiCauHoi,
                            MucDo = dto.MucDo,
                            CongKhai = dto.CongKhai,
                            NgayTao = DateTime.UtcNow,
                            NgayCapNhat = DateTime.UtcNow,
                            DaXoa = false
                        };

                        _context.CauHoi.Add(newCauHoi);

                        // B. Tạo đáp án mới cho câu hỏi mới
                        foreach (var lc in dto.CacLuaChon)
                        {
                            _context.LuaChonDapAn.Add(new LuaChonDapAn
                            {
                                MaLuaChon = Guid.NewGuid(),
                                MaCauHoi = newCauHoi.MaCauHoi, // Trỏ về câu hỏi mới
                                NoiDungDapAn = lc.NoiDungDapAn,
                                LaDapAnDung = lc.LaDapAnDung,
                                ThuTu = lc.ThuTu
                            });
                        }

                        // C. Xử lý "Thay thế" (Swap) trong Ngân hàng câu hỏi
                        // Câu hỏi cũ: Đánh dấu là "Đã Xóa" (Soft Delete) để nó ẩn đi khỏi danh sách quản lý
                        // Nhưng dữ liệu lịch sử (CauTraLoi) vẫn trỏ về nó bình thường.
                        cauHoi.DaXoa = true;

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        // Trả về câu hỏi MỚI
                        return await GetByIdAsync(newCauHoi.MaCauHoi, userId);
                    }

                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<bool> DeleteAsync(Guid id, string userId)
        {
            var cauHoi = await _context.CauHoi.FindAsync(id);

            if (cauHoi == null || cauHoi.DaXoa)
                return false;

            // Kiểm tra quyền sở hữu
            if (cauHoi.NguoiTaoId != userId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xóa câu hỏi này");
            }

            // Soft delete - cho phép xóa mềm ngay cả khi câu hỏi đang trong bài thi
            // Vì xóa mềm chỉ ẩn câu hỏi khỏi ngân hàng, bài thi đã có câu hỏi vẫn truy cập được
            cauHoi.DaXoa = true;
            cauHoi.NgayCapNhat = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> CountByUserAsync(string userId)
        {
            return await _context.CauHoi
                .CountAsync(ch => ch.NguoiTaoId == userId && ch.DaXoa == false);
        }

        public async Task<int> CountByDanhMucAsync(Guid danhMucId)
        {
            return await _context.CauHoi
                .CountAsync(ch => ch.MaDanhMuc == danhMucId && ch.DaXoa == false);
        }

        public async Task<object> GetStatsAsync(string userId)
        {
            var query = _context.CauHoi
                .Where(ch => ch.NguoiTaoId == userId && ch.DaXoa == false);

            var total = await query.CountAsync();
            var easy = await query.CountAsync(ch => ch.MucDo == "De");
            var medium = await query.CountAsync(ch => ch.MucDo == "TrungBinh");
            var hard = await query.CountAsync(ch => ch.MucDo == "Kho");

            return new
            {
                total,
                easy,
                medium,
                hard
            };
        }
    }
}