using FluentValidation;
using QuizHub.Models.DTOs.BaiThi;

namespace QuizHub.Validators.BaiThi
{
    /// <summary>
    /// Validator cho CreateBaiThiDto
    /// </summary>
    public class CreateBaiThiDtoValidator : AbstractValidator<CreateBaiThiDto>
    {
        public CreateBaiThiDtoValidator()
        {
            RuleFor(x => x.TieuDe)
                .NotEmpty().WithMessage("Tiêu đề bài thi không được để trống")
                .MaximumLength(500).WithMessage("Tiêu đề bài thi không được quá 500 ký tự");

            RuleFor(x => x.MoTa)
                .MaximumLength(2000).WithMessage("Mô tả không được quá 2000 ký tự");

            RuleFor(x => x.ThoiGianLamBai)
                .GreaterThan(0).WithMessage("Thời gian làm bài phải lớn hơn 0")
                .LessThanOrEqualTo(300).WithMessage("Thời gian làm bài không được quá 300 phút (5 tiếng)");

            RuleFor(x => x.CheDoCongKhai)
                .Must(x => x == "CongKhai" || x == "RiengTu" || x == "CoMatKhau")
                .WithMessage("Chế độ công khai phải là 'CongKhai', 'RiengTu' hoặc 'CoMatKhau'");

            RuleFor(x => x.MatKhau)
                .MinimumLength(4).When(x => x.CheDoCongKhai == "CoMatKhau" && !string.IsNullOrEmpty(x.MatKhau))
                .WithMessage("Mật khẩu bài thi phải có ít nhất 4 ký tự");

            RuleFor(x => x.CacCauHoi)
                .NotEmpty().WithMessage("Bài thi phải có ít nhất 1 câu hỏi");

            RuleFor(x => x.DiemDat)
                .InclusiveBetween(0, 100).When(x => x.DiemDat.HasValue)
                .WithMessage("Điểm đạt phải từ 0 đến 100");
        }
    }

    /// <summary>
    /// Validator cho UpdateBaiThiDto
    /// </summary>
    public class UpdateBaiThiDtoValidator : AbstractValidator<UpdateBaiThiDto>
    {
        public UpdateBaiThiDtoValidator()
        {
            RuleFor(x => x.TieuDe)
                .NotEmpty().WithMessage("Tiêu đề bài thi không được để trống")
                .MaximumLength(500).WithMessage("Tiêu đề bài thi không được quá 500 ký tự");

            RuleFor(x => x.MoTa)
                .MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.MoTa))
                .WithMessage("Mô tả không được quá 2000 ký tự");

            RuleFor(x => x.ThoiGianLamBai)
                .GreaterThan(0).WithMessage("Thời gian làm bài phải lớn hơn 0")
                .LessThanOrEqualTo(300).WithMessage("Thời gian làm bài không được quá 300 phút");

            RuleFor(x => x.CheDoCongKhai)
                .Must(x => x == "CongKhai" || x == "RiengTu" || x == "CoMatKhau")
                .WithMessage("Chế độ công khai phải là 'CongKhai', 'RiengTu' hoặc 'CoMatKhau'");

            // CacCauHoi có thể rỗng khi chỉ update metadata (không thay đổi câu hỏi)
            // Nếu có câu hỏi thì validate từng câu
            RuleForEach(x => x.CacCauHoi)
                .ChildRules(cauHoi =>
                {
                    cauHoi.RuleFor(c => c.MaCauHoi)
                        .NotEmpty().WithMessage("Mã câu hỏi không được để trống");
                });

            RuleFor(x => x.DiemDat)
                .InclusiveBetween(0, 100).When(x => x.DiemDat.HasValue)
                .WithMessage("Điểm đạt phải từ 0 đến 100");
        }
    }
}
