using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using DependencyWalker.Model;
using Microsoft.Build.Evaluation;
using net.r_eg.MvsSln;
using net.r_eg.MvsSln.Extensions;
using NuGet;
using Serilog;

namespace DependencyWalker
{

    public class Walker : IWalker
    {
        private readonly string solutionToAnalyse;
        private readonly List<IPackageRepository> packageRepositories;
        private readonly bool prerelease;
        private ObjectCache packageCache = System.Runtime.Caching.MemoryCache.Default;
        private ObjectCache dependencyCache = System.Runtime.Caching.MemoryCache.Default;

        public Walker(string solutionToAnalyse, List<IPackageRepository> packageRepositories, bool prerelease)
        {
            this.solutionToAnalyse = solutionToAnalyse;
            this.packageRepositories = packageRepositories;
            this.prerelease = prerelease;

        }

        public List<Project> Load()
        {
            var projectCollection = new ProjectCollection();

            List<Project> projects = new List<Project>();

            using (var sln = new Sln(solutionToAnalyse, SlnItems.Projects))
            {

                foreach (var p in sln.Result.ProjectItems)
                {
                    var project = projectCollection.LoadProject(p.fullPath);
                    projects.Add(project);
                }
            }

            return projects;
        }

        public ISolutionDependencyTree Walk()
        {
            var collection = new ConcurrentBag<ProjectInSolution>();
            Parallel.ForEach(Load(), project =>
            //foreach (var project in Load())
            {
                Log.Debug($"Starting {project.ProjectFileLocation}");
                var newProject = new ProjectInSolution(project.GetProjectName())
                {
                    NugetDependencyTree = WalkNuget(project),
                    ProjectDependencyTree = WalkProjects(project)
                };
                collection.Add(newProject);
            }
            );

            var masterTree = new SolutionDependencyTree(solutionToAnalyse);
            masterTree.Projects.AddRange(collection.ToList());
            return masterTree;
        }

        private IProjectDependencyTree WalkProjects(Project project)
        {
            //create a project dependency tree for that project
            var tree = new ProjectDependencyTree();

            foreach (var reference in project.GetItems("ProjectReference"))
            {
                tree.AddRoot(reference);
            }

            Log.Debug($"Found {tree.References.Count} project references for {project.GetProjectName()}");

            return tree;
        }

        private INugetDependencyTree WalkNuget(Project project)
        {
            //create a Nuget dependency tree for that project
            var tree = new NugetDependencyTree(project.FullPath, new DirectoryInfo(project.DirectoryPath));
            //find the first level of packages
            ConcurrentBag<IPackage> collection = new ConcurrentBag<IPackage>();
            {
                Parallel.ForEach(tree.Source.GetPackageReferences(), reference =>
                {
                    var package = FindPackage(reference.Id, reference.Version);
                    if (package == null)
                    {
                        WarnDependencyNotFound(reference);
                    }
                    else
                    {
                        collection.Add(package);
                    }
                });
            }

            tree.AddRoots(collection.ToList());

            Log.Debug($"Found {tree.Packages.Count} root packages for {project.GetProjectName()}");


            //now iterate down the tree
            foreach (var package in tree.Packages)
            {
                Walk(package, 0);
            }

            Log.Debug($"Finished walking packages for {project.GetProjectName()}");

            return tree;
        }

        private void Walk(INugetDependency package, int level)
        {
            var dependencies = package.Package.DependencySets.FirstOrDefault()?.Dependencies;
            if (dependencies == null)
            {
                return;
            }

            ConcurrentBag<object> results = new ConcurrentBag<object>();

            Parallel.ForEach(dependencies, dependency =>
            {
                IPackage subPackage = ResolveDependency(dependency);
                if (subPackage != null)
                {
                    var subDependency = new NugetDependency(subPackage);
                Walk(subDependency, level + 1);
                    results.Add(subDependency);
                }
                else
                {
                    WarnDependencyNotFound(dependency);
                results.Add(dependency);
                }

            });

            package.AddDependencies(results.ToList());

        }

        //interrogating Nuget feeds in parallel can result in the same package being interrogated (from two different projects) at hte same time
        //this causes an error in Nuget
        //to prevent this, use a cache (which isn't a terrible idea so we stop thrashing the server!)
        private IPackage FindPackage(string id, SemanticVersion version)
        {
            var uniqueidentifier = $"{id}-{version}";

            //check the cache first
            var package = packageCache[uniqueidentifier] as IPackage;

            //otherwise ask one of our many Nuget servers
            if (package == null)
            {
                foreach (var repository in packageRepositories)
                {
                    try
                    {
                        package = repository.FindPackage(id, version, prerelease, true);
                    }
                    catch (InvalidOperationException)
                    {
                        //we ran into a collision due to parallel operations
                        NumberOfCollisions++;
                        return FindPackage(id, version);
                    }

                    //we found it, don't need to keep looking
                    if (package != null)
                    {
                        //cache it thanking you
                        packageCache[uniqueidentifier] = package;
                        return package;
                    }

                    //fallback to other nuget locations and see if it's there instead
                }
                //if we got here then we didn't find the package
                return null;
            }
            return package;
        }

        private IPackage ResolveDependency(PackageDependency dependency)
        {


            try
            {
                //this is an expensive lookup as it iterates over all packages with that id
                //if we can jump straight to a specific version, do so
                if (dependency.VersionSpec != null && (dependency.VersionSpec.MaxVersion == null ||
                    dependency.VersionSpec.MaxVersion == dependency.VersionSpec.MinVersion))
                {
                    return FindPackage(dependency.Id, dependency.VersionSpec.MaxVersion);
                }
            }
            catch (NullReferenceException e)
            {
                Log.Error(e,
                    $"Couldn't resolve dependency. Dependency is {(dependency == null ? "null" : "not null")}. Version is {(dependency.VersionSpec == null ? "null" : "not null")}. {dependency}");
                throw;
            }

            var uniqueidentifier = dependency.ToString();

            //check the cache first
            var package = dependencyCache[uniqueidentifier] as IPackage;

            //otherwise ask one of our many Nuget servers
            if (package == null)
            {
                foreach (var repository in packageRepositories)
                {
                    package = repository.ResolveDependency(dependency, prerelease, true);

                    //we found it, don't need to keep looking
                    if (package != null)
                    {
                        //cache it thanking you
                        dependencyCache[uniqueidentifier] = package;
                        return package;
                    }

                    //fallback to other nuget locations and see if it's there instead
                }
                //if we got here then we didn't find the package
                return null;
            }

            return package;
        }

        private void WarnDependencyNotFound(PackageDependency dependency)
        {
            Log.Warning($"** Dependency {dependency} isn't on this server - did you get it elsewhere?");
        }

        private void WarnDependencyNotFound(PackageReference reference)
        {
            Log.Warning($"** Dependency {reference} isn't on this server - did you get it elsewhere?");
        }
    }
}
