using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NuGet;


namespace DependencyWalker.NugetDependencyResolution
{
    public class CpmPropsFile: IFileWithNugetDependencies
    {
        private readonly XElement root;
        
        public CpmPropsFile(string path)
        {
            root = XElement.Load(path);
        }
        public IEnumerable<PackageReference> GetPackageReferences()
        {
            return root.Descendants("PackageVersion")
                .Select(CreatePackageReference);
        }

        private PackageReference CreatePackageReference(XElement xElement)
        {
            var packageId = xElement.Attribute("Include")?.Value;
            var rawVersion = xElement.Attribute("Version")?.Value;
            return new PackageReference(packageId, SemanticVersion.Parse(rawVersion), null, null, false);
        }
    }
}