using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Evaluation;
using NuGet;

namespace DependencyWalker.NugetDependencyResolution
{
    public class CSProjFile : IFileWithNugetDependencies
    {

        private readonly Project csproj;
        private readonly CpmPropsFileResolver cpmPropsFileResolver;
        private IEnumerable<PackageReference> CmpPackageReferences
        {
            get
            {
                if (cpmPackageReferences != null) return cpmPackageReferences;
                var pathToPropsFile = cpmPropsFileResolver.FindPropsFile();
                var propsFile = new CpmPropsFile(pathToPropsFile);
                cpmPackageReferences = propsFile.GetPackageReferences();
                return cpmPackageReferences;
            }
        }

        private IEnumerable<PackageReference> cpmPackageReferences;

        public CSProjFile(Project project)
        {
            csproj = project;
            cpmPropsFileResolver = new CpmPropsFileResolver(csproj.DirectoryPath);
        }

        // Searches for package reference elements in a csproj 
        // Such as <PackageReference Include="AutoMapper" Version="10.0.0" />
        // If the Version attribute is missing then it attempts to use Central Package Management to resolve the version 
        public IEnumerable<PackageReference> GetPackageReferences()
        {
            var packageReferences = new List<PackageReference>();
            
            foreach (var projectItem in csproj.Items.Where(IsPackageReference))
            {
                if (IsAbleToResolvePackageAndVersionDirectly(projectItem))
                {
                    packageReferences.Add(CreatePackageReference(projectItem));
                }
                else
                {
                    packageReferences.Add(CreatePackageReferenceWithPropsFile(projectItem));
                }
            }

            return packageReferences;
        }

        private PackageReference CreatePackageReferenceWithPropsFile(ProjectItem packageReference)
        {
            var item = CmpPackageReferences.FirstOrDefault(reference => reference.Id == packageReference.EvaluatedInclude);
            return CreatePackageReference(packageReference.EvaluatedInclude, item.Version);
        }

        private static bool IsAbleToResolvePackageAndVersionDirectly(ProjectItem packageReferenceItem)
        {
            return packageReferenceItem.Metadata.Any(m =>
                m.Name.Equals("Version", StringComparison.OrdinalIgnoreCase));
        }

        public bool HasPackageReferences()
        {
            return csproj.Items.Any(IsPackageReference);
        }
    
        private PackageReference CreatePackageReference(ProjectItem packageReferenceItem)
        {
            var packageId = packageReferenceItem.EvaluatedInclude;
            var version = GetSemanticVersion(packageReferenceItem);
            return CreatePackageReference(packageId, version);
        }
        
        private static PackageReference CreatePackageReference(string packageId, SemanticVersion version)
        {
            return new PackageReference(packageId, version, null, null, false);
        }

        private bool IsPackageReference(ProjectItem item)
        {
            return item.ItemType.Equals("PackageReference", StringComparison.OrdinalIgnoreCase);
        }

        private SemanticVersion GetSemanticVersion(ProjectItem packageReferenceItem)
        {
            var rawVersion = packageReferenceItem.Metadata.FirstOrDefault(m =>
                m.Name.Equals("Version", StringComparison.OrdinalIgnoreCase))?.EvaluatedValue;
            return SemanticVersion.Parse(rawVersion);
        }
    }

}