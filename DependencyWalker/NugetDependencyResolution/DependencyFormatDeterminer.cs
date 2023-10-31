using Microsoft.Build.Evaluation;
using Microsoft.IO;
using Serilog;

namespace DependencyWalker.NugetDependencyResolution
{
    public class DependencyFormatDeterminer
    {
        private readonly Project project;
        private readonly DirectoryInfo projectDirectory;

        public DependencyFormatDeterminer(Project project)
        {
            this.project = project;
            this.projectDirectory = new DirectoryInfo(this.project.DirectoryPath);
        }

        
        // First check to see if a packages.config file exists.
        // If it does load it, otherwise assume the project is using package reference format
        public IFileWithNugetDependencies LoadFileThatSpecifiesNugetDependencies()
        {
            var csprojFile = new CSProjFile(project);
            var hasPackageConfigFile = PackagesConfigFileExists();
            if (csprojFile.HasPackageReferences())
            {
                Log.Information($"Detected package reference used for project: {project.GetProjectName()}");
                if (hasPackageConfigFile)
                {
                    Log.Warning($"{project.GetProjectName()} is using package reference format and has a packages.config file. packages.config file is being ignored in favour of package reference format");
                }

                return csprojFile;
            }
            
            Log.Information($"Using packages.config for project: {project.GetProjectName()}");
            return new PackagesConfigFile(Path.Combine(projectDirectory.FullName, "packages.config"));

        }

        public bool PackagesConfigFileExists()
        {
            var packagesConfigPath = Path.Combine(projectDirectory.FullName, "packages.config");
            return File.Exists(packagesConfigPath);
        }
    }
}