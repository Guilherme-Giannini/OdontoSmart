using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OdontoSmart.Application.Pacientes;
using OdontoSmart.Infraestructure.Data;
using OdontoSmart.Infraestructure.Pacientes;

namespace OdontoSmart.Infraestructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfraestrutura(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IPacienteRepository, PacienteRepository>();

        return services;
    }
}
