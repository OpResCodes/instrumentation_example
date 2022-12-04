using System.Diagnostics;

namespace ComputeService
{
    static class MatActivitySource
    {

        //considered good practice to expose the named activitysource:
        public static ActivitySource Instance { get; } =
            new ActivitySource(name: "Matthes.ComputeService", version: "1.0");


    }
}
