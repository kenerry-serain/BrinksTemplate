using $DomainCommandsNamespace$.$EntityName$;
using $InfrastructureLocalizationNamespace$;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace $DomainCommandValidationNamespace$.$EntityName$
{
    /// <summary>
    /// Classe para implementação de validações e regras de negócio do comando Register$EntityName$CommandValidator
    /// </summary>
    public class Register$EntityName$CommandValidator : AbstractValidator<Register$EntityName$Command>
    {
        public Register$EntityName$CommandValidator
        (
            IStringLocalizer<Resource> localizerResource
        )
        {
        }
    }
}
