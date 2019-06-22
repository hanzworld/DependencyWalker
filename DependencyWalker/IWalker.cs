using System.Collections.Generic;
using DependencyWalker.Model;
using Microsoft.Build.Evaluation;

namespace DependencyWalker
{
    public interface IWalker
    {
        List<Project> Load();
        ISolutionDependencyTree Walk();
    }
}