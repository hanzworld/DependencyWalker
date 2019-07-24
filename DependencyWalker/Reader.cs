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

using System.IO;
using DependencyWalker.Model;
using Newtonsoft.Json;

namespace DependencyWalker
{

    public static class Reader
    {

        public static ISolutionDependencyTree Read(string path)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new NugetDependencyConverter());
            settings.Converters.Add(new NugetDependencyTreeConverter());
            settings.Converters.Add(new ProjectDependencyConverter());
            settings.Converters.Add(new ProjectDependencyTreeConverter());
            settings.Converters.Add(new ProjectInSolutionConverter());
            settings.Converters.Add(new SolutionDependencyTreeConverter());
            settings.Converters.Add(new PackageDependencyConverter());
            settings.TraceWriter = new SerilogTraceWriter();

            return JsonConvert.DeserializeObject<SolutionDependencyTree>(
                File.ReadAllText(path), settings);
        }
    }
}
