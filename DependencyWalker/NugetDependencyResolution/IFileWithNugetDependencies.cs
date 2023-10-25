using System.Collections.Generic;
using NuGet;

namespace DependencyWalker.NugetDependencyResolution
{
    public interface IFileWithNugetDependencies
    {
        IEnumerable<PackageReference> GetPackageReferences();
    }
}