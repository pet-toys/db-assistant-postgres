using System;
using NpgsqlTypes;

namespace PetToys.DbAssistant.Postgres.Extensions;

public static class BooleanTypeExtensions
{
    public static BulkContextBuilder<TEntity> MapBoolean<TEntity>(this BulkContextBuilder<TEntity> builder, string columnName, Func<TEntity, bool?> getter)
        where TEntity : class
    {
        return builder.Map(columnName, getter, NpgsqlDbType.Boolean);
    }
}