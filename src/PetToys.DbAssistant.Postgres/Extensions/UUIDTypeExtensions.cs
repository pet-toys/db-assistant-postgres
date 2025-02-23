using System;
using NpgsqlTypes;

namespace PetToys.DbAssistant.Postgres.Extensions;

public static class UUIDTypeExtensions
{
    public static BulkContextBuilder<TEntity> MapUUID<TEntity>(this BulkContextBuilder<TEntity> builder, string columnName, Func<TEntity, Guid?> getter)
        where TEntity : class
    {
        return builder.Map(columnName, getter, NpgsqlDbType.Uuid);
    }
}