using System.Globalization;
using ComandaFacil.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Banco de dados (SQLite). Para PostgreSQL, troque por UseNpgsql (ver README).
builder.Services.AddDbContext<AppDbContext>(opcoes =>
    opcoes.UseSqlite(builder.Configuration.GetConnectionString("Padrao")));

// Login por cookie
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opcoes =>
    {
        opcoes.LoginPath = "/Conta/Login";
        opcoes.AccessDeniedPath = "/Conta/AcessoNegado";
        opcoes.ExpireTimeSpan = TimeSpan.FromHours(12);
        opcoes.SlidingExpiration = true;
    });

// Todas as telas exigem login, exceto as marcadas com [AllowAnonymous]
builder.Services.AddControllersWithViews(opcoes =>
{
    var politica = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    opcoes.Filters.Add(new AuthorizeFilter(politica));
});

var app = builder.Build();

// Cria/atualiza o banco e insere os dados iniciais
using (var escopo = app.Services.CreateScope())
{
    var db = escopo.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.GetMigrations().Any())
        db.Database.Migrate();
    else
        db.Database.EnsureCreated();

    SeedData.Popular(db);
}

// Valores, datas e moeda no padrão brasileiro (R$ 12,50)
var cultura = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = cultura;
CultureInfo.DefaultThreadCurrentUICulture = cultura;
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultura),
    SupportedCultures = new[] { cultura },
    SupportedUICultures = new[] { cultura }
});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Mesas}/{action=Index}/{id?}");

app.Run();
