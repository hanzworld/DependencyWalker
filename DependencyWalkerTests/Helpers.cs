/* DEPENDENCY WALKER
 * Copyright (c) 2019 Gray Barn Limited. All Rights Reserved.
 *
 * This library is free software: you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library.  If not, see
 * <https://www.gnu.org/licenses/>.
 */
using System.Collections.Concurrent;
using DependencyWalker.Model;
using Moq;
using System.Collections.Generic;
using System.Linq;

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


        private static IEnumerable<IProjectInSolution> CreateProjects(IEnumerable<TestData> data)
        {
            foreach (var row in data)
            {
                Mock<IProjectInSolution> project = new Mock<IProjectInSolution>();
                project.Setup(p => p.Name).Returns(row.Project);
                project.Setup(p => p.NugetDependencyTree.Packages).Returns(CreatePackages(row.Dependencies, 0).ToList());
                yield return project.Object;
            }
        }


        private static ConcurrentBag<INugetDependency> CreatePackages(IReadOnlyList<string[]> packages, int depth)
        {
            var toReturn = new ConcurrentBag<INugetDependency>();

            if (depth >= packages.Count) return toReturn;

            foreach (var package in packages[depth])
            {
                Mock<INugetDependency> info = new Mock<INugetDependency>();
                info.Setup(i => i.Package.Id).Returns(package);
                info.Setup(i => i.FoundDependencies).Returns(CreatePackages(packages, depth + 1));
                toReturn.Add(info.Object);
            }

            return toReturn;
        }
    }
}
