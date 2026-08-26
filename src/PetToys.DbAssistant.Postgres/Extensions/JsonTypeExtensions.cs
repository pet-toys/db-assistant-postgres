using System;
using NpgsqlTypes;

namespace PetToys.DbAssistant.Postgres.Extensions;

/// <summary>
/// Maps entity properties to PostgreSQL JSON columns.
/// </summary>
public static class JsonTypeExtensions
{
    /// <summary>
    /// Maps a <c>string</c> value to a PostgreSQL <c>json</c> column.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="builder">The builder being configured.</param>
    /// <param name="columnName">The name of the target column.</param>
    /// <param name="getter">Reads the value from an entity; <see langword="null"/> is written as SQL <c>NULL</c>.</param>
    /// <returns>The same <paramref name="builder"/>, so that calls can be chained.</returns>
    public static BulkContextBuilder<TEntity> MapJson<TEntity>(this BulkContextBuilder<TEntity> builder, string columnName, Func<TEntity, string?> getter)
        where TEntity : class
    {
        return builder.Map(columnName, getter, NpgsqlDbType.Json);
    }

    /// <summary>
    /// Maps a <c>string</c> value to a PostgreSQL <c>jsonb</c> column.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="builder">The builder being configured.</param>
    /// <param name="columnName">The name of the target column.</param>
    /// <param name="getter">Reads the value from an entity; <see langword="null"/> is written as SQL <c>NULL</c>.</param>
    /// <returns>The same <paramref name="builder"/>, so that calls can be chained.</returns>
    public static BulkContextBuilder<TEntity> MapJsonb<TEntity>(this BulkContextBuilder<TEntity> builder, string columnName, Func<TEntity, string?> getter)
        where TEntity : class
    {
        return builder.Map(columnName, getter, NpgsqlDbType.Jsonb);
    }
}
