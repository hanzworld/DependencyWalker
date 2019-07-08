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
                new JsonSerializerSettings { ContractResolver = new ShouldSerializeContractResolver() }
            );
        }
    }

}
