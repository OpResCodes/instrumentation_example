# Diagnostic Port
Diagnostic port stellt trace events bereit, VS z.b. fängt diese beim Debuggen auf. Je nach OS schickt die dotnet runtime diese events an unterschiedliche Stellen (über EventPipe runtime). Bei Windows an "named pipes", Linux "Domain Sockets". 

global installierte dotnet tools auflisten:

    dotnet tool list -g

tracing tool global installieren:

    dotnet tool install -g dotnet-trace

   Auflisten möglicher Prozesse für dotnet trace (über diagnostic port bereitgestellte Events):
   
    dotnet trace ps

Trace Collecting ausführen von Commandline:

    dotnet trace collect -- .\meinprogramm.exe

Es wird eine .nettrace Datei erstellt, die z.B. in Visual Studio oder perfview geöffnet werden kann.
Möglich: Tracing im speedscope Format, 
so dass es über die speedscope website betrachtet werden kann (nicht so gut wie in VS)

    dotnet trace collect --format SpeedScope -- .\meinprogramm.exe

EventSource schickt diese Events ab. Man kann eigene Subklassen 
der Eventsource erstellen, um diese im Diagnostic port anzubieten.
Events sind einfache voids, die dann im Quellcode benutzt werden können.

Beispiel:
    
    using System.Diagnostics.Tracing;

    [EventSource(Name ="Matthes.ComputeService")]
    internal class MatEventSource : EventSource
    {
        public static MatEventSource Log { get; } = new MatEventSource();

        [Event(1)]
        public void ComputeStart(double computeStartValue, int step) {
            WriteEvent(1,computeStartValue,step);
        }

        [Event(2)]
        public void ComputeStop(double computeEndValue, int step)
        {
            WriteEvent(2,computeEndValue,step);
        }

    }
Man gibt gerne eine Instanz der Eventsource als statische Property dazu (Konvention).
Außerdem kann man die Eventsource benennen per Klassenattribut, ansonsten heißt sie wie die Klasse.

Einbinden in den eigentlich Code:

    //First pass
    MatEventSource.Log.ComputeStart(v, 1);
    v = FirstComputationStep(v);
    MatEventSource.Log.ComputeStop(v, 1);

    //second pass
    MatEventSource.Log.ComputeStart(v, 2);
    v = SecondComputationStep(v);
    MatEventSource.Log.ComputeStop(v, 2);

Damit die Events dann auch wirklich vom Tracing aufgefangen werden, muss
die neue EventSource als Provider explizit genannt werden (die dotnet runtime kennt sie noch nicht,
aber ein paar default Provider der runtime werden immer getraced).
Mehrere event sources per komma-separierter Liste:

    dotnet trace collect --providers Matthes.ComputeService -- .\meinprogramm.exe

Die .nettrace Datei kann in VS oder perfview geöffnet werden.

Wenn auch CPU-Sampling in der .nettrace Datei enthalten sein soll, muss das als zusätzlicher 
Provider angegeben werden:

    dotnet trace collect --providers Matthes.ComputeService,Microsoft-DotNETCore-SampleProfiler -- .\meinProgramm.exe arg1 arg2

Weitere Provider für tracing events gibts hier:
https://github.com/dotnet/diagnostics/blob/main/documentation/dotnet-trace-instructions.md
