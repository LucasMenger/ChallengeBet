using ChallengeBet.Application.Bets.Dtos;
using ChallengeBet.Application.Common;
using ChallengeBet.Application.Wallets;
using Microsoft.EntityFrameworkCore;

namespace ChallengeBet.Infrastructure.Services;

public class WalletService(AppDbContext db) : IWalletService
{
    public async Task<WalletDto> GetMyWalletAsync(long playerId, CancellationToken ct)
    {
        var w = await db.Wallets.AsNoTracking().FirstOrDefaultAsync(x => x.PlayerId == playerId, ct)
                ?? throw new InvalidOperationException("Carteira não encontrada.");
        return new WalletDto { Balance = w.Balance, Currency = w.Currency };
    }

    public async Task<PagedResult<TransactionDto>> ListMyTransactionsAsync(long playerId, int page, int pageSize, CancellationToken ct)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0 || pageSize > 100) pageSize = 20;

        var q = db.Transactions.AsNoTracking()
            .Where(t => t.PlayerId == playerId)
            .OrderByDescending(t => t.CreatedAt);

        var total = await q.LongCountAsync(ct);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                Type = t.Type.ToString(),
                Value = t.Value,
                CreatedAt = t.CreatedAt
            }).ToListAsync(ct);

        return new PagedResult<TransactionDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            Total = total
        };
    }
}