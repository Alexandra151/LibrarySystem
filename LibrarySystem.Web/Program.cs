using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        opt.LoginPath = "/Account/Login";
        opt.LogoutPath = "/Account/Logout";
        opt.AccessDeniedPath = "/Account/Denied";
    });

builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

builder.Services.AddTransient<HeaderHandler>();

builder.Services.AddHttpClient("api", (sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var baseUrl = (cfg["Api:BaseUrl"] ?? "https://localhost:7182").TrimEnd('/');
    client.BaseAddress = new Uri(baseUrl);
})
.AddHttpMessageHandler<HeaderHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();

public class HeaderHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _ctx;
    private readonly IConfiguration _cfg;

    public HeaderHandler(IHttpContextAccessor ctx, IConfiguration cfg)
    {
        _ctx = ctx;
        _cfg = cfg;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var clientName = _cfg["Api:ClientName"] ?? "razor-admin";
        if (!request.Headers.Contains("X-Client-Name"))
            request.Headers.Add("X-Client-Name", clientName);

        var token = _ctx.HttpContext?.Session?.GetString("jwt");
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return base.SendAsync(request, ct);
    }
}
