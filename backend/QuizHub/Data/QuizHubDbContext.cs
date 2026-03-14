using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuizHub.Models.Entities;

namespace QuizHub.Data
{
    public class QuizHubDbContext : IdentityDbContext<NguoiDung>
    {
        public QuizHubDbContext(DbContextOptions<QuizHubDbContext> options)
            : base(options)
        {
        }

        // ============================================
        // DbSets cho các bảng
        // ============================================
        public DbSet<DanhMuc> DanhMuc { get; set; }
        public DbSet<CauHoi> CauHoi { get; set; }
        public DbSet<LuaChonDapAn> LuaChonDapAn { get; set; }
        public DbSet<BaiThi> BaiThi { get; set; }
        public DbSet<CauHoiTrongBaiThi> CauHoiTrongBaiThi { get; set; }
        public DbSet<LuotLamBai> LuotLamBai { get; set; }
        public DbSet<CauTraLoi> CauTraLoi { get; set; }
        public DbSet<DanhGia> DanhGia { get; set; }
        public DbSet<BaoCao> BaoCao { get; set; }
        public DbSet<PhienNhapDuLieu> PhienNhapDuLieu { get; set; }
        public DbSet<ThongBao> ThongBao { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ============================================
            // Đổi tên bảng Identity sang tiếng Việt
            // ============================================
            builder.Entity<NguoiDung>().ToTable("NguoiDung");
            builder.Entity<IdentityRole>().ToTable("VaiTro");
            builder.Entity<IdentityUserRole<string>>().ToTable("NguoiDung_VaiTro");
            builder.Entity<IdentityUserClaim<string>>().ToTable("NguoiDung_Claims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("NguoiDung_DangNhapNgoai");
            builder.Entity<IdentityUserToken<string>>().ToTable("NguoiDung_Tokens");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("VaiTro_Claims");

            // ============================================
            // Cấu hình Decimal Precision
            // ============================================
            ConfigureDecimalPrecision(builder);

            // ============================================
            // Cấu hình Relationships
            // ============================================
            ConfigureRelationships(builder);

            // ============================================
            // Cấu hình Indexes
            // ============================================
            ConfigureIndexes(builder);

            // ============================================
            // Cấu hình Unique Constraints
            // ============================================
            ConfigureUniqueConstraints(builder);
        }

        private void ConfigureDecimalPrecision(ModelBuilder builder)
        {
            // BaiThi - DiemTrungBinh
            builder.Entity<BaiThi>()
                .Property(b => b.DiemTrungBinh)
                .HasColumnType("decimal(5,2)");

            builder.Entity<BaiThi>()
                .Property(b => b.XepHangTrungBinh)
                .HasColumnType("decimal(2,1)");

            // LuotLamBai - Diem
            builder.Entity<LuotLamBai>()
                .Property(l => l.Diem)
                .HasColumnType("decimal(5,2)");
        }

        private void ConfigureRelationships(ModelBuilder builder)
        {
            // ============================================
            // 1. DANHМUC RELATIONSHIPS
            // ============================================
            builder.Entity<DanhMuc>()
                .HasMany(d => d.CacCauHoi)
                .WithOne(c => c.DanhMuc)
                .HasForeignKey(c => c.MaDanhMuc)
                .OnDelete(DeleteBehavior.SetNull);

            // ============================================
            // 2. CAUHOI RELATIONSHIPS
            // ============================================

            // CauHoi -> NguoiDung (NguoiTao)
            builder.Entity<CauHoi>()
                .HasOne<NguoiDung>()
                .WithMany(n => n.CacCauHoiDaTao)
                .HasForeignKey(c => c.NguoiTaoId)
                .OnDelete(DeleteBehavior.Cascade);

            // CauHoi -> LuaChonDapAn (1-nhiều)
            builder.Entity<CauHoi>()
                .HasMany(c => c.CacLuaChon)
                .WithOne(l => l.CauHoi)
                .HasForeignKey(l => l.MaCauHoi)
                .OnDelete(DeleteBehavior.Cascade);

            // ============================================
            // 3. LUACHONDAPAN RELATIONSHIPS
            // ============================================

            builder.Entity<LuaChonDapAn>()
                .HasOne(l => l.CauHoi)
                .WithMany(c => c.CacLuaChon)
                .HasForeignKey(l => l.MaCauHoi)
                .OnDelete(DeleteBehavior.Cascade);

            // ============================================
            // 4. BAITHI RELATIONSHIPS
            // ============================================

            // BaiThi -> NguoiDung (NguoiTao)
            builder.Entity<BaiThi>()
                .HasOne<NguoiDung>()
                .WithMany(n => n.CacBaiThiDaTao)
                .HasForeignKey(b => b.NguoiTaoId)
                .OnDelete(DeleteBehavior.Cascade);

            // BaiThi -> CauHoiTrongBaiThi
            builder.Entity<BaiThi>()
                .HasMany(b => b.CacCauHoi)
                .WithOne(c => c.BaiThi)
                .HasForeignKey(c => c.MaBaiThi)
                .OnDelete(DeleteBehavior.Cascade);

            // BaiThi -> LuotLamBai
            builder.Entity<BaiThi>()
                .HasMany(b => b.CacLuotLamBai)
                .WithOne(l => l.BaiThi)
                .HasForeignKey(l => l.MaBaiThi)
                .OnDelete(DeleteBehavior.Cascade);

            // BaiThi -> DanhGia
            builder.Entity<BaiThi>()
                .HasMany(b => b.CacDanhGia)
                .WithOne(d => d.BaiThi)
                .HasForeignKey(d => d.MaBaiThi)
                .OnDelete(DeleteBehavior.Cascade);

            // BaiThi -> DanhMuc (optional relationship)
            builder.Entity<BaiThi>()
                .HasOne(b => b.DanhMuc)
                .WithMany()
                .HasForeignKey(b => b.MaDanhMuc)
                .OnDelete(DeleteBehavior.SetNull);

            // ============================================
            // 5. CAUHOITRONGBAITHI RELATIONSHIPS
            // ============================================

            // CauHoiTrongBaiThi -> BaiThi
            builder.Entity<CauHoiTrongBaiThi>()
                .HasOne(c => c.BaiThi)
                .WithMany(b => b.CacCauHoi)
                .HasForeignKey(c => c.MaBaiThi)
                .OnDelete(DeleteBehavior.Cascade);

            // CauHoiTrongBaiThi -> CauHoi (NO ACTION để tránh multiple cascade)
            builder.Entity<CauHoiTrongBaiThi>()
                .HasOne(c => c.CauHoi)
                .WithMany(ch => ch.CacBaiThiChuaCauHoiNay)
                .HasForeignKey(c => c.MaCauHoi)
                .OnDelete(DeleteBehavior.NoAction);

            // ============================================
            // 6. LUOTLAMBAI RELATIONSHIPS
            // ============================================

            // LuotLamBai -> BaiThi
            builder.Entity<LuotLamBai>()
                .HasOne(l => l.BaiThi)
                .WithMany(b => b.CacLuotLamBai)
                .HasForeignKey(l => l.MaBaiThi)
                .OnDelete(DeleteBehavior.Cascade);

            // LuotLamBai -> NguoiDung (nullable)
            builder.Entity<LuotLamBai>()
                .HasOne<NguoiDung>()
                .WithMany(n => n.CacLuotLamBai)
                .HasForeignKey(l => l.NguoiDungId)
                .OnDelete(DeleteBehavior.NoAction);

            // LuotLamBai -> CauTraLoi
            builder.Entity<LuotLamBai>()
                .HasMany(l => l.CacCauTraLoi)
                .WithOne(c => c.LuotLamBai)
                .HasForeignKey(c => c.MaLuotLamBai)
                .OnDelete(DeleteBehavior.Cascade);

            // ============================================
            // 7. CAUTRALOI RELATIONSHIPS
            // ============================================

            // CauTraLoi -> LuotLamBai
            builder.Entity<CauTraLoi>()
                .HasOne(c => c.LuotLamBai)
                .WithMany(l => l.CacCauTraLoi)
                .HasForeignKey(c => c.MaLuotLamBai)
                .OnDelete(DeleteBehavior.Cascade);

            // CauTraLoi -> CauHoi (NO ACTION)
            builder.Entity<CauTraLoi>()
                .HasOne(c => c.CauHoi)
                .WithMany()
                .HasForeignKey(c => c.MaCauHoi)
                .OnDelete(DeleteBehavior.NoAction);

            // CauTraLoi -> LuaChonDapAn (NO ACTION)
            builder.Entity<CauTraLoi>()
                .HasOne(c => c.LuaChonDaChon)
                .WithMany()
                .HasForeignKey(c => c.MaLuaChonDaChon)
                .OnDelete(DeleteBehavior.NoAction);

            // ============================================
            // 8. DANHGIA RELATIONSHIPS
            // ============================================

            // DanhGia -> BaiThi
            builder.Entity<DanhGia>()
                .HasOne(d => d.BaiThi)
                .WithMany(b => b.CacDanhGia)
                .HasForeignKey(d => d.MaBaiThi)
                .OnDelete(DeleteBehavior.Cascade);

            // DanhGia -> NguoiDung (NO ACTION)
            builder.Entity<DanhGia>()
                .HasOne<NguoiDung>()
                .WithMany()
                .HasForeignKey(d => d.NguoiDungId)
                .OnDelete(DeleteBehavior.NoAction);

            // ============================================
            // 9. BAOCAO RELATIONSHIPS
            // ============================================

            // BaoCao -> NguoiDung (NguoiBaoCao) (NO ACTION)
            builder.Entity<BaoCao>()
                .HasOne<NguoiDung>()
                .WithMany()
                .HasForeignKey(b => b.NguoiBaoCaoId)
                .OnDelete(DeleteBehavior.NoAction);

            // BaoCao -> NguoiDung (NguoiXuLy) (NO ACTION)
            builder.Entity<BaoCao>()
                .HasOne<NguoiDung>()
                .WithMany()
                .HasForeignKey(b => b.NguoiXuLyId)
                .OnDelete(DeleteBehavior.NoAction);

            // ============================================
            // 10. PHIENNHAPDULIEU RELATIONSHIPS
            // ============================================

            // PhienNhapDuLieu -> NguoiDung
            builder.Entity<PhienNhapDuLieu>()
                .HasOne<NguoiDung>()
                .WithMany()
                .HasForeignKey(p => p.NguoiDungId)
                .OnDelete(DeleteBehavior.NoAction);

            // ============================================
            // 11. THONGBAO RELATIONSHIPS
            // ============================================

            // ThongBao -> NguoiDung
            builder.Entity<ThongBao>()
                .HasOne<NguoiDung>()
                .WithMany()
                .HasForeignKey(t => t.NguoiDungId)
                .OnDelete(DeleteBehavior.NoAction);
        }

        private void ConfigureIndexes(ModelBuilder builder)
        {
            // ============================================
            // DANHМUC INDEXES
            // ============================================
            builder.Entity<DanhMuc>()
                .HasIndex(d => d.DuongDan)
                .IsUnique();

            // ============================================
            // CAUHOI INDEXES
            // ============================================
            builder.Entity<CauHoi>()
                .HasIndex(c => c.NguoiTaoId);

            builder.Entity<CauHoi>()
                .HasIndex(c => c.MaDanhMuc);

            builder.Entity<CauHoi>()
                .HasIndex(c => new { c.NguoiTaoId, c.MaDanhMuc });

            builder.Entity<CauHoi>()
                .HasIndex(c => c.DaXoa);

            // Index cho filter câu hỏi (DaXoa + NguoiTaoId + CongKhai)
            builder.Entity<CauHoi>()
                .HasIndex(c => new { c.DaXoa, c.NguoiTaoId, c.CongKhai });

            // Index cho lấy câu hỏi theo danh mục
            builder.Entity<CauHoi>()
                .HasIndex(c => new { c.MaDanhMuc, c.DaXoa });

            // Index cho lấy câu hỏi của user với sorting
            builder.Entity<CauHoi>()
                .HasIndex(c => new { c.NguoiTaoId, c.DaXoa, c.NgayTao });

            // ============================================
            // BAITHI INDEXES
            // ============================================
            builder.Entity<BaiThi>()
                .HasIndex(b => b.NguoiTaoId);

            builder.Entity<BaiThi>()
                .HasIndex(b => b.MaTruyCapDinhDanh)
                .IsUnique();

            builder.Entity<BaiThi>()
                .HasIndex(b => new { b.CheDoCongKhai, b.TrangThai, b.DaXoa });

            builder.Entity<BaiThi>()
                .HasIndex(b => b.NgayTao);

            // Index cho tìm kiếm bài thi theo mã truy cập + DaXoa
            builder.Entity<BaiThi>()
                .HasIndex(b => new { b.MaTruyCapDinhDanh, b.DaXoa });

            // Index cho lấy bài thi của user
            builder.Entity<BaiThi>()
                .HasIndex(b => new { b.NguoiTaoId, b.DaXoa, b.NgayTao });

            // Index cho filter bài thi với DaXoa ở đầu
            builder.Entity<BaiThi>()
                .HasIndex(b => new { b.DaXoa, b.CheDoCongKhai, b.TrangThai });

            // ============================================
            // LUOTLAMBAI INDEXES
            // ============================================
            builder.Entity<LuotLamBai>()
                .HasIndex(l => l.MaBaiThi);

            builder.Entity<LuotLamBai>()
                .HasIndex(l => l.NguoiDungId);

            builder.Entity<LuotLamBai>()
                .HasIndex(l => new { l.MaBaiThi, l.NguoiDungId });

            builder.Entity<LuotLamBai>()
                .HasIndex(l => l.TrangThai);

            // Index cho Background Service query expired sessions
            builder.Entity<LuotLamBai>()
                .HasIndex(l => new { l.TrangThai, l.ThoiGianKetThuc });

            // Index cho thống kê bài thi
            builder.Entity<LuotLamBai>()
                .HasIndex(l => new { l.MaBaiThi, l.TrangThai, l.Diem });

            // ============================================
            // CAUTRALOI INDEXES
            // ============================================
            // Index cho lookup câu trả lời
            builder.Entity<CauTraLoi>()
                .HasIndex(c => new { c.MaLuotLamBai, c.MaCauHoi });

            // ============================================
            // DANHGIA INDEXES
            // ============================================
            builder.Entity<DanhGia>()
                .HasIndex(d => d.MaBaiThi);

            builder.Entity<DanhGia>()
                .HasIndex(d => d.NguoiDungId);

            // ============================================
            // BAOCAO INDEXES
            // ============================================
            builder.Entity<BaoCao>()
                .HasIndex(b => new { b.TrangThai, b.NgayTao });

            builder.Entity<BaoCao>()
                .HasIndex(b => new { b.LoaiDoiTuong, b.MaDoiTuong });

            // Index đã có sẵn: IX_BaoCao_TrangThai_NgayTao (dòng 427)
            // Không cần thêm index trùng lặp

            // ============================================
            // THONGBAO INDEXES
            // ============================================
            builder.Entity<ThongBao>()
                .HasIndex(t => new { t.NguoiDungId, t.DaDoc });

            builder.Entity<ThongBao>()
                .HasIndex(t => t.NgayTao);
        }

        private void ConfigureUniqueConstraints(ModelBuilder builder)
        {
            // DanhGia: Cho phép user đánh giá nhiều lần cho 1 bài thi

            // CauHoiTrongBaiThi: Mỗi câu hỏi chỉ xuất hiện 1 lần trong 1 bài thi
            builder.Entity<CauHoiTrongBaiThi>()
                .HasIndex(c => new { c.MaBaiThi, c.MaCauHoi })
                .IsUnique();
        }
    }
}