using $DomainCommandsNamespace$.$EntityName$;
using $InfrastructureLocalizationNamespace$;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace $DomainCommandValidationNamespace$.$EntityName$
{
    /// <summary>
    /// Classe para implementação de validações e regras de negócio do comando Remove$EntityName$CommandValidator
    /// </summary>
    public class Remove$EntityName$CommandValidator : AbstractValidator<Remove$EntityName$Command>
    {
        public Remove$EntityName$CommandValidator
         (
             IStringLocalizer<Resource> localizerResource
         )
        {
        }
    }
}
