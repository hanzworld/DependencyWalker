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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DependencyWalker.NugetDependencyResolution;
using Microsoft.Build.Evaluation;
using Newtonsoft.Json;
using NuGet;

namespace DependencyWalker.Model
{
    [Serializable]
    public class NugetDependencyTree : INugetDependencyTree
    {
        public List<INugetDependency> Packages { get; }
        [JsonIgnore]
        public IFileWithNugetDependencies Source { get; }

        public NugetDependencyTree()
        {
            Packages = new List<INugetDependency>();    
        }
        public NugetDependencyTree(Project project)
        {
            Packages = new List<INugetDependency>();
            Source = new DependencyFormatDeterminer(project).LoadFileThatSpecifiesNugetDependencies();
        }
        
        internal void AddRoots(List<IPackage> list)
        {
            Packages.AddRange(list.Select(p => new NugetDependency(p)));
        }
    }
}
