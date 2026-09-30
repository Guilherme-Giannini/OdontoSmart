using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OdontoSmart.Application.Usuarios;
using OdontoSmart.Domain.Common;
using OdontoSmart.Infraestructure.Identity;

namespace OdontoSmart.Infraestructure.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<UsuarioIdentity>
{
    public const string IndiceEmailUnico = "IX_Usuarios_NormalizedEmail";
    public const string IndiceNomeUsuarioUnico = "IX_Usuarios_NormalizedUserName";

    public void Configure(EntityTypeBuilder<UsuarioIdentity> builder)
    {
        builder.ToTable("Usuarios");

        // O Id é gerado pela aplicação (RN001).
        builder.Property(u => u.Id)
            .ValueGeneratedNever();

        builder.Property(u => u.NomeCompleto)
            .IsRequired()
            .HasMaxLength(UsuarioValidador.NomeCompletoTamanhoMaximo);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(Email.TamanhoMaximo);

        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(Email.TamanhoMaximo);

        builder.Property(u => u.Perfil)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(u => u.DataCadastro)
            .IsRequired();

        // O e-mail normalizado (maiúsculas) garante a unicidade sem diferenciar maiúsculas e minúsculas (RN004).
        builder.HasIndex(u => u.NormalizedEmail)
            .IsUnique()
            .HasDatabaseName(IndiceEmailUnico);

        builder.HasIndex(u => u.NormalizedUserName)
            .IsUnique()
            .HasDatabaseName(IndiceNomeUsuarioUnico);

        builder.HasIndex(u => u.NomeCompleto);
    }
}
