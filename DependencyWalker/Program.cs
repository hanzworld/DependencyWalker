using NuGet;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using Microsoft.Extensions.DependencyInjection;
using net.r_eg.MvsSln;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace DependencyWalker
{


    public class Program
    {        
        static readonly string[] PackageSources = {"https://www.nuget.org/api/v2/"};
        private static List<IPackageRepository> PackageRepositories;
        private static readonly string[] DependenciesOfInterest = { "Newtonsoft.Json" };
        private const string SolutionToAnalyse = @"C:\dev\project\Something.sln";        
        private const bool PreRelease = false;
        private static ServiceProvider ServiceProvider;
        public static void Main()
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

        private static void Run()
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