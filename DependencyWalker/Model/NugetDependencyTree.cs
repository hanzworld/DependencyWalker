using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NuGet;

namespace DependencyWalker.Model
{
    [Serializable]
    public class NugetDependencyTree : INugetDependencyTree
    {
        public List<INugetDependency> Packages { get; }
        [JsonIgnore]
        public PackageReferenceFile Source { get; }


        public NugetDependencyTree(string projectName, FileSystemInfo directory)
        {
            Packages = new List<INugetDependency>();
            Source = new PackageReferenceFile(Path.Combine(directory.FullName, "packages.config"));
        }
        public void Print()
        {
            foreach (var package in Packages)
            {
                Print(package, 0);
            }
        }

        private static void Print(INugetDependency package, int level)
        {
            Log.Information("{0}{1}", new string(' ', level * 3), package.ToString());
            foreach (var dependency in package.UnresolvedDependencies)
            {
                if (dependency != null)
                {
                    continue;
                }

                WarnDependencyNotFound(level, package);
                return;
            }
            foreach (var dependency in package.FoundDependencies)
            {

                Print(dependency, level + 1);
            }
        }

        private static void WarnDependencyNotFound(int level, PackageDependency dependency)
        {
            Log.Error("{0}** Couldn't find dependency {1} - incomplete dependency tree", new string(' ', level + 1 * 3), dependency);
        }

        internal void AddRoot(IPackage package)
        {
            Packages.Add(new NugetDependency(package));
        }

        internal void AddRoots(List<IPackage> list)
        {
            Packages.AddRange(list.Select(p => new NugetDependency(p)));
        }
    }
}
