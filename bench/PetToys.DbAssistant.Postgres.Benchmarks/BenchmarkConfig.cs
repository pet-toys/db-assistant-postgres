using System;
using System.IO;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;

namespace PetToys.DbAssistant.Postgres.Benchmarks;

/// <summary>
/// The one configuration every benchmark in this assembly runs under.
/// </summary>
/// <remarks>
/// <para>
/// It is built on top of <see cref="DefaultConfig"/> rather than from nothing, so the default
/// loggers, columns, analysers, validators and exporters stay in place - including the
/// GitHub-flavoured markdown export the recorded baseline is a copy of - and it is applied
/// through <c>BenchmarkSwitcher</c> rather than through an attribute on every class, so the
/// command line keeps its say over the job and the runtimes.
/// </para>
/// <para>
/// The job is the part that is not default. One benchmark here is one binary <c>COPY</c> of ten
/// or a hundred thousand rows, which is several orders of magnitude past the point where
/// BenchmarkDotNet's pilot stage and its batching of invocations per iteration earn their keep,
/// and the destination table has to be emptied between copies or every iteration after the first
/// measures a larger table than the one before it. <see cref="RunStrategy.Monitoring"/> with an
/// invocation count and an unroll factor of one is what makes <c>[IterationSetup]</c> meaningful:
/// one setup, one timed copy, one sample. The cost is that a run collects tens of samples rather
/// than thousands, so the standard deviation in the report is worth a look before quoting a mean.
/// The warmup is five rather than the default: a freshly started server spends its first few
/// copies filling caches and settling, and those belong outside the measurement.
/// </para>
/// <para>
/// The memory diagnoser is the other addition. Allocation per copy is the one number here that
/// does not depend on the server at all, which makes it the most portable thing the report
/// carries.
/// </para>
/// <para>
/// The artifacts path is pinned to the directory the benchmark assembly was built into, rather
/// than left at its default of the current working directory. The default puts a run's output
/// wherever the caller happened to be standing, so running from the repository root and running
/// from the project folder produce two artifact directories that neither knows about the other.
/// Keying it to the assembly also separates the target frameworks, which is right: a net8.0 run
/// and a net10.0 run are not each other's results.
/// </para>
/// </remarks>
public static class BenchmarkConfig
{
    /// <summary>Builds the configuration.</summary>
    /// <returns>The configuration to hand to the switcher.</returns>
    public static IConfig Create() =>
        ManualConfig.Create(DefaultConfig.Instance)
            .AddDiagnoser(MemoryDiagnoser.Default)
            .AddJob(Job.Default
                .WithStrategy(RunStrategy.Monitoring)
                .WithInvocationCount(1)
                .WithUnrollFactor(1)
                .WithWarmupCount(5)
                .WithIterationCount(15)
                .WithId("Copy"))
            .WithArtifactsPath(Path.Combine(AppContext.BaseDirectory, "BenchmarkDotNet.Artifacts"));
}
