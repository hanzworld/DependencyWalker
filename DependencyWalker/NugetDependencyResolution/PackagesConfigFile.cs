using System.Collections.Generic;
using NuGet;

namespace DependencyWalker.NugetDependencyResolution
{
    public class PackagesConfigFile: IFileWithNugetDependencies
    {
        private readonly PackageReferenceFile packageReferenceFile;

        public PackagesConfigFile(string path)
        {
            packageReferenceFile = new PackageReferenceFile(path);
        }
        
        public IEnumerable<PackageReference> GetPackageReferences()
        {
            return packageReferenceFile.GetPackageReferences();
        }
    }
}