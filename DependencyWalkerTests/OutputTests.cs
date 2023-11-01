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
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;
using Xunit.Abstractions;

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
            var walker = new Walker(Path.GetFullPath(@"..\..\..\TestSolution\TestSolution.sln"), PackageRepositories, false);  
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
        private readonly ITestOutputHelper testOutputHelper;

        public OutputTests(ITestOutputHelper testOutputHelper, OutputFixture fixture)
        {
            this.fixture = fixture;
            this.testOutputHelper = testOutputHelper;
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

            Assert.True(tree.IsValid(schema));
        }


        [Fact]
        public void DoesNotSerialiseEmptyCollections()
        {
            var tree = JObject.Parse(fixture.SerializedTree);
            testOutputHelper.WriteLine(tree.ToString());
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
