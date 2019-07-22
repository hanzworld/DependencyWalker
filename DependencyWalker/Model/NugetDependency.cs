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
using System.Collections.Concurrent;
using Newtonsoft.Json;
using NuGet;

namespace DependencyWalker.Model
{
    public class NugetDependency : INugetDependency
    {
        public IPackage Package { get; }
        public ConcurrentBag<INugetDependency> FoundDependencies { get; set; }
        public ConcurrentBag<PackageDependency> UnresolvedDependencies { get; set; }

        [JsonConstructor]
        public NugetDependency(IPackage package)
        {
            FoundDependencies = new ConcurrentBag<INugetDependency>();
            UnresolvedDependencies = new ConcurrentBag<PackageDependency>();
            Package = package;
        }
        
        public override string ToString()
        {
            return Package.ToString();
        }

    }

}
