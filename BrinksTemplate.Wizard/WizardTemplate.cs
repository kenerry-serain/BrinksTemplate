using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

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
        , _entityQuery
        , _entityServiceInterface
        , _entityService
        , _entityRepositoryInterface
        , _entityRepository
        , _entityReadOnlyRepositoryInterface
        , _entityReadOnlyRepository
        , _partialSolutionDirectory
        , _solutionDirectory;
        private Dictionary<string, string> _replacementsDictionary;
        private IList<string> commandCollection, commandValidationCollection;

        public string _databaseEntity;
        public WizardTemplate()
        {
            _dte = (DTE)Package.GetGlobalService(typeof(DTE));
            SetSolutionConfig();
        }

        public void BeforeOpeningFile(ProjectItem projectItem) { }
        public void ProjectFinishedGenerating(Project project) { }

        /// <summary>
        /// Insere cada classe dentro do seu projeto e dentro da sua pasta(a).
        /// </summary>
        /// <param name="projectItem"></param>
        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
            var item = Path.GetFileNameWithoutExtension(projectItem.Name);
            var folder = string.Empty;
            var project = default(Project);

            InitializeEnvironmentVariables();

            /* Domain entity */
            if (item == _entityName)
            {
                project = GetProjectByName(_domainProject);
                folder = "Entities";
            }

            /* Domain repository interface */
            else if (item == _entityRepositoryInterface)
            {
                project = GetProjectByName(_domainProject);
                folder = "Abstractions.Repositories";
            }

            /* Domain read only repository interface */
            else if (item == _entityReadOnlyRepositoryInterface)
            {
                project = GetProjectByName(_domainProject);
                folder = "Abstractions.Repositories.ReadOnly";
            }

            /* Domain service interface*/
            else if (item == _entityServiceInterface)
            {
                project = GetProjectByName(_domainProject);
                folder = "Abstractions.Services";
            }

            /* Domain commands */
            else if (commandCollection.Contains(item))
            {
                project = GetProjectByName(_domainProject);
                folder = $"DTOs.Commands.{_entityName}";
            }

            /* Domain query */
            else if (item == _entityQuery)
            {
                project = GetProjectByName(_domainProject);
                folder = $"DTOs.Queries.{_entityName}";
            }

            /* Domain commands validation */
            else if (commandValidationCollection.Contains(item))
            {
                project = GetProjectByName(_domainProject);
                folder = $"Services.Validations.{_entityName}";
            }

            /* Domain service */
            else if (item == _entityService)
            {
                project = GetProjectByName(_domainProject);
                folder = "Services";
            }

            /* Domain repository */
            else if (item == _entityRepository)
            {
                project = GetProjectByName(_domainProject);
                folder = "Data.Repositories";
            }

            /* Domain read only repository */
            else if (item == _entityReadOnlyRepository)
            {
                project = GetProjectByName(_domainProject);
                folder = "Data.Repositories.ReadOnly";
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
                folder = "Controllers";
            }

            project.AddItemInFolder(projectItem, folder);
            _pastaTemp = (ProjectItem)projectItem.Collection.Parent;
        }

        /// <summary>
        /// Setup váriaveis de ambiente
        /// </summary>
        private void InitializeEnvironmentVariables()
        {
            commandCollection = new List<string>
            {
                $"Register{_entityName}Command",
                $"Update{_entityName}Command",
                $"Remove{_entityName}Command"
            };
            commandValidationCollection = new List<string>
            {
                $"Register{_entityName}CommandValidation",
                $"Update{_entityName}CommandValidation",
                $"Remove{_entityName}CommandValidation"
            };

            _entityQuery = $"{_entityName}Query";
            _entityRepositoryInterface = $"I{_entityName}Repository";
            _entityReadOnlyRepositoryInterface = $"I{_entityName}ReadOnlyRepository";
            _entityServiceInterface = $"I{_entityName}Service";
            _entityService = $"{_entityName}Service";
            _entityRepository = $"{_entityName}Repository";
            _entityReadOnlyRepository = $"{_entityName}ReadOnlyRepository";
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
            _replacementsDictionary = replacementsDictionary;
        }

        /// <summary>
        /// Depois que tudo estiver pronto. Apaga a pasta temp criada pelo visual studio e deixa somente as pastas criadas personalizadamente.
        /// Insere toda injeção de depêndencia. Faz o Mapeamento. Configura o DbSet.
        /// </summary>
        public void RunFinished()
        {
            _pastaTemp?.Remove();
            _pastaTemp?.Delete();

            IoCConfig();
            AutoMapperConfig();
            //ContextConfig();
            //CreateEntity();
        }


        public bool ShouldAddProjectItem(string filePath)
        {
            return !OptionsForm.IsCanceled;
        }

        /// <summary>
        /// Setup IoC Config
        /// </summary>
        private void IoCConfig()
        {
            AutofacApplicationModules();
            AutofacInfrastructureModules();
        }

        /// <summary>
        /// Injeta as dependências referente ao modulo de aplicação
        /// </summary>
        private void AutofacApplicationModules()
        {
            var folderName = "\\AutofacModules";
            var applicationModuleName = "\\ApplicationModule.cs";
            var directory = Directory.GetDirectories(_partialSolutionDirectory).FirstOrDefault(dir => dir.EndsWith(_webApiProject));
            var infraModuleDirectory = string.Concat(directory, folderName, applicationModuleName);
            var infraModuleLines = File.ReadAllLines(infraModuleDirectory);

            using (var writer = new StreamWriter(infraModuleDirectory))
            {
                for (int currentLine = 1; currentLine <= infraModuleLines.Count(); ++currentLine)
                {
                    if (infraModuleLines[currentLine - 1].Contains("();"))
                    {
                        writer.WriteLine(infraModuleLines[currentLine - 1]);
                        writer.WriteLine($"builder.RegisterType<{_entityServiceInterface}>().As<{_entityService}>()");
                    }
                    else
                    {
                        writer.WriteLine(infraModuleLines[currentLine - 1]);
                    }
                }
            }
        }

        /// <summary>
        /// Injeta as dependências referente ao modulo de infraestrutura
        /// </summary>
        private void AutofacInfrastructureModules()
        {
            var folderName = "\\AutofacModules";
            var infraModuleName = "\\InfraModule.cs";
            var directory = Directory.GetDirectories(_partialSolutionDirectory).FirstOrDefault(dir => dir.EndsWith(_webApiProject));
            var infraModuleDirectory = string.Concat(directory, folderName, infraModuleName);
            var infraModuleLines = File.ReadAllLines(infraModuleDirectory);

            using (var writer = new StreamWriter(infraModuleDirectory))
            {
                for (int currentLine = 1; currentLine <= infraModuleLines.Count(); ++currentLine)
                {
                    if (infraModuleLines[currentLine - 1].Contains("();"))
                    {
                        writer.WriteLine(infraModuleLines[currentLine - 1]);
                        writer.WriteLine($"builder.RegisterType<{_entityRepositoryInterface}>().As<{_entityRepository}>()");
                        writer.WriteLine($"builder.RegisterType<{_entityReadOnlyRepositoryInterface}>().As<{_entityReadOnlyRepository}>()");
                    }
                    else
                    {
                        writer.WriteLine(infraModuleLines[currentLine - 1]);
                    }
                }
            }
        }

        /// <summary>
        /// Adiciona a configuração de DbSet.
        /// </summary>
        private void ContextConfig()
        {
            var folderName = "\\Data\\Contexts";
            var context = "\\OccurrenceDbContext.cs";
            var directory = Directory.GetDirectories(_partialSolutionDirectory).FirstOrDefault(dir => dir.EndsWith(_webApiProject));
            var infraModuleDirectory = string.Concat(directory, folderName, context);
            var infraModuleLines = File.ReadAllLines(infraModuleDirectory);

            using (var writer = new StreamWriter(infraModuleDirectory))
            {
                for (int currentLine = 1; currentLine <= infraModuleLines.Count(); ++currentLine)
                {
                    if (infraModuleLines[currentLine - 1].Contains("();"))
                    {
                        writer.WriteLine(infraModuleLines[currentLine - 1]);
                        writer.WriteLine($"builder.RegisterType<{_entityRepositoryInterface}>().As<{_entityRepository}>()");
                        writer.WriteLine($"builder.RegisterType<{_entityReadOnlyRepositoryInterface}>().As<{_entityReadOnlyRepository}>()");
                    }
                    else
                    {
                        writer.WriteLine(infraModuleLines[currentLine - 1]);
                    }
                }
            }
        }


        /// <summary>
        /// Adiciona configuração de mapeamento.
        /// </summary>
        private void AutoMapperConfig()
        {
            SetupCommandToDomainProfile();
            SetupDomainToQueryProfile();
        }

        /// <summary>
        /// AutoMapper domain to query
        /// </summary>
        private void SetupDomainToQueryProfile()
        {
            var folderName = "\\Mappers";
            var mapperProfileName = "\\DomainToQueryProfile.cs";
            var directory = Directory.GetDirectories(_partialSolutionDirectory).FirstOrDefault(dir => dir.EndsWith(_domainProject));
            var mapperDirectory = string.Concat(directory, folderName, mapperProfileName);
            var domainToQueryProfileLines = File.ReadAllLines(mapperDirectory);
            var alreadyWritedMap = false;
            var alreadyWritedUsing = false;
            using (var writer = new StreamWriter(mapperDirectory))
            {
                var domainQueriesNamespace = _replacementsDictionary["$DomainQueriesNamespace$"];
                var entityQueryNamespace = $"using {domainQueriesNamespace}.{_entityName};";
                for (int currentLine = 1; currentLine <= domainToQueryProfileLines.Count(); ++currentLine)
                {
                    var alreadyWritedDefaultLine = false;
                    if (!alreadyWritedUsing)
                    {
                        if (!domainToQueryProfileLines.Contains(entityQueryNamespace))
                        {
                            if (domainToQueryProfileLines[currentLine - 1].Contains("using"))
                            {
                                writer.WriteLine(domainToQueryProfileLines[currentLine - 1]);
                                writer.WriteLine($"{entityQueryNamespace}");
                                alreadyWritedUsing = true;
                                alreadyWritedDefaultLine = true;
                            }
                        }
                    }

                    if (!alreadyWritedMap)
                    {
                        if (domainToQueryProfileLines[currentLine - 1].Contains("();"))
                        {
                            writer.WriteLine(domainToQueryProfileLines[currentLine - 1]);
                            writer.WriteLine($"\t\t\tCreateMap<{_entityName}, {_entityQuery}>();");
                            alreadyWritedMap = true;
                            alreadyWritedDefaultLine = true;
                        }
                    }

                    if (!alreadyWritedDefaultLine)
                        writer.WriteLine(domainToQueryProfileLines[currentLine - 1]);
                }
            }
        }

        /// <summary>
        /// AutoMapper command to domain
        /// </summary>
        private void SetupCommandToDomainProfile()
        {
            var folderName = "\\Mappers";
            var mapperProfileName = "\\CommandToDomainProfile.cs";
            var directory = Directory.GetDirectories(_partialSolutionDirectory).FirstOrDefault(dir => dir.EndsWith(_domainProject));
            var mapperDirectory = string.Concat(directory, folderName, mapperProfileName);
            var commandToDomainProfileLines = File.ReadAllLines(mapperDirectory);
            var alreadyWritedMap = false;
            var alreadyWritedUsing = false;
            using (var writer = new StreamWriter(mapperDirectory))
            {
                var domainCommandNamespace = _replacementsDictionary["$DomainCommandsNamespace$"];
                var entityCommandNamespace = $"using {domainCommandNamespace}.{_entityName};";
                for (int currentLine = 1; currentLine <= commandToDomainProfileLines.Count(); ++currentLine)
                {
                    var alreadyWritedDefaultLine = false;
                    if (!alreadyWritedUsing)
                    {
                        if (!commandToDomainProfileLines.Contains(entityCommandNamespace))
                        {
                            if (commandToDomainProfileLines[currentLine - 1].Contains("using"))
                            {
                                writer.WriteLine(commandToDomainProfileLines[currentLine - 1]);
                                writer.WriteLine($"{entityCommandNamespace}");
                                alreadyWritedUsing = true;
                                alreadyWritedDefaultLine = true;
                            }
                        }
                    }

                    if (commandToDomainProfileLines[currentLine - 1].Contains("();"))
                    {
                        if (!alreadyWritedMap)
                        {
                            foreach (var item in commandCollection)
                            {
                                writer.WriteLine(commandToDomainProfileLines[currentLine - 1]);
                                writer.WriteLine($"\t\t\tCreateMap<{item}, {_entityName}>();");
                            }
                            alreadyWritedMap = true;
                            alreadyWritedDefaultLine = true;
                        }
                    }

                    if (!alreadyWritedDefaultLine)
                        writer.WriteLine(commandToDomainProfileLines[currentLine - 1]);
                }
            }
        }

        /// <summary>
        /// Encontra um projeto pelo nome
        /// </summary>
        /// <param name="nomeProjeto"></param>
        /// <returns></returns>
        private Project GetProjectByName(string nomeProjeto)
        {
            return _solutionProjectCollection.FirstOrDefault(it => it.Name == nomeProjeto);
        }

        /// <summary>
        /// Inicializa váriaveis de solução
        /// </summary>
        private void SetSolutionConfig()
        {
            _partialSolutionDirectory = _dte.Solution.FullName.Substring(0, _dte.Solution.FullName.LastIndexOf('\\'));
            _solutionName = _dte.Solution.FullName.Substring(_partialSolutionDirectory.Length).Replace("\\", "").Replace(".sln", "");
            _solutionDirectory = string.Concat(_partialSolutionDirectory, "\\", _solutionName);
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

            replacementsDictionary.Add("$InfrastructureDataAccessNamespace$", "Brinks.Infra.DataAccess");
            replacementsDictionary.Add("$InfrastructureDataAccessEFNamespace$", "Brinks.Infra.DataAccess.EntityFramework");
            replacementsDictionary.Add("$InfrastructureDataAccessEFInterfacesNamespace$", "Brinks.Infra.DataAccess.EntityFramework.Abstractions");
            replacementsDictionary.Add("$InfrastructureLocalizationNamespace$", "Brinks.Infra.Localization");
            replacementsDictionary.Add("$InfrastructureDataAccessInterfacesNamespace$", "Brinks.Infra.DataAccess.Abstractions");
        }
    }
}
