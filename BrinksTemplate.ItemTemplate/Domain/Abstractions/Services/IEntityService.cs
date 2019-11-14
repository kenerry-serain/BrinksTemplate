using $DomainCoreInterfacesNamespace$;
using $DomainCommandsNamespace$.$EntityName$;
using $DomainQueriesNamespace$.$EntityName$;
using $DomainEntitiesNamespace$;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace $DomainServicesInterfaceNamespace$
{
    /// <summary>
    /// Interface do serviço da entidade $EntityName$. 
    /// </summary>
    public interface I$EntityName$Service : IService<$EntityName$>
    {
        Task<IEnumerable<$EntityName$Query>> GetAllAsync();
        Task<$EntityName$Query> GetByIdAsync(int $LowerEntityName$Id);
        Task<$EntityName$Query> RegisterAsync(Register$EntityName$Command command);
        Task<$EntityName$Query> UpdateAsync(Update$EntityName$Command command);
        Task<int> RemoveAsync(Remove$EntityName$Command command);
    }
}
