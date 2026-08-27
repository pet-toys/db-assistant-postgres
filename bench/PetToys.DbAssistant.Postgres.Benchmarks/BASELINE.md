# Recorded baseline

The run this repository compares against, taken on 2026-08-27 against the
`postgres:18-alpine` container the benchmark starts for itself, over Docker
Desktop on WSL2. Twelve benchmarks, 1 minute 23 seconds, no failures and no
warnings.

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

| Method                  | RowCount | Mean     | Error    | StdDev   | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------------ |--------- |---------:|---------:|---------:|------:|--------:|----------:|------------:|
| 'Hand-written importer' | 10000    | 26.96 ms | 5.501 ms | 5.145 ms |  1.04 |    0.28 |   6.86 KB |        1.00 |
| 'Mapped bulk context'   | 10000    | 33.50 ms | 3.782 ms | 3.538 ms |  1.29 |    0.28 |    8.8 KB |        1.28 |
|                         |          |          |          |          |       |         |           |             |
| 'Hand-written importer' | 100000   | 48.44 ms | 3.109 ms | 2.908 ms |  1.00 |    0.08 |  44.27 KB |        1.00 |
| 'Mapped bulk context'   | 100000   | 50.25 ms | 5.048 ms | 4.722 ms |  1.04 |    0.11 |   46.2 KB |        1.04 |

## Wide row - twelve columns

| Method                  | RowCount | Mean      | Error    | StdDev   | Median    | Ratio | RatioSD | Allocated  | Alloc Ratio |
|------------------------ |--------- |----------:|---------:|---------:|----------:|------:|--------:|-----------:|------------:|
| 'Hand-written importer' | 10000    |  43.44 ms | 30.23 ms | 28.28 ms |  28.54 ms |  1.29 |    1.00 |  482.48 KB |        1.00 |
| 'Mapped bulk context'   | 10000    |  50.99 ms | 42.53 ms | 39.78 ms |  28.84 ms |  1.51 |    1.35 |  487.15 KB |        1.01 |
|                         |          |           |          |          |           |       |         |            |             |
| 'Hand-written importer' | 100000   | 245.19 ms | 25.36 ms | 23.72 ms | 241.71 ms |  1.01 |    0.13 | 4796.65 KB |        1.00 |
| 'Mapped bulk context'   | 100000   | 257.88 ms | 21.95 ms | 20.53 ms | 253.19 ms |  1.06 |    0.12 | 4797.48 KB |        1.00 |

## Source shape - the two `WriteDataAsync` overloads

| Method                    | RowCount | Mean     | Error    | StdDev   | Ratio | RatioSD | Allocated | Alloc Ratio |
|-------------------------- |--------- |---------:|---------:|---------:|------:|--------:|----------:|------------:|
| 'IEnumerable source'      | 10000    | 35.54 ms | 6.767 ms | 6.330 ms |  1.03 |    0.25 |   8.82 KB |        1.00 |
| 'IAsyncEnumerable source' | 10000    | 36.25 ms | 7.372 ms | 6.896 ms |  1.05 |    0.27 |   9.05 KB |        1.03 |
|                           |          |          |          |          |       |         |           |             |
| 'IEnumerable source'      | 100000   | 50.56 ms | 3.991 ms | 3.733 ms |  1.00 |    0.10 |  46.23 KB |        1.00 |
| 'IAsyncEnumerable source' | 100000   | 52.17 ms | 2.981 ms | 2.789 ms |  1.04 |    0.09 |  46.46 KB |        1.01 |

## What the run says

**The mapping layer costs a few percent.** At a hundred thousand rows it is 1.04
over the hand-written importer on the narrow row and 1.06 on the wide one, both
with a ratio standard deviation of about 0.1. Tripling the column count moves
the overhead by two points, which is what a per-column delegate call should look
like against the cost of writing the same value to a socket.

**The async source costs about the same.** 1.04 at a hundred thousand rows, over
an iterator that never actually suspends - so that number is the floor of what
`IAsyncEnumerable` adds, and a real streaming source pays whatever it costs on
top.

**The extra allocation is per copy, not per row.** The mapped arm allocates
about 2 KB more than the hand-written one, and it is the same 2 KB at ten
thousand rows and at a hundred thousand: the builder, the column list and the
delegates, all built once. Per-row the two arms allocate the same, which is
visible on the wide row, where both sit on 4.80 MB per copy of a hundred
thousand rows - Npgsql's own writing, identical on both sides.

**The ten-thousand-row rows are not worth quoting.** Their ratio standard
deviations run from 0.25 to 1.35, and the wide row's mean of 43 ms against a
median of 28 ms says the distribution is bimodal, not noisy-around-a-centre. At
that size a copy is mostly its own fixed cost, and a handful of slow iterations
moves the mean more than the thing being measured does. They are kept because
the fixed cost is worth seeing; they are not a number to compare a change
against.

**Warmup is load-bearing.** The same three classes measured with the default two
warmup iterations put the wide row at 232 ms against 349 ms, a ratio of 1.50,
which the five-warmup run does not reproduce anywhere. A copy benchmark's first
iterations measure a server filling its caches, and reading them as a result is
how a mapping layer gets accused of costing 50%.
