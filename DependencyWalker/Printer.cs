using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using DependencyWalker.Model;
using Microsoft.Build.Evaluation;
using net.r_eg.MvsSln;
using net.r_eg.MvsSln.Extensions;
using Newtonsoft.Json;
using NuGet;
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
                new JsonSerializerSettings { ContractResolver = new ShouldSerializeContractResolver() }
            );
        }
    }

}
