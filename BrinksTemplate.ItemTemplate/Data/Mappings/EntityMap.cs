using $DomainEntitiesNamespace$;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace $DomainDataMappingNamespace$
{
    public class EntityMap : IEntityTypeConfiguration<$EntityName$>
    {
        public void Configure(EntityTypeBuilder<$EntityName$> builder)
        {

        }
    }
}
