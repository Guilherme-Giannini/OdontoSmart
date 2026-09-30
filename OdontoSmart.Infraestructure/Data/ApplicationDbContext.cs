using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OdontoSmart.Domain.Entities;
using OdontoSmart.Infraestructure.Identity;

namespace OdontoSmart.Infraestructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<UsuarioIdentity, PerfilIdentity, Guid>(options)
{
    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<Orcamento> Orcamentos => Set<Orcamento>();
    public DbSet<OrcamentoItem> OrcamentoItens => Set<OrcamentoItem>();
    public DbSet<Profissional> Profissionais => Set<Profissional>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Mapeamento padrão do Identity; os nomes das tabelas são ajustados abaixo e nas configurações.
        base.OnModelCreating(modelBuilder);

        // Tabelas auxiliares do Identity em português e sem o prefixo "AspNet".
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UsuarioPerfis");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UsuarioClaims");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UsuarioLogins");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UsuarioTokens");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("PerfilClaims");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
