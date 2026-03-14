using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizHub.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDanhGiaUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DanhGia_MaBaiThi_NguoiDungId",
                table: "DanhGia");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_DanhGia_MaBaiThi_NguoiDungId",
                table: "DanhGia",
                columns: new[] { "MaBaiThi", "NguoiDungId" },
                unique: true);
        }
    }
}
