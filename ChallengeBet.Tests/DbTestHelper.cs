using ChallengeBet.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ChallengeBet.Tests;

public static class DbTestHelper
{
    public static AppDbContext CreateNewDb(string? name = null)
    {
        name ??= "ChallengeBetTest_" + Guid.NewGuid().ToString("N");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer($"Server=localhost,1433;Database={name};User Id=sa;Password=Challenge2025@Bet;Encrypt=false;TrustServerCertificate=true")
            .Options;

        var db = new AppDbContext(options);
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated(); 
        return db;
    }
}