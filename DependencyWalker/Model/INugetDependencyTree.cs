using System.Collections.Generic;
using Newtonsoft.Json;
using NuGet;

namespace DependencyWalker.Model
{
    public interface INugetDependencyTree
    {
        List<INugetDependency> Packages { get; }
        PackageReferenceFile Source { get; }

        void Print();
    }
}