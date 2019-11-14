﻿using EnvDTE;
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
        private Project _webApiProject, _domainProject;
        private IEnumerable<Project> _solutionProjectCollection;
        private string _entityName
        , _contextName
        , _webApiProjectName
        , _domainProjectName
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
        private IList<string> commandCollection, commandValidatorCollection;

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
                project = GetProjectByName(_domainProjectName);
                folder = "Entities";
                _domainProject = project;
            }

            /* Domain repository interface */
            else if (item == _entityRepositoryInterface)
            {
                project = GetProjectByName(_domainProjectName);
                folder = "Abstractions.Repositories";
                _domainProject = project;
            }

            /* Domain read only repository interface */
            else if (item == _entityReadOnlyRepositoryInterface)
            {
                project = GetProjectByName(_domainProjectName);
                folder = "Abstractions.Repositories.ReadOnly";
                _domainProject = project;
            }

            /* Domain service interface*/
            else if (item == _entityServiceInterface)
            {
                project = GetProjectByName(_domainProjectName);
                folder = "Abstractions.Services";
                _domainProject = project;
            }

            /* Domain commands */
            else if (commandCollection.Contains(item))
            {
                project = GetProjectByName(_domainProjectName);
                folder = $"DTOs.Commands.{_entityName}";
                _domainProject = project;
            }

            /* Domain query */
            else if (item == _entityQuery)
            {
                project = GetProjectByName(_domainProjectName);
                folder = $"DTOs.Queries.{_entityName}";
                _domainProject = project;
            }

            /* Domain commands validation */
            else if (commandValidatorCollection.Contains(item))
            {
                project = GetProjectByName(_domainProjectName);
                folder = $"Services.Validations.{_entityName}";
                _domainProject = project;
            }

            /* Domain service */
            else if (item == _entityService)
            {
                project = GetProjectByName(_domainProjectName);
                folder = "Services";
                _domainProject = project;
            }

            /* Domain repository */
            else if (item == _entityRepository)
            {
                project = GetProjectByName(_domainProjectName);
                folder = "Data.Repositories";
                _domainProject = project;
            }

            /* Domain read only repository */
            else if (item == _entityReadOnlyRepository)
            {
                project = GetProjectByName(_domainProjectName);
                folder = "Data.Repositories.ReadOnly";
                _domainProject = project;
            }

            /* Domain map */
            else if (item == $"{_entityName}Map")
            {
                project = GetProjectByName(_domainProjectName);
                folder = "Data.Mappings";
                _domainProject = project;
            }

            /* Entity controller */
            else if (item == $"{_entityName}Controller")
            {
                project = GetProjectByName(_webApiProjectName);
                folder = "Controllers";
                _webApiProject = project;
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
            commandValidatorCollection = new List<string>
            {
                $"Register{_entityName}CommandValidator",
                $"Update{_entityName}CommandValidator",
                $"Remove{_entityName}CommandValidator"
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

            _webApiProjectName = optionsForm.WebApiProject;
            _domainProjectName = optionsForm.DomainProject;

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
            AutofacValidatorModules();
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
        /// Injeta as dependências referente ao modulo de aplicação
        /// </summary>
        private void AutofacApplicationModules()
        {
            var folderName = "\\AutofacModules";
            var applicationModuleName = "\\ApplicationModule.cs";
            var directory = _webApiProject.FullName.Substring(0, _webApiProject.FullName.LastIndexOf('\\'));
            var applicationModuleDirectory = string.Concat(directory, folderName, applicationModuleName);
            var applicationModuleLines = File.ReadAllLines(applicationModuleDirectory);
            var alreadyWritedMap = false;
            using (var writer = new StreamWriter(applicationModuleDirectory))
            {
                for (int currentLine = 1; currentLine <= applicationModuleLines.Count(); ++currentLine)
                {
                    var alreadyWritedDefaultLine = false;
                    if (!alreadyWritedMap && currentLine >= 3)
                    {
                        if (currentLine >= 3 && applicationModuleLines[currentLine - 3].Contains("Load(ContainerBuilder"))
                        {
                            writer.WriteLine($"\t\t\tbuilder.RegisterType<{_entityServiceInterface}>().As<{_entityService}>();");
                            writer.WriteLine(applicationModuleLines[currentLine - 1]);
                            alreadyWritedDefaultLine = true;
                            alreadyWritedMap = true;
                        }
                    }

                    if (!alreadyWritedDefaultLine)
                        writer.WriteLine(applicationModuleLines[currentLine - 1]);
                }
            }
        }

        /// <summary>
        /// Injeta as dependências referente ao modulo de validações
        /// </summary>
        private void AutofacValidatorModules()
        {
            var folderName = "\\AutofacModules";
            var applicationModuleName = "\\ValidatorModule.cs";
            var directory = _webApiProject.FullName.Substring(0, _webApiProject.FullName.LastIndexOf('\\'));
            var validatorModuleDirectory = string.Concat(directory, folderName, applicationModuleName);
            var validatorModuleLines = File.ReadAllLines(validatorModuleDirectory);
            var alreadyWritedUsing = false;
            var alreadyWritedMap = false;
            var domainCommandNamespace = _replacementsDictionary["$DomainCommandsNamespace$"];
            var domainCommandValidationNamespace = _replacementsDictionary["$DomainCommandValidationNamespace$"];
            var entityCommandValidationNamespace = $"using {domainCommandValidationNamespace}.{_entityName};";
            var entityCommandNamespace = $"using {domainCommandNamespace}.{_entityName};";
            using (var writer = new StreamWriter(validatorModuleDirectory))
            {
                for (int currentLine = 1; currentLine <= validatorModuleLines.Count(); ++currentLine)
                {
                    var alreadyWritedDefaultLine = false;
                    if (!alreadyWritedUsing)
                    {
                        if (!validatorModuleLines.Contains(entityCommandValidationNamespace) && !validatorModuleLines.Contains(entityCommandNamespace))
                        {
                            if (validatorModuleLines[currentLine - 1].Contains("using"))
                            {
                                writer.WriteLine($"{entityCommandValidationNamespace}");
                                writer.WriteLine($"{entityCommandNamespace}");
                                writer.WriteLine(validatorModuleLines[currentLine - 1]);
                                alreadyWritedUsing = true;
                                alreadyWritedDefaultLine = true;
                            }
                        }
                    }

                    if (!alreadyWritedMap && currentLine >= 3)
                    {
                        if (validatorModuleLines[currentLine - 3].Contains("Load(ContainerBuilder"))
                        {
                            foreach (var command in commandCollection)
                            {
                                var commandValidator = commandValidatorCollection.FirstOrDefault(validator => validator.Equals($"{command}Validator"));
                                writer.WriteLine($"\t\t\tbuilder.RegisterType<{commandValidator}>().As<IValidator<{command}>>();");
                            }
                            writer.WriteLine(validatorModuleLines[currentLine - 1]);
                            alreadyWritedDefaultLine = true;
                            alreadyWritedMap = true;
                        }
                    }

                    if (!alreadyWritedDefaultLine)
                        writer.WriteLine(validatorModuleLines[currentLine - 1]);
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
            var directory = _webApiProject.FullName.Substring(0, _webApiProject.FullName.LastIndexOf('\\'));
            var infraModuleDirectory = string.Concat(directory, folderName, infraModuleName);
            var infraModuleLines = File.ReadAllLines(infraModuleDirectory);
            var alreadyWritedMap = false;
            using (var writer = new StreamWriter(infraModuleDirectory))
            {
                for (int currentLine = 1; currentLine <= infraModuleLines.Count(); ++currentLine)
                {
                    var alreadyWritedDefaultLine = false;
                    if (!alreadyWritedMap && currentLine >= 3)
                    {
                        if (infraModuleLines[currentLine - 3].Contains("Load(ContainerBuilder"))
                        {
                            writer.WriteLine($"\t\t\tbuilder.RegisterType<{_entityRepositoryInterface}>().As<{_entityRepository}>();");
                            writer.WriteLine($"\t\t\tbuilder.RegisterType<{_entityReadOnlyRepositoryInterface}>().As<{_entityReadOnlyRepository}>();");
                            writer.WriteLine(infraModuleLines[currentLine - 1]);
                            alreadyWritedDefaultLine = true;
                            alreadyWritedMap = true;
                        }
                    }

                    if (!alreadyWritedDefaultLine)
                        writer.WriteLine(infraModuleLines[currentLine - 1]);
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
            var directory = Directory.GetDirectories(_partialSolutionDirectory).FirstOrDefault(dir => dir.EndsWith(_webApiProjectName));
            var infraModuleDirectory = string.Concat(directory, folderName, context);
            var infraModuleLines = File.ReadAllLines(infraModuleDirectory);

            using (var writer = new StreamWriter(infraModuleDirectory))
            {
                for (int currentLine = 1; currentLine <= infraModuleLines.Count(); ++currentLine)
                {
                    if (infraModuleLines[currentLine - 1].Contains("();"))
                    {
                        writer.WriteLine($"\t\tbuilder.RegisterType<{_entityRepositoryInterface}>().As<{_entityRepository}>();");
                        writer.WriteLine($"\t\tbuilder.RegisterType<{_entityReadOnlyRepositoryInterface}>().As<{_entityReadOnlyRepository}>();");
                        writer.WriteLine(infraModuleLines[currentLine - 1]);
                    }
                    else
                        writer.WriteLine(infraModuleLines[currentLine - 1]);
                }
            }
        }

        /// <summary>
        /// AutoMapper domain to query
        /// </summary>
        private void SetupDomainToQueryProfile()
        {
            var folderName = "\\Mappers";
            var mapperProfileName = "\\DomainToQueryProfile.cs";
            var directory = _domainProject.FullName.Substring(0, _domainProject.FullName.LastIndexOf('\\'));
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
                                writer.WriteLine($"{entityQueryNamespace}");
                                writer.WriteLine(domainToQueryProfileLines[currentLine - 1]);
                                alreadyWritedUsing = true;
                                alreadyWritedDefaultLine = true;
                            }
                        }
                    }

                    if (!alreadyWritedMap && currentLine >= 3)
                    {
                        if (domainToQueryProfileLines[currentLine - 3].Contains("DomainToQueryProfile()"))
                        {
                            writer.WriteLine($"\t\t\tCreateMap<{_entityName}, {_entityQuery}>();");
                            writer.WriteLine(domainToQueryProfileLines[currentLine - 1]);
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
            var directory = _domainProject.FullName.Substring(0, _domainProject.FullName.LastIndexOf('\\'));
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
                                writer.WriteLine($"{entityCommandNamespace}");
                                writer.WriteLine(commandToDomainProfileLines[currentLine - 1]);
                                alreadyWritedUsing = true;
                                alreadyWritedDefaultLine = true;
                            }
                        }
                    }

                    if (!alreadyWritedMap && currentLine >= 3)
                    {
                        if (commandToDomainProfileLines[currentLine - 3].Contains("CommandToDomainProfile()"))
                        {
                            foreach (var item in commandCollection)
                                writer.WriteLine($"\t\t\tCreateMap<{item}, Entities.{_entityName}>();");

                            writer.WriteLine(commandToDomainProfileLines[currentLine - 1]);
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

            replacementsDictionary.Add("$APIControllersNamespace$", $"{_webApiProjectName}.Controllers");
            replacementsDictionary.Add("$APIAutofacModules$", $"{_webApiProjectName}.AutofacModules");

            replacementsDictionary.Add("$DomainNamespace$", $"{_domainProjectName}");
            replacementsDictionary.Add("$DomainServicesNamespace$", $"{_domainProjectName}.Services");
            replacementsDictionary.Add("$DomainServicesInterfaceNamespace$", $"{_domainProjectName}.Abstractions.Services");
            replacementsDictionary.Add("$DomainRepositoriesInterfaceNamespace$", $"{_domainProjectName}.Abstractions.Repositories");
            replacementsDictionary.Add("$DomainReadOnlyRepositoriesInterfaceNamespace$", $"{_domainProjectName}.Abstractions.Repositories.ReadOnly");
            replacementsDictionary.Add("$DomainCommandsNamespace$", $"{_domainProjectName}.DTOs.Commands");
            replacementsDictionary.Add("$DomainQueriesNamespace$", $"{_domainProjectName}.DTOs.Queries");
            replacementsDictionary.Add("$DomainEntitiesNamespace$", $"{_domainProjectName}.Entities");
            replacementsDictionary.Add("$DomainMappersNamespace$", $"{_domainProjectName}.Mappers");
            replacementsDictionary.Add("$DomainCommandValidationNamespace$", $"{_domainProjectName}.Services.Validations");
            replacementsDictionary.Add("$DomainContextsNamespace$", $"{_domainProjectName}.Data.Contexts");
            replacementsDictionary.Add("$DomainDataMappingNamespace$", $"{_domainProjectName}.Data.Mapping");
            replacementsDictionary.Add("$DomainRepositoriesNamespace$", $"{_domainProjectName}.Data.Repositories");
            replacementsDictionary.Add("$DomainReadOnlyRepositoriesNamespace$", $"{_domainProjectName}.Data.Repositories.ReadOnly");
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
