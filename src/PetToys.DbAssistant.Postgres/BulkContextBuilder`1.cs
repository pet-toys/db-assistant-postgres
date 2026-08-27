using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using PetToys.DbAssistant.Postgres.Model;

namespace PetToys.DbAssistant.Postgres;

/// <summary>
/// Maps entity properties to the columns of a destination table and copies the
/// entities into it with a binary <c>COPY</c>.
/// </summary>
/// <typeparam name="TEntity">The entity type being copied.</typeparam>
public sealed class BulkContextBuilder<TEntity>
    where TEntity : class
{
    private readonly NpgsqlConnection _connection;
    private readonly string _tableName;
    private readonly string? _schemaName;
    private readonly List<ColumnDefinition<TEntity>> _columns = [];

    internal BulkContextBuilder(NpgsqlConnection connection, string tableName, string? schemaName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        if (schemaName is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(schemaName);
        }

        _connection = connection;
        _tableName = tableName;
        _schemaName = schemaName;
    }

    /// <summary>
    /// Copies the entities into the destination table.
    /// </summary>
    /// <param name="entities">The entities to copy.</param>
    /// <param name="cancellationToken">Cancels the copy.</param>
    /// <returns>The number of rows written.</returns>
    /// <remarks>
    /// A closed connection is opened for the copy and closed again afterwards, as
    /// is a broken one; a connection that was already open is left open.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="entities"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">
    /// No column has been mapped, or the connection is busy with another operation.
    /// </exception>
    public async ValueTask<ulong> WriteDataAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);

        return await CopyAsync((writer, token) => WriteToStreamAsync(writer, entities, token), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Copies the entities of an asynchronous sequence into the destination table.
    /// </summary>
    /// <param name="entities">The entities to copy.</param>
    /// <param name="cancellationToken">Cancels the copy.</param>
    /// <returns>The number of rows written.</returns>
    /// <remarks>
    /// A closed connection is opened for the copy and closed again afterwards, as
    /// is a broken one; a connection that was already open is left open.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="entities"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">
    /// No column has been mapped, or the connection is busy with another operation.
    /// </exception>
    public async ValueTask<ulong> WriteDataAsync(IAsyncEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);

        return await CopyAsync((writer, token) => WriteToStreamAsync(writer, entities, token), cancellationToken).ConfigureAwait(false);
    }

    internal BulkContextBuilder<TEntity> Map<TProperty>(string columnName, Func<TEntity, TProperty?> getter, NpgsqlDbType dbType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        return AddColumn(
            columnName,
            async (writer, entity, cancellationToken) =>
            {
                var value = getter(entity);

                if (value is null)
                {
                    await writer.WriteNullAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await writer.WriteAsync(value, dbType, cancellationToken).ConfigureAwait(false);
                }
            },
            dbType,
            typeof(TProperty));
    }

    internal BulkContextBuilder<TEntity> MapUtcTimeStampTz(string columnName, Func<TEntity, DateTime?> getter)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        return AddColumn(
            columnName,
            async (writer, entity, cancellationToken) =>
            {
                var value = getter(entity);

                if (value is null)
                {
                    await writer.WriteNullAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    try
                    {
                        await writer.WriteAsync(value.Value, NpgsqlDbType.TimestampTz, cancellationToken).ConfigureAwait(false);
                    }
                    // The filter keeps the hot path free of a per-row Kind check: it only
                    // runs when the write already threw, turning Npgsql's unqualified
                    // "Cannot write DateTime with Kind=..." into an actionable, column-named error.
                    catch (Exception exception) when (value.Value.Kind != DateTimeKind.Utc)
                    {
                        throw new InvalidOperationException(
                            $"Column '{columnName}' maps a DateTime to timestamptz, which requires DateTimeKind.Utc, but the value has Kind={value.Value.Kind}. " +
                            "Convert it to UTC (e.g. DateTime.ToUniversalTime()) or map the column from a DateTimeOffset via MapTimeStampTz(..., Func<TEntity, DateTimeOffset?>).",
                            exception);
                    }
                }
            },
            NpgsqlDbType.TimestampTz,
            typeof(DateTime));
    }

    /// <summary>
    /// Runs one binary <c>COPY</c>: it validates the mapping and the connection,
    /// opens the connection for the duration if it is not already open - which
    /// includes a broken one, see <see cref="RequiresOpening"/> - and hands the
    /// importer to <paramref name="writeRows"/>, which is the only part the two
    /// public overloads differ in.
    /// </summary>
    private async ValueTask<ulong> CopyAsync(Func<NpgsqlBinaryImporter, CancellationToken, Task> writeRows, CancellationToken cancellationToken)
    {
        if (_columns.Count == 0)
        {
            throw new InvalidOperationException(
                "At least one column must be mapped before data can be copied. Call a Map* method on the bulk context first.");
        }

        var mustOpen = RequiresOpening();
        try
        {
            if (mustOpen) await _connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            var binaryCopyWriter = await _connection.BeginBinaryImportAsync(GetCopyCommand(), cancellationToken).ConfigureAwait(false);
            await using (binaryCopyWriter.ConfigureAwait(false))
            {
                await writeRows(binaryCopyWriter, cancellationToken).ConfigureAwait(false);
                return await binaryCopyWriter.CompleteAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            if (mustOpen) await _connection.CloseAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Decides whether the copy has to open the connection itself, and rejects the
    /// states it can do neither from.
    /// </summary>
    /// <remarks>
    /// The decision reads <see cref="NpgsqlConnection.FullState"/>, not
    /// <see cref="NpgsqlConnection.State"/>: the latter collapses every state into
    /// <see cref="ConnectionState.Open"/> or <see cref="ConnectionState.Closed"/>,
    /// so a connection busy with another command reports itself as open, the copy
    /// proceeds, and the importer fails with an error that names neither the
    /// connection nor what it was doing. A broken connection is opened like a
    /// closed one - Npgsql resets and reconnects it, which is the behaviour this
    /// type has always had - and is closed again afterwards.
    /// </remarks>
    private bool RequiresOpening()
    {
        var state = _connection.FullState;

        return state switch
        {
            ConnectionState.Closed or ConnectionState.Broken => true,
            ConnectionState.Open => false,
            _ => throw new InvalidOperationException(
                $"The connection cannot start a copy while it is {state}. Let it finish its current " +
                "operation, or run the copy on a connection of its own."),
        };
    }

    private async Task WriteToStreamAsync(NpgsqlBinaryImporter writer, IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        foreach (var entity in entities)
        {
            await WriteToStreamAsync(writer, entity, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task WriteToStreamAsync(NpgsqlBinaryImporter writer, IAsyncEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        await foreach (var entity in entities.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            await WriteToStreamAsync(writer, entity, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task WriteToStreamAsync(NpgsqlBinaryImporter writer, TEntity entity, CancellationToken cancellationToken)
    {
        await writer.StartRowAsync(cancellationToken).ConfigureAwait(false);

        foreach (var columnDefinition in _columns)
        {
            await columnDefinition.WriteAsync(writer, entity, cancellationToken).ConfigureAwait(false);
        }
    }

    private BulkContextBuilder<TEntity> AddColumn(string columnName, Func<NpgsqlBinaryImporter, TEntity, CancellationToken, Task> action, NpgsqlDbType dbType, Type clrType)
    {
        if (_columns.Any(column => string.Equals(column.ColumnName, columnName, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException($"Column '{columnName}' is already mapped.");
        }

        _columns.Add(new ColumnDefinition<TEntity>
        {
            ColumnName = columnName,
            DbType = dbType,
            ClrType = clrType,
            WriteAsync = action,
        });

        return this;
    }

    private string GetCopyCommand()
    {
        var commaSeparatedColumns = string.Join(", ", _columns.Select(x => x.ColumnName.QuoteIdentifier()));
        var tableName = string.IsNullOrWhiteSpace(_schemaName)
            ? _tableName.QuoteIdentifier()
            : $"{_schemaName.QuoteIdentifier()}.{_tableName.QuoteIdentifier()}";

        return $"COPY {tableName}({commaSeparatedColumns}) FROM STDIN BINARY;";
    }
}
