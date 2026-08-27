using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Npgsql;

namespace PetToys.DbAssistant.Postgres.Benchmarks;

/// <summary>
/// What every benchmark class in this assembly shares: the server, the connection, the
/// destination table and the rows to copy into it.
/// </summary>
/// <typeparam name="TRow">The row type being copied.</typeparam>
/// <remarks>
/// <para>
/// The setup runs once per parameter combination and the truncate once per iteration, so a timed
/// region holds one binary <c>COPY</c> into an empty table and nothing else - no connection to
/// open, no rows to build, no table to create.
/// </para>
/// <para>
/// Every destination table is <c>UNLOGGED</c>. Write-ahead logging and the checkpoints it
/// triggers are the loudest source of variance in a copy, they fall on both arms of a comparison
/// equally, and they are the server's cost rather than this library's. Removing them does not
/// make the ratio less true; it makes it visible in ten iterations instead of a hundred. A
/// duration read off this benchmark is therefore a floor, not what a copy into a logged table
/// costs.
/// </para>
/// </remarks>
public abstract class CopyBenchmark<TRow>
    where TRow : class
{
    private PostgresServer _server = null!;

    /// <summary>How many rows one copy carries.</summary>
    [Params(10_000, 100_000)]
    public int RowCount { get; set; }

    /// <summary>The open connection every benchmark copies through.</summary>
    protected NpgsqlConnection Connection { get; private set; } = null!;

    /// <summary>The rows to copy, built before anything is timed.</summary>
    protected IReadOnlyList<TRow> Rows { get; private set; } = null!;

    /// <summary>The unquoted name of the destination table.</summary>
    protected abstract string TableName { get; }

    /// <summary>The statement creating the destination table.</summary>
    protected abstract string CreateTableStatement { get; }

    /// <summary>Builds the rows for one parameter combination.</summary>
    /// <param name="count">How many rows to build.</param>
    protected abstract IReadOnlyList<TRow> GenerateRows(int count);

    [GlobalSetup]
    public async Task GlobalSetupAsync()
    {
        _server = await PostgresServer.StartAsync();
        Connection = new NpgsqlConnection(_server.ConnectionString);
        await Connection.OpenAsync();

        await ExecuteAsync($"DROP TABLE IF EXISTS {TableName};");
        await ExecuteAsync(CreateTableStatement);

        Rows = GenerateRows(RowCount);
    }

    /// <summary>
    /// Empties the table before every timed copy. <c>TRUNCATE</c> rather than
    /// <c>DROP</c>/<c>CREATE</c>: the table definition is not what a copy benchmark should be
    /// measuring, and a table that grew across iterations would make each one slower than the
    /// last for a reason that has nothing to do with this library.
    /// </summary>
    [IterationSetup]
    public void TruncateTable()
    {
        using var command = Connection.CreateCommand();
        command.CommandText = $"TRUNCATE {TableName};";
        command.ExecuteNonQuery();
    }

    [GlobalCleanup]
    public async Task GlobalCleanupAsync()
    {
        await Connection.DisposeAsync();
        await _server.DisposeAsync();
    }

    private async Task ExecuteAsync(string statement)
    {
        await using var command = Connection.CreateCommand();
        command.CommandText = statement;
        await command.ExecuteNonQueryAsync();
    }
}
