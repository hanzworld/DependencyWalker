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
using System.Diagnostics;
using System.IO;
using Xunit;
using DependencyWalker;
using DependencyWalker.Model;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using NuGet;

namespace DependencyWalkerTests
{

    public class InputFixture : IDisposable
    {
        public InputFixture ()
        {
            Tree = Reader.Read(Source);
            Graph = Grapher.GenerateDGML(Tree);
        }

        public ISolutionDependencyTree Tree { get; }
        public XDocument Graph {get; }
        public string Source { get; } = "test-tree.json";

        public void Dispose()
        {
        }
    }

    public class InputTests : IClassFixture<InputFixture>
    {
        private readonly InputFixture fixture;

        public InputTests(InputFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void ReadAllProjects()
        {
           Assert.Equal(11, fixture.Tree.Projects.Count);
        }

        [Fact]
        public void ReadProjectToNugetRelationships()
        {
            Assert.Equal(120, fixture.Tree.Projects.First(p => p.Name == "App.Business").NugetDependencyTree.Packages.Count);
        }

        [Fact]
        public void ReadProjectToProjectRelationships()
        {
            Assert.Equal(6, fixture.Tree.Projects.First(p => p.Name == "App.Business").ProjectDependencyTree.References.Count);
        }

        [Fact]
        public void ReadUnresolvedDependenciesAccurately()
        {
            var UnresolvedDependencies = fixture.Tree.Projects.First(p => p.Name == "App.Frontend.Assets")
                .NugetDependencyTree
                .Packages.First(pa => pa.Package.Id == "App.Logging")
                .UnresolvedDependencies;

            Assert.Single(UnresolvedDependencies);
        }

        [Fact]
        public void ReadVersionSpecAccurately()
        {
            var anUnresolvedDependency = fixture.Tree.Projects.First(p => p.Name == "App.Frontend.Assets")
                .NugetDependencyTree
                .Packages.First(pa => pa.Package.Id == "App.Logging")
                .UnresolvedDependencies.First();

            Assert.Equal(new SemanticVersion(1,0,0,0), anUnresolvedDependency.VersionSpec.MinVersion);
            Assert.True(anUnresolvedDependency.VersionSpec.IsMinInclusive);
            Assert.Null(anUnresolvedDependency.Include);
            Assert.Null(anUnresolvedDependency.Exclude);

        }

        [Fact]
        public void ReadsNugetToNugetRelationships()
        {
            Assert.Equal(14, fixture.Tree.Projects.First(p => p.Name == "App.ZooManagement.BackChannel").NugetDependencyTree.Packages[0].FoundDependencies.Count);
        }

    }
}
