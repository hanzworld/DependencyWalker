using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NuGet;
using Serilog;

namespace DependencyWalker.Model
{
    [Serializable]
    public class NugetDependencyTree : INugetDependencyTree
    {
        public List<INugetDependency> Packages { get; private set; }
        [JsonIgnore]
        public PackageReferenceFile Source { get; private set; }


        public NugetDependencyTree(string projectName, DirectoryInfo directory)
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

        private void Print(INugetDependency package, int level)
        {
            Log.Information("{0}{1}", new string(' ', level * 3), package.ToString());
            foreach (var dependency in package.UnresolvedDependencies)
            {
                if (dependency == null)
                {
                    WarnDependencyNotFound(level, dependency);
                    return;
                }
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

    }
}
