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
    public class FilteringFixture : IDisposable
    {
        public FilteringFixture()
        {
            Data = new []
            {
                new TestData("Project1", new [] {"Newtonsoft.Json"}),
                new TestData("Project2", new [] {"RestSharp"}, new [] { "Subdependency1"}),
                new TestData("Project3", new[] {"RestSharp", "Newtonsoft.Json"}, new [] {"Subdependency2"}, new [] { "Subdependency1", "Subdependency3"})
        };

            Tree = CreateMeAMockDependencyTree(Data).Object;
        }

        public TestData[] Data { get; }
        public ISolutionDependencyTree Tree { get; }



        public void Dispose()
        {
        }
    }


    public class FilteringTests : IClassFixture<FilteringFixture>
    {
        readonly FilteringFixture fixture;

        public FilteringTests(FilteringFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void FlattensAndFiltersProjectToNugetRelationships()
        {
            fixture.Tree.Filter = new [] { "Newtonsoft.Json" };
            var pn = fixture.Tree.GetProjectToNugetRelationships();

            Assert.Contains(pn, n => n.Source.Name == fixture.Data[0].Project && n.Target.Package.Id == fixture.Data[0].Dependencies[0][0]);
            Assert.Contains(pn, n => n.Source.Name == fixture.Data[2].Project && n.Target.Package.Id == fixture.Data[2].Dependencies[0][1]);
            //shouldn't have any projects that have no matching dependencies
            Assert.DoesNotContain(pn, n => n.Source.Name == fixture.Data[1].Project);
            //shouldn't have any non matching dependencies
            Assert.DoesNotContain(pn, n => n.Source.Name == fixture.Data[2].Project && n.Target.Package.Id == fixture.Data[2].Dependencies[0][0]);



        }

        [Fact]
        public void FlattensAndFiltersNugetToNugetRelationships()
        {
            fixture.Tree.Filter = new[] { "Subdependency1" };
            var pn = fixture.Tree.GetNugetToNugetRelationships().ToList();

            Assert.Equal(4, pn.Count);

            Assert.Contains(pn, n => n.Source.Package.Id == fixture.Data[1].Dependencies[0][0] && n.Target.Package.Id ==  fixture.Data[1].Dependencies[1][0]);
            Assert.Contains(pn, n => n.Source.Package.Id == fixture.Data[2].Dependencies[1][0] && n.Target.Package.Id == fixture.Data[2].Dependencies[2][0]);
            Assert.DoesNotContain(pn, n => n.Source.Package.Id == fixture.Data[2].Dependencies[1][0] && n.Target.Package.Id == fixture.Data[2].Dependencies[2][1]);

        }
    }
}
