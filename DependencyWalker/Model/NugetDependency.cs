using System.Collections.Generic;
using Newtonsoft.Json;
using NuGet;

namespace DependencyWalker.Model
{
    public class NugetDependency : INugetDependency
    {
        public IPackage Package { get; }
        public List<INugetDependency> FoundDependencies { get; }
        public List<PackageDependency> UnresolvedDependencies { get; }

        [JsonConstructor]
        public NugetDependency(IPackage package)
        {
            FoundDependencies = new List<INugetDependency>();
            UnresolvedDependencies = new List<PackageDependency>();
            Package = package;
        }

        public void AddDependency(IPackage subPackage)
        {
            var x = new NugetDependency(subPackage);
            FoundDependencies.Add(x);
        }


        public void AddDependency(PackageDependency subPackage)
        {
            UnresolvedDependencies.Add(subPackage);
        }

        public override string ToString()
        {
            return Package.ToString();
        }

        public void AddDependencies(List<object> list)
        {
            foreach (var item in list)
            {
                switch (item)
                {
                    case IPackage package:
                        AddDependency(package);
                        break;
                    case PackageDependency dependency:
                        AddDependency(dependency);
                        break;
                    case NugetDependency dependency:
                        FoundDependencies.Add(dependency);
                        break;
                }

            }
        }
    }

}
