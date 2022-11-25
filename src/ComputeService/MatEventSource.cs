using System.Diagnostics.Tracing;

namespace ComputeService
{
    [EventSource(Name ="Matthes.ComputeService")]
    internal class MatEventSource : EventSource
    {
        public static MatEventSource Log { get; } = new MatEventSource();

        [Event(1)]
        public void ComputeStart(double computeStartValue, int step, int iter) {
            WriteEvent(1,computeStartValue,step, iter);
        }

        [Event(2)]
        public void ComputeStop(double computeEndValue, int step, int iter)
        {
            WriteEvent(2,computeEndValue,step, iter);
        }

    }
}
