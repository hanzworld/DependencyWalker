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
                new TestData("Project2", new [] {"RestSharp"}, new [] { "Subdependency"}),
                new TestData("Project3", new[] {"RestSharp", "Newtonsoft.Json"}, new [] {"Subdependency"}, new [] { "Subsubdependency"})
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
            fixture.Tree.Filter = new[] { "Subdependency" };
            var pn = fixture.Tree.GetNugetToNugetRelationships().ToList();

            Assert.Equal(2, pn.Count);

            Assert.Contains(pn, n => n.Source.Package.Id == fixture.Data[1].Dependencies[0][0] && n.Target.Package.Id ==  fixture.Data[1].Dependencies[1][0]);
            Assert.Contains(pn, n => n.Source.Package.Id == fixture.Data[2].Dependencies[0][0] && n.Target.Package.Id == fixture.Data[2].Dependencies[1][0]);
            Assert.Contains(pn, n => n.Source.Package.Id == fixture.Data[2].Dependencies[0][1] && n.Target.Package.Id == fixture.Data[2].Dependencies[1][0]);
            Assert.DoesNotContain(pn, n => n.Source.Package.Id == fixture.Data[2].Dependencies[1][0] && n.Target.Package.Id == fixture.Data[2].Dependencies[2][0]);

        }
    }
}
