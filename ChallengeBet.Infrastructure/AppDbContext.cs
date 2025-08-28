using ChallengeBet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChallengeBet.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Player> Players => Set<Player>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Bet> Bets => Set<Bet>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<BonusRule> BonusRules => Set<BonusRule>();
    public DbSet<BonusClaim> BonusClaims => Set<BonusClaim>();
    public DbSet<PlayerPoints> PlayerPoints => Set<PlayerPoints>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Player>(b =>
        {
            b.Property(p => p.Name).HasMaxLength(120).IsRequired();
            b.Property(p => p.Email).HasMaxLength(160).IsRequired();
            b.Property(p => p.PasswordHash).HasMaxLength(255).IsRequired();
            b.HasIndex(p => p.Email).IsUnique();
            b.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            b.HasOne(p => p.Wallet)
                .WithOne(w => w.Player!)
                .HasForeignKey<Wallet>(w => w.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Wallet>(b =>
        {
            b.Property(w => w.Currency).HasMaxLength(3).IsRequired();
            b.Property(w => w.Balance).HasPrecision(18, 2);
            b.Property(w => w.RowVersion).IsRowVersion();
            b.HasIndex(w => w.PlayerId).IsUnique();
        });

        modelBuilder.Entity<Bet>(b =>
        {
            b.Property(e => e.Amount).HasPrecision(18, 2);
            b.Property(e => e.Multiplier).HasPrecision(9, 2);
            b.Property(e => e.Prize).HasPrecision(18, 2);
            b.Property(e => e.Status).HasConversion<int>();
            b.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            b.HasIndex(e => new { e.PlayerId, e.CreatedAt });
            b.HasCheckConstraint("CK_Bet_Min", "Amount >= 1.0");
        });

        modelBuilder.Entity<Transaction>(b =>
        {
            b.Property(t => t.Value).HasPrecision(18, 2);
            b.Property(t => t.Type).HasConversion<int>();
            b.Property(t => t.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            b.HasIndex(t => new { t.PlayerId, t.CreatedAt });
            b.HasCheckConstraint("CK_Transaction_Positive", "Value > 0");
        });

        modelBuilder.Entity<BonusRule>(b =>
        {
            b.Property(r => r.RewardValue).HasPrecision(18, 2);
            b.HasIndex(r => r.PointsThreshold);
        });

        modelBuilder.Entity<BonusClaim>(b =>
        {
            b.Property(c => c.ClaimedAt).HasDefaultValueSql("GETUTCDATE()");
            b.HasIndex(c => new { c.PlayerId, c.RuleId }).IsUnique();
        });
        modelBuilder.Entity<Player>(b =>
        {
            b.Property(p => p.Name).HasMaxLength(120).IsRequired();
            b.Property(p => p.Email).HasMaxLength(160).IsRequired();
            b.Property(p => p.PasswordHash).HasMaxLength(255).IsRequired();
            b.HasIndex(p => p.Email).IsUnique();
            b.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            b.HasOne(p => p.Wallet)
                .WithOne(w => w.Player!)
                .HasForeignKey<Wallet>(w => w.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Wallet>(b =>
        {
            b.Property(w => w.Currency).HasMaxLength(3).IsRequired();
            b.Property(w => w.Balance).HasPrecision(18, 2);
            b.Property(w => w.RowVersion).IsRowVersion();
            b.HasIndex(w => w.PlayerId).IsUnique();
        });

        modelBuilder.Entity<Bet>(b =>
        {
            b.Property(e => e.Amount).HasPrecision(18, 2);
            b.Property(e => e.Multiplier).HasPrecision(9, 2);
            b.Property(e => e.Prize).HasPrecision(18, 2);
            b.Property(e => e.Status).HasConversion<int>();
            b.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            b.HasIndex(e => new { e.PlayerId, e.CreatedAt });
            b.HasCheckConstraint("CK_Bet_Min", "Amount >= 1.0");
        });

        modelBuilder.Entity<Transaction>(b =>
        {
            b.Property(t => t.Value).HasPrecision(18, 2);
            b.Property(t => t.Type).HasConversion<int>();
            b.Property(t => t.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            b.HasIndex(t => new { t.PlayerId, t.CreatedAt });
            b.HasCheckConstraint("CK_Transaction_Positive", "Value > 0");

            b.HasOne(t => t.Wallet)
                .WithMany(w => w.Transactions)
                .HasForeignKey(t => t.WalletId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(t => t.Player)
                .WithMany() 
                .HasForeignKey(t => t.PlayerId)
                .OnDelete(DeleteBehavior.Restrict); 
        });

        modelBuilder.Entity<PlayerPoints>(b =>
        {
            b.HasKey(pp => pp.PlayerId);
            b.HasOne<Player>()
                .WithOne()
                .HasForeignKey<PlayerPoints>(pp => pp.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BonusRule>(b =>
        {
            b.Property(r => r.RewardValue).HasPrecision(18, 2);
            b.HasIndex(r => r.PointsThreshold);
        });

        modelBuilder.Entity<BonusClaim>(b =>
        {
            b.Property(c => c.ClaimedAt).HasDefaultValueSql("GETUTCDATE()");
            b.HasIndex(c => new { c.PlayerId, c.RuleId }).IsUnique();

            b.HasOne<Player>()
                .WithMany()
                .HasForeignKey(c => c.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}