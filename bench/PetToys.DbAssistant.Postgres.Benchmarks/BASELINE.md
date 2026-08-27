# Recorded baseline

The run this repository compares against, taken on 2026-08-27 against the
`postgres:18-alpine` container the benchmark starts for itself, over Docker
Desktop on WSL2. Twelve benchmarks, 40 seconds, no failures and no warnings.
One container for the whole run - all twelve cases measured against the same
server, which is what makes a ratio between two of them mean anything.

The environment header was byte-identical in all three exports, so it is stated
once here rather than three times:

```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9168/25H2/2025Update/HudsonValley2)
12th Gen Intel Core i7-12700H 2.30GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 10.0.400
  [Host] : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3
  Copy   : .NET 10.0.11 (10.0.11, 10.0.1126.37416), X64 RyuJIT x86-64-v3

Job=Copy  InvocationCount=1  IterationCount=15
RunStrategy=Monitoring  UnrollFactor=1  WarmupCount=5

```

`Ratio` travels to another machine; `Mean` does not, and here it does not travel
to another *server* either. The destination tables are `UNLOGGED`, so every
duration below is a floor rather than the cost of a copy into a logged table.
See [README.md](README.md) for what that means and how to compare a fresh run
against this one.

## Narrow row - four columns

| Method                  | RowCount | Mean     | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------------ |--------- |---------:|----------:|----------:|------:|--------:|----------:|------------:|
| 'Hand-written importer' | 10000    | 27.64 ms |  3.118 ms |  2.917 ms |  1.01 |    0.15 |   6.86 KB |        1.00 |
| 'Mapped bulk context'   | 10000    | 32.04 ms | 11.562 ms | 10.815 ms |  1.17 |    0.41 |    8.8 KB |        1.28 |
|                         |          |          |           |           |       |         |           |             |
| 'Hand-written importer' | 100000   | 48.59 ms |  3.798 ms |  3.553 ms |  1.00 |    0.09 |  45.57 KB |        1.00 |
| 'Mapped bulk context'   | 100000   | 52.43 ms |  4.640 ms |  4.340 ms |  1.08 |    0.11 |   46.2 KB |        1.01 |

## Wide row - twelve columns

| Method                  | RowCount | Mean      | Error    | StdDev   | Median    | Ratio | RatioSD | Allocated  | Alloc Ratio |
|------------------------ |--------- |----------:|---------:|---------:|----------:|------:|--------:|-----------:|------------:|
| 'Hand-written importer' | 10000    |  43.56 ms | 27.51 ms | 25.74 ms |  30.12 ms |  1.27 |    0.93 |  482.48 KB |        1.00 |
| 'Mapped bulk context'   | 10000    |  47.76 ms | 41.78 ms | 39.09 ms |  27.01 ms |  1.40 |    1.31 |  487.15 KB |        1.01 |
|                         |          |           |          |          |           |       |         |            |             |
| 'Hand-written importer' | 100000   | 241.53 ms | 12.81 ms | 11.98 ms | 237.54 ms |  1.00 |    0.07 | 4798.09 KB |        1.00 |
| 'Mapped bulk context'   | 100000   | 249.70 ms | 13.52 ms | 12.64 ms | 247.75 ms |  1.04 |    0.07 | 4805.12 KB |        1.00 |

## Source shape - the two `WriteDataAsync` overloads

| Method                    | RowCount | Mean     | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|-------------------------- |--------- |---------:|----------:|----------:|------:|--------:|----------:|------------:|
| 'IEnumerable source'      | 10000    | 34.91 ms |  6.151 ms |  5.753 ms |  1.03 |    0.25 |   8.82 KB |        1.00 |
| 'IAsyncEnumerable source' | 10000    | 33.98 ms | 11.653 ms | 10.900 ms |  1.00 |    0.36 |   8.75 KB |        0.99 |
|                           |          |          |           |           |       |         |           |             |
| 'IEnumerable source'      | 100000   | 52.47 ms |  4.184 ms |  3.914 ms |  1.00 |    0.10 |  47.09 KB |        1.00 |
| 'IAsyncEnumerable source' | 100000   | 52.99 ms |  3.318 ms |  3.104 ms |  1.01 |    0.09 |  46.16 KB |        0.98 |

## What the run says

**The mapping layer costs a few percent.** At a hundred thousand rows it is 1.08
over the hand-written importer on the narrow row and 1.04 on the wide one, both
with a ratio standard deviation around 0.1. It is the narrow row that pays more,
and that is the right way round: the overhead is per row and per column, while
the server's share of a copy grows faster than either, so tripling the columns
dilutes the mapping rather than multiplying it.

**The async source is free.** 1.01 at a hundred thousand rows, over an iterator
that never actually suspends - so that number is the floor of what
`IAsyncEnumerable` adds, and a real streaming source pays whatever it costs on
top.

**The extra allocation is per copy, not per row.** The mapped arm allocates
about 2 KB more than the hand-written one, and it is the same 2 KB at ten
thousand rows and at a hundred thousand: the builder, the column list and the
delegates, all built once. Per row the two arms allocate the same, which is
visible on the wide row, where both sit on 4.80 MB per copy of a hundred
thousand rows - Npgsql's own writing, identical on both sides.

**The ten-thousand-row rows are not worth quoting.** Their ratio standard
deviations run from 0.15 to 1.31, and the wide row's mean of 44 ms against a
median of 30 ms says the distribution is bimodal, not noisy-around-a-centre. At
that size a copy is mostly its own fixed cost, and a handful of slow iterations
moves the mean more than the thing being measured does. They are kept because
the fixed cost is worth seeing; they are not a number to compare a change
against.

**Two things about the measurement itself, learned by getting them wrong.**
Warmup is load-bearing: at BenchmarkDotNet's default of two, an earlier run put
the wide row at 232 ms against 349 ms, a ratio of 1.50 that no run since
reproduces. And the server has to be the same one for both arms: while each
benchmark case started a container of its own - which is what `[GlobalSetup]`
does, since every case gets its own process - the two halves of a ratio were
measured against two different servers, and the wide row's 10,000-row ratio
wandered between 1.40 and 1.87 across runs. The container is now started once,
by the runner, and handed to the cases through the environment.
