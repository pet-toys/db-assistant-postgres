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

    public async ValueTask<ulong> WriteDataAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        if (_columns.Count == 0) return 0;
        var wasClosed = _connection.State == ConnectionState.Closed;
        try
        {
            if (wasClosed) await _connection.OpenAsync(cancellationToken);
            await using var binaryCopyWriter = await _connection.BeginBinaryImportAsync(GetCopyCommand(), cancellationToken);
            await WriteToStreamAsync(binaryCopyWriter, entities, cancellationToken);
            return await binaryCopyWriter.CompleteAsync(cancellationToken);
        }
        finally
        {
            if (wasClosed) await _connection.CloseAsync();
        }
    }

    public async ValueTask<ulong> WriteDataAsync(IAsyncEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        if (_columns.Count == 0) return 0;
        var wasClosed = _connection.State == ConnectionState.Closed;
        try
        {
            if (wasClosed) await _connection.OpenAsync(cancellationToken);
            await using var binaryCopyWriter = await _connection.BeginBinaryImportAsync(GetCopyCommand(), cancellationToken);
            await WriteToStreamAsync(binaryCopyWriter, entities, cancellationToken);
            return await binaryCopyWriter.CompleteAsync(cancellationToken);
        }
        finally
        {
            if (wasClosed) await _connection.CloseAsync();
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
                    await writer.WriteNullAsync(cancellationToken);
                }
                else
                {
                    await writer.WriteAsync(value, dbType, cancellationToken);
                }
            },
            dbType,
            typeof(TProperty));
    }

    private async Task WriteToStreamAsync(NpgsqlBinaryImporter writer, IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        foreach (var entity in entities)
        {
            await WriteToStreamAsync(writer, entity, cancellationToken);
        }
    }

    private async Task WriteToStreamAsync(NpgsqlBinaryImporter writer, IAsyncEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        await foreach (var entity in entities.WithCancellation(cancellationToken))
        {
            await WriteToStreamAsync(writer, entity, cancellationToken);
        }
    }

    private async Task WriteToStreamAsync(NpgsqlBinaryImporter writer, TEntity entity, CancellationToken cancellationToken)
    {
        await writer.StartRowAsync(cancellationToken);

        foreach (var columnDefinition in _columns)
        {
            await columnDefinition.WriteAsync(writer, entity, cancellationToken);
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