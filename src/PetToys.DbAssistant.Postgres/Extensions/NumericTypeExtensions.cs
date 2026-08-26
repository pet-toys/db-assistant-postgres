using System;
using NpgsqlTypes;

namespace PetToys.DbAssistant.Postgres.Extensions;

/// <summary>
/// Maps entity properties to PostgreSQL numeric columns.
/// </summary>
public static class NumericTypeExtensions
{
    /// <summary>
    /// Maps a <c>short</c> value to a PostgreSQL <c>smallint</c> column.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="builder">The builder being configured.</param>
    /// <param name="columnName">The name of the target column.</param>
    /// <param name="getter">Reads the value from an entity; <see langword="null"/> is written as SQL <c>NULL</c>.</param>
    /// <returns>The same <paramref name="builder"/>, so that calls can be chained.</returns>
    public static BulkContextBuilder<TEntity> MapSmallInt<TEntity>(this BulkContextBuilder<TEntity> builder, string columnName, Func<TEntity, short?> getter)
        where TEntity : class
    {
        return builder.Map(columnName, getter, NpgsqlDbType.Smallint);
    }

    /// <summary>
    /// Maps an <c>int</c> value to a PostgreSQL <c>integer</c> column.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="builder">The builder being configured.</param>
    /// <param name="columnName">The name of the target column.</param>
    /// <param name="getter">Reads the value from an entity; <see langword="null"/> is written as SQL <c>NULL</c>.</param>
    /// <returns>The same <paramref name="builder"/>, so that calls can be chained.</returns>
    public static BulkContextBuilder<TEntity> MapInteger<TEntity>(this BulkContextBuilder<TEntity> builder, string columnName, Func<TEntity, int?> getter)
        where TEntity : class
    {
        return builder.Map(columnName, getter, NpgsqlDbType.Integer);
    }

    /// <summary>
    /// Maps a <c>long</c> value to a PostgreSQL <c>bigint</c> column.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="builder">The builder being configured.</param>
    /// <param name="columnName">The name of the target column.</param>
    /// <param name="getter">Reads the value from an entity; <see langword="null"/> is written as SQL <c>NULL</c>.</param>
    /// <returns>The same <paramref name="builder"/>, so that calls can be chained.</returns>
    public static BulkContextBuilder<TEntity> MapBigInt<TEntity>(this BulkContextBuilder<TEntity> builder, string columnName, Func<TEntity, long?> getter)
        where TEntity : class
    {
        return builder.Map(columnName, getter, NpgsqlDbType.Bigint);
    }

    /// <summary>
    /// Maps a <c>decimal</c> value to a PostgreSQL <c>numeric</c> column.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="builder">The builder being configured.</param>
    /// <param name="columnName">The name of the target column.</param>
    /// <param name="getter">Reads the value from an entity; <see langword="null"/> is written as SQL <c>NULL</c>.</param>
    /// <returns>The same <paramref name="builder"/>, so that calls can be chained.</returns>
    public static BulkContextBuilder<TEntity> MapNumeric<TEntity>(this BulkContextBuilder<TEntity> builder, string columnName, Func<TEntity, decimal?> getter)
        where TEntity : class
    {
        return builder.Map(columnName, getter, NpgsqlDbType.Numeric);
    }

    /// <summary>
    /// Maps a <c>float</c> value to a PostgreSQL <c>real</c> column.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="builder">The builder being configured.</param>
    /// <param name="columnName">The name of the target column.</param>
    /// <param name="getter">Reads the value from an entity; <see langword="null"/> is written as SQL <c>NULL</c>.</param>
    /// <returns>The same <paramref name="builder"/>, so that calls can be chained.</returns>
    public static BulkContextBuilder<TEntity> MapReal<TEntity>(this BulkContextBuilder<TEntity> builder, string columnName, Func<TEntity, float?> getter)
        where TEntity : class
    {
        return builder.Map(columnName, getter, NpgsqlDbType.Real);
    }

    /// <summary>
    /// Maps a <c>double</c> value to a PostgreSQL <c>double precision</c> column.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="builder">The builder being configured.</param>
    /// <param name="columnName">The name of the target column.</param>
    /// <param name="getter">Reads the value from an entity; <see langword="null"/> is written as SQL <c>NULL</c>.</param>
    /// <returns>The same <paramref name="builder"/>, so that calls can be chained.</returns>
    public static BulkContextBuilder<TEntity> MapDouble<TEntity>(this BulkContextBuilder<TEntity> builder, string columnName, Func<TEntity, double?> getter)
        where TEntity : class
    {
        return builder.Map(columnName, getter, NpgsqlDbType.Double);
    }
}
