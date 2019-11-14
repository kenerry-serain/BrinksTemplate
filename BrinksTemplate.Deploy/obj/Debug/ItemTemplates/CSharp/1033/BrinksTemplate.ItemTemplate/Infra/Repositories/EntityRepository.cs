using $DomainRepositoriesInterfaceNamespace$;
using $DomainEntitiesNamespace$;
using $InfrastructureDataAccessEFNamespace$;
using $InfrastructureDataAccessEFInterfacesNamespace$;

namespace $DomainRepositoriesNamespace$
{
    /// <summary>
    /// Implementação do repositório de escrita da entidade $EntityName$. 
    /// </summary>
    public class $EntityName$Repository : Repository<$EntityName$>, I$EntityName$Repository
    {
        public $EntityName$Repository(IEfDbContext context) : base(context)
        {
        }
    }
}
