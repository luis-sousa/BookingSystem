using FluentValidation;
using UserService.DTOs;

namespace UserService.Validators
{
    public class UpdateUserValidator : AbstractValidator<UpdateUserDto>
    {
        public UpdateUserValidator()
        {
            RuleFor(x => x.Username)
                 .NotEmpty().WithMessage("Username is required.")
                 .MinimumLength(3).WithMessage("Username must have at least 3 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Formato de email inválido");
        }
    }
}
