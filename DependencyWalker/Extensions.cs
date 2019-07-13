using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DependencyWalker.Model;

[assembly: InternalsVisibleTo("DependencyWalkerTests")]
namespace DependencyWalker
{
    internal static class NugetDependencyExtensions
    {
        private static bool IsOfInterest(this INugetDependency package, ISolutionDependencyTree tree)
        {

            if (tree.Filter.Contains(package.Package.Id))
            {
                return true;
            }

            return package.FoundDependencies.Any(x => x.IsOfInterest(tree));
        }


        internal static IEnumerable<INugetDependency> GetPackagesThatMatchFilter(this List<INugetDependency> packages, ISolutionDependencyTree tree)
        {

            //either we don't have a filter, it is directly something I'm interested in
            // or one of its subdependencies is
            if (tree.Filter == null || tree.Filter.Length == 0)
            {
                return packages;
            }

            return packages.Where(p => p.IsOfInterest(tree));
        }
    }
    internal static class SolutionDependencyTreeExtensions
    {

        internal static IEnumerable<NugetRelationship> GetNugetToNugetRelationships(this ISolutionDependencyTree tree)
        {
            //get all the child of this package
            IEnumerable<NugetRelationship> GetChildren(INugetDependency package)
            {
                var toReturn = new List<NugetRelationship>();
                var subpackages = package.FoundDependencies.ToList().GetPackagesThatMatchFilter(tree).ToList();
                foreach (var p in subpackages)
                {
                    toReturn.Add(new NugetRelationship { Source = package, Target = p });
                }

                foreach (var dependency in subpackages)
                {
                    GetChildren(dependency);
                }
                return toReturn;
            }

            List<NugetRelationship> x = new List<NugetRelationship>();
            //get all the packages of this tree
            foreach (var proj in tree.Projects)
            {
                var DependenciesToChase = proj.NugetDependencyTree.Packages.GetPackagesThatMatchFilter(tree);
                x.AddRange(DependenciesToChase.SelectMany(GetChildren));
            }

            //and finally de-duplicate
            var groups = x.GroupBy(r => $"{r.Source.Package.Id}{r.Target.Package.Id}");

            return groups.Select(g => g.First());
        }

        internal static List<ProjectNugetRelationship> GetProjectToNugetRelationships(this ISolutionDependencyTree tree)
        {
            var toReturn = new List<ProjectNugetRelationship>();
            foreach (var proj in tree.Projects)
            {
                var packages = proj.NugetDependencyTree.Packages.GetPackagesThatMatchFilter(tree);
                var transform = packages.Select(nug => new ProjectNugetRelationship { Source = proj, Target = nug });
                toReturn.AddRange(transform);
            }
            return toReturn;
        }

        internal static List<ProjectProjectRelationship> GetProjectToProjectRelationships(
            this ISolutionDependencyTree tree)
        {
            return tree.Projects.SelectMany(p =>
                p.ProjectDependencyTree.References.Select(proj => new ProjectProjectRelationship
                { Source = p, Target = proj })).ToList();

        }

        public class NugetRelationship
        {
            public INugetDependency Source { get; set; }
            public INugetDependency Target { get; set; }

        }
        public class ProjectNugetRelationship
        {
            public IProjectInSolution Source { get; set; }
            public INugetDependency Target { get; set; }

        }

        internal class ProjectProjectRelationship
        {
            public IProjectInSolution Source { get; set; }
            public IProjectDependency Target { get; set; }
        }
    }
}