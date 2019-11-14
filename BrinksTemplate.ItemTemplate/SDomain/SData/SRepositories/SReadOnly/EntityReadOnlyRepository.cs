using $DomainReadOnlyRepositoriesInterfaceNamespace$;
using $DomainEntitiesNamespace$;
using $InfrastructureDataAccessEFNamespace$;
using $InfrastructureDataAccessEFInterfacesNamespace$;

namespace $DomainReadOnlyRepositoriesNamespace$
{
    /// <summary>
    /// Implementação do repositório de leitura da entidade $EntityName$. 
    /// </summary>
    public class $EntityName$ReadOnlyRepository : ReadOnlyRepository<$EntityName$>, I$EntityName$ReadOnlyRepository
    {
        public $EntityName$ReadOnlyRepository(IEfReadOnlyDbContext context) : base(context)
        {
        }
    }
}
