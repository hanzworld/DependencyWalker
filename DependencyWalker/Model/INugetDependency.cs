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

        void AddDependencies(List<object> list);
    }
}