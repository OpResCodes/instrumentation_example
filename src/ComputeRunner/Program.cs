using ComputeService;

namespace ComputeRunner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ComputeRunner v1.0");
            int.TryParse(args[0], out int startValue);
            Console.WriteLine("Starting with value: {0}",startValue);

            var service = new SampleServiceWithEvents();
            double computed = service.Compute(startValue);
            Console.WriteLine("The result is: {0}", computed);
        }
    }
}