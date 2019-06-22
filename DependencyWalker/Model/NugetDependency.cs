using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NuGet;

namespace DependencyWalker.Model
{
    public class NugetDependency : INugetDependency
    {
        public IPackage Package { get; private set; }
        public List<INugetDependency> FoundDependencies { get; private set; }
        public List<PackageDependency> UnresolvedDependencies { get; private set; }

        [JsonConstructor]
        public NugetDependency(IPackage package)
        {
            FoundDependencies = new List<INugetDependency>();
            UnresolvedDependencies = new List<PackageDependency>();
            Package = package;
        }

        public INugetDependency AddDependency(IPackage subPackage)
        {
            var x = new NugetDependency(subPackage);
            FoundDependencies.Add(x);
            return x;
        }


        public void AddDependency(PackageDependency subPackage)
        {
            UnresolvedDependencies.Add(subPackage);
        }

        public override string ToString()
        {
            return Package.ToString();
        }


    }

}
