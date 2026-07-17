using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using System.Security.Claims;
using NewsWave.Data;
using NewsWave.NewsMaker;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddDataProtection().SetApplicationName("NewsWave");
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });
builder.Services.AddAuthorization(options => options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
builder.Services.AddHealthChecks();
builder.Services.AddSingleton<NewsWaveStore>();
builder.Services.AddSingleton<NewsMakerSettingsStore>();
builder.Services.AddSingleton<BridgeNoteRepository>();
builder.Services.AddSingleton<NewsletterArchiveBuilder>();
builder.Services.AddSingleton<NewsletterSmtpSender>();
builder.Services.AddSingleton<NewsMakerHistoryStore>();
builder.Services.AddSingleton<NewsMakerRunService>();
builder.Services.AddSingleton<INewsMakerRunner>(services => services.GetRequiredService<NewsMakerRunService>());
builder.Services.AddHostedService(services => services.GetRequiredService<NewsMakerRunService>());

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.Use(async (context, next) =>
{
    string? adminPassword = app.Configuration["NewsWave:AdminPassword"];
    if (context.User.Identity?.IsAuthenticated != true &&
        string.IsNullOrWhiteSpace(adminPassword) &&
        context.Connection.RemoteIpAddress is IPAddress address &&
        IPAddress.IsLoopback(address))
    {
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, "Локальный администратор")],
            "NewsWaveLocal"));
    }

    if (context.User.Identity?.IsAuthenticated != true &&
        string.IsNullOrWhiteSpace(adminPassword) &&
        !context.Request.Path.StartsWithSegments("/Login"))
    {
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        await context.Response.WriteAsync("Для удалённого доступа задайте NewsWave__AdminPassword.");
        return;
    }
    await next();
});
app.UseAuthorization();
app.MapHealthChecks("/health").AllowAnonymous();
app.MapRazorPages();
app.Run();
