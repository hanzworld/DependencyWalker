using System.Collections.Generic;

namespace DependencyWalker.Model
{
    public interface INugetDependencyTree
    {
        List<INugetDependency> Packages { get; }

        void Print();
    }
}