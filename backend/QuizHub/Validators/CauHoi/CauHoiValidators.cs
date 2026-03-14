using FluentValidation;
using QuizHub.Models.DTOs.CauHoi;

namespace QuizHub.Validators.CauHoi
{
    /// <summary>
    /// Validator cho CreateCauHoiDto
    /// </summary>
    public class CreateCauHoiDtoValidator : AbstractValidator<CreateCauHoiDto>
    {
        public CreateCauHoiDtoValidator()
        {
            RuleFor(x => x.NoiDungCauHoi)
                .NotEmpty().WithMessage("Nội dung câu hỏi không được để trống")
                .MaximumLength(5000).WithMessage("Nội dung câu hỏi không được quá 5000 ký tự");

            RuleFor(x => x.MucDo)
                .Must(x => string.IsNullOrEmpty(x) || x == "De" || x == "TrungBinh" || x == "Kho")
                .WithMessage("Mức độ phải là 'De', 'TrungBinh' hoặc 'Kho'");

            RuleFor(x => x.CacLuaChon)
                .NotEmpty().WithMessage("Câu hỏi phải có ít nhất 2 lựa chọn")
                .Must(x => x != null && x.Count >= 2).WithMessage("Câu hỏi phải có ít nhất 2 lựa chọn")
                .Must(x => x != null && x.Count <= 4).WithMessage("Câu hỏi không được có quá 4 lựa chọn")
                .Must(x => x != null && x.Count(lc => lc.LaDapAnDung) == 1)
                .WithMessage("Câu hỏi phải có đúng 1 đáp án đúng");

            RuleForEach(x => x.CacLuaChon)
                .ChildRules(luaChon =>
                {
                    luaChon.RuleFor(lc => lc.NoiDungDapAn)
                        .NotEmpty().WithMessage("Nội dung lựa chọn không được để trống")
                        .MaximumLength(1000).WithMessage("Nội dung lựa chọn không được quá 1000 ký tự");
                });

            RuleFor(x => x.GiaiThich)
                .MaximumLength(5000).WithMessage("Giải thích không được quá 5000 ký tự");
        }
    }

    /// <summary>
    /// Validator cho UpdateCauHoiDto
    /// </summary>
    public class UpdateCauHoiDtoValidator : AbstractValidator<UpdateCauHoiDto>
    {
        public UpdateCauHoiDtoValidator()
        {
            RuleFor(x => x.NoiDungCauHoi)
                .NotEmpty().WithMessage("Nội dung câu hỏi không được để trống")
                .MaximumLength(5000).WithMessage("Nội dung câu hỏi không được quá 5000 ký tự");

            RuleFor(x => x.MucDo)
                .Must(x => string.IsNullOrEmpty(x) || x == "De" || x == "TrungBinh" || x == "Kho")
                .WithMessage("Mức độ phải là 'De', 'TrungBinh' hoặc 'Kho'");

            RuleFor(x => x.CacLuaChon)
                .NotEmpty().WithMessage("Câu hỏi phải có ít nhất 2 lựa chọn")
                .Must(x => x != null && x.Count >= 2).WithMessage("Câu hỏi phải có ít nhất 2 lựa chọn")
                .Must(x => x != null && x.Count <= 4).WithMessage("Câu hỏi không được có quá 4 lựa chọn");

            RuleFor(x => x.GiaiThich)
                .MaximumLength(5000).When(x => !string.IsNullOrEmpty(x.GiaiThich))
                .WithMessage("Giải thích không được quá 5000 ký tự");
        }
    }
}
