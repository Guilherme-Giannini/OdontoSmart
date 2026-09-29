using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OdontoSmart.Domain.Entities;

namespace OdontoSmart.Infraestructure.Data.Configurations;

public class OrcamentoConfiguration : IEntityTypeConfiguration<Orcamento>
{
    public const string ChaveEstrangeiraPaciente = "FK_Orcamentos_Pacientes_PacienteId";

    public void Configure(EntityTypeBuilder<Orcamento> builder)
    {
        builder.ToTable("Orcamentos", tabela =>
            tabela.HasCheckConstraint("CK_Orcamentos_ValorTotal", "\"ValorTotal\" >= 0"));

        builder.HasKey(o => o.Id);

        // O Id é gerado pela aplicação (RN001).
        builder.Property(o => o.Id)
            .ValueGeneratedNever();

        builder.Property(o => o.DataCadastro)
            .IsRequired();

        builder.Property(o => o.Validade)
            .HasColumnType("date");

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(o => o.Observacoes)
            .HasMaxLength(Orcamento.ObservacoesTamanhoMaximo);

        builder.Property(o => o.ValorTotal)
            .HasPrecision(18, 2);

        // Um paciente com orçamentos não pode ser excluído.
        builder.HasOne<Paciente>()
            .WithMany()
            .HasForeignKey(o => o.PacienteId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName(ChaveEstrangeiraPaciente);

        builder.HasMany(o => o.Itens)
            .WithOne()
            .HasForeignKey(i => i.OrcamentoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.Itens)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(o => o.PacienteId);
        builder.HasIndex(o => o.DataCadastro);
        builder.HasIndex(o => o.Status);
    }
}
