using FluentValidation;
using Microsoft.AspNetCore.JsonPatch;
using UserService.DTOs;

namespace UserService.Validators
{
    public class PatchUserValidator : AbstractValidator<JsonPatchDocument<UpdateUserDto>>
    {
        public PatchUserValidator()
        {
            RuleFor(x => x).NotNull().WithMessage("O patch não pode ser nulo");

            // Se quiseres, podes validar operações específicas, por exemplo:
            RuleForEach(x => x.Operations).Must(op =>
            {
                // Apenas permite operações de replace, add ou remove
                return op.op == "replace" || op.op == "add" || op.op == "remove";
            }).WithMessage("Operação de patch inválida");
        }
    }
}
