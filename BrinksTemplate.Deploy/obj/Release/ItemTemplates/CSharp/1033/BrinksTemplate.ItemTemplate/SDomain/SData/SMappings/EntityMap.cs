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

        }
    }
}
