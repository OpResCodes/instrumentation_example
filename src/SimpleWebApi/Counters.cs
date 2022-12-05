using System.Diagnostics.Metrics;

namespace SimpleWebApi
{

    public static class Counters
    {
        public static Meter m = new Meter("Matthes.ComputeService.Api", "1.0.0");

        public static Counter<int> CachePuts = m.CreateCounter<int>(name: "cache-put");
        public static Counter<int> CacheHits = m.CreateCounter<int>(name: "cache-hit");
    }

}