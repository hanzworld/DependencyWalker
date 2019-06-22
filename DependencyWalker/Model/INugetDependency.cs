using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NuGet;

namespace DependencyWalker.Model
{
    public interface INugetDependency
    {
        List<INugetDependency> FoundDependencies { get; }
        [JsonConverter(typeof(IPackageConverter))]
        IPackage Package { get; }
        List<PackageDependency> UnresolvedDependencies { get; }

        string ToString();
        INugetDependency AddDependency(IPackage subPackage);
        void AddDependency(PackageDependency subPackage);
    }
}