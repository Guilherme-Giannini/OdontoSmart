using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.WebEncoders;
using OdontoSmart.Application.Orcamentos;
using OdontoSmart.Application.Pacientes;
using OdontoSmart.Infraestructure;
using OdontoSmart.Infraestructure.Data;

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
});

// Evita que o Razor codifique caracteres acentuados como entidades HTML (ex.: "&#xE1;").
builder.Services.Configure<WebEncoderOptions>(options =>
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

var connectionString = builder.Configuration.GetConnectionString("OdontoSmart")
    ?? throw new InvalidOperationException("A connection string 'OdontoSmart' não foi configurada.");

builder.Services.AddInfraestrutura(connectionString);
builder.Services.AddScoped<IPacienteService, PacienteService>();
builder.Services.AddScoped<IOrcamentoService, OrcamentoService>();

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

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Pacientes}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
