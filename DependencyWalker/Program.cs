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
using NuGet;
using Serilog;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using DependencyWalker.Model;
using Newtonsoft.Json;

namespace DependencyWalker
{
    [Command("dependencywalker")]
    [Subcommand(typeof(Graph), typeof(Walk))]
    class Program : WalkerCommandBase
    {

        public static int Main(string[] args)
            => CommandLineApplication.Execute<Program>(args);

        protected override void Run()
        {
            throw new NotImplementedException();
        }

        protected override void Configure()
        {
            throw new NotImplementedException();
        }
    }

    [Command("walk")]
    class Walk : WalkerCommandBase
    {

        [Required]
        [FileExists]
        [Option(Description = "Required. Path to a .sln file to analyse", ShortName = "s", LongName = "solution")]
        public string SolutionToAnalyse { get; }

        [Option(Description = "Nuget feeds to use. Defaults to public Nuget feed. Can be specified multiple times.", ShortName = "n", LongName = "nuget-url")]
        [Url]
        [NugetApiEndPoint]
        public string[] PackageSources { get; } = { "https://www.nuget.org/api/v2/" };


        [SuppressMessage("ReSharper", "UnusedMember.Local",
            Justification = @"This method is invoked through reflection by CommandLineApplication.Execute. 
            Resharper is wrong, the application will break if you delete this!")]
#pragma warning disable IDE0051 // See message above
        protected override void Configure()
#pragma warning restore IDE0051 // See message above
        {
            const bool PreRelease = false;

            //some packages are on our private nuget server and some are on the public nuget server
            //in order to be able to traverse down a full dependency graph, we need to be able to talk to both
            List<IPackageRepository> PackageRepositories = new List<IPackageRepository>();
            foreach (var uri in PackageSources)
            {
                PackageRepositories.Add(PackageRepositoryFactory.Default.CreateRepository(uri));
            }

            //setup DI
            ServiceProvider = new ServiceCollection()
                .AddSingleton<List<IPackageRepository>>(PackageRepositories)
                .AddSingleton<IWalker>(w => new Walker(SolutionToAnalyse, PackageRepositories, PreRelease))
                .BuildServiceProvider();
        }

        protected override void Run()
        {
            var walker = ServiceProvider.GetService<IWalker>();

            var tree = walker.Walk();
            tree.Filter = DependenciesOfInterest;

            Parallel.Invoke(
                () =>
                {
                    Printer.PersistToFile(tree);
                },
                () =>
                {
                    Log.Debug("Now generating graph");
                    Grapher.GenerateDGML(tree);
                });
        }

        class NugetApiEndPointAttribute : ValidationAttribute
        {
            public NugetApiEndPointAttribute()
                : base("The URL {0} must be valid Nuget v2 endpoint")
            {
            }

            protected override ValidationResult IsValid(object value, ValidationContext context)
            {
                var repository = PackageRepositoryFactory.Default.CreateRepository(value as string);
                try
                {
                    var searchResults = repository.Search("Newtonsoft", false);
                }
                catch (System.Net.WebException)
                {

                    return new ValidationResult(FormatErrorMessage(context.DisplayName));
                }
                return ValidationResult.Success;
            }
        }
    }

    [Command("graph")]
    class Graph : WalkerCommandBase
    {
        [Required]
        [FileExists]
        [Option(Description = "Required. Path to a .json file from a previous analysis.", ShortName = "i", LongName = "input")]
        public string SourceFile { get; }

        protected override void Configure()
        {

        }

        protected override void Run()
        {
            Log.Debug($"Loading existing dependency analysis from '{SourceFile}'");
            var tree = Reader.Read(SourceFile);
            tree.Filter = DependenciesOfInterest;

            Log.Debug("Now generating graph");
            Grapher.GenerateDGML(tree);
        }
    }

    [HelpOption("--help")]
    abstract class WalkerCommandBase
    {
        [Option(
            Description =
                "Id of a Nuget package you specifically want to find all relationships to. Can be specified multiple times.",
            ShortName = "f", LongName = "filter")]
        public string[] DependenciesOfInterest { get; }

        protected static ServiceProvider ServiceProvider;

        protected abstract void Run();
        protected abstract void Configure();
        protected void OnExecute()
        {
            //setup the logging file
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
                .WriteTo.Console()
                .CreateLogger();

            Configure();

            //do actual work
            Run();

            //close off the logger
            Log.CloseAndFlush();

            //finish the console
            Console.WriteLine();
            Console.WriteLine("Press Enter...");
            Console.ReadLine();
        }
    }
}