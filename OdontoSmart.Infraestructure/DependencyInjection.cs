using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OdontoSmart.Application.Common;
using OdontoSmart.Application.Orcamentos;
using OdontoSmart.Application.Pacientes;
using OdontoSmart.Application.Profissionais;
using OdontoSmart.Application.Usuarios;
using OdontoSmart.Infraestructure.Data;
using OdontoSmart.Infraestructure.Identity;
using OdontoSmart.Infraestructure.Orcamentos;
using OdontoSmart.Infraestructure.Pacientes;
using OdontoSmart.Infraestructure.Profissionais;

namespace OdontoSmart.Infraestructure;

public static class DependencyInjection
{
    /// <summary>Intervalo máximo para que uma alteração de acesso derrube as sessões abertas (RN008).</summary>
    public static readonly TimeSpan IntervaloValidacaoSessao = TimeSpan.FromMinutes(1);

    public static IServiceCollection AddInfraestrutura(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IPacienteRepository, PacienteRepository>();
        services.AddScoped<IOrcamentoRepository, OrcamentoRepository>();
        services.AddScoped<IProfissionalRepository, ProfissionalRepository>();
        services.AddScoped<ITransacoes, Transacoes>();

        AddIdentidade(services);

        return services;
    }

    private static void AddIdentidade(IServiceCollection services)
    {
        services
            .AddIdentity<UsuarioIdentity, PerfilIdentity>(options =>
            {
                // RN005 — a regra completa (letra + número, até 128) é validada na Application.
                options.Password.RequiredLength = SenhaValidador.TamanhoMinimo;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredUniqueChars = 1;

                // RN007 — 5 tentativas incorretas bloqueiam por 15 minutos.
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;

                // O e-mail é o login: único e com qualquer caractere válido em e-mails.
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = string.Empty;

                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddClaimsPrincipalFactory<UsuarioClaimsPrincipalFactory>()
            .AddDefaultTokenProviders();

        // Desativação, mudança de perfil e redefinição de senha trocam o security stamp;
        // as sessões são revalidadas a cada minuto (RN008).
        services.Configure<SecurityStampValidatorOptions>(options =>
            options.ValidationInterval = IntervaloValidacaoSessao);

        services.AddScoped<IGestaoUsuarios, GestaoUsuarios>();
    }
}
