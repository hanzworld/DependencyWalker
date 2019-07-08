namespace DependencyWalker.Model
{
    public class ProjectInSolution : IProjectInSolution
    {
        public ProjectInSolution(string fullPath)
        {
            Name = fullPath;
        }

        public string Name { get; }
        public INugetDependencyTree NugetDependencyTree { get; set; }
        public IProjectDependencyTree ProjectDependencyTree { get; set; }

    }
}