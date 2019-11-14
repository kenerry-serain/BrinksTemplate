using $DomainEntitiesNamespace$;
using $InfrastructureDataAccessInterfacesNamespace$;

namespace $DomainReadOnlyRepositoriesInterfaceNamespace$
{
    /// <summary>
    /// Interface do repositório de leitura da entidade $EntityName$. Assinatura de métodos específicos p/ entidade $EntityName$
    /// </summary>
    public interface I$EntityName$ReadOnlyRepository : IReadOnlyRepository<$EntityName$>
    {
    }
}
