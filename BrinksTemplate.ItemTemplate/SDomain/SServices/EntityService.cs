using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using $CoreSharedKernelNamespace$;
using $DomainFiltersNamespace$.$EntityName$;
using $DomainCoreExceptionsNamespace$;
using $DomainCoreServicesNamespace$;
using $DomainRepositoriesInterfaceNamespace$;
using $DomainReadOnlyRepositoriesInterfaceNamespace$;
using $DomainServicesInterfaceNamespace$;
using $DomainCommandsNamespace$.$EntityName$;
using $DomainQueriesNamespace$.$EntityName$;
using $DomainEntitiesNamespace$;
using FluentValidation;

namespace $DomainServicesNamespace$
{
    /// <summary>
    /// Serviços e validações da entidade $EntityName$.
    /// </summary>
    public class $EntityName$Service : Service<$EntityName$>, I$EntityName$Service
    {
        private readonly IMapper _mapper;
        private readonly I$EntityName$Repository _writeRepository;
        private readonly I$EntityName$ReadOnlyRepository _readRepository;
        private readonly IValidator<Register$EntityName$Command> _registerValidator;
        private readonly IValidator<Update$EntityName$Command> _updateValidator;
        private readonly IValidator<Remove$EntityName$Command> _removeValidator;
        public $EntityName$Service
        (
            IMapper mapper,
            I$EntityName$Repository writeRepository,
            I$EntityName$ReadOnlyRepository readRepository,
            IValidator<Register$EntityName$Command> registerValidator,
            IValidator<Update$EntityName$Command> updateValidator,
            IValidator<Remove$EntityName$Command> removeValidator

        ) : base(writeRepository, readRepository)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _writeRepository = writeRepository ?? throw new ArgumentNullException(nameof(writeRepository));
            _readRepository = readRepository ?? throw new ArgumentNullException(nameof(readRepository));
            _registerValidator = registerValidator ?? throw new ArgumentNullException(nameof(registerValidator));
            _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
            _removeValidator = removeValidator ?? throw new ArgumentNullException(nameof(removeValidator));
        }
        
        /// <summary>
        /// Obtem todos os registro da entidade $EntityName$ a partir do filtro.
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public async Task<(IEnumerable<$EntityName$Query> $LowerEntityName$collection, int totalCount)> GetAllAsync(FilterParams<$EntityName$Filter> $LowerEntityName$filter)
        {
            var ($LowerEntityName$Collection, count) =await _readRepository.FindAsync($LowerEntityName$filter, CancellationToken.None).ConfigureAwait(false);
            var $LowerEntityName$QueryCollection = _mapper.Map<IEnumerable<$EntityName$Query>>($LowerEntityName$Collection);
            return ($LowerEntityName$QueryCollection, count);
        }

        /// <summary>
        /// Obtem um registro por Id da entidade $EntityName$.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<$EntityName$Query> GetByIdAsync(int $LowerEntityName$Id)
        {
            var $LowerEntityName$ = await _readRepository.GetByKeyAsync($LowerEntityName$Id).ConfigureAwait(false);
            var $LowerEntityName$Query = _mapper.Map<$EntityName$Query>($LowerEntityName$);
            return $LowerEntityName$Query;
        }

        /// <summary>
        /// Adiciona um registro da entidade $EntityName$.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<$EntityName$Query> RegisterAsync(Register$EntityName$Command command)
        {
            var created$EntityName$ = default($EntityName$);
            var created$EntityName$Query = default($EntityName$Query);
            var $LowerEntityName$ToCreate = _mapper.Map<$EntityName$>(command);
            await InvokeCommandValidation(command).ConfigureAwait(false);
            await _writeRepository.UnitOfWork.ExecuteInTransactionAsync(async cancellationToken =>
            {
                created$EntityName$ = await _writeRepository.AddAsync($LowerEntityName$ToCreate, cancellationToken).ConfigureAwait(false);
                await Commit().ConfigureAwait(false);
            }).ConfigureAwait(false);

            if (created$EntityName$ != default($EntityName$))
            {
                created$EntityName$Query = _mapper.Map<$EntityName$Query>(created$EntityName$);
                return created$EntityName$Query;
            }

            return created$EntityName$Query;
        }

        /// <summary>
        /// Atualiza um registro da entidade $EntityName$.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<$EntityName$Query> UpdateAsync(Update$EntityName$Command command)
        {
            var $LowerEntityName$Exists = await Exists(command.Id).ConfigureAwait(false);
            if (!$LowerEntityName$Exists)
                return default;

            var updated$EntityName$ = default($EntityName$);
            var updated$EntityName$Query = default($EntityName$Query);
            var $LowerEntityName$DataToUpdate = _mapper.Map<$EntityName$>(command);
            await InvokeCommandValidation(command).ConfigureAwait(false);
            await _writeRepository.UnitOfWork.ExecuteInTransactionAsync(async cancellationToken =>
            {
                await Task.Run(() => _writeRepository.Update($LowerEntityName$DataToUpdate)).ConfigureAwait(false);
                await Commit(cancellationToken).ConfigureAwait(false); 
                updated$EntityName$ = $LowerEntityName$DataToUpdate;
            }).ConfigureAwait(false);

            if (updated$EntityName$ != default($EntityName$))
            {
                updated$EntityName$Query = _mapper.Map<$EntityName$Query>(updated$EntityName$);
                return updated$EntityName$Query;
            }

            return updated$EntityName$Query;
        }

        /// <summary>
        /// Deleta um registro da entidade $EntityName$.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<int> RemoveAsync(Remove$EntityName$Command command)
        {
            var $LowerEntityName$Exists = await Exists(command.Id).ConfigureAwait(false);
            if (!$LowerEntityName$Exists)
                return 0;

            await InvokeCommandValidation(command).ConfigureAwait(false);
            await _writeRepository.UnitOfWork.ExecuteInTransactionAsync(async cancellationToken =>
            {
                var $LowerEntityName$ToRemove = await _readOnlyRepository.GetByKeyAsync(command.Id).ConfigureAwait(false);
                $LowerEntityName$ToRemove.Delete();
                await Task.Run(() => _writeRepository.Update($LowerEntityName$ToRemove)).ConfigureAwait(false);
                await Commit(cancellationToken).ConfigureAwait(false);
            }).ConfigureAwait(false);

            return 1;
        }

        /// <summary>
        /// Verifica se um registro da entidade $EntityName$ existe.
        /// </summary>
        /// <param name="$LowerEntityName$Id"></param>
        /// <returns></returns>
        private async Task<bool> Exists(int $LowerEntityName$Id)
        {
            return (await _readOnlyRepository.GetByKeyAsync($LowerEntityName$Id).ConfigureAwait(false)) != null;
        }

        /// <summary>
        /// Committa uma transação pendente na base de dados
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task Commit(CancellationToken cancellationToken = default)
        {
            var sqlCommandResult = await _writeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            if (sqlCommandResult < 0)
                throw new DomainValidationException("Invalid database operation.");
        }

        /// <summary>
        /// Invoca a validator de um comando de acordo com seu tipo
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private async Task InvokeCommandValidation(object command)
        {
            if (command.GetType() == typeof(Register$EntityName$Command))
            {
                var validationResult = await _registerValidator.ValidateAsync(command, CancellationToken.None).ConfigureAwait(false);
                if (!validationResult.IsValid)
                    throw new DomainValidationException(validationResult.ToString("<br />"));
            }

            else if (command.GetType() == typeof(Update$EntityName$Command))
            {
                var validationResult = await _updateValidator.ValidateAsync(command, CancellationToken.None).ConfigureAwait(false);
                if (!validationResult.IsValid)
                    throw new DomainValidationException(validationResult.ToString("<br />"));
            }

            else if (command.GetType() == typeof(Remove$EntityName$Command))
            {
                var validationResult = await _removeValidator.ValidateAsync(command, CancellationToken.None).ConfigureAwait(false);
                if (!validationResult.IsValid)
                    throw new DomainValidationException(validationResult.ToString("<br />"));
            }

            else throw new ArgumentException($"Unknow command type: {command.GetType()}");
        }
    }
}
