using System;
using NpgsqlTypes;

namespace PetToys.DbAssistant.Postgres.Extensions;

public static class MonetaryTypeExtensions
{
    public static BulkContextBuilder<TEntity> MapMoney<TEntity>(this BulkContextBuilder<TEntity> builder, string columnName, Func<TEntity, decimal?> getter)
        where TEntity : class
    {
        return builder.Map(columnName, getter, NpgsqlDbType.Money);
    }
}