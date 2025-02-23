using System;
using NpgsqlTypes;

namespace PetToys.DbAssistant.Postgres.Extensions;

public static class StringTypeExtensions
{
    public static BulkContextBuilder<TEntity> MapVarchar<TEntity>(this BulkContextBuilder<TEntity> builder, string columnName, Func<TEntity, string?> getter)
        where TEntity : class
    {
        return builder.Map(columnName, getter, NpgsqlDbType.Varchar);
    }

    public static BulkContextBuilder<TEntity> MapCharacter<TEntity>(this BulkContextBuilder<TEntity> builder, string columnName, Func<TEntity, string?> getter)
        where TEntity : class
    {
        return builder.Map(columnName, getter, NpgsqlDbType.Char);
    }

    public static BulkContextBuilder<TEntity> MapText<TEntity>(this BulkContextBuilder<TEntity> builder, string columnName, Func<TEntity, string?> getter)
        where TEntity : class
    {
        return builder.Map(columnName, getter, NpgsqlDbType.Text);
    }
}