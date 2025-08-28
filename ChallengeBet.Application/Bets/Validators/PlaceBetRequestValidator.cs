using ChallengeBet.Application.Bets.Dtos;
using FluentValidation;

namespace ChallengeBet.Application.Bets.Validators;

public class PlaceBetRequestValidator : AbstractValidator<PlaceBetRequest>
{
    public PlaceBetRequestValidator()
    {
        RuleFor(x => x.Amount).NotEmpty().GreaterThanOrEqualTo(1.00m);
        RuleFor(x => x.Multiplier).GreaterThanOrEqualTo(1.0m).When(x => x.Multiplier.HasValue);
    }
}