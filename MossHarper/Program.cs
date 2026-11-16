using System.Threading.RateLimiting;
using System.Net;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using MossHarper.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
var trustedProxyAddresses = builder.Configuration.GetSection("ReverseProxy:KnownProxies").Get<string[]>() ?? [];
if (trustedProxyAddresses.Length > 0)
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        foreach (var address in trustedProxyAddresses)
            options.KnownProxies.Add(IPAddress.Parse(address));
    });
}

var keysPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrWhiteSpace(keysPath))
    builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(keysPath));

builder.Services.Configure<ContactDeliveryOptions>(builder.Configuration.GetSection("ContactDelivery"));
builder.Services.Configure<GmailSmtpOptions>(builder.Configuration.GetSection("GmailSmtp"));
builder.Services.Configure<MailerSendOptions>(builder.Configuration.GetSection("MailerSend"));
var mailProvider = builder.Configuration["ContactDelivery:Provider"] ?? "GmailSmtp";
if (mailProvider.Equals("GmailSmtp", StringComparison.OrdinalIgnoreCase))
    builder.Services.AddTransient<IContactMailer, GmailSmtpContactMailer>();
else if (mailProvider.Equals("MailerSend", StringComparison.OrdinalIgnoreCase))
    builder.Services.AddHttpClient<IContactMailer, MailerSendContactMailer>(client =>
    {
        client.BaseAddress = new Uri("https://api.mailersend.com/");
        client.Timeout = TimeSpan.FromSeconds(15);
    });
else
    throw new InvalidOperationException("ContactDelivery:Provider must be GmailSmtp or MailerSend.");
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("contact", context =>
        HttpMethods.IsPost(context.Request.Method)
            ? RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 30,
                    Window = TimeSpan.FromMinutes(10),
                    QueueLimit = 0
                })
            : RateLimitPartition.GetNoLimiter("contact-page"));
});

var app = builder.Build();

if (trustedProxyAddresses.Length > 0)
    app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.MapRazorPages();
app.Run();
