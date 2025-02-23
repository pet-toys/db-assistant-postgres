using System;
using NpgsqlTypes;

namespace PetToys.DbAssistant.Postgres.Extensions;

public static class NumericTypeExtensions
{
    public static BulkContextBuilder<TEntity> MapSmallInt<TEntity>(this BulkContextBuilder<TEntity> builder, string columnName, Func<TEntity, short?> getter)
        where TEntity : class
    {
        return builder.Map(columnName, getter, NpgsqlDbType.Smallint);
    }

    public static BulkContextBuilder<TEntity> MapInteger<TEntity>(this BulkContextBuilder<TEntity> builder, string columnName, Func<TEntity, int?> getter)
        where TEntity : class
    {
        return builder.Map(columnName, getter, NpgsqlDbType.Integer);
    }

    public static BulkContextBuilder<TEntity> MapBigInt<TEntity>(this BulkContextBuilder<TEntity> builder, string columnName, Func<TEntity, long?> getter)
        where TEntity : class
    {
        return builder.Map(columnName, getter, NpgsqlDbType.Bigint);
    }

    public static BulkContextBuilder<TEntity> MapNumeric<TEntity>(this BulkContextBuilder<TEntity> builder, string columnName, Func<TEntity, decimal?> getter)
        where TEntity : class
    {
        return builder.Map(columnName, getter, NpgsqlDbType.Numeric);
    }

    public static BulkContextBuilder<TEntity> MapReal<TEntity>(this BulkContextBuilder<TEntity> builder, string columnName, Func<TEntity, float?> getter)
        where TEntity : class
    {
        return builder.Map(columnName, getter, NpgsqlDbType.Real);
    }

    public static BulkContextBuilder<TEntity> MapDouble<TEntity>(this BulkContextBuilder<TEntity> builder, string columnName, Func<TEntity, double?> getter)
        where TEntity : class
    {
        return builder.Map(columnName, getter, NpgsqlDbType.Double);
    }
}