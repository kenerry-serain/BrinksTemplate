using $DomainEntitiesNamespace$;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace $DomainDataMappingNamespace$
{
    /// <summary>
    /// Classe para mapear nomes de propriedades da aplicação para o banco de dados
    /// </summary>
    public class $EntityName$Map : IEntityTypeConfiguration<$EntityName$>
    {
        public void Configure(EntityTypeBuilder<$EntityName$> builder)
        {
            builder.ToTable("$LowerEntityName$");

            builder
                .Property(p => p.Id)
                .HasColumnName("id");

            builder
                .Property(p => p.CreationDate)
                .HasColumnName("creation_date");
        }
    }
}
