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
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using DependencyWalker;
using NuGet;

namespace DependencyWalkerTests
{

    public class WalkerFixture : IDisposable
    {
        public WalkerFixture()
        {
            string[] PackageSources = {"https://www.nuget.org/api/v2/" };
            List<IPackageRepository> PackageRepositories = new List<IPackageRepository>();
            foreach (var uri in PackageSources)
            {
                PackageRepositories.Add(PackageRepositoryFactory.Default.CreateRepository(uri));
            }
            WalkerUnderTest = new Walker(System.IO.Path.GetFullPath(@"..\..\..\TestSolution\TestSolution.sln"), PackageRepositories, false);  

        }

        public Walker WalkerUnderTest { get; }

        public void Dispose()
        {
        }
    }

    public class WalkerTests : IClassFixture<WalkerFixture>
    {
        private readonly WalkerFixture fixture;

        public WalkerTests(WalkerFixture fixture)
        {
            this.fixture = fixture;
        }
        [Fact]
        public void FindsProjects()
        {
            var projects = fixture.WalkerUnderTest.Load();
            Assert.Equal(3, projects.Count);
        }

        [Fact]
        public void FindsNugetSubDependencies()
        {
            
            var tree = fixture.WalkerUnderTest.Walk();

            var project3dependencies = tree.Projects.Single(p => p.Name.Contains("FullFramework3")).NugetDependencyTree.Packages;

            Assert.Contains(project3dependencies, p => p.Package.Id == "ExcelDataReader");
            Assert.NotEmpty(project3dependencies.First(p => p.Package.Id == "ExcelDataReader").FoundDependencies);
        }


        [Fact]
        public void FindsNugetDependencies()
        {
            
            var tree = fixture.WalkerUnderTest.Walk();

            var project1dependencies = tree.Projects.Single(p => p.Name.Contains("FullFramework1")).NugetDependencyTree.Packages;
            var project2dependencies = tree.Projects.Single(p => p.Name.Contains("FullFramework2")).NugetDependencyTree.Packages;
            var project3dependencies = tree.Projects.Single(p => p.Name.Contains("FullFramework3")).NugetDependencyTree.Packages;

            Assert.Contains(project1dependencies, p => p.Package.Id == "Newtonsoft.Json");
            Assert.Contains(project1dependencies, p => p.Package.Id == "RestSharp");
            
            Assert.Contains(project2dependencies, p => p.Package.Id == "Newtonsoft.Json");

            Assert.Contains(project3dependencies, p => p.Package.Id == "ExcelDataReader");
            Assert.Contains(project3dependencies, p => p.Package.Id == "SharpZipLib");
        }

        [Fact]
        public void FindsProjectDependencies()
        {

            var tree = fixture.WalkerUnderTest.Walk();

            var project1dependencies = tree.Projects.Single(p => p.Name.Contains("FullFramework1")).ProjectDependencyTree.References;
            var project2dependencies = tree.Projects.Single(p => p.Name.Contains("FullFramework2")).ProjectDependencyTree.References;
            var project3dependencies = tree.Projects.Single(p => p.Name.Contains("FullFramework3")).ProjectDependencyTree.References;

            Assert.Empty(project1dependencies);
            Assert.Contains(project2dependencies, p => p.Name == "FullFramework1");
            Assert.Contains(project3dependencies, p => p.Name == "FullFramework2");
        }
    }
}
