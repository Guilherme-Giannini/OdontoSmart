using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.WebEncoders;
using OdontoSmart.Application.Orcamentos;
using OdontoSmart.Application.Pacientes;
using OdontoSmart.Application.PrimeiroAcesso;
using OdontoSmart.Application.Profissionais;
using OdontoSmart.Application.Usuarios;
using OdontoSmart.Infraestructure;
using OdontoSmart.Infraestructure.Data;
using OdontoSmart.Web.Autorizacao;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews(options =>
{
    var mensagens = options.ModelBindingMessageProvider;
    mensagens.SetAttemptedValueIsInvalidAccessor((valor, campo) => $"O valor '{valor}' não é válido para {campo}.");
    mensagens.SetValueIsInvalidAccessor(valor => $"O valor '{valor}' é inválido.");
    mensagens.SetValueMustBeANumberAccessor(campo => $"O campo {campo} deve ser um número.");
    mensagens.SetValueMustNotBeNullAccessor(valor => $"O valor '{valor}' é inválido.");
    mensagens.SetMissingBindRequiredValueAccessor(campo => $"O campo {campo} é obrigatório.");

    options.Filters.Add<TrocaSenhaObrigatoriaFilter>();
});

// Evita que o Razor codifique caracteres acentuados como entidades HTML (ex.: "&#xE1;").
builder.Services.Configure<WebEncoderOptions>(options =>
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

// A connection string não fica no appsettings.json versionado: use appsettings.Development.json,
// user-secrets ou a variável de ambiente ConnectionStrings__OdontoSmart.
var connectionString = builder.Configuration.GetConnectionString("OdontoSmart");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("A connection string 'OdontoSmart' não foi configurada.");

builder.Services.AddInfraestrutura(connectionString);

// Sessão por cookie (RN008): expira após 8 horas sem atividade, sem opção "manter conectado".
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Conta/Login";
    options.LogoutPath = "/Conta/Sair";
    options.AccessDeniedPath = "/Conta/AcessoNegado";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Cookie.Name = "OdontoSmart.Sessao";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddAuthorization(Politicas.Registrar);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUsuarioAtual, UsuarioAtual>();

builder.Services.AddScoped<IPacienteService, PacienteService>();
builder.Services.AddScoped<IOrcamentoService, OrcamentoService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IProfissionalService, ProfissionalService>();
builder.Services.AddScoped<IPrimeiroAcessoService, PrimeiroAcessoService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

// Números e datas trafegam em formato invariante ("1234.56", "2026-09-28"), que é o formato
// enviado por <input type="number"> e <input type="date">. A apresentação em pt-BR (R$ 1.234,56)
// é feita explicitamente pelas classes de formatação das ViewModels.
var culturaInvariante = CultureInfo.InvariantCulture;
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culturaInvariante),
    SupportedCultures = [culturaInvariante],
    SupportedUICultures = [culturaInvariante],
    RequestCultureProviders = []
});

if (app.Environment.IsDevelopment())
{
    // Em desenvolvimento, cria/atualiza o banco automaticamente a partir das migrations.
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseMiddleware<PrimeiroAcessoMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Arquivos estáticos (CSS/JS) são públicos: a tela de login também os utiliza.
app.MapStaticAssets().AllowAnonymous();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Pacientes}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
