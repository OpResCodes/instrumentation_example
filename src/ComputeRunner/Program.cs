using ComputeService;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Spectre.Console;
using System.Data;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace ComputeRunner
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            AddActivityOutput();

            //Enable JAeger Export (see jaeger website)
            using TracerProvider? tracerProvider = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("ComputeRunner.ClientConsole"))
                .AddSource(MatActivitySource.Instance.Name)
                .AddJaegerExporter(o =>
                {
                    o.Protocol = OpenTelemetry.Exporter.JaegerExportProtocol.HttpBinaryThrift;
                })
                .AddHttpClientInstrumentation()
                .Build();

            Console.WriteLine("ComputeRunner v1.0");
            int.TryParse(args[0], out int startValue);
            Console.WriteLine("Starting with value: {0}",startValue);

            var service = new SampleServiceWithActivities();
            double computed = await service.ComputeAsync(startValue);
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