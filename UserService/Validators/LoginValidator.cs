using FluentValidation;
using UserService.DTOs;

namespace UserService.Validators
{
    public class LoginValidator : AbstractValidator<LoginDto>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Email)
           .NotEmpty().WithMessage("O e-mail é obrigatório.")
           .EmailAddress().WithMessage("Formato de e-mail inválido.");
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("A senha é obrigatória.");

        }
    }
}
