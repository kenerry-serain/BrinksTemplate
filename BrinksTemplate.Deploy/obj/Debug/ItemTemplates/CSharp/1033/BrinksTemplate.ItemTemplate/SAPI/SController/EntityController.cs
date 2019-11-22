using $DomainServicesInterfaceNamespace$;
using $DomainCommandsNamespace$.$EntityName$;
using $DomainQueriesNamespace$.$EntityName$;
using $DomainFiltersNamespace$.$EntityName$;
using $CoreSharedKernelNamespace$;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace $APIControllersNamespace$
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class $EntityName$Controller : ControllerBase
    {
        private readonly I$EntityName$Service _$LowerEntityName$Service;
        public $EntityName$Controller
        (
            I$EntityName$Service $LowerEntityName$Service
        )
        {
            _$LowerEntityName$Service = $LowerEntityName$Service ?? throw new ArgumentNullException(nameof(_$LowerEntityName$Service));
        }

        /// <summary>
        /// Seleciona uma lista de $EntityName$ a partir do filtro
        /// </summary>
        /// <param name="filterParams"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] FilterParams<$EntityName$Filter> $LowerEntityName$Filter)
        {
            var ($LowerEntityName$QueryCollection, totalCount) = await _$LowerEntityName$Service.GetAllAsync($LowerEntityName$Filter).ConfigureAwait(false);
            if (totalCount > 0)
                return Ok(new { list= $LowerEntityName$QueryCollection, totalCount });

            return NoContent();
        }

        /// <summary>
        /// Seleciona um(a) $EntityName$.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetByIdAsync([FromRoute]int id)
        {
            var $LowerEntityName$Query = await _$LowerEntityName$Service.GetByIdAsync(id).ConfigureAwait(false);
            if ($LowerEntityName$Query == default($EntityName$Query))
                return NotFound();
            return Ok($LowerEntityName$Query);
        }

        /// <summary>
        /// Insere um(a) $EntityName$.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> RegisterAsync([FromBody] Register$EntityName$Command command)
        {
            var created$EntityName$Query = await _$LowerEntityName$Service.RegisterAsync(command).ConfigureAwait(false);
            return Created($"/{created$EntityName$Query.Id}", created$EntityName$Query);
        }

        /// <summary>
        /// Atualiza um(a) $EntityName$.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] Update$EntityName$Command command)
        {
            var updated$EntityName$Query = await _$LowerEntityName$Service.UpdateAsync(command).ConfigureAwait(false);
            if (updated$EntityName$Query == default($EntityName$Query))
                return NotFound();
            return Accepted(updated$EntityName$Query);
        }

        /// <summary>
        /// Deleta um(a) $EntityName$.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> RemoveAsync([FromRoute] int id)
        {
            var commandResult = await _$LowerEntityName$Service.RemoveAsync(new Remove$EntityName$Command(id)).ConfigureAwait(false);
            if (commandResult <= 0)
                return NotFound();
            return Ok();
        }
    }
}
