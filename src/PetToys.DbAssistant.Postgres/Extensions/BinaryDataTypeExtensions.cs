using System;
using NpgsqlTypes;

namespace PetToys.DbAssistant.Postgres.Extensions;

public static class BinaryDataTypeExtensions
{
    public static BulkContextBuilder<TEntity> MapByteArray<TEntity>(this BulkContextBuilder<TEntity> builder, string columnName, Func<TEntity, byte[]?> getter)
        where TEntity : class
    {
        return builder.Map(columnName, getter, NpgsqlDbType.Bytea);
    }
}