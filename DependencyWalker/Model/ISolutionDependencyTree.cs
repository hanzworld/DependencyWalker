using Newtonsoft.Json;
using System.Collections.Generic;

namespace DependencyWalker.Model
{
    public interface ISolutionDependencyTree
    {
        [JsonIgnore]
        string[] Filter { get; set; }
        List<IProjectInSolution> Projects { get; }

        void Print();
    }
}