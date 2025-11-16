using FluentValidation;
using UserService.DTOs;

namespace UserService.Validators
{
    public class CreateUserValidator : AbstractValidator<CreateUserDto>
    {
        //Fluent Validator
        public CreateUserValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required.")
                .MinimumLength(3).WithMessage("Username must have at least 3 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Formato de email inválido"); 

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must have at least 6 characters.");
        }
    }
}
