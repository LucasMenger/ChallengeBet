using ChallengeBet.Application.Bets.Dtos;
using ChallengeBet.Domain.Entities;
using ChallengeBet.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using FluentAssertions;
using Xunit;

namespace ChallengeBet.Tests;

public class ConcurrencyTests
{
    [Fact]
    public async Task Parallel_Bets_Should_Not_Overdraw()
    {
        using var db = DbTestHelper.CreateNewDb();
        // seed player+wallet
        var player = new Player { Name="Test", Email="t@t.com", PasswordHash="x" };
        var wallet = new Wallet { Player=player, Balance=100m, Currency="BRL" };
        db.AddRange(player, wallet, new PlayerPoints{ PlayerId = player.Id, Points = 0 });
        await db.SaveChangesAsync();

        var betSvc = new BetService(db, new RtpConfig(new ConfigurationBuilder().Build()), new RandomProvider());

        var req = new PlaceBetRequest { Amount = 10m, Multiplier = 2m, AutoSettle=false };
        var tasks = Enumerable.Range(0, 20).Select(_ => betSvc.PlaceBetAsync(player.Id, req, default)).ToList();

        await Task.WhenAll(tasks.Select(t => t.ContinueWith(_ => { }))); 

        var refreshed = await db.Wallets.AsNoTracking().SingleAsync(w => w.Id == wallet.Id);
        refreshed.Balance.Should().BeGreaterThanOrEqualTo(0m);
        var success = tasks.Count(t => t.Status == TaskStatus.RanToCompletion);
        success.Should().BeGreaterThanOrEqualTo(10);
    }
}