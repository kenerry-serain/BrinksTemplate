using $DomainReadOnlyRepositoriesInterfaceNamespace$;
using $DomainEntitiesNamespace$;
using $InfrastructureDataAccessEFNamespace$;
using $InfrastructureDataAccessEFInterfacesNamespace$;

namespace $DomainReadOnlyRepositoriesNamespace$
{
    public class $EntityName$ReadOnlyRepository : ReadOnlyRepository<$EntityName$>, I$EntityName$ReadOnlyRepository
    {
        public $EntityName$ReadOnlyRepository(IEfReadOnlyDbContext context) : base(context)
        {
        }
    }
}
