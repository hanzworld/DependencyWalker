using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Serilog;

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

        public void Print()
        {
            foreach (var project in Projects)
            {
                Log.Information("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
                Log.Information(project.Name.ToUpper());
                foreach (var package in project.NugetDependencyTree.Packages.GetPackagesThatMatchFilter(this))
                {
                    Print(package, 0);
                }
            }
        }


        //todo these should be in the NugetPcakgeInfo class, not here

        private void Print(INugetDependency package, int level)
        {
            Log.Information("{0}{1}", new string(' ', level * 3), package.ToString());
            foreach (var dependency in package.UnresolvedDependencies)
            {
                if (dependency == null)
                {
                    return;
                }
            }
            foreach (var dependency in package.FoundDependencies)
            {
                Print(dependency, level + 1);
            }
        }
    }


}