using FluentValidation;
using QuizHub.Models.DTOs.Auth;

namespace QuizHub.Validators.Auth
{
    /// <summary>
    /// Validator cho RegisterDto
    /// </summary>
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống")
                .EmailAddress().WithMessage("Email không đúng định dạng")
                .MaximumLength(256).WithMessage("Email không được quá 256 ký tự");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Mật khẩu không được để trống")
                .MinimumLength(6).WithMessage("Mật khẩu phải có ít nhất 6 ký tự")
                .MaximumLength(100).WithMessage("Mật khẩu không được quá 100 ký tự")
                .Matches("[A-Z]").WithMessage("Mật khẩu phải chứa ít nhất 1 chữ hoa")
                .Matches("[a-z]").WithMessage("Mật khẩu phải chứa ít nhất 1 chữ thường")
                .Matches("[0-9]").WithMessage("Mật khẩu phải chứa ít nhất 1 số");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Xác nhận mật khẩu không được để trống")
                .Equal(x => x.Password).WithMessage("Xác nhận mật khẩu không khớp");

            RuleFor(x => x.HoTen)
                .NotEmpty().WithMessage("Họ tên không được để trống")
                .MaximumLength(200).WithMessage("Họ tên không được quá 200 ký tự");
        }
    }

    /// <summary>
    /// Validator cho LoginDto
    /// </summary>
    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống")
                .EmailAddress().WithMessage("Email không đúng định dạng");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Mật khẩu không được để trống");
        }
    }

    /// <summary>
    /// Validator cho ChangePasswordDto
    /// </summary>
    public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordDtoValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Mật khẩu hiện tại không được để trống");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Mật khẩu mới không được để trống")
                .MinimumLength(6).WithMessage("Mật khẩu mới phải có ít nhất 6 ký tự")
                .MaximumLength(100).WithMessage("Mật khẩu mới không được quá 100 ký tự")
                .Matches("[A-Z]").WithMessage("Mật khẩu mới phải chứa ít nhất 1 chữ hoa")
                .Matches("[a-z]").WithMessage("Mật khẩu mới phải chứa ít nhất 1 chữ thường")
                .Matches("[0-9]").WithMessage("Mật khẩu mới phải chứa ít nhất 1 số")
                .NotEqual(x => x.CurrentPassword).WithMessage("Mật khẩu mới phải khác mật khẩu hiện tại");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Xác nhận mật khẩu không được để trống")
                .Equal(x => x.NewPassword).WithMessage("Xác nhận mật khẩu không khớp");
        }
    }
}
