using Newtonsoft.Json;
using System.Collections.Generic;

namespace DependencyWalker.Model
{
    public interface IProjectDependencyTree
    {
        List<IProjectDependency> References { get; set; }
    }
}