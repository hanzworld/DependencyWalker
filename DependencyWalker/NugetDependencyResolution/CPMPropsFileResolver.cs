using System;
using System.IO;
using Serilog;

namespace DependencyWalker.NugetDependencyResolution
{
    public class CpmPropsFileResolver
    {
        private readonly string startingPath;
        private const string CpmFileName = "Directory.Packages.props";
        
        public CpmPropsFileResolver(string startingPath)
        {
            this.startingPath = startingPath;
        }

        public string FindPropsFile()
        {
            return FindPropsFile(startingPath);
        }

        /// <summary>
        /// Starts at the path of the cs proj, and checks if the CPM file is defined.
        /// If not it recursively walks up directories until it finds CPM file
        /// This should roughly match the approximation defined here https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management#central-package-management-rules
        /// </summary>
        /// <param name="path">Path that might contain a cpm file</param>
        /// <param name="depth">Current amount of directories we have walked up</param>
        /// <param name="maxDepth">Amount of directories to walk before throwing an exception</param>
        /// <returns>path to a central package management file</returns>
        /// <exception cref="Exception"></exception>
        private static string FindPropsFile(string path, int depth=0, int maxDepth = 5)
        {
            if (depth > maxDepth)
            {
                throw new Exception($"Could not find path to file {CpmFileName}. Final check was at {path}. Exceeded max depth of {maxDepth}");
            }
            
            if (ExistsInDirectory(path))
            {
                Log.Debug("Found CPM file at {path}", GetPathToCpmFile(path));
                return GetPathToCpmFile(path);
            }

            var parentDirectory = Directory.GetParent(path);
            return FindPropsFile(parentDirectory.FullName, depth+1);
        }

        private static bool ExistsInDirectory(string path)
        {
            var potentialPathToCpmFile = GetPathToCpmFile(path); 
            return File.Exists(potentialPathToCpmFile);
        }

        private static string GetPathToCpmFile(string path)
        {
            return Path.Combine(path, CpmFileName);
        }
        
        
    }
}