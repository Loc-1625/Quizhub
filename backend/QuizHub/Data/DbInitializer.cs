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