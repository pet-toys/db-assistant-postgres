using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using NpgsqlTypes;
using PetToys.DbAssistant.Postgres.Extensions;

namespace PetToys.DbAssistant.Postgres.Benchmarks;

/// <summary>
/// The four-column copy: an integer key, a text column, a <c>timestamptz</c> and a flag.
/// </summary>
/// <remarks>
/// The baseline is the importer loop a caller writes by hand, which is what this library exists
/// to replace, so the ratio column is the price of the fluent mapping and nothing else. Both
/// methods write the same values, in the same order, with the same <see cref="NpgsqlDbType"/>,
/// into the same table.
/// </remarks>
public class NarrowRowBenchmarks : CopyBenchmark<NarrowRow>
{
    private const string Table = "narrow_row";

    private const string CopyCommand =
        """COPY "narrow_row"("id", "name", "created_at", "active") FROM STDIN BINARY;""";

    protected override string TableName => Table;

    protected override string CreateTableStatement =>
        $"""
         CREATE UNLOGGED TABLE {Table} (
             id integer NOT NULL,
             name text NOT NULL,
             created_at timestamptz NOT NULL,
             active boolean NOT NULL
         );
         """;

    protected override IReadOnlyList<NarrowRow> GenerateRows(int count) => RowSet.Narrow(count);

    [Benchmark(Baseline = true, Description = "Hand-written importer")]
    public async Task<ulong> HandWritten()
    {
        var writer = await Connection.BeginBinaryImportAsync(CopyCommand);
        await using (writer)
        {
            foreach (var row in Rows)
            {
                await writer.StartRowAsync();
                await writer.WriteAsync(row.Id, NpgsqlDbType.Integer);
                await writer.WriteAsync(row.Name, NpgsqlDbType.Text);
                await writer.WriteAsync(row.CreatedAt, NpgsqlDbType.TimestampTz);
                await writer.WriteAsync(row.Active, NpgsqlDbType.Boolean);
            }

            return await writer.CompleteAsync();
        }
    }

    [Benchmark(Description = "Mapped bulk context")]
    public async ValueTask<ulong> Mapped() =>
        await Connection.CreateBulkContext<NarrowRow>(Table)
            .MapInteger("id", row => row.Id)
            .MapText("name", row => row.Name)
            .MapTimeStampTz("created_at", row => row.CreatedAt)
            .MapBoolean("active", row => row.Active)
            .WriteDataAsync(Rows);
}
