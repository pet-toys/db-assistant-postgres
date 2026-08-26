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
    /// <returns>
    /// The number of rows written, or zero when no column has been mapped, in
    /// which case nothing is sent.
    /// </returns>
    /// <remarks>
    /// A closed connection is opened for the copy and closed again afterwards;
    /// one that was already open is left open.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="entities"/> is <see langword="null"/>.</exception>
    public async ValueTask<ulong> WriteDataAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);
        if (_columns.Count == 0) return 0;
        var wasClosed = _connection.State == ConnectionState.Closed;
        try
        {
            if (wasClosed) await _connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            var binaryCopyWriter = await _connection.BeginBinaryImportAsync(GetCopyCommand(), cancellationToken).ConfigureAwait(false);
            await using (binaryCopyWriter.ConfigureAwait(false))
            {
                await WriteToStreamAsync(binaryCopyWriter, entities, cancellationToken).ConfigureAwait(false);
                return await binaryCopyWriter.CompleteAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            if (wasClosed) await _connection.CloseAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Copies the entities of an asynchronous sequence into the destination table.
    /// </summary>
    /// <param name="entities">The entities to copy.</param>
    /// <param name="cancellationToken">Cancels the copy.</param>
    /// <returns>
    /// The number of rows written, or zero when no column has been mapped, in
    /// which case nothing is sent.
    /// </returns>
    /// <remarks>
    /// A closed connection is opened for the copy and closed again afterwards;
    /// one that was already open is left open.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="entities"/> is <see langword="null"/>.</exception>
    public async ValueTask<ulong> WriteDataAsync(IAsyncEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);
        if (_columns.Count == 0) return 0;
        var wasClosed = _connection.State == ConnectionState.Closed;
        try
        {
            if (wasClosed) await _connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            var binaryCopyWriter = await _connection.BeginBinaryImportAsync(GetCopyCommand(), cancellationToken).ConfigureAwait(false);
            await using (binaryCopyWriter.ConfigureAwait(false))
            {
                await WriteToStreamAsync(binaryCopyWriter, entities, cancellationToken).ConfigureAwait(false);
                return await binaryCopyWriter.CompleteAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            if (wasClosed) await _connection.CloseAsync().ConfigureAwait(false);
        }
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
