using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OdontoSmart.Domain.Enums;
using OdontoSmart.Infraestructure.Identity;

namespace OdontoSmart.Infraestructure.Data.Configurations;

public class PerfilConfiguration : IEntityTypeConfiguration<PerfilIdentity>
{
    public void Configure(EntityTypeBuilder<PerfilIdentity> builder)
    {
        builder.ToTable("Perfis");

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        // Os perfis são dados de referência inseridos pela migration. Ids e stamps fixos mantêm o
        // modelo estável entre migrations. Nenhum usuário é criado aqui (RN002).
        builder.HasData(
            Perfil(PerfilUsuario.Administrador, "7a0f3a52-5d0e-4c1a-9c11-000000000001"),
            Perfil(PerfilUsuario.Dentista, "7a0f3a52-5d0e-4c1a-9c11-000000000002"),
            Perfil(PerfilUsuario.Recepcao, "7a0f3a52-5d0e-4c1a-9c11-000000000003"));
    }

    private static PerfilIdentity Perfil(PerfilUsuario perfil, string id) => new()
    {
        Id = Guid.Parse(id),
        Name = perfil.ToString(),
        NormalizedName = perfil.ToString().ToUpperInvariant(),
        ConcurrencyStamp = id
    };
}
