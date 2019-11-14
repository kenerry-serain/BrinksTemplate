using EnvDTE;
using EnvDTE80;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BrinksTemplate.Wizard
{
    public static class WizardExtension
    {
        /// <summary>
        /// Seleciona um item de projeto como pasta ou classe ou qualquer tipo de item de projeto.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="name"> Nome do item a ser buscado </param>
        /// <param name="type"> Tipo do item a ser buscado. Exemplo: Classe e Pasta. </param>
        /// <returns></returns>
        public static ProjectItem GetProjectItem(this Project project, string name, string type)
        {
            foreach (ProjectItem item in project.ProjectItems)
            {
                if (item.Name == name && item.Kind == type)
                {
                    return item;
                }
            }
            return null;
        }
        
        /// <summary>
        /// Adiciona um item em uma pasta/subpasta. Profundidade máxima de duas pastas.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="item"> Objeto a ser adicionado. </param>
        /// <param name="folderStructure"> Pasta/Subpasta onde o objeto deve ser adicionado. </param>
        public static void AddItemInFolder(this Project project, ProjectItem item, string folderStructure)
        {
            var lastFolder = default(ProjectItem);
            var beforeFolder = default(ProjectItem);
            var folderCollection = folderStructure.Split('.').ToList();
            var folderCollectionCopy = folderCollection;
            var pathTillNow = string.Empty;

            /* Percorrendo a lista de pastas */
            foreach (var folder in folderCollection) 
            {
                pathTillNow += $"\\{folder}";

                /* Criando subpasta na pasta anterior */
                if (beforeFolder != default(ProjectItem))
                {
                    var actualFolderIndex = folderCollectionCopy.FindIndex(p => p == folder);
                    var projectSubFolder = GetSubfolder(folderCollectionCopy[actualFolderIndex], beforeFolder.ProjectItems);

                    /* A pasta já existe na solução? */
                    if (projectSubFolder == default(ProjectItem))//[data][repostiorry]
                    {
                        var folderDirectory = $"{project.FullName.Substring(0, project.FullName.LastIndexOf('\\'))}{pathTillNow}";
                        var subFolderExists = Directory.Exists(folderDirectory);
                        
                        if (subFolderExists)
                            Directory.Delete(folderDirectory);

                        projectSubFolder = beforeFolder.ProjectItems.AddFolder(folderCollectionCopy[actualFolderIndex]);
                    }
                    
                    beforeFolder = projectSubFolder;
                    lastFolder = projectSubFolder;
                }
                else
                {
                    /* Criando primeira pasta */
                    var rootFolder = project.GetProjectItem(folder, Constants.vsProjectItemKindPhysicalFolder);
                    if (rootFolder == default(ProjectItem))
                        rootFolder = project.ProjectItems.AddFolder(folder);

                    beforeFolder = rootFolder;
                    lastFolder = rootFolder;
                }
            }

            lastFolder.ProjectItems.AddFromFileCopy(item.FileNames[0]);
        }
        private static ProjectItem GetSubfolder(string folderName, ProjectItems folderProjectItems)
        {
            var subFolder = default(ProjectItem);
            foreach (ProjectItem folderItem in folderProjectItems)
            {
                if (folderItem.Name == folderName && folderItem.Kind == Constants.vsProjectItemKindPhysicalFolder)
                {
                    subFolder = folderItem;
                    break;
                }
            }

            return subFolder;
        }

        /// <summary>
        /// Seleciona projetos da solution.
        /// </summary>
        /// <param name="solution"></param>
        /// <returns></returns>
        public static IEnumerable<Project> GetAllSolutionProjects(this Solution solution)
        {
            var projetoCollection = new List<Project>();
            foreach (Project item in solution.Projects)
            {
                /* Se o projeto estiver dentro de uma solution folder pode ser que existam mais projetos */
                if (item.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                {
                    /* Adicionando todos projetos da solution folder */
                    projetoCollection.AddRange(GetAllProjectsInSolutionFolder(item));
                }
                else
                {
                    projetoCollection.Add(item);
                }
            }
            return projetoCollection;
        }

        /// <summary>
        /// Seleciona projetos na dentro de uma Solution Folder.
        /// </summary>
        /// <param name="solutionFolder"></param>
        /// <returns></returns>
        static IEnumerable<Project> GetAllProjectsInSolutionFolder(Project solutionFolder)
        {
            var projetoCollection = new List<Project>();
            foreach (ProjectItem item in solutionFolder.ProjectItems)
            {
                if (item.SubProject != null)
                {
                    if (item.SubProject.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                    {
                        projetoCollection.AddRange(GetAllProjectsInSolutionFolder(item.SubProject));
                    }
                    else
                    {
                        projetoCollection.Add(item.SubProject);
                    }
                }
            }
            return projetoCollection;
        }
    }
}
