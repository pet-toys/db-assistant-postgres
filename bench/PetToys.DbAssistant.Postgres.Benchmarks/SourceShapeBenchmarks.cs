using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using PetToys.DbAssistant.Postgres.Extensions;

namespace PetToys.DbAssistant.Postgres.Benchmarks;

/// <summary>
/// The two <c>WriteDataAsync</c> overloads against each other, over the narrow row.
/// </summary>
/// <remarks>
/// Both overloads share one lifecycle helper and differ only in how they walk the rows, so what
/// is measured here is the enumeration: a <c>foreach</c> against an <c>await foreach</c> over an
/// iterator that never actually suspends. A caller streaming from a real source pays whatever
/// that source costs on top; this pair only says what the overload itself adds, which is the part
/// this repository can be held to.
/// </remarks>
public class SourceShapeBenchmarks : CopyBenchmark<NarrowRow>
{
    private const string Table = "source_shape_row";

    protected override string TableName => Table;

    // The same four columns NarrowRowBenchmarks uses, in a table of their own: two classes
    // sharing one destination would have each other's leftovers to truncate, and BenchmarkDotNet
    // runs them in processes that know nothing about one another.
    protected override IReadOnlyList<ColumnSpec> Columns { get; } =
    [
        new("id", "integer"),
        new("name", "text"),
        new("created_at", "timestamptz"),
        new("active", "boolean"),
    ];

    protected override IReadOnlyList<NarrowRow> GenerateRows(int count) => RowSet.Narrow(count);

    [Benchmark(Baseline = true, Description = "IEnumerable source")]
    public async ValueTask<ulong> Synchronous() => await Map().WriteDataAsync(Rows);

    [Benchmark(Description = "IAsyncEnumerable source")]
    public async ValueTask<ulong> Asynchronous() => await Map().WriteDataAsync(AsAsyncEnumerable(Rows));

    private static async IAsyncEnumerable<NarrowRow> AsAsyncEnumerable(IReadOnlyList<NarrowRow> rows)
    {
        foreach (var row in rows)
        {
            yield return row;
        }

        await Task.CompletedTask;
    }

    private BulkContextBuilder<NarrowRow> Map() =>
        Connection.CreateBulkContext<NarrowRow>(Table)
            .MapInteger("id", row => row.Id)
            .MapText("name", row => row.Name)
            .MapTimeStampTz("created_at", row => row.CreatedAt)
            .MapBoolean("active", row => row.Active);
}
