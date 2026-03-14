using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizHub.Migrations
{
    /// <inheritdoc />
    public partial class AddMaDanhMucToBaiThi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MaDanhMuc",
                table: "BaiThi",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BaiThi_MaDanhMuc",
                table: "BaiThi",
                column: "MaDanhMuc");

            migrationBuilder.AddForeignKey(
                name: "FK_BaiThi_DanhMuc_MaDanhMuc",
                table: "BaiThi",
                column: "MaDanhMuc",
                principalTable: "DanhMuc",
                principalColumn: "MaDanhMuc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaiThi_DanhMuc_MaDanhMuc",
                table: "BaiThi");

            migrationBuilder.DropIndex(
                name: "IX_BaiThi_MaDanhMuc",
                table: "BaiThi");

            migrationBuilder.DropColumn(
                name: "MaDanhMuc",
                table: "BaiThi");
        }
    }
}
