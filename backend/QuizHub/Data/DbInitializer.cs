using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuizHub.Models.Entities;

namespace QuizHub.Data
{
    public static class DbInitializer
    {
        public static async Task SeedData(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<NguoiDung>>();
            var context = serviceProvider.GetRequiredService<QuizHubDbContext>();

            // Tạo roles
            string[] roleNames = { "Admin", "User" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Tạo admin user
            var adminEmail = "admin@quizhub.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var admin = new NguoiDung
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    HoTen = "Quản trị viên",
                    EmailConfirmed = true,
                    TrangThaiKichHoat = true,
                    NgayTao = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(admin, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            // Tạo danh mục mẫu nếu chưa có
            if (!await context.DanhMuc.AnyAsync())
            {
                var danhMucs = new List<DanhMuc>
                {
                    new DanhMuc { TenDanhMuc = "Toán học", MoTa = "Các câu hỏi và bài thi môn Toán học", DuongDan = "toan-hoc" },
                    new DanhMuc { TenDanhMuc = "Vật lý", MoTa = "Các câu hỏi và bài thi môn Vật lý", DuongDan = "vat-ly" },
                    new DanhMuc { TenDanhMuc = "Hóa học", MoTa = "Các câu hỏi và bài thi môn Hóa học", DuongDan = "hoa-hoc" },
                    new DanhMuc { TenDanhMuc = "Tiếng Anh", MoTa = "Các câu hỏi và bài thi môn Tiếng Anh", DuongDan = "tieng-anh" },
                    new DanhMuc { TenDanhMuc = "Lập trình", MoTa = "Kiến thức công nghệ thông tin và lập trình", DuongDan = "lap-trinh" },
                    new DanhMuc { TenDanhMuc = "Lịch sử", MoTa = "Các câu hỏi về kiến thức lịch sử", DuongDan = "lich-su" },
                    new DanhMuc { TenDanhMuc = "Địa lý", MoTa = "Các câu hỏi về kiến thức địa lý", DuongDan = "dia-ly" },
                    new DanhMuc { TenDanhMuc = "Sinh học", MoTa = "Các câu hỏi về kiến thức sinh học", DuongDan = "sinh-hoc" },
                };

                await context.DanhMuc.AddRangeAsync(danhMucs);
                await context.SaveChangesAsync();
            }

            // Recalculate điểm trung bình từ đánh giá (một lần)
            await RecalculateRatingsAsync(context);
        }

        private static async Task RecalculateRatingsAsync(QuizHubDbContext context)
        {

            var baiThiList = await context.BaiThi
            .Select(b => new
            {
                BaiThi = b, // EF sẽ track object này
                AvgRating = context.DanhGia
                .Where(dg => dg.MaBaiThi == b.MaBaiThi)
                .Average(dg => (decimal?)dg.XepHang) ?? 0
            })
            .ToListAsync();

            if (baiThiList.Count == 0) return;

            foreach (var item in baiThiList)
            {
                item.BaiThi.XepHangTrungBinh = item.AvgRating;
            }

            await context.SaveChangesAsync();
        }
    }
}