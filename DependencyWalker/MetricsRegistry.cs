using App.Metrics.Counter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyWalker
{
    public static class MetricsRegistry
    {
        public static CounterOptions NugetCacheHitCounter => new CounterOptions
        {
            Name = "Nuget Cache Hits"
        };

        public static CounterOptions NugetCacheMissCounter => new CounterOptions
        {
            Name = "Nuget Cache Misses"
        };
    }
}
