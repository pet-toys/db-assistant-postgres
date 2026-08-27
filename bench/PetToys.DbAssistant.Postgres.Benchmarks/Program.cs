using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;

namespace PetToys.DbAssistant.Postgres.Benchmarks;

/// <summary>The entry point of the benchmark runner.</summary>
public static class Program
{
    /// <summary>
    /// Switches BenchmarkDotNet ask about rather than run: none of them needs a server.
    /// </summary>
    private static readonly string[] QuestionSwitches = ["--help", "-h", "-?", "--list", "--version", "--info"];

    /// <summary>Runs the benchmarks the command line selects.</summary>
    /// <param name="args">
    /// BenchmarkDotNet's own switches - <c>--filter</c>, <c>--runtimes</c>, <c>--job</c> and the
    /// rest. They are passed through untouched, which is why the configuration lives in one place
    /// instead of on every benchmark class.
    /// </param>
    /// <remarks>
    /// The server is started here, once, and not in <c>[GlobalSetup]</c>. BenchmarkDotNet runs
    /// every benchmark case - each method at each parameter value - in a process of its own, so a
    /// container started from the setup is a container per case: the recorded run started twelve
    /// of them, and the baseline and the measured arm of every ratio were therefore measured
    /// against two different servers, which is exactly the difference a ratio is supposed to
    /// cancel out. Child processes inherit this process's environment, so publishing the
    /// connection string into <see cref="PostgresServer.ConnectionStringVariable"/> is what makes
    /// all of them share the one server - and an operator who set that variable themselves keeps
    /// their own.
    /// </remarks>
    public static async Task Main(string[] args)
    {
        if (args.Any(argument => QuestionSwitches.Contains(argument, StringComparer.OrdinalIgnoreCase)))
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, BenchmarkConfig.Create());
            return;
        }

        var server = await PostgresServer.StartAsync();
        await using (server.ConfigureAwait(false))
        {
            Environment.SetEnvironmentVariable(PostgresServer.ConnectionStringVariable, server.ConnectionString);

            Console.WriteLine(string.Create(
                CultureInfo.InvariantCulture,
                $"Measuring against {(server.IsProvisioned ? $"a {PostgresServer.Image} container started for this run" : $"the server named by {PostgresServer.ConnectionStringVariable}")}."));

            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, BenchmarkConfig.Create());
        }
    }
}
