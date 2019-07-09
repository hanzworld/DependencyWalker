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


        public NugetDependencyTree(FileSystemInfo directory)
        {
            Packages = new List<INugetDependency>();
            Source = new PackageReferenceFile(Path.Combine(directory.FullName, "packages.config"));
        }
        
        internal void AddRoots(List<IPackage> list)
        {
            Packages.AddRange(list.Select(p => new NugetDependency(p)));
        }
    }
}
