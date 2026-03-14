using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizHub.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DanhMuc",
                columns: table => new
                {
                    MaDanhMuc = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenDanhMuc = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DuongDan = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HinhAnh = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMuc", x => x.MaDanhMuc);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDung",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnhDaiDien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LanDangNhapCuoi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThaiKichHoat = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDung", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "The",
                columns: table => new
                {
                    MaThe = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenThe = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoLanSuDung = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_The", x => x.MaThe);
                });

            migrationBuilder.CreateTable(
                name: "VaiTro",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaiTro", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BaiThi",
                columns: table => new
                {
                    MaBaiThi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiTaoId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnhBia = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ThoiGianLamBai = table.Column<int>(type: "int", nullable: false),
                    DiemDat = table.Column<int>(type: "int", nullable: true),
                    CheDoCongKhai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HienThiDapAnSauKhiNop = table.Column<bool>(type: "bit", nullable: false),
                    ChoPhepXemLai = table.Column<bool>(type: "bit", nullable: false),
                    MaTruyCapDinhDanh = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LuotXem = table.Column<int>(type: "int", nullable: false),
                    TongLuotLamBai = table.Column<int>(type: "int", nullable: false),
                    DiemTrungBinh = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    DaXoa = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguoiCapNhatId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaiThi", x => x.MaBaiThi);
                    table.ForeignKey(
                        name: "FK_BaiThi_NguoiDung_NguoiTaoId",
                        column: x => x.NguoiTaoId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BaoCao",
                columns: table => new
                {
                    MaBaoCao = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiBaoCaoId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoaiDoiTuong = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MaDoiTuong = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LyDo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NguoiXuLyId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ThoiGianXuLy = table.Column<DateTime>(type: "datetime2", nullable: true),
                    KetQuaXuLy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaoCao", x => x.MaBaoCao);
                    table.ForeignKey(
                        name: "FK_BaoCao_NguoiDung_NguoiBaoCaoId",
                        column: x => x.NguoiBaoCaoId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BaoCao_NguoiDung_NguoiXuLyId",
                        column: x => x.NguoiXuLyId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CauHoi",
                columns: table => new
                {
                    MaCauHoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiTaoId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaDanhMuc = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NoiDungCauHoi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GiaiThich = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoaiCauHoi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MucDo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CongKhai = table.Column<bool>(type: "bit", nullable: false),
                    NguonNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DaXoa = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguoiCapNhatId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHoi", x => x.MaCauHoi);
                    table.ForeignKey(
                        name: "FK_CauHoi_DanhMuc_MaDanhMuc",
                        column: x => x.MaDanhMuc,
                        principalTable: "DanhMuc",
                        principalColumn: "MaDanhMuc",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CauHoi_NguoiDung_NguoiTaoId",
                        column: x => x.NguoiTaoId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDung_Claims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDung_Claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NguoiDung_Claims_NguoiDung_UserId",
                        column: x => x.UserId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDung_DangNhapNgoai",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDung_DangNhapNgoai", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_NguoiDung_DangNhapNgoai_NguoiDung_UserId",
                        column: x => x.UserId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDung_Tokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDung_Tokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_NguoiDung_Tokens_NguoiDung_UserId",
                        column: x => x.UserId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NhatKyHeThong",
                columns: table => new
                {
                    MaNhatKy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiDungId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    HanhDong = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LoaiDoiTuong = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaDoiTuong = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GiaTriCu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GiaTriMoi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiaChiIP = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ThongTinTrinhDuyet = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ThoiGian = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhatKyHeThong", x => x.MaNhatKy);
                    table.ForeignKey(
                        name: "FK_NhatKyHeThong_NguoiDung_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PhienNhapDuLieu",
                columns: table => new
                {
                    MaPhien = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiDungId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenTepTin = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LoaiTepTin = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    KichThuocTepTin = table.Column<long>(type: "bigint", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SoCauHoiTrichXuat = table.Column<int>(type: "int", nullable: false),
                    SoCauHoiNhap = table.Column<int>(type: "int", nullable: false),
                    ThongBaoLoi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MoHinhAI = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ThoiGianXuLy = table.Column<int>(type: "int", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhienNhapDuLieu", x => x.MaPhien);
                    table.ForeignKey(
                        name: "FK_PhienNhapDuLieu_NguoiDung_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ThongBao",
                columns: table => new
                {
                    MaThongBao = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiDungId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoaiThongBao = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DaDoc = table.Column<bool>(type: "bit", nullable: false),
                    MaDoiTuongLienQuan = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongBao", x => x.MaThongBao);
                    table.ForeignKey(
                        name: "FK_ThongBao_NguoiDung_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NguoiDung_VaiTro",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDung_VaiTro", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_NguoiDung_VaiTro_NguoiDung_UserId",
                        column: x => x.UserId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NguoiDung_VaiTro_VaiTro_RoleId",
                        column: x => x.RoleId,
                        principalTable: "VaiTro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VaiTro_Claims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaiTro_Claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VaiTro_Claims_VaiTro_RoleId",
                        column: x => x.RoleId,
                        principalTable: "VaiTro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BaiThiThe",
                columns: table => new
                {
                    MaBaiThi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaThe = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaiThiThe", x => new { x.MaBaiThi, x.MaThe });
                    table.ForeignKey(
                        name: "FK_BaiThiThe_BaiThi_MaBaiThi",
                        column: x => x.MaBaiThi,
                        principalTable: "BaiThi",
                        principalColumn: "MaBaiThi",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaiThiThe_The_MaThe",
                        column: x => x.MaThe,
                        principalTable: "The",
                        principalColumn: "MaThe",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DanhGia",
                columns: table => new
                {
                    MaDanhGia = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaBaiThi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiDungId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    XepHang = table.Column<int>(type: "int", nullable: false),
                    BinhLuan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhGia", x => x.MaDanhGia);
                    table.ForeignKey(
                        name: "FK_DanhGia_BaiThi_MaBaiThi",
                        column: x => x.MaBaiThi,
                        principalTable: "BaiThi",
                        principalColumn: "MaBaiThi",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DanhGia_NguoiDung_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LuotLamBai",
                columns: table => new
                {
                    MaLuotLamBai = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaBaiThi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiDungId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TenNguoiThamGia = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ThoiGianBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianKetThuc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ThoiGianNopBai = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ThoiGianLamBaiThucTe = table.Column<int>(type: "int", nullable: true),
                    Diem = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    TongSoCauHoi = table.Column<int>(type: "int", nullable: false),
                    SoCauDung = table.Column<int>(type: "int", nullable: false),
                    SoCauSai = table.Column<int>(type: "int", nullable: false),
                    SoCauChuaLam = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DiaChiIP = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ThongTinTrinhDuyet = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LuotLamBai", x => x.MaLuotLamBai);
                    table.ForeignKey(
                        name: "FK_LuotLamBai_BaiThi_MaBaiThi",
                        column: x => x.MaBaiThi,
                        principalTable: "BaiThi",
                        principalColumn: "MaBaiThi",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LuotLamBai_NguoiDung_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "NguoiDung",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CauHoiTrongBaiThi",
                columns: table => new
                {
                    MaCauHoiTrongBaiThi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaBaiThi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaCauHoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ThuTu = table.Column<int>(type: "int", nullable: false),
                    Diem = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHoiTrongBaiThi", x => x.MaCauHoiTrongBaiThi);
                    table.ForeignKey(
                        name: "FK_CauHoiTrongBaiThi_BaiThi_MaBaiThi",
                        column: x => x.MaBaiThi,
                        principalTable: "BaiThi",
                        principalColumn: "MaBaiThi",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CauHoiTrongBaiThi_CauHoi_MaCauHoi",
                        column: x => x.MaCauHoi,
                        principalTable: "CauHoi",
                        principalColumn: "MaCauHoi");
                });

            migrationBuilder.CreateTable(
                name: "LuaChonDapAn",
                columns: table => new
                {
                    MaLuaChon = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaCauHoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NoiDungDapAn = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    LaDapAnDung = table.Column<bool>(type: "bit", nullable: false),
                    ThuTu = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LuaChonDapAn", x => x.MaLuaChon);
                    table.ForeignKey(
                        name: "FK_LuaChonDapAn_CauHoi_MaCauHoi",
                        column: x => x.MaCauHoi,
                        principalTable: "CauHoi",
                        principalColumn: "MaCauHoi",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CauTraLoi",
                columns: table => new
                {
                    MaCauTraLoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaLuotLamBai = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaCauHoi = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaLuaChonDaChon = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LaDapAnDung = table.Column<bool>(type: "bit", nullable: false),
                    ThoiGianTraLoi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianSuDung = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauTraLoi", x => x.MaCauTraLoi);
                    table.ForeignKey(
                        name: "FK_CauTraLoi_CauHoi_MaCauHoi",
                        column: x => x.MaCauHoi,
                        principalTable: "CauHoi",
                        principalColumn: "MaCauHoi");
                    table.ForeignKey(
                        name: "FK_CauTraLoi_LuaChonDapAn_MaLuaChonDaChon",
                        column: x => x.MaLuaChonDaChon,
                        principalTable: "LuaChonDapAn",
                        principalColumn: "MaLuaChon");
                    table.ForeignKey(
                        name: "FK_CauTraLoi_LuotLamBai_MaLuotLamBai",
                        column: x => x.MaLuotLamBai,
                        principalTable: "LuotLamBai",
                        principalColumn: "MaLuotLamBai",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaiThi_CheDoCongKhai_TrangThai_DaXoa",
                table: "BaiThi",
                columns: new[] { "CheDoCongKhai", "TrangThai", "DaXoa" });

            migrationBuilder.CreateIndex(
                name: "IX_BaiThi_MaTruyCapDinhDanh",
                table: "BaiThi",
                column: "MaTruyCapDinhDanh",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BaiThi_NgayTao",
                table: "BaiThi",
                column: "NgayTao");

            migrationBuilder.CreateIndex(
                name: "IX_BaiThi_NguoiTaoId",
                table: "BaiThi",
                column: "NguoiTaoId");

            migrationBuilder.CreateIndex(
                name: "IX_BaiThiThe_MaThe",
                table: "BaiThiThe",
                column: "MaThe");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCao_LoaiDoiTuong_MaDoiTuong",
                table: "BaoCao",
                columns: new[] { "LoaiDoiTuong", "MaDoiTuong" });

            migrationBuilder.CreateIndex(
                name: "IX_BaoCao_NguoiBaoCaoId",
                table: "BaoCao",
                column: "NguoiBaoCaoId");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCao_NguoiXuLyId",
                table: "BaoCao",
                column: "NguoiXuLyId");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCao_TrangThai_NgayTao",
                table: "BaoCao",
                columns: new[] { "TrangThai", "NgayTao" });

            migrationBuilder.CreateIndex(
                name: "IX_CauHoi_DaXoa",
                table: "CauHoi",
                column: "DaXoa");

            migrationBuilder.CreateIndex(
                name: "IX_CauHoi_MaDanhMuc",
                table: "CauHoi",
                column: "MaDanhMuc");

            migrationBuilder.CreateIndex(
                name: "IX_CauHoi_NguoiTaoId",
                table: "CauHoi",
                column: "NguoiTaoId");

            migrationBuilder.CreateIndex(
                name: "IX_CauHoi_NguoiTaoId_MaDanhMuc",
                table: "CauHoi",
                columns: new[] { "NguoiTaoId", "MaDanhMuc" });

            migrationBuilder.CreateIndex(
                name: "IX_CauHoiTrongBaiThi_MaBaiThi_MaCauHoi",
                table: "CauHoiTrongBaiThi",
                columns: new[] { "MaBaiThi", "MaCauHoi" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CauHoiTrongBaiThi_MaCauHoi",
                table: "CauHoiTrongBaiThi",
                column: "MaCauHoi");

            migrationBuilder.CreateIndex(
                name: "IX_CauTraLoi_MaCauHoi",
                table: "CauTraLoi",
                column: "MaCauHoi");

            migrationBuilder.CreateIndex(
                name: "IX_CauTraLoi_MaLuaChonDaChon",
                table: "CauTraLoi",
                column: "MaLuaChonDaChon");

            migrationBuilder.CreateIndex(
                name: "IX_CauTraLoi_MaLuotLamBai",
                table: "CauTraLoi",
                column: "MaLuotLamBai");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGia_MaBaiThi",
                table: "DanhGia",
                column: "MaBaiThi");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGia_MaBaiThi_NguoiDungId",
                table: "DanhGia",
                columns: new[] { "MaBaiThi", "NguoiDungId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DanhGia_NguoiDungId",
                table: "DanhGia",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_DanhMuc_DuongDan",
                table: "DanhMuc",
                column: "DuongDan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LuaChonDapAn_MaCauHoi",
                table: "LuaChonDapAn",
                column: "MaCauHoi");

            migrationBuilder.CreateIndex(
                name: "IX_LuotLamBai_MaBaiThi",
                table: "LuotLamBai",
                column: "MaBaiThi");

            migrationBuilder.CreateIndex(
                name: "IX_LuotLamBai_MaBaiThi_NguoiDungId",
                table: "LuotLamBai",
                columns: new[] { "MaBaiThi", "NguoiDungId" });

            migrationBuilder.CreateIndex(
                name: "IX_LuotLamBai_NguoiDungId",
                table: "LuotLamBai",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_LuotLamBai_TrangThai",
                table: "LuotLamBai",
                column: "TrangThai");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "NguoiDung",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "NguoiDung",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_Claims_UserId",
                table: "NguoiDung_Claims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_DangNhapNgoai_UserId",
                table: "NguoiDung_DangNhapNgoai",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_VaiTro_RoleId",
                table: "NguoiDung_VaiTro",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyHeThong_NguoiDungId",
                table: "NhatKyHeThong",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_PhienNhapDuLieu_NguoiDungId",
                table: "PhienNhapDuLieu",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_The_TenThe",
                table: "The",
                column: "TenThe",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_NgayTao",
                table: "ThongBao",
                column: "NgayTao");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBao_NguoiDungId_DaDoc",
                table: "ThongBao",
                columns: new[] { "NguoiDungId", "DaDoc" });

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "VaiTro",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VaiTro_Claims_RoleId",
                table: "VaiTro_Claims",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaiThiThe");

            migrationBuilder.DropTable(
                name: "BaoCao");

            migrationBuilder.DropTable(
                name: "CauHoiTrongBaiThi");

            migrationBuilder.DropTable(
                name: "CauTraLoi");

            migrationBuilder.DropTable(
                name: "DanhGia");

            migrationBuilder.DropTable(
                name: "NguoiDung_Claims");

            migrationBuilder.DropTable(
                name: "NguoiDung_DangNhapNgoai");

            migrationBuilder.DropTable(
                name: "NguoiDung_Tokens");

            migrationBuilder.DropTable(
                name: "NguoiDung_VaiTro");

            migrationBuilder.DropTable(
                name: "NhatKyHeThong");

            migrationBuilder.DropTable(
                name: "PhienNhapDuLieu");

            migrationBuilder.DropTable(
                name: "ThongBao");

            migrationBuilder.DropTable(
                name: "VaiTro_Claims");

            migrationBuilder.DropTable(
                name: "The");

            migrationBuilder.DropTable(
                name: "LuaChonDapAn");

            migrationBuilder.DropTable(
                name: "LuotLamBai");

            migrationBuilder.DropTable(
                name: "VaiTro");

            migrationBuilder.DropTable(
                name: "CauHoi");

            migrationBuilder.DropTable(
                name: "BaiThi");

            migrationBuilder.DropTable(
                name: "DanhMuc");

            migrationBuilder.DropTable(
                name: "NguoiDung");
        }
    }
}
