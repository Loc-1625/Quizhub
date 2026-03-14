using QuizHub.Models.DTOs.Admin;
using QuizHub.Models.DTOs.CauHoi;

namespace QuizHub.Services.Interfaces
{
  public interface IAdminService
  {
    /// <summary>
    /// [AD-02] L?y th?ng k� t?ng quan Dashboard
    /// </summary>
    Task<DashboardStatsDto> GetDashboardStatsAsync();

    /// <summary>
    /// [AD-03] L?y danh s�ch ng??i d�ng
    /// </summary>
    Task<(IEnumerable<UserManagementDto> Items, int TotalCount)> GetUsersAsync(
   string? searchTerm = null,
      bool? activeStatus = null,
      string? role = null,
  int pageNumber = 1,
    int pageSize = 20);

    /// <summary>
    /// [AD-03] L?y chi ti?t m?t ng??i d�ng
    /// </summary>
    Task<UserManagementDto?> GetUserByIdAsync(string userId);

    /// <summary>
    /// [AD-03] Kích hoạt/Vô hiệu hóa tài khoản
    /// </summary>
    Task<bool> ToggleUserStatusAsync(string userId);

    /// <summary>
    /// [AD-03] Cập nhật vai trò người dùng
    /// </summary>
    Task<(bool Success, string Message)> UpdateUserRoleAsync(string userId, string role);

    /// <summary>
    /// [AD-03] Xóa người dùng
    /// </summary>
    Task<(bool Success, string Message)> DeleteUserAsync(string userId);

    /// <summary>
    /// [AD-04] Lấy danh sách tất cả nội dung (Bài thi + Câu hỏi)
    /// </summary>
    Task<(IEnumerable<ContentManagementDto> Items, int TotalCount)> GetAllContentAsync(
    string? loaiNoiDung = null, // "BaiThi" or "CauHoi"
            string? searchTerm = null,
            string? tenNguoiTao = null,
      bool? daXoa = null,
      string? sortBy = "NgayTao",
      string? sortOrder = "DESC",
      Guid? maDanhMuc = null,
          int pageNumber = 1,
       int pageSize = 20);

    Task<CauHoiDto?> GetQuestionAndAnswerByAdminAsync(Guid id);

    /// <summary>
    /// [AD-04] Xóa nội dung vi phạm (soft delete hoặc hard delete)
    /// </summary>
    Task<bool> DeleteContentAsync(string loaiNoiDung, Guid id, bool hardDelete = false);

    /// <summary>
    /// [AD-04] Khôi phục nội dung đã xóa
    /// </summary>
    Task<bool> RestoreContentAsync(string loaiNoiDung, Guid id);

    /// <summary>
    /// Recalculate điểm trung bình đánh giá cho tất cả bài thi
    /// </summary>
    Task<int> RecalculateAllRatingsAsync();
  }
}
