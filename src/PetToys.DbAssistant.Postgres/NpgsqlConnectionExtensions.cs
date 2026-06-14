using System;
using Npgsql;

namespace PetToys.DbAssistant.Postgres;

public static class NpgsqlConnectionExtensions
{
    public static BulkContextBuilder<TEntity> CreateBulkContext<TEntity>(this NpgsqlConnection connection, string tableName, string? schemaName = null)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(connection);

        return new BulkContextBuilder<TEntity>(connection, tableName, schemaName);
    }
}
