using System;
using System.Collections.Generic;
using Xunit;
using DependencyWalker;
using NuGet;
using DependencyWalker.Model;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;

namespace DependencyWalkerTests
{

    public class OutputFixture : IDisposable
    {
        public OutputFixture()
        {
            string[] PackageSources = {"https://www.nuget.org/api/v2/" };
            List<IPackageRepository> PackageRepositories = new List<IPackageRepository>();
            foreach (var uri in PackageSources)
            {
                PackageRepositories.Add(PackageRepositoryFactory.Default.CreateRepository(uri));
            }
            var walker = new Walker(@"..\..\..\TestSolution\TestSolution.sln", PackageRepositories, false);  
            walker.Load();
            Tree = walker.Walk();


             SerializedTree = Printer.SerializeToJSON(Tree);
        }

        public ISolutionDependencyTree Tree { get; }
        public string SerializedTree {get; }

        public void Dispose()
        {
        }
    }

    public class OutputTests : IClassFixture<OutputFixture>
    {
        private readonly OutputFixture fixture;

        public OutputTests(OutputFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void SerialisesToJSON()
        {
           Assert.NotNull(fixture.SerializedTree);
           Assert.False(String.IsNullOrWhiteSpace(fixture.SerializedTree));
        }

        [Fact]
        public void MeetsJSONSchema()
        {
            JSchema schema = JSchema.Parse(File.ReadAllText("tree-json-schema.json"));
            JObject tree = JObject.Parse(fixture.SerializedTree);

            tree.IsValid(schema);
        }


        [Fact]
        public void DoesNotSerialiseEmptyCollections()
        {
            var tree = JObject.Parse(fixture.SerializedTree);
            var collection = tree["Projects"][0]["NugetDependencyTree"]["Packages"][0]["FoundDependencies"];
            Assert.Null(collection);
            
        }
        
        [Fact]
        public void EmptyCollectionsAreInitialisedOnDeserialize()
        {
            const string json = @"{}";

            var obj = JsonConvert.DeserializeObject<SolutionDependencyTree>(json);
            Assert.NotNull(obj.Projects);
            Assert.Empty(obj.Projects);
        }
    }
}
