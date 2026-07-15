using NewsWave.Mail;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddOptions<MailOptions>()
    .Bind(builder.Configuration.GetSection(MailOptions.SectionName))
    .Validate(x => x.Port is > 0 and <= 65535, "SMTP port is invalid.");
builder.Services.AddSingleton<MailOutbox>();
builder.Services.AddSingleton<IMailQueue>(services => services.GetRequiredService<MailOutbox>());
builder.Services.AddHostedService(services => services.GetRequiredService<MailOutbox>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
