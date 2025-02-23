using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;

namespace PetToys.DbAssistant.Postgres.Model;

internal sealed class ColumnDefinition<TEntity>
    where TEntity : class
{
    public required string ColumnName { get; init; }

    public required NpgsqlDbType DbType { get; init; }

    public required Type ClrType { get; init; }

    public required Func<NpgsqlBinaryImporter, TEntity, CancellationToken, Task> WriteAsync { get; init; }
}
