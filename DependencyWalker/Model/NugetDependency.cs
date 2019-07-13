using System.Collections.Concurrent;
using Newtonsoft.Json;
using NuGet;

namespace DependencyWalker.Model
{
    public class NugetDependency : INugetDependency
    {
        public IPackage Package { get; }
        public ConcurrentBag<INugetDependency> FoundDependencies { get; }
        public ConcurrentBag<PackageDependency> UnresolvedDependencies { get; }

        [JsonConstructor]
        public NugetDependency(IPackage package)
        {
            FoundDependencies = new ConcurrentBag<INugetDependency>();
            UnresolvedDependencies = new ConcurrentBag<PackageDependency>();
            Package = package;
        }
        
        public override string ToString()
        {
            return Package.ToString();
        }

    }

}
