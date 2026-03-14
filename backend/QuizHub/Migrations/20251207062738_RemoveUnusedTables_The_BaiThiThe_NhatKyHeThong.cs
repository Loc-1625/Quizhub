using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizHub.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedTables_The_BaiThiThe_NhatKyHeThong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaiThiThe");

            migrationBuilder.DropTable(
                name: "NhatKyHeThong");

            migrationBuilder.DropTable(
                name: "The");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NhatKyHeThong",
                columns: table => new
                {
                    MaNhatKy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DiaChiIP = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GiaTriCu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GiaTriMoi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HanhDong = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LoaiDoiTuong = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaDoiTuong = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiDungId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ThoiGian = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThongTinTrinhDuyet = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
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
                name: "The",
                columns: table => new
                {
                    MaThe = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SoLanSuDung = table.Column<int>(type: "int", nullable: false),
                    TenThe = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_The", x => x.MaThe);
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

            migrationBuilder.CreateIndex(
                name: "IX_BaiThiThe_MaThe",
                table: "BaiThiThe",
                column: "MaThe");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyHeThong_NguoiDungId",
                table: "NhatKyHeThong",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_The_TenThe",
                table: "The",
                column: "TenThe",
                unique: true);
        }
    }
}
