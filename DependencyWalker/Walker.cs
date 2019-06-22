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
        private ObjectCache cache = System.Runtime.Caching.MemoryCache.Default;

        public Walker(string solutionToAnalyse, List<IPackageRepository> packageRepositories, bool prerelease)
        {
            this.solutionToAnalyse = solutionToAnalyse;
            this.packageRepositories = packageRepositories;
            this.prerelease = prerelease;

        }

        public List<Project> Load()
        {
            List<Project> projects = new List<Project>();

            using (var sln = new Sln(solutionToAnalyse, SlnItems.Projects))
            {
                var projectCollection = new ProjectCollection();

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
            foreach (PackageReference packageReference in tree.Source.GetPackageReferences())
            {

                var package = FindPackage(packageReference.Id, packageReference.Version);
                if (package == null)
                {
                    WarnDependencyNotFound(packageReference);
                    continue;
                }
                tree.AddRoot(package);
            }


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

            foreach (PackageDependency dependency in dependencies)
            {
                IPackage subPackage = ResolveDependency(dependency);
                if (subPackage != null)
                {
                    var subDependency = package.AddDependency(subPackage);
                    Walk(subDependency, level + 1);
                }
                else
                {
                    WarnDependencyNotFound(dependency);
                    package.AddDependency(dependency);
                }

            }

        }

        //interrogating Nuget feeds in parallel can result in the same package being interrogated (from two different projects) at hte same time
        //this causes an error in Nuget
        //to prevent this, use a cache (which isn't a terrible idea so we stop thrashing the server!)
        private IPackage FindPackage(string id, SemanticVersion version)
        {
            //check the cache first
            var package = cache[$"{id}-{version}"] as IPackage;

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
                        return FindPackage(id, version);
                    }

                    //we found it, don't need to keep looking
                    if (package != null)
                    {
                        //cache it thanking you
                        cache[$"{id}-{version}"] = package;
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
            foreach (var repository in packageRepositories)
            {
                var package = repository.ResolveDependency(dependency, prerelease, true);
                //we found it, don't need to keep looking
                if (package != null) return package;

                //fallback to other nuget locations and see if it's there instead                
            }
            //if we got here then we didn't find the package
            return null;
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
