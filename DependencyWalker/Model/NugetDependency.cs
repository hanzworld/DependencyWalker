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

        public void AddDependency(IPackage subPackage)
        {
            var x = new NugetDependency(subPackage);
            FoundDependencies.Add(x);
        }


        public void AddDependency(PackageDependency subPackage)
        {
            UnresolvedDependencies.Add(subPackage);
        }

        public void AddDependency<T>(object subPackage) where T : PackageDependency
        {
            UnresolvedDependencies.Add(subPackage as T);
        }

        public override string ToString()
        {
            return Package.ToString();
        }

        public void AddDependencies(List<object> list)
        {
            foreach (var item in list)
            {
                if (item is IPackage package)
                {
                    AddDependency(package);
                }

                if (item is PackageDependency dependency)
                {
                    AddDependency(dependency);
                }
				if (item is NugetDependency dependency)
				{
					FoundDependencies.Add(dependency);
				}
            }
        }
    }

}
