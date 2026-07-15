using NewsWave.Data;
using NewsWave.Mail;
using NewsWave.NewsMaker;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddOptions<MailOptions>()
    .Bind(builder.Configuration.GetSection(MailOptions.SectionName))
    .Validate(x => x.Port is > 0 and <= 65535, "SMTP port is invalid.");
builder.Services.AddSingleton<NewsWaveStore>();
builder.Services.AddSingleton<NewsMakerSettingsStore>();
builder.Services.AddSingleton<BridgeNoteRepository>();
builder.Services.AddSingleton<NewsletterArchiveBuilder>();
builder.Services.AddSingleton<NewsletterSmtpSender>();
builder.Services.AddSingleton<NewsMakerRunService>();
builder.Services.AddSingleton<INewsMakerRunner>(services => services.GetRequiredService<NewsMakerRunService>());
builder.Services.AddHostedService(services => services.GetRequiredService<NewsMakerRunService>());
builder.Services.AddSingleton<MailOutbox>();
builder.Services.AddSingleton<IMailQueue>(services => services.GetRequiredService<MailOutbox>());
builder.Services.AddHostedService(services => services.GetRequiredService<MailOutbox>());

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.Run();
