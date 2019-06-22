using System.Collections.Generic;
using Microsoft.Build.Evaluation;

namespace DependencyWalker.Model
{
    internal class ProjectDependencyTree : IProjectDependencyTree
    {
        public ProjectDependencyTree()
        {
            References = new List<IProjectDependency>();
        }

        public List<IProjectDependency> References { get; set; }
        
        internal void AddRoot(ProjectItem reference)
        {
            References.Add(new ProjectDependency(){Name = reference.GetMetadataValue("Name")});
        }
    }
}