var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(); // still needs this in order to Ardalis.ApiEndpoints to work

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers() // still needs this in order to Ardalis.ApiEndpoints to work
    .RequireAuthorization();

app.Run();