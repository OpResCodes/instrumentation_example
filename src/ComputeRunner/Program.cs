using ComputeService;
using Spectre.Console;
using System.Data;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace ComputeRunner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            AddActivityOutput();

            Console.WriteLine("ComputeRunner v1.0");
            int.TryParse(args[0], out int startValue);
            Console.WriteLine("Starting with value: {0}",startValue);

            var service = new SampleServiceWithActivities();
            double computed = service.Compute(startValue);
            Console.WriteLine("The result is: {0}", computed);
        }


        static void AddActivityOutput()
        {
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            Activity.ForceDefaultIdFormat = true;

            int level = 0;

            ActivitySource.AddActivityListener(listener: new ActivityListener()
            {
                ShouldListenTo = (source) => true,
                Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllDataAndRecorded,

                ActivityStarted = (activity) =>
                {
                    string pad = new string(' ', level * 2);
                    string title = $"{pad}[red]>[/] [green]{activity.DisplayName}[/]";
                    AnsiConsole.MarkupLine(title);

                    level += 1;
                    pad = new string(' ', level * 2);
                    AnsiConsole.MarkupLine($"{pad}span (activity) id:   {activity.SpanId}");
                    AnsiConsole.MarkupLine($"{pad}id:                   {activity.Id}");
                    AnsiConsole.MarkupLine($"{pad}parent span id:       {activity.ParentSpanId}");
                },
                ActivityStopped = (activity) =>
                {
                    level -= 1;
                    string pad = new string(' ', level * 2);
                    AnsiConsole.MarkupLine($"{pad}[red]<[/] -- [blue]{activity.Duration.TotalMilliseconds}ms[/]");
                }
            });
        }
    }
}