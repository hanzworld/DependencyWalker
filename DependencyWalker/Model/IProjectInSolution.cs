namespace DependencyWalker.Model
{
    public interface IProjectInSolution
    {
        string Name { get; }
        INugetDependencyTree NugetDependencyTree { get; }
        IProjectDependencyTree ProjectDependencyTree { get; }
    }
}