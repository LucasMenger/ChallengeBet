using System.Reflection;
using ChallengeBet.Api.Configurations;
using ChallengeBet.Api.Configurations.Errors;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddSecurity();
builder.AddDataContexts();
builder.AddServices();
builder.AddCrossOrigin();
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
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors(ApiConfiguration.CorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.ConfigureDevEnvironment();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();