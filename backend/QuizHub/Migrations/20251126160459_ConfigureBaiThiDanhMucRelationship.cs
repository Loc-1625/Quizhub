using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizHub.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureBaiThiDanhMucRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaiThi_DanhMuc_MaDanhMuc",
                table: "BaiThi");

            migrationBuilder.AddForeignKey(
                name: "FK_BaiThi_DanhMuc_MaDanhMuc",
                table: "BaiThi",
                column: "MaDanhMuc",
                principalTable: "DanhMuc",
                principalColumn: "MaDanhMuc",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaiThi_DanhMuc_MaDanhMuc",
                table: "BaiThi");

            migrationBuilder.AddForeignKey(
                name: "FK_BaiThi_DanhMuc_MaDanhMuc",
                table: "BaiThi",
                column: "MaDanhMuc",
                principalTable: "DanhMuc",
                principalColumn: "MaDanhMuc");
        }
    }
}
