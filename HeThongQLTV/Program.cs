using HeThongQLTV.Data;
using HeThongQLTV.Models;
using HeThongQLTV.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
var builder = WebApplication.CreateBuilder(args);
// Local development secret; environment variables take precedence.
var localEnv = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", ".env.local"));
if (builder.Environment.IsDevelopment() && File.Exists(localEnv) && string.IsNullOrWhiteSpace(builder.Configuration["OPENAI_API_KEY"]))
{
    var line = File.ReadLines(localEnv).FirstOrDefault(l => l.StartsWith("OPENAI_API_KEY="));
    if (line != null) builder.Configuration["OPENAI_API_KEY"] = line[(line.IndexOf('=') + 1)..].Trim().Trim('"', '\'');
}
builder.Services.AddHttpClient("OpenAI", c => { c.BaseAddress = new Uri("https://api.openai.com/v1/"); c.Timeout = TimeSpan.FromSeconds(45); });
builder.Services.AddRateLimiter(o => { o.RejectionStatusCode = 429; o.AddPolicy("chat", context => System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous", _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 })); });
builder.Services.AddControllersWithViews(o => { o.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true; o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()); });
builder.Services.AddDbContext<LibraryDb>(o => o.UseSqlite(builder.Configuration.GetConnectionString("Library") ?? "Data Source=database/library.db"));
builder.Services.AddOptions<LibraryRules>().Bind(builder.Configuration.GetSection("LibraryRules")).Validate(r => r.MaxLoans > 0 && r.LoanDays > 0 && r.LoanDays <= 365 && r.RenewalDays > 0 && r.RenewalDays <= 365 && r.MaxRenewals >= 0 && r.FinePerDay >= 0, "Quy định thư viện không hợp lệ.").ValidateOnStart();
builder.Services.AddScoped<CirculationService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o =>
{
    o.LoginPath = "/Account/Login"; o.AccessDeniedPath = "/Account/Denied"; o.ExpireTimeSpan = TimeSpan.FromHours(8); o.Cookie.HttpOnly = true; o.Cookie.SameSite = SameSiteMode.Lax;
    o.Events.OnValidatePrincipal = async context =>
    {
        var db = context.HttpContext.RequestServices.GetRequiredService<LibraryDb>();
        var id = int.TryParse(context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var n) ? n : 0;
        var user = await db.Users.FindAsync(id);
        if (user is null || !user.Active || user.Role != context.Principal?.FindFirstValue(ClaimTypes.Role)) context.RejectPrincipal();
    };
});
builder.Services.AddAuthorization();
var app = builder.Build();
Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "database"));
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LibraryDb>();
    if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("SeedDemo")) db.Seed(); else db.Database.EnsureCreated();
}
app.UseExceptionHandler("/Home/Error");
app.UseStatusCodePagesWithReExecute("/Home/Status", "?code={0}");
app.UseStaticFiles(); app.UseRouting(); app.UseAuthentication(); app.UseAuthorization();
app.UseRateLimiter();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();
