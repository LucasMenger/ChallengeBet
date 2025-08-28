using System.Reflection;
using ChallengeBet.Api.Configurations;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddSecurity();
builder.AddDataContexts();
builder.AddServices();

builder.Services.AddAuthorization();

builder.Services
    .AddControllers()
    .AddFluentValidation(config =>
    {
        config.RegisterValidatorsFromAssemblyContaining<Program>();
    });builder.Services.AddValidatorsFromAssembly(Assembly.Load("ChallengeBet.Application"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.ConfigureDevEnvironment();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();