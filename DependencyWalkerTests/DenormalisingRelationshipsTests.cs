using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using DependencyWalker;
using DependencyWalker.Model;
using Moq;
using static DependencyWalkerTests.Helpers;

namespace DependencyWalkerTests
{
    public class ExtensionFixture : IDisposable
    {
        public ExtensionFixture()
        {
            Data = new TestData[]
            {
                new TestData("Project1", new[] {"Newtonsoft.Json"}),
                new TestData("Project2", new[] {"RestSharp"}),
                new TestData("Project3", new[] {"RestSharp", "Newtonsoft.Json"})
            };

            Tree = Helpers.CreateMeAMockDependencyTree(Data).Object;
        }

        public TestData[] Data { get; }
        public ISolutionDependencyTree Tree { get; }



        public void Dispose()
        {
        }
    }


    public class DenormalisingRelationshipsTests : IClassFixture<ExtensionFixture>
    {
        ExtensionFixture fixture;

        public DenormalisingRelationshipsTests(ExtensionFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void FlattensProjectToNugetRelationships()
        {
            var pn = fixture.Tree.GetProjectToNugetRelationships();
            foreach (var row in fixture.Data)
            {
                foreach (var package in row.Dependencies[0])
                {
                    Assert.Contains(pn, n => n.Source.Name == row.Project && n.Target.Package.Id == package);
                }

            }

        }


        [Fact]
        public void FlattensNugetToNugetRelationships()
        {
            var pn = fixture.Tree.GetNugetToNugetRelationships();
            foreach (var row in fixture.Data)
            {
                for (int depth = 0; depth < row.Dependencies.Length; depth++)
                {
                    for (int parentpackage = 0; parentpackage < row.Dependencies[depth].Length; parentpackage++)
                    {
                        if (depth + 1 < row.Dependencies.Length)
                        {
                            for (int childpackage = 0; childpackage < row.Dependencies[depth + 1].Length; childpackage++)
                            {
                                Assert.Contains(pn, n => n.Source.Package.Id == row.Dependencies[depth][parentpackage] && n.Target.Package.Id == row.Dependencies[depth + 1][childpackage]);
                            }
                        }
                    }
                }
            }

        }

    }




}

