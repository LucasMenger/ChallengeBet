using System.Text;
using ChallengeBet.Api.Hubs;
using ChallengeBet.Application.Abstractions;
using ChallengeBet.Application.Bets;
using ChallengeBet.Application.Players;
using ChallengeBet.Application.Wallets;
using ChallengeBet.Domain.Entities;
using ChallengeBet.Infrastructure;
using ChallengeBet.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ChallengeBet.Api.Configurations;

public static class BuilderExtension
{
    public static void AddSecurity(this WebApplicationBuilder builder)
    {
        var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-key-change-me";
        var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ChallengeBet";

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => {
                options.TokenValidationParameters = new TokenValidationParameters {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });
    }

    public static void AddCrossOrigin(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
            options.AddPolicy(
                ApiConfiguration.CorsPolicyName,
                policy => policy
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
            ));
    }

    public static void AddDataContexts(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
            )
        );
    }

    public static void AddServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IPasswordHasher<Player>, PasswordHasher<Player>>();
        builder.Services.AddScoped<ITokenService, JwtTokenService>();
        builder.Services.AddScoped<IPlayerService, PlayerService>();
        builder.Services.AddScoped<IWalletService, WalletService>();
        builder.Services.AddScoped<IBetService, BetService>();
        builder.Services.AddSingleton<IRandomProvider, RandomProvider>();
        builder.Services.AddSingleton<IRtpConfig, RtpConfig>();
        builder.Services.AddScoped<IWalletNotifier, SignalRWalletNotifier>();

    }
    
}