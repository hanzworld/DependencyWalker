using DependencyWalker.Model;

namespace DependencyWalker
{
    public interface IWalker
    {
        ISolutionDependencyTree Walk();
    }
}