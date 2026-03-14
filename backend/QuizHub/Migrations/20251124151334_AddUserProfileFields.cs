using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizHub.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DiaChi",
                table: "NguoiDung",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ProfileCongKhai",
                table: "NguoiDung",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TieuSu",
                table: "NguoiDung",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiaChi",
                table: "NguoiDung");

            migrationBuilder.DropColumn(
                name: "ProfileCongKhai",
                table: "NguoiDung");

            migrationBuilder.DropColumn(
                name: "TieuSu",
                table: "NguoiDung");
        }
    }
}
