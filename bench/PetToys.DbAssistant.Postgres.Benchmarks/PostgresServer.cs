using System;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;

namespace PetToys.DbAssistant.Postgres.Benchmarks;

/// <summary>
/// The server a run measures against.
/// </summary>
/// <remarks>
/// <para>
/// By default the run provisions its own: a container from a pinned image, started once per
/// benchmark class, since BenchmarkDotNet gives every class a process of its own. Seconds of
/// startup against minutes of measurement.
/// </para>
/// <para>
/// A container on an overlay filesystem, with default <c>fsync</c> and default
/// <c>shared_buffers</c>, is a fair place to compare two versions of this library against each
/// other and a poor stand-in for the server anybody actually copies into. Setting
/// <see cref="ConnectionStringVariable"/> points the run at a server of the operator's own, which
/// is the only way to get a duration that means anything outside this repository.
/// </para>
/// </remarks>
public sealed class PostgresServer : IAsyncDisposable
{
    /// <summary>Names a server to measure against instead of starting a container.</summary>
    public const string ConnectionStringVariable = "POSTGRES_BENCHMARK_CONNECTION_STRING";

    /// <summary>
    /// The image a provisioned server runs. Pinned rather than left to the Testcontainers
    /// default: the recorded baseline names a server version, and a run that silently moved to
    /// another one would not be comparable against it.
    /// </summary>
    public const string Image = "postgres:18-alpine";

    private readonly PostgreSqlContainer? _container;

    private PostgresServer(PostgreSqlContainer? container, string connectionString)
    {
        _container = container;
        ConnectionString = connectionString;
    }

    /// <summary>The connection string of the server this run measures against.</summary>
    public string ConnectionString { get; }

    /// <summary>Whether the server was provisioned by this run rather than named by the operator.</summary>
    public bool IsProvisioned => _container is not null;

    /// <summary>Starts, or connects to, the server for one benchmark class.</summary>
    public static async Task<PostgresServer> StartAsync()
    {
        var configured = Environment.GetEnvironmentVariable(ConnectionStringVariable);
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return new PostgresServer(container: null, configured);
        }

        var container = new PostgreSqlBuilder(Image).Build();
        await container.StartAsync();

        return new PostgresServer(container, container.GetConnectionString());
    }

    /// <summary>Stops the container this run started, if it started one.</summary>
    public async ValueTask DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }
}
