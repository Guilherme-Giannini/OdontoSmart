using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OdontoSmart.Domain.Entities;

namespace OdontoSmart.Infraestructure.Data.Configurations;

public class OrcamentoItemConfiguration : IEntityTypeConfiguration<OrcamentoItem>
{
    public void Configure(EntityTypeBuilder<OrcamentoItem> builder)
    {
        builder.ToTable("OrcamentoItens", tabela =>
        {
            tabela.HasCheckConstraint("CK_OrcamentoItens_Quantidade", "\"Quantidade\" > 0");
            tabela.HasCheckConstraint("CK_OrcamentoItens_ValorUnitario", "\"ValorUnitario\" >= 0");
            tabela.HasCheckConstraint("CK_OrcamentoItens_ValorTotal", "\"ValorTotal\" >= 0");
        });

        builder.HasKey(i => i.Id);

        // Gerado pela aplicação: itens novos descobertos pelo EF são inseridos, não atualizados.
        builder.Property(i => i.Id)
            .ValueGeneratedNever();

        builder.Property(i => i.Ordem)
            .IsRequired();

        builder.Property(i => i.Descricao)
            .IsRequired()
            .HasMaxLength(OrcamentoItem.DescricaoTamanhoMaximo);

        builder.Property(i => i.Quantidade)
            .HasPrecision(12, 2);

        builder.Property(i => i.ValorUnitario)
            .HasPrecision(18, 2);

        builder.Property(i => i.ValorTotal)
            .HasPrecision(18, 2);

        builder.Property(i => i.Observacoes)
            .HasMaxLength(OrcamentoItem.ObservacoesTamanhoMaximo);
    }
}
