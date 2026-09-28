using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OdontoSmart.Domain.Common;
using OdontoSmart.Domain.Entities;

namespace OdontoSmart.Infraestructure.Data.Configurations;

public class PacienteConfiguration : IEntityTypeConfiguration<Paciente>
{
    public const string IndiceCpfUnico = "IX_Pacientes_Cpf";

    public void Configure(EntityTypeBuilder<Paciente> builder)
    {
        builder.ToTable("Pacientes");

        builder.HasKey(p => p.Id);

        // O Id é gerado pela aplicação (RN001).
        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.NomeCompleto)
            .IsRequired()
            .HasMaxLength(Paciente.NomeCompletoTamanhoMaximo);

        builder.Property(p => p.Cpf)
            .HasMaxLength(Cpf.Tamanho);

        builder.Property(p => p.DataNascimento)
            .HasColumnType("date");

        builder.Property(p => p.Sexo)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.Telefone)
            .HasMaxLength(Paciente.TelefoneTamanhoMaximo);

        builder.Property(p => p.Email)
            .HasMaxLength(Email.TamanhoMaximo);

        builder.Property(p => p.Observacoes)
            .HasMaxLength(Paciente.ObservacoesTamanhoMaximo);

        builder.Property(p => p.DataCadastro)
            .IsRequired();

        // CPF nulo não conflita: no PostgreSQL, valores NULL não violam índices únicos.
        builder.HasIndex(p => p.Cpf)
            .IsUnique()
            .HasDatabaseName(IndiceCpfUnico);

        builder.HasIndex(p => p.NomeCompleto);
    }
}
