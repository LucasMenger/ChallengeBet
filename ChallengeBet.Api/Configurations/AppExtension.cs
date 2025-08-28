namespace ChallengeBet.Api.Configurations;

public static class AppExtension
{
    public static void ConfigureDevEnvironment(this WebApplication app)
    {
        if (app.Environment.IsDevelopment()) {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
    }
}