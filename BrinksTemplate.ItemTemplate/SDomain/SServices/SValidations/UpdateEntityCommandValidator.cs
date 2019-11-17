using $DomainCommandsNamespace$.$EntityName$;
using $InfrastructureLocalizationNamespace$;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace $DomainCommandValidationNamespace$.$EntityName$
{
    /// <summary>
    /// Classe para implementação de validações e regras de negócio do comando Update$EntityName$CommandValidator
    /// </summary>
    public class Update$EntityName$CommandValidator : AbstractValidator<Update$EntityName$Command>
    {
        public Update$EntityName$CommandValidator
        (
            IStringLocalizer<Resource> localizerResource
        )
        {
        }
    }
}
