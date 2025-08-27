using ChallengeBet.Application.Players.Dtos;
using FluentValidation;

namespace ChallengeBet.Application.Players.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}