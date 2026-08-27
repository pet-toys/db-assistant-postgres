# Benchmarks

What one binary `COPY` costs through this library, measured against the
`NpgsqlBinaryImporter` loop it exists to replace. The ratio column is the price
of the fluent mapping; everything else in the number is PostgreSQL.

Nothing here is a gate. No build, pull request or release fails because of a
number in this project - see [Why this is not in CI](#why-this-is-not-in-ci).

## Running it

From the repository root. The project multi-targets, so a framework has to be
named:

```bash
dotnet run -c Release -f net10.0 --project bench/PetToys.DbAssistant.Postgres.Benchmarks -- --filter "*"
```

The run needs a Docker engine that can run Linux containers, the same thing the
integration tests need. It starts `postgres:18-alpine` itself, once per
benchmark class, and stops it again at the end.

One group at a time is usually what you want:

```bash
dotnet run -c Release -f net10.0 --project bench/PetToys.DbAssistant.Postgres.Benchmarks -- --filter "*WideRowBenchmarks*"
```

`--list flat` prints the benchmark names without running anything, which is the
quickest way to write a filter that matches what you meant. To measure every
supported runtime in one report, add `--runtimes net8.0 net9.0 net10.0`.

## Measuring against your own server

Set `POSTGRES_BENCHMARK_CONNECTION_STRING` and no container is started:

```bash
POSTGRES_BENCHMARK_CONNECTION_STRING="Host=db.internal;Username=loader;Password=...;Database=staging" \
  dotnet run -c Release -f net10.0 --project bench/PetToys.DbAssistant.Postgres.Benchmarks -- --filter "*"
```

This is the only way to get a duration that means anything outside this
repository. A container on an overlay filesystem is a fair place to compare two
versions of this library against each other and a poor stand-in for the server
you actually copy into.

The run creates its three tables in whatever database the connection string
names, dropping them first if they are already there: `narrow_row`, `wide_row`
and `source_shape_row`. It truncates them between iterations and leaves them
behind at the end. Point it at a scratch database.

## What is measured

| Class                    | Baseline                | Measured                | The question               |
| ------------------------ | ----------------------- | ----------------------- | -------------------------- |
| `NarrowRowBenchmarks`    | hand-written importer   | mapped bulk context     | overhead over four columns |
| `WideRowBenchmarks`      | hand-written importer   | mapped bulk context     | overhead over twelve       |
| `SourceShapeBenchmarks`  | `IEnumerable` source    | `IAsyncEnumerable`      | what the async source adds |

Each runs at 10,000 and 100,000 rows. The rows come from one seeded generator
and are built in `[GlobalSetup]`, so two runs of the same revision copy
identical bytes and no part of building a row lands inside a measurement.

Both arms of a comparison write the same values, in the same order, with the
same `NpgsqlDbType`, into the same table. The only difference is who writes the
loop.

Two things are deliberately outside the measurement: rows carrying `null`, and
any batching or staging-table strategy. Each would be a benchmark of its own.

## Reading a run

| Column      | What it is                                                   |
| ----------- | ------------------------------------------------------------ |
| `Mean`      | The average duration of one copy of `RowCount` rows           |
| `Ratio`     | `Mean` divided by the baseline's - the library's overhead     |
| `StdDev`    | How much the copies varied; read it before quoting a mean     |
| `Allocated` | Bytes allocated per copy, from the memory diagnoser           |

`Allocated` is the one number here that does not depend on the server at all,
which makes it the most portable thing the report carries.

The destination tables are `UNLOGGED`. Write-ahead logging and the checkpoints
it triggers are the loudest source of variance in a copy, they fall on both
arms equally, and they are the server's cost rather than this library's.
Removing them does not make the ratio less true; it makes it visible in fifteen
iterations instead of a hundred. A duration read off this benchmark is a floor,
not what a copy into a logged table costs.

The job is `RunStrategy.Monitoring` with one invocation per iteration, because
one benchmark here is one copy of tens of thousands of rows and the table has to
be truncated between them. A run therefore collects tens of samples rather than
thousands.

## Where the output lands

`BenchmarkDotNet.Artifacts/results/` beside the built benchmark assembly, which
for the command above is
`bench/PetToys.DbAssistant.Postgres.Benchmarks/bin/Release/net10.0/`. Several
formats land there; the one that matters is `*-report-github.md`, the format
[`BASELINE.md`](BASELINE.md) is a copy of, and it opens with the processor,
operating system, SDK and runtime of the run.

The location is pinned by the configuration rather than left at BenchmarkDotNet's
default, which is the working directory the run was launched from. Two runs
launched from two directories would otherwise leave two artifact sets that
neither overwrites nor mentions the other. The artifacts directory is
git-ignored; `BASELINE.md` is a deliberate copy kept outside it, and it is the
only run output this repository keeps.

## Comparing against the baseline

`BASELINE.md` was taken on one machine, against one server. Two rules follow:

- **Ratios travel.** Comparing `Ratio` against the baseline's is valid from any
  machine - that is the whole reason the comparison is written as one.
- **Durations do not.** `Mean` is only comparable within the environment the
  baseline was recorded in, and here that includes the server: a different
  image, a different host, a different disk, or a connection over a network
  rather than a loopback moves every number without anything in the library
  changing.

A result is not comparable at all if the run used a different job. `--job short`
and `--job dry` trade iterations for wall-clock; they are for a quick look while
working, not for a number anyone quotes.

When a change alters the copy path, re-take the baseline and commit it with that
change. Nothing enforces this: the value of a recorded baseline is exactly the
discipline of keeping it current.

## Why this is not in CI

No workflow runs this project. `build-deploy.yml` packs `src/**/*.csproj` and
never sees it; `test.yml` compiles it with the rest of the solution - which is
the point, it keeps building and keeps being analysed - and runs nothing from
it.

GitHub's hosted runners are shared, virtualised and share their disk with
whatever neighbour is on the same host. A copy benchmark there measures the
runner, not the library, and a gate on it would fail on noise - and a gate that
fails on noise gets switched off within a week, leaving the repository with a
disabled gate instead of an honest manual measurement.
