using DependencyWalker.Model;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyWalkerTests
{
    public static class Helpers
    {
        public class TestData
        {
            public string Project;
            public string[][] Dependencies;

            public TestData(string name, params string[][] dependencies)
            {
                Project = name;
                Dependencies = dependencies;
            }

        }


        public static Mock<ISolutionDependencyTree> CreateMeAMockDependencyTree(TestData[] data)
        {
            Mock<ISolutionDependencyTree> tree = new Mock<ISolutionDependencyTree>();
            tree.Setup(t => t.Projects).Returns(() => CreateProjects(data).ToList());
            tree.SetupProperty(t => t.Filter);
            return tree;
        }


        private static IEnumerable<IProjectInSolution> CreateProjects(TestData[] data)
        {
            foreach (var row in data)
            {
                Mock<IProjectInSolution> project = new Mock<IProjectInSolution>();
                project.Setup(p => p.Name).Returns(row.Project);
                project.Setup(p => p.NugetDependencyTree.Packages).Returns(CreatePackages(row.Dependencies, 0).ToList());
                yield return project.Object;
            }
        }


        private static IEnumerable<INugetDependency> CreatePackages(string[][] packages, int depth)
        {
            if (depth >= packages.Length) yield break;

            foreach (var package in packages[depth])
            {
                Mock<INugetDependency> info = new Mock<INugetDependency>();
                info.Setup(i => i.Package.Id).Returns(package);
                info.Setup(i => i.FoundDependencies).Returns(CreatePackages(packages, depth + 1).ToList());
                yield return info.Object;
            }
        }
    }
}
