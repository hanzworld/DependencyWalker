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
using System.Linq;
using Xunit;
using DependencyWalker;
using DependencyWalker.Model;
using static DependencyWalkerTests.Helpers;

namespace DependencyWalkerTests
{
    public class ExtensionFixture : IDisposable
    {
        public ExtensionFixture()
        {
            Data = new []
            {
                new TestData("Project1", new[] {"Newtonsoft.Json"}),
                new TestData("Project2", new[] {"RestSharp"}),
                new TestData("Project3", new[] {"RestSharp", "Newtonsoft.Json"})
            };

            Tree = CreateMeAMockDependencyTree(Data).Object;
        }

        public TestData[] Data { get; }
        public ISolutionDependencyTree Tree { get; }



        public void Dispose()
        {
        }
    }


    public class DenormalisingRelationshipsTests : IClassFixture<ExtensionFixture>
    {
        readonly ExtensionFixture fixture;

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
            var pn = fixture.Tree.GetNugetToNugetRelationships().ToList();
            foreach (var row in fixture.Data)
            {
                for (int depth = 0; depth < row.Dependencies.Length; depth++)
                {
                    for (int parentpackage = 0; parentpackage < row.Dependencies[depth].Length; parentpackage++)
                    {
                        if (depth + 1 >= row.Dependencies.Length)
                        {
                            continue;
                        }

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

