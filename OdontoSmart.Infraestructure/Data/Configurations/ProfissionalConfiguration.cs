using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OdontoSmart.Domain.Common;
using OdontoSmart.Domain.Entities;
using OdontoSmart.Infraestructure.Identity;

namespace OdontoSmart.Infraestructure.Data.Configurations;

public class ProfissionalConfiguration : IEntityTypeConfiguration<Profissional>
{
    public const string IndiceCroUnico = "IX_Profissionais_CroUf_Cro";
    public const string IndiceUsuarioUnico = "IX_Profissionais_UsuarioId";

    public void Configure(EntityTypeBuilder<Profissional> builder)
    {
        builder.ToTable("Profissionais");

        builder.HasKey(p => p.Id);

        // O Id é gerado pela aplicação (RN001).
        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.NomeExibicao)
            .IsRequired()
            .HasMaxLength(Profissional.NomeExibicaoTamanhoMaximo);

        builder.Property(p => p.Cro)
            .IsRequired()
            .HasMaxLength(Profissional.CroTamanhoMaximo);

        builder.Property(p => p.CroUf)
            .IsRequired()
            .HasMaxLength(UnidadeFederativa.Tamanho);

        builder.Property(p => p.Especialidade)
            .HasMaxLength(Profissional.EspecialidadeTamanhoMaximo);

        builder.Property(p => p.Telefone)
            .HasMaxLength(Profissional.TelefoneTamanhoMaximo);

        builder.Property(p => p.Email)
            .HasMaxLength(Email.TamanhoMaximo);

        builder.Property(p => p.DataCadastro)
            .IsRequired();

        // Usuários não são excluídos; a restrição impede exclusões acidentais.
        builder.HasOne<UsuarioIdentity>()
            .WithMany()
            .HasForeignKey(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.CroUf, p.Cro })
            .IsUnique()
            .HasDatabaseName(IndiceCroUnico);

        // Um usuário vinculado a no máximo um profissional; NULL não conflita no PostgreSQL (RN017).
        builder.HasIndex(p => p.UsuarioId)
            .IsUnique()
            .HasDatabaseName(IndiceUsuarioUnico);

        builder.HasIndex(p => p.NomeExibicao);
    }
}
