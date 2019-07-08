
using System.Linq;
using System.Xml.Linq;
using DependencyWalker.Model;

namespace DependencyWalker
{
    internal static class Grapher
    {
        private static readonly XNamespace dgmlns = "http://schemas.microsoft.com/vs/2009/dgml";

        internal static void GenerateDGML(ISolutionDependencyTree tree)
        {

            var pn = tree.GetProjectToNugetRelationships().ToList();
            var nn = tree.GetNugetToNugetRelationships().ToList();
            var pp = tree.GetProjectToProjectRelationships().ToList();


            var graph = new XElement(
                dgmlns + "DirectedGraph",
                new XAttribute("GraphDirection", "LeftToRight"),
                new XElement(dgmlns + "Nodes",
                    tree.Projects.Select(p => CreateNode(p.Name, "Project")),
                    nn.Select(r => CreateNode(r.Source.Package.Id, "Package")),
                    nn.Select(r => CreateNode(r.Target.Package.Id, "Package"))
                ),
                new XElement(dgmlns + "Links",
                    pn.Select(r => CreateLink(r.Source.Name, r.Target.Package.Id, "Package Reference")),
                    nn.Select(r => CreateLink(r.Source.Package.Id, r.Target.Package.Id, "Nuget Subdependency")),
                    pp.Select(r => CreateLink(r.Source.Name, r.Target.Name, "Project Reference"))
                ),
                // No need to declare Categories, auto generated
                new XElement(dgmlns + "Styles",
                    CreateStyle("Project", "Blue"),
                    CreateStyle("Package", "Purple")
                 ));

            var doc = new XDocument(graph);
            doc.Save("mydgml.dgml");

        }

        private static XElement CreateNode(string name, string category, string label = null, string group = null)
        {
            var labelAtt = label != null ? new XAttribute("Label", label) : null;
            var groupAtt = group != null ? new XAttribute("Group", group) : null;
            return new XElement(dgmlns + "Node", new XAttribute("Id", name), labelAtt, groupAtt,
                new XAttribute("Category", category));
        }

        private static XElement CreateLink(string source, string target, string category)
        {
            return new XElement(dgmlns + "Link", new XAttribute("Source", source), new XAttribute("Target", target),
                new XAttribute("Category", category));
        }

        private static XElement CreateStyle(string label, string color)
        {
            return new XElement(dgmlns + "Style", new XAttribute("TargetType", "Node"),
                new XAttribute("GroupLabel", label), new XAttribute("ValueLabel", "True"),
                new XElement(dgmlns + "Condition", new XAttribute("Expression", "HasCategory('" + label + "')")),
                new XElement(dgmlns + "Setter", new XAttribute("Property", "Background"),
                    new XAttribute("Value", color)));
        }

    }

}