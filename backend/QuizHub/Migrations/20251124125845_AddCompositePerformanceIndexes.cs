using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizHub.Migrations
{
    /// <inheritdoc />
    public partial class AddCompositePerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CauTraLoi_MaLuotLamBai",
                table: "CauTraLoi");

            migrationBuilder.CreateIndex(
                name: "IX_LuotLamBai_MaBaiThi_TrangThai_Diem",
                table: "LuotLamBai",
                columns: new[] { "MaBaiThi", "TrangThai", "Diem" });

            migrationBuilder.CreateIndex(
                name: "IX_LuotLamBai_TrangThai_ThoiGianKetThuc",
                table: "LuotLamBai",
                columns: new[] { "TrangThai", "ThoiGianKetThuc" });

            migrationBuilder.CreateIndex(
                name: "IX_CauTraLoi_MaLuotLamBai_MaCauHoi",
                table: "CauTraLoi",
                columns: new[] { "MaLuotLamBai", "MaCauHoi" });

            migrationBuilder.CreateIndex(
                name: "IX_CauHoi_DaXoa_NguoiTaoId_CongKhai",
                table: "CauHoi",
                columns: new[] { "DaXoa", "NguoiTaoId", "CongKhai" });

            migrationBuilder.CreateIndex(
                name: "IX_CauHoi_MaDanhMuc_DaXoa",
                table: "CauHoi",
                columns: new[] { "MaDanhMuc", "DaXoa" });

            migrationBuilder.CreateIndex(
                name: "IX_CauHoi_NguoiTaoId_DaXoa_NgayTao",
                table: "CauHoi",
                columns: new[] { "NguoiTaoId", "DaXoa", "NgayTao" });

            migrationBuilder.CreateIndex(
                name: "IX_BaiThi_DaXoa_CheDoCongKhai_TrangThai",
                table: "BaiThi",
                columns: new[] { "DaXoa", "CheDoCongKhai", "TrangThai" });

            migrationBuilder.CreateIndex(
                name: "IX_BaiThi_MaTruyCapDinhDanh_DaXoa",
                table: "BaiThi",
                columns: new[] { "MaTruyCapDinhDanh", "DaXoa" });

            migrationBuilder.CreateIndex(
                name: "IX_BaiThi_NguoiTaoId_DaXoa_NgayTao",
                table: "BaiThi",
                columns: new[] { "NguoiTaoId", "DaXoa", "NgayTao" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LuotLamBai_MaBaiThi_TrangThai_Diem",
                table: "LuotLamBai");

            migrationBuilder.DropIndex(
                name: "IX_LuotLamBai_TrangThai_ThoiGianKetThuc",
                table: "LuotLamBai");

            migrationBuilder.DropIndex(
                name: "IX_CauTraLoi_MaLuotLamBai_MaCauHoi",
                table: "CauTraLoi");

            migrationBuilder.DropIndex(
                name: "IX_CauHoi_DaXoa_NguoiTaoId_CongKhai",
                table: "CauHoi");

            migrationBuilder.DropIndex(
                name: "IX_CauHoi_MaDanhMuc_DaXoa",
                table: "CauHoi");

            migrationBuilder.DropIndex(
                name: "IX_CauHoi_NguoiTaoId_DaXoa_NgayTao",
                table: "CauHoi");

            migrationBuilder.DropIndex(
                name: "IX_BaiThi_DaXoa_CheDoCongKhai_TrangThai",
                table: "BaiThi");

            migrationBuilder.DropIndex(
                name: "IX_BaiThi_MaTruyCapDinhDanh_DaXoa",
                table: "BaiThi");

            migrationBuilder.DropIndex(
                name: "IX_BaiThi_NguoiTaoId_DaXoa_NgayTao",
                table: "BaiThi");

            migrationBuilder.CreateIndex(
                name: "IX_CauTraLoi_MaLuotLamBai",
                table: "CauTraLoi",
                column: "MaLuotLamBai");
        }
    }
}
