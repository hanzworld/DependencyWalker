using System.Collections.Concurrent;
using System.Collections.Generic;
using Newtonsoft.Json;
using NuGet;

namespace DependencyWalker.Model
{
    public interface INugetDependency
    {
        ConcurrentBag<INugetDependency> FoundDependencies { get; }
        ConcurrentBag<PackageDependency> UnresolvedDependencies { get; }

        [JsonConverter(typeof(IPackageConverter))]
        IPackage Package { get; }
    }
}