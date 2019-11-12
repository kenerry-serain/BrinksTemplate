using EnvDTE;
using EnvDTE80;
using System.Collections.Generic;
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
        /// <param name="folderPath"> Pasta/Subpasta onde o objeto deve ser adicionado. </param>
        public static void AddItemInFolder(this Project project, ProjectItem item, string folderPath)
        {
            var folderCollection = folderPath.Split('.').ToList();
            var copyListaCollection = folderCollection;

            /* Percorrendo a lista de pastas */
            foreach (var folder in folderCollection)
            {
                /* Verificando se a primeira pasta existe */
                var firstFolder = project.GetProjectItem(folder, Constants.vsProjectItemKindPhysicalFolder);
                if (firstFolder != null)
                {
                    if (copyListaCollection.Count() > 1)
                    {
                        var actualFolderIndex = copyListaCollection.FindIndex(p => p == folder);
                        var secondFolder = project.GetProjectItem(copyListaCollection[actualFolderIndex + 1], Constants.vsProjectItemKindPhysicalFolder);
                        if (secondFolder != null)
                        {
                            secondFolder.ProjectItems.AddFromFileCopy(item.FileNames[0]);
                            break;
                        }
                        else
                        {
                            /* FOLDER REPOSITORY ALREADY EXISTS */
                            secondFolder = firstFolder.ProjectItems.AddFolder(copyListaCollection[actualFolderIndex + 1]);
                            if (secondFolder != null)
                            {
                                secondFolder.ProjectItems.AddFromFileCopy(item.FileNames[0]);
                                break;
                            }
                        }
                    }
                    else
                    {
                        firstFolder.ProjectItems.AddFromFileCopy(item.FileNames[0]);
                    }
                }
                else /* Caso a primeira pasta não exista */
                {
                    firstFolder = project.ProjectItems.AddFolder(folder);
                    if (firstFolder != null)
                    {
                        if (copyListaCollection.Count > 1) /* Caso seja para inserir o arquivo em uma pasta dentro de outra pasta */
                        {
                            var actualFolderIndex = copyListaCollection.FindIndex(p => p == folder);
                            var secondFolder = firstFolder.ProjectItems.AddFolder(copyListaCollection[actualFolderIndex + 1]);
                            if (secondFolder != null)
                            {
                                secondFolder.ProjectItems.AddFromFileCopy(item.FileNames[0]);
                                break;
                            }
                            else
                            {
                                secondFolder = firstFolder.ProjectItems.AddFolder(copyListaCollection[actualFolderIndex + 1]);
                                if (secondFolder != null)
                                {
                                    secondFolder.ProjectItems.AddFromFileCopy(item.FileNames[0]);
                                    break;
                                }
                            }
                        }
                        else /* Caso seja para inserir o arquivo nesta pasta */
                        {
                            firstFolder.ProjectItems.AddFromFileCopy(item.FileNames[0]);
                        }
                    }
                }
            }
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
