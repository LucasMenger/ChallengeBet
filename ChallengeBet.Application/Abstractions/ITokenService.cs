using ChallengeBet.Domain.Entities;

namespace ChallengeBet.Application.Abstractions;

public interface ITokenService
{
    string GenerateToken(Player player);
}