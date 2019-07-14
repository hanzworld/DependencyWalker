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
using System.Linq;
using System.Xml.Linq;
using DependencyWalker.Model;

namespace DependencyWalker
{
    internal static class Grapher
    {
        private static readonly XNamespace dgmlns = "http://schemas.microsoft.com/vs/2009/dgml";

        internal static XDocument GenerateDGML(ISolutionDependencyTree tree)
        {

            var pn = tree.GetProjectToNugetRelationships().ToList();
            var nn = tree.GetNugetToNugetRelationships().ToList();
            var pp = tree.GetProjectToProjectRelationships().ToList();


            var graph = new XElement(
                dgmlns + "DirectedGraph",
                new XAttribute("GraphDirection", "LeftToRight"),
                new XElement(dgmlns + "Nodes",
                    tree.Projects.Select(p => CreateNode(p.Name, "Project")),
                    pn.Select(r => CreateNode(r.Target.Package.Id, "Package")),
                    nn.Select(r => CreateNode(r.Target.Package.Id, "Package"))
                ),
                new XElement(dgmlns + "Links",
                    pn.Select(r => CreateLink(r.Source.Name, r.Target.Package.Id, "Package Reference")),
                    nn.Select(r => CreateLink(r.Source.Package.Id, r.Target.Package.Id, "Transitive Dependency")),
                    pp.Select(r => CreateLink(r.Source.Name, r.Target.Name, "Project Reference"))
                ),
                // No need to declare Categories, auto generated
                new XElement(dgmlns + "Styles",
                    CreateStyle("Project", "Blue"),
                    CreateStyle("Package", "Purple")
                 ));

            var doc = new XDocument(graph);
            doc.Save("mydgml.dgml");

            return doc;

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