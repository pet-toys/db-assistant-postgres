using System;
using NpgsqlTypes;

namespace PetToys.DbAssistant.Postgres.Extensions;

/// <summary>
/// Maps entity properties to PostgreSQL character columns.
/// </summary>
public static class StringTypeExtensions
{
    /// <summary>
    /// Maps a <c>string</c> value to a PostgreSQL <c>character varying</c> column.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="builder">The builder being configured.</param>
    /// <param name="columnName">The name of the target column.</param>
    /// <param name="getter">Reads the value from an entity; <see langword="null"/> is written as SQL <c>NULL</c>.</param>
    /// <returns>The same <paramref name="builder"/>, so that calls can be chained.</returns>
    public static BulkContextBuilder<TEntity> MapVarchar<TEntity>(this BulkContextBuilder<TEntity> builder, string columnName, Func<TEntity, string?> getter)
        where TEntity : class
    {
        return builder.Map(columnName, getter, NpgsqlDbType.Varchar);
    }

    /// <summary>
    /// Maps a <c>string</c> value to a PostgreSQL <c>character</c> column.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="builder">The builder being configured.</param>
    /// <param name="columnName">The name of the target column.</param>
    /// <param name="getter">Reads the value from an entity; <see langword="null"/> is written as SQL <c>NULL</c>.</param>
    /// <returns>The same <paramref name="builder"/>, so that calls can be chained.</returns>
    public static BulkContextBuilder<TEntity> MapCharacter<TEntity>(this BulkContextBuilder<TEntity> builder, string columnName, Func<TEntity, string?> getter)
        where TEntity : class
    {
        return builder.Map(columnName, getter, NpgsqlDbType.Char);
    }

    /// <summary>
    /// Maps a <c>string</c> value to a PostgreSQL <c>text</c> column.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="builder">The builder being configured.</param>
    /// <param name="columnName">The name of the target column.</param>
    /// <param name="getter">Reads the value from an entity; <see langword="null"/> is written as SQL <c>NULL</c>.</param>
    /// <returns>The same <paramref name="builder"/>, so that calls can be chained.</returns>
    public static BulkContextBuilder<TEntity> MapText<TEntity>(this BulkContextBuilder<TEntity> builder, string columnName, Func<TEntity, string?> getter)
        where TEntity : class
    {
        return builder.Map(columnName, getter, NpgsqlDbType.Text);
    }
}
