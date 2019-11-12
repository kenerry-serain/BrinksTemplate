using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TemplateWizard;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace BrinksTemplate.Wizard
{
    public class WizardTemplate : IWizard
    {
        private readonly DTE _dte;
        private ProjectItem _pastaTemp;
        private IEnumerable<Project> _solutionProjectCollection;
        private string _entityName
        , _contextName
        , _webApiProject
        , _domainProject
        , _solutionName
        , _partialSolutionDirectory
        , _solutionDirectory;

        public string _databaseEntity;
        public WizardTemplate()
        {
            _dte = (DTE)Package.GetGlobalService(typeof(DTE));
            SetSolutionName();
        }

        public void BeforeOpeningFile(ProjectItem projectItem) { }
        public void ProjectFinishedGenerating(Project project) { }

        /// <summary>
        /// Este metodo coloca cada classe dentro do seu projeto e dentro da sua pasta(a).
        /// </summary>
        /// <param name="projectItem"></param>
        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
            var item = Path.GetFileNameWithoutExtension(projectItem.Name);
            var folder = string.Empty;
            var project = default(Project);

            /* Domain entity */
            if (item == _entityName)
            {
                project = GetProjectByName(_domainProject);
                folder = "Entities";
            }

            /* Domain repository interface */
            else if (item == $"I{_entityName}Repository")
            {
                project = GetProjectByName(_domainProject);
                folder = "Abstractions.Repositories";
            }

            /* Domain read only repository interface */
            else if (item == $"I{_entityName}ReadOnlyRepository")
            {
                project = GetProjectByName(_domainProject);
                folder = "Abstractions.Repositories.ReadOnly";
            }

            /* Domain service interface*/
            else if (item == $"I{_entityName}Service")
            {
                project = GetProjectByName(_domainProject);
                folder = "Abstractions.Services";
            }

            /* Domain commands */
            else if (item == $"Register{_entityName}Command" || item == $"Update{_entityName}Command" || item == $"Remove{_entityName}Command")
            {
                project = GetProjectByName(_domainProject);
                folder = "DTOs.Commands";
            }

            /* Domain query */
            else if (item == $"{_entityName}Query")
            {
                project = GetProjectByName(_domainProject);
                folder = "DTOs.Queries";
            }

            /* Domain commands validation */
            else if (item == $"Register{_entityName}CommandValidation" || item == $"Update{_entityName}CommandValidation" || item == $"Remove{_entityName}CommandValidation")
            {
                project = GetProjectByName(_domainProject);
                folder = "Services";
            }

            /* Domain service */
            else if (item == $"{_entityName}Service")
            {
                project = GetProjectByName(_domainProject);
                folder = "Services";
            }

            /* Domain repository */
            else if (item == $"{_entityName}Repository")
            {
                project = GetProjectByName(_domainProject);
                folder = "Data.Repositories";
            }

            /* Domain read only repository */
            else if (item == $"{_entityName}ReadOnlyRepository")
            {
                project = GetProjectByName(_domainProject);
                folder = "Data.Repositories";
            }

            /* Domain map */
            else if (item == $"{_entityName}Map")
            {
                project = GetProjectByName(_domainProject);
                folder = "Data.Mappings";
            }

            /* Entity controller */
            else if (item == $"{_entityName}Controller")
            {
                project = GetProjectByName(_webApiProject);
                folder = "API.Controllers";
            }
            
            project.AddItemInFolder(projectItem, folder);
            _pastaTemp = (ProjectItem)projectItem.Collection.Parent;
        }

        /// <summary>
        /// Metodo acionado antes dos demais. Inicializamos variaveis aqui.
        /// </summary>
        /// <param name="automationObject"></param>
        /// <param name="replacementsDictionary"></param>
        /// <param name="runKind"></param>
        /// <param name="customParams"></param>
        public void RunStarted(object automationObject,
                                Dictionary<string, string> replacementsDictionary,
                                WizardRunKind runKind,
                                object[] customParams)
        {
            _solutionProjectCollection = _dte.Solution.GetAllSolutionProjects();

            var optionsForm = new OptionsForm(_solutionProjectCollection.OrderBy(project => project.Name).Select(project => project.Name));
            optionsForm.ShowDialog();

            _webApiProject = optionsForm.WebApiProject;
            _domainProject = optionsForm.DomainProject;

            _entityName = replacementsDictionary["$safeitemname$"];
            _entityName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_entityName);

            //_contextName = GetContextName();
            SetParameters(replacementsDictionary);
        }

        /// <summary>
        /// Depois que tudo estiver pronto. Apaga a pasta temp criada pelo visual studio e deixa somente as pastas criadas personalizadamente.
        /// Insere toda injeção de depêndencia. Faz o Mapeamento. Configura o DbSet.
        /// </summary>
        public void RunFinished()
        {
            _pastaTemp?.Remove();
            _pastaTemp?.Delete();

            //ApplicationIoCConfig();
            //RepositoryIoCConfig();
            //AutoMapperConfig();
            //ContextConfig();
            //CreateEntity();
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return !OptionsForm.IsCanceled;
        }


        ///// <summary>
        ///// Seleciona o nome do contexto padrão da solução.
        ///// </summary>
        ///// <returns></returns>
        //private string GetContextName()
        //{
        //    var contextName = "Contexto";

        //    var contextDirectory = string.Concat(_solutionDirectory, "\\", _repositoryProject, "\\Contexto");
        //    var filesContextDirectory = Directory.GetFiles(contextDirectory);

        //    if (filesContextDirectory.Any())
        //    {
        //        var contextExists = filesContextDirectory
        //                            ?.Where(c => c.EndsWith(string.Concat("Finamax", contextName, ".cs")))
        //                            ?.ToList()
        //                            ?.FirstOrDefault();

        //        return contextExists != null ? string.Concat("Finamax", contextName) : string.Empty;
        //    }

        //    return string.Empty;
        //}

        /////// <summary>
        /////// Injeta as dependências referente a aplicação.
        /////// </summary>
        //private void ApplicationIoCConfig()
        //{
        //    var folderName = "\\Contexto";
        //    var applicationName = string.Concat(_entityName, "AppService");
        //    var applicationIoCDirectory = string.Concat(_solutionDirectory, "\\", _iocProject, folderName, "\\ApplicationIoC.cs");

        //    var applicationLines = File.ReadAllLines(applicationIoCDirectory);

        //    using (var writer = new StreamWriter(applicationIoCDirectory))
        //    {
        //        for (int currentLine = 1; currentLine <= applicationLines.Count(); ++currentLine)
        //        {
        //            if (currentLine == applicationLines.Count() - 3)
        //            {
        //                writer.WriteLine(string.Concat(applicationLines[currentLine - 1],
        //                                 ApplicationIoC.Container(_entityName)));
        //            }
        //            else
        //            {
        //                writer.WriteLine(applicationLines[currentLine - 1]);
        //            }
        //        }
        //    }
        //}

        /////// <summary>
        /////// Injeta as dependências referente ao repositório.
        /////// </summary>
        //private void RepositoryIoCConfig()
        //{
        //    var folderName = "\\Contexto";
        //    //var repositoryName = string.Concat(_entityName, "Repositorio");
        //    var repositoryIoCDirectory = string.Concat(_solutionDirectory, "\\", _iocProject, folderName, "\\RepositoryIoC.cs");

        //    var repositoryLines = File.ReadAllLines(repositoryIoCDirectory);

        //    using (var writer = new StreamWriter(repositoryIoCDirectory))
        //    {
        //        for (int currentLine = 1; currentLine <= repositoryLines.Count(); ++currentLine)
        //        {
        //            if (currentLine == repositoryLines.Count() - 3)
        //            {
        //                writer.WriteLine(string.Concat(repositoryLines[currentLine - 1],
        //                                 RepositoryIoC.Container(_entityName)));
        //            }
        //            else
        //            {
        //                writer.WriteLine(repositoryLines[currentLine - 1]);
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// Adiciona a configuração de DbSet.
        ///// </summary>
        //private void ContextConfig()
        //{
        //    var contextName = string.Concat(GetContextName(), ".cs");
        //    var contextDirectory = string.Concat(_solutionDirectory, "\\", _repositoryProject, "\\Contexto\\", contextName);

        //    var contextLines = File.ReadAllLines(contextDirectory);
        //    var writed = false;
        //    using (var writer = new StreamWriter(contextDirectory))
        //    {
        //        for (int currentLine = 1; currentLine <= contextLines.Count(); ++currentLine)
        //        {
        //            if (contextLines[currentLine - 1].Contains("DbSet"))
        //            {
        //                if (!writed)
        //                {
        //                    writer.WriteLine(string.Concat(contextLines[currentLine - 1],
        //                                                     Context.ConfigDbSet(_entityName)));
        //                    writed = true;
        //                }
        //                else
        //                {
        //                    writer.WriteLine(contextLines[currentLine - 1]);
        //                }
        //            }
        //            else
        //            {
        //                writer.WriteLine(contextLines[currentLine - 1]);
        //            }
        //        }
        //    }
        //}


        /////// <summary>
        /////// Adiciona configuração de mapeamento.
        ///// </summary>
        //private void AutoMapperConfig()
        //{
        //    var folderName = "\\Mapeamento";
        //    var mapperProfileName = "\\MapeamentoProfile.cs";
        //    var mapperDirectory = string.Concat(_solutionDirectory, "\\", _applicationProject, folderName, mapperProfileName);

        //    var contextLines = File.ReadAllLines(mapperDirectory);
        //    var writed = false;
        //    using (var writer = new StreamWriter(mapperDirectory))
        //    {
        //        for (int currentLine = 1; currentLine <= contextLines.Count(); ++currentLine)
        //        {
        //            if (contextLines[currentLine - 1].Contains("CreateMap"))
        //            {
        //                if (!writed)
        //                {
        //                    writer.WriteLine(string.Concat(contextLines[currentLine - 1],
        //                                     string.Format("{0}", MapperConfig.CreateMap(_entityName))));
        //                    writed = true;
        //                }
        //                else
        //                {
        //                    writer.WriteLine(contextLines[currentLine - 1]);
        //                }
        //            }
        //            else
        //            {
        //                writer.WriteLine(contextLines[currentLine - 1]);
        //            }
        //        }
        //    }
        //}

        private Project GetProjectByName(string nomeProjeto)
        {
            return _solutionProjectCollection.FirstOrDefault(it => it.Name == nomeProjeto);
        }

        private void SetSolutionName()
        {
            _partialSolutionDirectory = _dte.Solution.FullName.Substring(0, _dte.Solution.FullName.LastIndexOf('\\'));
            _solutionName = _dte.Solution.FullName.Substring(_partialSolutionDirectory.Length).Replace("\\", "").Replace(".sln", "");
            _solutionDirectory = string.Concat(_partialSolutionDirectory, "\\", _solutionName);

            if (_solutionName.Contains("."))
            {
                if (_solutionName.Split('.').Length > 1)
                {
                    var lastPosition = _solutionName.Split('.').Length;
                    _solutionName = _solutionName.Split('.')[lastPosition - 1];
                }
            }
        }

        /// <summary>
        /// Deparando as variáveis criadas no contexto da solução.
        /// </summary>
        /// <param name="replacementsDictionary"></param>
        private void SetParameters(Dictionary<string, string> replacementsDictionary)
        {
            replacementsDictionary.Add("$EntityName$", _entityName);
            replacementsDictionary.Add("$LowerEntityName$", _entityName.ToLower());
            replacementsDictionary.Add("$CoreSharedKernelNamespace$", $"{_solutionName}.Core.SharedKernel");

            replacementsDictionary.Add("$APIControllersNamespace$", $"{_webApiProject}.Controllers");
            replacementsDictionary.Add("$APIAutofacModules$", $"{_webApiProject}.AutofacModules");

            replacementsDictionary.Add("$DomainNamespace$", $"{_domainProject}");
            replacementsDictionary.Add("$DomainServicesNamespace$", $"{_domainProject}.Services");
            replacementsDictionary.Add("$DomainServicesInterfaceNamespace$", $"{_domainProject}.Abstractions.Services");
            replacementsDictionary.Add("$DomainRepositoriesInterfaceNamespace$", $"{_domainProject}.Abstractions.Repositories");
            replacementsDictionary.Add("$DomainReadOnlyRepositoriesInterfaceNamespace$", $"{_domainProject}.Abstractions.Repositories.ReadOnly");
            replacementsDictionary.Add("$DomainCommandsNamespace$", $"{_domainProject}.DTOs.Commands");
            replacementsDictionary.Add("$DomainQueriesNamespace$", $"{_domainProject}.DTOs.Queries");
            replacementsDictionary.Add("$DomainEntitiesNamespace$", $"{_domainProject}.Entities");
            replacementsDictionary.Add("$DomainMappersNamespace$", $"{_domainProject}.Mappers");
            replacementsDictionary.Add("$DomainCommandValidationNamespace$", $"{_domainProject}.Services.Validations");
            replacementsDictionary.Add("$DomainContextsNamespace$", $"{_domainProject}.Data.Contexts");
            replacementsDictionary.Add("$DomainDataMappingNamespace$", $"{_domainProject}.Data.Mapping");
            replacementsDictionary.Add("$DomainRepositoriesNamespace$", $"{_domainProject}.Data.Repositories");
            replacementsDictionary.Add("$DomainReadOnlyRepositoriesNamespace$", $"{_domainProject}.Data.Repositories.ReadOnly");
            replacementsDictionary.Add("$DomainCoreNamespace$", $"{_solutionName}.Core.Domain");
            replacementsDictionary.Add("$DomainCoreInterfacesNamespace$", $"{_solutionName}.Core.Domain.Abstractions");

            replacementsDictionary.Add("$DomainCoreExceptionsNamespace$", $"{_solutionName}.Core.Exceptions");
            replacementsDictionary.Add("$DomainCoreServicesNamespace$", $"{_solutionName}.Core.Services");

            replacementsDictionary.Add("InfrastructureDataAccessNamespace$", "Brinks.Infra.DataAccess");
            replacementsDictionary.Add("InfrastructureDataAccessEFNamespace$", "Brinks.Infra.DataAccess.EntityFramework");
            replacementsDictionary.Add("InfrastructureDataAccessEFInterfacesNamespace$", "Brinks.Infra.DataAccess.EntityFramework.Abstractions");
            replacementsDictionary.Add("$InfrastructureLocalizationNamespace$", "Brinks.Infra.Localization");
        }
    }
}
