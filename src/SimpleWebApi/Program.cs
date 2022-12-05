using ComputeService;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry;
using Spectre.Console;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace SimpleWebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);
            WebApplication app = builder.Build();

            AddActivityOutput();

            //Enable JAeger Export (see jaeger website, cannot be extracted)
            using TracerProvider? tracerProvider = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("ComputeRunner.CacheService"))
                .AddSource(MatActivitySource.Instance.Name)
                .AddJaegerExporter(o => {
                    o.Protocol = OpenTelemetry.Exporter.JaegerExportProtocol.HttpBinaryThrift;
                })
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .Build();

            //write to cache
            app.MapPost("/cache/{inputValue}", async (HttpRequest request) =>
            {
                string md5 = request.RouteValues["inputValue"] as string;
                var ms = new MemoryStream();    
                await request.Body.CopyToAsync(ms);
                string content = Encoding.UTF8.GetString(ms.ToArray());

                using(Activity? diskWriteActivity = MatActivitySource.Instance.StartActivity("Write to disk"))
                {
                    string fileName = $"{md5}.txt";
                    await File.WriteAllTextAsync(fileName,content);
                }

                //increase cache put counter
                Counters.CachePuts.Add(1);

            });

            //read from cache
            app.MapGet("/cache/{md5}", async (string md5) =>
            {
                if (!File.Exists($"{md5}.txt"))
                    return Results.NotFound();

                //increase cache hit counter
                Counters.CacheHits.Add(1);

                var result = File.ReadAllText($"{md5}.txt");
                return Results.Text(result);
            });

            app.Run("http://localhost:5000");
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