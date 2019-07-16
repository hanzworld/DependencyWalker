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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using DependencyWalker.Model;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using NuGet;
using Serilog;
using ProjectInSolution = DependencyWalker.Model.ProjectInSolution;

namespace DependencyWalker
{

    public class Walker : IWalker
    {
        private readonly string solutionToAnalyse;
        private readonly List<IPackageRepository> packageRepositories;
        private readonly bool prerelease;
        private readonly ObjectCache packageCache = System.Runtime.Caching.MemoryCache.Default;
        private readonly ObjectCache dependencyCache = System.Runtime.Caching.MemoryCache.Default;
        private readonly HashSet<string> unavailablePackageCache = new HashSet<string>();
        private int NumberOfCollisions;

        public int ShortCircuitResolveDependency { get; private set; }
        public int DependencyCacheHit { get; private set; }
        public int PackageCacheHit { get; private set; }

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

            var sln = SolutionFile.Parse(solutionToAnalyse);
            {

                foreach (var p in sln.ProjectsInOrder.Where(p => p.ProjectType == SolutionProjectType.KnownToBeMSBuildFormat))
                {
                    var project = projectCollection.LoadProject(p.AbsolutePath);
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
                Log.Information($"So far, had {NumberOfCollisions} collisions.");
                Log.Information($"So far, had {ShortCircuitResolveDependency} times we could short circuit ResolveDependency.");
                Log.Information($"So far, had {PackageCacheHit} package cache hits.");
                Log.Information($"So far, had {DependencyCacheHit} dependency cache hits.");
            }
            );

            var masterTree = new SolutionDependencyTree(solutionToAnalyse);
            masterTree.Projects.AddRange(collection.ToList());
            return masterTree;
        }

        private static IProjectDependencyTree WalkProjects(Project project)
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
            var tree = new NugetDependencyTree(new DirectoryInfo(project.DirectoryPath));
            //find the first level of packages
            ConcurrentBag<IPackage> collection = new ConcurrentBag<IPackage>();
            {
                Parallel.ForEach(tree.Source.GetPackageReferences(), reference =>
                {
                    try
                    {
                        var package = FindPackage(reference.Id, reference.Version);
                        collection.Add(package);
                    }
                    catch (UnableToRetrievePackageException e)
                    {
                        Log.Error(e.Message);
                    }

                });
            }

            tree.AddRoots(collection.ToList());

            Log.Debug($"Found {tree.Packages.Count} root packages for {project.GetProjectName()}");


            //now iterate down the tree
            foreach (var package in tree.Packages)
            {
                Walk(package);
            }

            Log.Debug($"Finished walking packages for {project.GetProjectName()}");
            Log.Debug($"Cache has {packageCache.GetCount()} items");

            return tree;
        }

        private void Walk(INugetDependency package)
        {
            var dependencies = package.Package.DependencySets.FirstOrDefault()?.Dependencies;
            if (dependencies == null)
            {
                return;
            }

            Parallel.ForEach(dependencies, dependency =>
            {
                try
                {
                    IPackage subPackage = ResolveDependency(dependency);
                    var subDependency = new NugetDependency(subPackage);
                    Walk(subDependency);
                    package.FoundDependencies.Add(subDependency);
                }
                catch (UnableToResolvePackageDependencyException e)
                {
                    Log.Error(e.Message);
                    package.UnresolvedDependencies.Add(dependency);
                }
                catch (UnableToRetrievePackageException e)
                {
                    Log.Error(e.Message);
                    package.UnresolvedDependencies.Add(dependency);
                }
                catch (ShortCircuitingResolveDependencyException e)
                {
                    Log.Error(e.Message);
                    package.UnresolvedDependencies.Add(dependency);
                }
            });

        }

        /// <summary>
        /// Calls Nugets own FindPackage to retrieve the metadata of a specific version of a package
        /// including dependency information. Replicates what would occur when called `nuget restore`,
        /// but with added caching and performance optimisations in front of it.
        /// </summary>
        /// <param name="id">The Nuget ID of the package you want to retrieve</param>
        /// <param name="version">The Nuget Version of the package you want to retrieve</param>
        /// <returns></returns>
        /// <exception cref="UnableToRetrievePackageException"></exception>
        private IPackage FindPackage(string id, SemanticVersion version)
        {

            var uniqueidentifier = $"{id}-{version}";

            //interrogating Nuget feeds in parallel can result in the same package being interrogated (from two different projects) at hte same time
            //this causes an error in Nuget
            //to prevent this, use a cache (which isn't a terrible idea so we stop thrashing the server!)
            //check the cache first

            if (packageCache.Contains(uniqueidentifier))
            {
                PackageCacheHit++;
                var package = packageCache[uniqueidentifier] as IPackage;
                return package;

            }

            if (unavailablePackageCache.Contains(uniqueidentifier))
            {
                //we've tried to retrieve this before and failed, don't bother trying again
                    throw new UnableToRetrievePackageException(id, version);
            }

            //otherwise ask one of our many Nuget servers

            foreach (var repository in packageRepositories)
            {
                IPackage package;
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

                //if we found it, don't need to keep looking through other servers
                if (package == null)
                {
                    continue;
                }

                //cache it thanking you
                packageCache[uniqueidentifier] = package;
                return package;

                //fallback to other nuget locations and see if it's there instead
            }

            //if we got here then we didn't find the package
            unavailablePackageCache.Add(uniqueidentifier);
            throw new UnableToRetrievePackageException(id, version);
        }

        /// <summary>
        /// Calls Nuget's own ResolveDependency to find the exact package which will satisfy the dependency
        /// specifications. Replicates what would occur when called `nuget restore`, but with added
        /// caching and performance optimisations in front of it.
        /// </summary>
        /// <param name="dependency">A definition of a dependency, including a version specification</param>
        /// <returns></returns>
        /// <exception cref="ShortCircuitingResolveDependencyException"></exception>
        /// <exception cref="UnableToResolvePackageDependencyException"></exception>
        /// <exception cref="UnableToRetrievePackageException"></exception>
        private IPackage ResolveDependency(PackageDependency dependency)
        {
            try
            {
                //this is an expensive lookup as it iterates over all packages with that id
                //if we can jump straight to a specific version, do so
                if (dependency.VersionSpec != null && (dependency.VersionSpec.MaxVersion == null ||
                    dependency.VersionSpec.MaxVersion == dependency.VersionSpec.MinVersion))
                {
                    ShortCircuitResolveDependency++;
                    return FindPackage(dependency.Id, dependency.VersionSpec.MinVersion);
                }
            }
            catch (NullReferenceException)
            {
                throw new ShortCircuitingResolveDependencyException(dependency);
            }

            var uniqueidentifier = dependency.ToString();

            //check the cache first
            var package = dependencyCache[uniqueidentifier] as IPackage;

            //otherwise ask one of our many Nuget servers
            if (package != null)
            {
                DependencyCacheHit++;
                return package;
            }

            foreach (var repository in packageRepositories)
            {
                package = repository.ResolveDependency(dependency, prerelease, true);

                //we found it, don't need to keep looking
                if (package == null)
                {
                    continue;
                }

                //cache it thanking you
                dependencyCache[uniqueidentifier] = package;
                return package;

                //fallback to other nuget locations and see if it's there instead
            }
            //if we got here then we didn't find the package
            throw new UnableToResolvePackageDependencyException(dependency);

        }
    }
}
