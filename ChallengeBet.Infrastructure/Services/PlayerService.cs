using ChallengeBet.Application.Abstractions;
using ChallengeBet.Application.Players;
using ChallengeBet.Application.Players.Dtos;
using ChallengeBet.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChallengeBet.Infrastructure.Services;

public class PlayerService(
    AppDbContext db,
    IPasswordHasher<Player> hasher,
    ITokenService tokenSvc
) : IPlayerService
{
    public async Task<(PlayerDto player, WalletDto wallet)> RegisterAsync(RegisterPlayerRequest req, CancellationToken ct)
    {
        var emailExists = await db.Players.AnyAsync(p => p.Email == req.Email, ct);
        if (emailExists)
            throw new InvalidOperationException("E-mail já cadastrado.");

        var player = new Player
        {
            Name = req.Name.Trim(),
            Email = req.Email.Trim().ToLower()
        };
        player.PasswordHash = hasher.HashPassword(player, req.Password);

        var wallet = new Wallet
        {
            Player = player,
            Balance = req.InitialBalance ?? 0m,
            Currency = string.IsNullOrWhiteSpace(req.Currency) ? "BRL" : req.Currency.ToUpper()
        };

        db.Players.Add(player);
        db.Wallets.Add(wallet);
        db.PlayerPoints.Add(new PlayerPoints { PlayerId = player.Id, Points = 0 });

        await db.SaveChangesAsync(ct);

        return (new PlayerDto { Id = player.Id, Name = player.Name, Email = player.Email },
                new WalletDto { Balance = wallet.Balance, Currency = wallet.Currency });
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest req, CancellationToken ct)
    {
        var player = await db.Players
            .Include(p => p.Wallet)
            .FirstOrDefaultAsync(p => p.Email == req.Email.ToLower(), ct);

        if (player is null)
            throw new InvalidOperationException("Credenciais inválidas.");

        var result = hasher.VerifyHashedPassword(player, player.PasswordHash, req.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new InvalidOperationException("Credenciais inválidas.");

        var token = tokenSvc.GenerateToken(player);

        return new AuthResponse
        {
            Token = token,
            Player = new PlayerDto { Id = player.Id, Name = player.Name, Email = player.Email },
            Wallet = new WalletDto { Balance = player.Wallet?.Balance ?? 0, Currency = player.Wallet?.Currency ?? "BRL" }
        };
    }
}