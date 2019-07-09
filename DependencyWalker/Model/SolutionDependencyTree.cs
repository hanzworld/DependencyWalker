using System.Collections.Generic;

namespace DependencyWalker.Model
{
    public class SolutionDependencyTree : ISolutionDependencyTree
    {
        public SolutionDependencyTree(string solutionToAnalyse)
        {
            SolutionToAnalyse = solutionToAnalyse;
            Projects = new List<IProjectInSolution>();
        }

        public List<IProjectInSolution> Projects { get; internal set; }
        public string SolutionToAnalyse { get; }
        public string[] Filter { get; set; }

    }


}