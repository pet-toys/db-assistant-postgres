using System;
using NpgsqlTypes;

namespace PetToys.DbAssistant.Postgres.Extensions;

public static class JsonTypeExtensions
{
    public static BulkContextBuilder<TEntity> MapJson<TEntity>(this BulkContextBuilder<TEntity> builder, string columnName, Func<TEntity, string?> getter)
        where TEntity : class
    {
        return builder.Map(columnName, getter, NpgsqlDbType.Json);
    }

    public static BulkContextBuilder<TEntity> MapJsonb<TEntity>(this BulkContextBuilder<TEntity> builder, string columnName, Func<TEntity, string?> getter)
        where TEntity : class
    {
        return builder.Map(columnName, getter, NpgsqlDbType.Jsonb);
    }
}