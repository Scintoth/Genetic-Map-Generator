using GeneticMapGenerator.Application.Composition;
using GeneticMapGenerator.Application.Observers;
using GeneticMapGenerator.Headless.Cli;
using GeneticMapGenerator.Headless.Observers;
using GeneticMapGenerator.Headless.Rendering;

// Headless host (driving adapter): parses the command line, wires console + image observers onto the
// core's observer port, and runs the use case on the calling thread.
try
{
    var options = CliOptions.Parse(args);
    if (options.ShowHelp)
    {
        Console.WriteLine(CliOptions.Usage);
        return 0;
    }

    var command = options.ToCommand();
    using var cancellation = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) =>
    {
        e.Cancel = true;
        cancellation.Cancel();
    };

    var observer = new CompositeWorldGenerationObserver(
        new ConsoleProgressObserver(Console.Out),
        new ImageExportObserver(options.OutputDirectory, options.SnapshotEvery, new WorldImageRenderer(), Console.Out));

    var useCase = WorldGenerator.Create(new WorldGeneratorOptions { ParallelEvaluation = !options.Sequential });
    useCase.Execute(command, observer, cancellation.Token);
    return 0;
}
catch (CliException exception)
{
    Console.Error.WriteLine(exception.Message);
    return 2;
}
catch (OperationCanceledException)
{
    Console.Error.WriteLine("Cancelled.");
    return 130;
}
