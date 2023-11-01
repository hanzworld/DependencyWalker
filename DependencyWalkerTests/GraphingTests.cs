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
using Xunit;
using DependencyWalker;
using NuGet;
using DependencyWalker.Model;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;

namespace DependencyWalkerTests
{

    public class GraphFixture : IDisposable
    {
        public GraphFixture()
        {
            string[] PackageSources = {"https://www.nuget.org/api/v2/" };
            List<IPackageRepository> PackageRepositories = new List<IPackageRepository>();
            foreach (var uri in PackageSources)
            {
                PackageRepositories.Add(PackageRepositoryFactory.Default.CreateRepository(uri));
            }
            var walker = new Walker(Path.GetFullPath(@"..\..\..\TestSolutions\TestSolution.sln"), PackageRepositories, false);  
            walker.Load();
            Tree = walker.Walk();


            Graph = Grapher.GenerateDGML(Tree);
        }

        public ISolutionDependencyTree Tree { get; }
        public XDocument Graph {get; }

        public void Dispose()
        {
        }
    }

    public class GraphTests : IClassFixture<GraphFixture>
    {
        private readonly GraphFixture fixture;
        private readonly XNamespace dgmlNamespace;
        private readonly IEnumerable<XElement> nodes;
        private readonly IEnumerable<XElement> links;

        public GraphTests(GraphFixture fixture)
        {
            this.fixture = fixture;
            this.dgmlNamespace = fixture.Graph.Root.Name.Namespace;
            this.nodes = fixture.Graph.Root.Descendants(dgmlNamespace.GetName("Node"));
            this.links = fixture.Graph.Root.Descendants(dgmlNamespace.GetName("Link"));
        }

        [Fact]
        public void ProducesGraph()
        {
           Assert.NotNull(fixture.Graph);
        }

        [Fact]
        public void ConvertsAllProjectsToNodes()
        {
            var projects = nodes
                .Where(d => d.Attribute("Category").Value == "Project")
                .Select(e => e.Attribute("Id").Value).ToList();

            Assert.Equal(5, projects.Count());
            Assert.Contains("CentralPackageManagementProject", projects);
            Assert.Contains("FullFramework1", projects);
            Assert.Contains("FullFramework2", projects);
            Assert.Contains("FullFramework3", projects);
            Assert.Contains("PackageReferenceProject", projects);
        }

        [Fact]
        public void ConvertsAllPackagesToNodes()
        {
            var packages = nodes
                .Where(d => d.Attribute("Category").Value == "Package")
                .Select(e => e.Attribute("Id").Value).ToList();

            Assert.Equal(9, packages.Count());
            Assert.Contains("Newtonsoft.Json", packages);
            Assert.Contains("RestSharp", packages);
            Assert.Contains("ExcelDataReader", packages);
            Assert.Contains("SharpZipLib", packages);
            Assert.Contains("Serilog", packages);
            Assert.Contains("AutoMapper", packages);
        }

        [Fact]
        public void ContainsProjectToProjectLinks()
        {
            var projectLinks = links
                .Where(d => d.Attribute("Category").Value == "Project Reference")
                .Select(p => (p.Attribute("Source").Value, p.Attribute("Target").Value));

            Assert.Equal(2, projectLinks.Count());
            Assert.Contains(("FullFramework2", "FullFramework1"), projectLinks);
            Assert.Contains(("FullFramework3", "FullFramework2"), projectLinks);

        }

        [Fact]
        public void ContainsProjectToNugetLinks()
        {
            var packageLinks = links
                .Where(d => d.Attribute("Category").Value == "Package Reference")
                .Select(p => (p.Attribute("Source").Value, p.Attribute("Target").Value));

            Assert.Equal(7, packageLinks.Count());
            Assert.Contains(("CentralPackageManagementProject", "Serilog"), packageLinks);
            Assert.Contains(("FullFramework1", "Newtonsoft.Json"), packageLinks);
            Assert.Contains(("FullFramework1", "RestSharp"), packageLinks);
            Assert.Contains(("FullFramework2", "Newtonsoft.Json"), packageLinks);
            Assert.Contains(("FullFramework3", "ExcelDataReader"), packageLinks);
            Assert.Contains(("FullFramework3", "SharpZipLib"), packageLinks);
            Assert.Contains(("PackageReferenceProject", "AutoMapper"), packageLinks);
        }

        [Fact]
        public void ContainsNugetToNugetLinks()
        {
            var transitiveLinks = links
                .Where(d => d.Attribute("Category").Value == "Transitive Dependency")
                .Select(p => (p.Attribute("Source").Value, p.Attribute("Target").Value)).ToList();

            Assert.Equal(2, transitiveLinks.Count());
            Assert.Contains(("ExcelDataReader", "SharpZipLib"), transitiveLinks);
            Assert.Contains(("Serilog", "System.ValueTuple"), transitiveLinks);
        }

    }
}
