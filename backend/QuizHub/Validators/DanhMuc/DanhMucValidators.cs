using FluentValidation;
using QuizHub.Models.DTOs.DanhMuc;

namespace QuizHub.Validators.DanhMuc
{
    /// <summary>
    /// Validator cho CreateDanhMucDto
    /// </summary>
    public class CreateDanhMucDtoValidator : AbstractValidator<CreateDanhMucDto>
    {
        public CreateDanhMucDtoValidator()
        {
            RuleFor(x => x.TenDanhMuc)
                .NotEmpty().WithMessage("Tên danh mục không được để trống")
                .MaximumLength(200).WithMessage("Tên danh mục không được quá 200 ký tự")
                .Matches(@"^[\p{L}\p{N}\s\-_]+$").WithMessage("Tên danh mục chỉ được chứa chữ cái, số, khoảng trắng, dấu gạch ngang và gạch dưới");

            RuleFor(x => x.MoTa)
                .MaximumLength(1000).WithMessage("Mô tả không được quá 1000 ký tự");

            RuleFor(x => x.HinhAnh)
                .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.HinhAnh))
                .WithMessage("Đường dẫn hình ảnh không được quá 500 ký tự");
        }
    }

    /// <summary>
    /// Validator cho UpdateDanhMucDto
    /// </summary>
    public class UpdateDanhMucDtoValidator : AbstractValidator<UpdateDanhMucDto>
    {
        public UpdateDanhMucDtoValidator()
        {
            RuleFor(x => x.TenDanhMuc)
                .NotEmpty().WithMessage("Tên danh mục không được để trống")
                .MaximumLength(200).WithMessage("Tên danh mục không được quá 200 ký tự");

            RuleFor(x => x.MoTa)
                .MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.MoTa))
                .WithMessage("Mô tả không được quá 1000 ký tự");

            RuleFor(x => x.HinhAnh)
                .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.HinhAnh))
                .WithMessage("Đường dẫn hình ảnh không được quá 500 ký tự");
        }
    }
}
