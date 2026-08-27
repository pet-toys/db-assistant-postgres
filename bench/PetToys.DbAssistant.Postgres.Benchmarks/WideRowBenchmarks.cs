using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using NpgsqlTypes;
using PetToys.DbAssistant.Postgres.Extensions;

namespace PetToys.DbAssistant.Postgres.Benchmarks;

/// <summary>
/// The twelve-column copy: one column from each family the library maps.
/// </summary>
/// <remarks>
/// Three times the columns of <see cref="NarrowRowBenchmarks"/> means three times the per-row
/// delegate calls and writes, which is where a mapping layer's overhead would show if it has any.
/// The pair is the point: a ratio that holds at four columns and at twelve is a ratio that does
/// not depend on the row.
/// </remarks>
public class WideRowBenchmarks : CopyBenchmark<WideRow>
{
    private const string Table = "wide_row";

    private const string CopyCommand =
        """
        COPY "wide_row"("id", "code", "name", "amount", "ratio", "flag", "identifier", "payload", "document", "address", "created_at", "duration") FROM STDIN BINARY;
        """;

    protected override string TableName => Table;

    protected override string CreateTableStatement =>
        $"""
         CREATE UNLOGGED TABLE {Table} (
             id integer NOT NULL,
             code varchar(32) NOT NULL,
             name text NOT NULL,
             amount numeric NOT NULL,
             ratio double precision NOT NULL,
             flag boolean NOT NULL,
             identifier uuid NOT NULL,
             payload bytea NOT NULL,
             document jsonb NOT NULL,
             address inet NOT NULL,
             created_at timestamptz NOT NULL,
             duration interval NOT NULL
         );
         """;

    protected override IReadOnlyList<WideRow> GenerateRows(int count) => RowSet.Wide(count);

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
                await writer.WriteAsync(row.Code, NpgsqlDbType.Varchar);
                await writer.WriteAsync(row.Name, NpgsqlDbType.Text);
                await writer.WriteAsync(row.Amount, NpgsqlDbType.Numeric);
                await writer.WriteAsync(row.Ratio, NpgsqlDbType.Double);
                await writer.WriteAsync(row.Flag, NpgsqlDbType.Boolean);
                await writer.WriteAsync(row.Identifier, NpgsqlDbType.Uuid);
                await writer.WriteAsync(row.Payload, NpgsqlDbType.Bytea);
                await writer.WriteAsync(row.Document, NpgsqlDbType.Jsonb);
                await writer.WriteAsync(row.Address, NpgsqlDbType.Inet);
                await writer.WriteAsync(row.CreatedAt, NpgsqlDbType.TimestampTz);
                await writer.WriteAsync(row.Duration, NpgsqlDbType.Interval);
            }

            return await writer.CompleteAsync();
        }
    }

    [Benchmark(Description = "Mapped bulk context")]
    public async ValueTask<ulong> Mapped() =>
        await Connection.CreateBulkContext<WideRow>(Table)
            .MapInteger("id", row => row.Id)
            .MapVarchar("code", row => row.Code)
            .MapText("name", row => row.Name)
            .MapNumeric("amount", row => row.Amount)
            .MapDouble("ratio", row => row.Ratio)
            .MapBoolean("flag", row => row.Flag)
            .MapUUID("identifier", row => row.Identifier)
            .MapByteArray("payload", row => row.Payload)
            .MapJsonb("document", row => row.Document)
            .MapInetAddress("address", row => row.Address)
            .MapTimeStampTz("created_at", row => row.CreatedAt)
            .MapInterval("duration", row => row.Duration)
            .WriteDataAsync(Rows);
}
