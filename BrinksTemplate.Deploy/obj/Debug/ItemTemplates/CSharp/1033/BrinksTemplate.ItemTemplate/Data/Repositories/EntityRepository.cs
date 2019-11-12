using $DomainRepositoriesInterfaceNamespace$;
using $DomainEntitiesNamespace$;
using $InfrastructureDataAccessEFNamespace$;
using $InfrastructureDataAccessEFInterfacesNamespace$;

namespace $DomainRepositoriesNamespace$
{
    public class $EntityName$Repository : Repository<$EntityName$>, I$EntityName$Repository
    {
        public $EntityName$Repository(IEfDbContext context) : base(context)
        {
        }
    }
}
