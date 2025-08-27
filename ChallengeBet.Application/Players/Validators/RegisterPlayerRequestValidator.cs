using ChallengeBet.Application.Players.Dtos;
using FluentValidation;

namespace ChallengeBet.Application.Players.Validators;

public class RegisterPlayerRequestValidator : AbstractValidator<RegisterPlayerRequest>
{
    public RegisterPlayerRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(2).MaximumLength(120);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(160);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.InitialBalance).GreaterThanOrEqualTo(0).When(x => x.InitialBalance.HasValue);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}