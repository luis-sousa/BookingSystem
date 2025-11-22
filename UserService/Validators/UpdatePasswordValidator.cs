using FluentValidation;
using UserService.DTOs;

namespace UserService.Validators
{
    public class UpdatePasswordValidator : AbstractValidator<UpdatePasswordDto>
    {
        public UpdatePasswordValidator()
        {
            RuleFor(x => x.CurrentPassword)
                 .NotEmpty().WithMessage("Current Password is required.")
                 .MinimumLength(6).WithMessage("Password must have at least 6 characters.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must have at least 6 characters.");
        }
    }
}
