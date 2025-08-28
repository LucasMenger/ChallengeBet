using ChallengeBet.Application.Abstractions;
using ChallengeBet.Application.Bets;
using ChallengeBet.Application.Bets.Dtos;
using ChallengeBet.Application.Common;
using ChallengeBet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChallengeBet.Infrastructure.Services;

public class BetService(
    AppDbContext db,
    IRtpConfig rtpCfg,
    IRandomProvider rng
) : IBetService
{
    private const int MaxRetries = 3;

    public async Task<BetDto> PlaceBetAsync(long playerId, PlaceBetRequest req, CancellationToken ct)
    {
        var multiplier = req.Multiplier ?? rtpCfg.GetDefaultMultiplier();
        if (multiplier <= 0) throw new InvalidOperationException("Multiplier inválido.");

        for (int attempt = 1; ; attempt++)
        {
            using var tx = await db.Database.BeginTransactionAsync(ct);
            try
            {
                var wallet = await db.Wallets.FirstOrDefaultAsync(w => w.PlayerId == playerId, ct)
                    ?? throw new InvalidOperationException("Carteira não encontrada.");

                if (req.Amount < 1.00m) throw new InvalidOperationException("Valor mínimo da aposta é R$ 1,00.");
                if (wallet.Balance < req.Amount) throw new InvalidOperationException("Saldo insuficiente.");

                wallet.Balance -= req.Amount;

                var bet = new Bet
                {
                    PlayerId = playerId,
                    Amount = req.Amount,
                    Multiplier = multiplier,
                    Status = Enums.BetStatus.Pending
                };
                db.Bets.Add(bet);

                db.Transactions.Add(new Transaction
                {
                    WalletId = wallet.Id,
                    PlayerId = playerId,
                    BetId = null, 
                    Type = Enums.TransactionType.BetDebit,
                    Value = req.Amount
                });

                await db.SaveChangesAsync(ct);

                var lastDebit = await db.Transactions
                    .Where(t => t.PlayerId == playerId && t.Type == Enums.TransactionType.BetDebit)
                    .OrderByDescending(t => t.Id)
                    .FirstOrDefaultAsync(ct);
                if (lastDebit != null && lastDebit.BetId is null)
                {
                    lastDebit.BetId = bet.Id;
                    await db.SaveChangesAsync(ct);
                }

                if (req.AutoSettle)
                {
                    await SettleBetAsync(playerId, wallet, bet, ct);
                }

                await tx.CommitAsync(ct);

                return new BetDto
                {
                    Id = bet.Id,
                    Amount = bet.Amount,
                    Multiplier = bet.Multiplier,
                    Status = bet.Status.ToString(),
                    Prize = bet.Prize,
                    CreatedAt = bet.CreatedAt,
                    CancelledAt = bet.CancelledAt
                };
            }
            catch (DbUpdateConcurrencyException) when (attempt < MaxRetries)
            {
                await tx.RollbackAsync(ct);
                await Task.Delay(25 * attempt, ct);
                db.ChangeTracker.Clear(); 
                continue;
            }
        }
    }

    private async Task SettleBetAsync(long playerId, Wallet wallet, Bet bet, CancellationToken ct)
    {
        var rtp = rtpCfg.GetRtp();
        var p = (double)(rtp / bet.Multiplier);
        if (p < 0) p = 0; if (p > 1) p = 1;

        var u = rng.NextUnitDouble();
        var win = u < p;

        if (win)
        {
            bet.Status = Enums.BetStatus.Won;
            bet.Prize = Math.Round(bet.Amount * bet.Multiplier, 2, MidpointRounding.AwayFromZero);

            wallet.Balance += bet.Prize.Value;

            db.Transactions.Add(new Transaction
            {
                WalletId = wallet.Id,
                PlayerId = playerId,
                BetId = bet.Id,
                Type = Enums.TransactionType.PrizeCredit,
                Value = bet.Prize.Value
            });

            await db.SaveChangesAsync(ct);
        }
        else
        {
            bet.Status = Enums.BetStatus.Lost;
            bet.Prize = null;

            var pp = await db.PlayerPoints.FirstOrDefaultAsync(x => x.PlayerId == playerId, ct);
            if (pp == null)
            {
                pp = new PlayerPoints { PlayerId = playerId, Points = 0 };
                db.PlayerPoints.Add(pp);
                await db.SaveChangesAsync(ct);
            }

            pp.Points += (long)decimal.Truncate(bet.Amount); 

            var rules = await db.BonusRules.AsNoTracking().OrderBy(r => r.PointsThreshold).ToListAsync(ct);
            var claimedIds = await db.BonusClaims.AsNoTracking()
                                .Where(c => c.PlayerId == playerId)
                                .Select(c => c.RuleId).ToListAsync(ct);

            foreach (var r in rules)
            {
                if (pp.Points >= r.PointsThreshold && !claimedIds.Contains(r.Id))
                {
                    db.BonusClaims.Add(new BonusClaim { PlayerId = playerId, RuleId = r.Id });

                    wallet.Balance += r.RewardValue;

                    db.Transactions.Add(new Transaction
                    {
                        WalletId = wallet.Id,
                        PlayerId = playerId,
                        BetId = bet.Id,
                        Type = Enums.TransactionType.BonusCredit,
                        Value = r.RewardValue
                    });
                }
            }

            await db.SaveChangesAsync(ct);
        }
    }

    public async Task<BetDto> CancelBetAsync(long playerId, long betId, CancellationToken ct)
    {
        for (int attempt = 1; ; attempt++)
        {
            using var tx = await db.Database.BeginTransactionAsync(ct);
            try
            {
                var bet = await db.Bets.FirstOrDefaultAsync(b => b.Id == betId && b.PlayerId == playerId, ct)
                          ?? throw new InvalidOperationException("Aposta não encontrada.");
                if (bet.Status != Enums.BetStatus.Pending)
                    throw new InvalidOperationException("Só é possível cancelar apostas pendentes.");

                var wallet = await db.Wallets.FirstOrDefaultAsync(w => w.PlayerId == playerId, ct)
                             ?? throw new InvalidOperationException("Carteira não encontrada.");

                wallet.Balance += bet.Amount;
                bet.Status = Enums.BetStatus.Cancelled;
                bet.CancelledAt = DateTime.UtcNow;

                db.Transactions.Add(new Transaction
                {
                    WalletId = wallet.Id,
                    PlayerId = playerId,
                    BetId = bet.Id,
                    Type = Enums.TransactionType.RefundCredit,
                    Value = bet.Amount
                });

                await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                return new BetDto
                {
                    Id = bet.Id,
                    Amount = bet.Amount,
                    Multiplier = bet.Multiplier,
                    Status = bet.Status.ToString(),
                    Prize = bet.Prize,
                    CreatedAt = bet.CreatedAt,
                    CancelledAt = bet.CancelledAt
                };
            }
            catch (DbUpdateConcurrencyException) when (attempt < MaxRetries)
            {
                await tx.RollbackAsync(ct);
                await Task.Delay(25 * attempt, ct);
                db.ChangeTracker.Clear();
                continue;
            }
        }
    }

    public async Task<PagedResult<BetDto>> ListMyBetsAsync(long playerId, BetListFilter filter, CancellationToken ct)
    {
        var q = db.Bets.AsNoTracking().Where(b => b.PlayerId == playerId);

        if (!string.IsNullOrWhiteSpace(filter.Status) &&
            Enum.TryParse<Enums.BetStatus>(filter.Status, ignoreCase: true, out var s))
        {
            q = q.Where(b => b.Status == s);
        }

        var page = filter.Page <= 0 ? 1 : filter.Page;
        var pageSize = (filter.PageSize <= 0 || filter.PageSize > 100) ? 20 : filter.PageSize;

        q = q.OrderByDescending(b => b.CreatedAt);

        var total = await q.LongCountAsync(ct);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(b => new BetDto
            {
                Id = b.Id,
                Amount = b.Amount,
                Multiplier = b.Multiplier,
                Status = b.Status.ToString(),
                Prize = b.Prize,
                CreatedAt = b.CreatedAt,
                CancelledAt = b.CancelledAt
            }).ToListAsync(ct);

        return new PagedResult<BetDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            Total = total
        };
    }
}