using NuGet;
using Serilog;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace DependencyWalker
{


    public class Program
    {
        static readonly string[] PackageSources = {"https://www.nuget.org/api/v2/"};
        private static List<IPackageRepository> PackageRepositories;
        private static readonly string[] DependenciesOfInterest = { "Newtonsoft.Json" };
        private const bool PreRelease = false;
        private static ServiceProvider ServiceProvider;

        public static int Main(string[] args)
            => CommandLineApplication.Execute<Program>(args);

        [Required]
        [FileExists]
        [Option(Description = "Required. Path to a .sln file to analyse", ShortName = "s", LongName = "solution")]
        public string SolutionToAnalyse { get; }

        [SuppressMessage("ReSharper", "UnusedMember.Local",
            Justification = @"This method is invoked through reflection by CommandLineApplication.Execute. 
            Resharper is wrong, the application will break if you delete this!")]
#pragma warning disable IDE0051 // See message above
        private void OnExecute()
#pragma warning restore IDE0051 // See message above
        {
            //some packages are on our private nuget server and some are on the public nuget server
            //in order to be able to traverse down a full dependency graph, we need to be able to talk to both
            PackageRepositories = new List<IPackageRepository>();
            foreach (var uri in PackageSources)
            {
                PackageRepositories.Add(PackageRepositoryFactory.Default.CreateRepository(uri));
            }

            //setup the logging file
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
                .WriteTo.Console()
                .CreateLogger();

            //setup DI
            ServiceProvider = new ServiceCollection()
                .AddSingleton<List<IPackageRepository>>(PackageRepositories)
                .AddSingleton<IWalker>(w => new Walker(SolutionToAnalyse, PackageRepositories, PreRelease))
                .BuildServiceProvider();



            //do actual work
            Run();

            //close off the logger
            Log.CloseAndFlush();

            //finish the console
            Console.WriteLine();
            Console.WriteLine("Press Enter...");
            Console.ReadLine();
        }

        private void Run()
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
    }
}