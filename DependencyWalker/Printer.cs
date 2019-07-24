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
using System.Runtime.CompilerServices;
using DependencyWalker.Model;
using Newtonsoft.Json;
using Serilog;

[assembly:InternalsVisibleTo("DependencyWalkerTests")]
namespace DependencyWalker
{

    public class Printer
    {

        public static void PersistToFile(ISolutionDependencyTree tree)
        {
            Log.Debug("Finished all walking, now generating to file");
            File.Delete("output.json");
            File.WriteAllText("output.json", SerializeToJSON(tree));
        }

        internal static string SerializeToJSON(ISolutionDependencyTree tree)
        {
            return JsonConvert.SerializeObject(
                tree,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    ContractResolver = new ShouldSerializeContractResolver(),
                    TraceWriter = new SerilogTraceWriter()
                }
            );
        }
    }

}
