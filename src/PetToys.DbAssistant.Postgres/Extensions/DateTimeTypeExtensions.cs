using System;
using NpgsqlTypes;

namespace PetToys.DbAssistant.Postgres.Extensions;

/// <summary>
/// Maps entity properties to PostgreSQL date and time columns.
/// </summary>
public static class DateTimeTypeExtensions
{
    /// <summary>
    /// Maps a <see cref="DateTime"/> value to a PostgreSQL <c>date</c> column.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="helper">The builder being configured.</param>
    /// <param name="columnName">The name of the target column.</param>
    /// <param name="propertyGetter">Reads the value from an entity; <see langword="null"/> is written as SQL <c>NULL</c>.</param>
    /// <returns>The same <paramref name="helper"/>, so that calls can be chained.</returns>
    public static BulkContextBuilder<TEntity> MapDate<TEntity>(this BulkContextBuilder<TEntity> helper, string columnName, Func<TEntity, DateTime?> propertyGetter)
        where TEntity : class
    {
        return helper.Map(columnName, propertyGetter, NpgsqlDbType.Date);
    }

    /// <summary>
    /// Maps a <see cref="TimeSpan"/> value to a PostgreSQL <c>time</c> column.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="helper">The builder being configured.</param>
    /// <param name="columnName">The name of the target column.</param>
    /// <param name="propertyGetter">Reads the value from an entity; <see langword="null"/> is written as SQL <c>NULL</c>.</param>
    /// <returns>The same <paramref name="helper"/>, so that calls can be chained.</returns>
    public static BulkContextBuilder<TEntity> MapTime<TEntity>(this BulkContextBuilder<TEntity> helper, string columnName, Func<TEntity, TimeSpan?> propertyGetter)
        where TEntity : class
    {
        return helper.Map(columnName, propertyGetter, NpgsqlDbType.Time);
    }

    /// <summary>
    /// Maps a <see cref="DateTime"/> value to a PostgreSQL <c>timestamp</c> column.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="helper">The builder being configured.</param>
    /// <param name="columnName">The name of the target column.</param>
    /// <param name="propertyGetter">Reads the value from an entity; <see langword="null"/> is written as SQL <c>NULL</c>.</param>
    /// <returns>The same <paramref name="helper"/>, so that calls can be chained.</returns>
    public static BulkContextBuilder<TEntity> MapTimeStamp<TEntity>(this BulkContextBuilder<TEntity> helper, string columnName, Func<TEntity, DateTime?> propertyGetter)
        where TEntity : class
    {
        return helper.Map(columnName, propertyGetter, NpgsqlDbType.Timestamp);
    }

    /// <summary>
    /// Maps a <see cref="DateTime"/> value to a PostgreSQL <c>timestamptz</c> column.
    /// </summary>
    /// <remarks>
    /// PostgreSQL <c>timestamptz</c> stores a UTC instant, so the source
    /// <see cref="DateTime"/> must have <see cref="DateTimeKind.Utc"/>. A
    /// <see cref="DateTimeKind.Local"/> or <see cref="DateTimeKind.Unspecified"/>
    /// value fails the copy with an error naming the column. Convert the value to
    /// UTC (for example with <see cref="DateTime.ToUniversalTime"/>), or, when the
    /// value already carries an offset, use the
    /// <see cref="MapTimeStampTz{TEntity}(BulkContextBuilder{TEntity}, string, Func{TEntity, DateTimeOffset?})"/>
    /// overload instead.
    /// </remarks>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="helper">The builder being configured.</param>
    /// <param name="columnName">The name of the target column.</param>
    /// <param name="propertyGetter">Reads the value from an entity; <see langword="null"/> is written as SQL <c>NULL</c>.</param>
    /// <returns>The same <paramref name="helper"/>, so that calls can be chained.</returns>
    public static BulkContextBuilder<TEntity> MapTimeStampTz<TEntity>(this BulkContextBuilder<TEntity> helper, string columnName, Func<TEntity, DateTime?> propertyGetter)
        where TEntity : class
    {
        return helper.MapUtcTimeStampTz(columnName, propertyGetter);
    }

    /// <summary>
    /// Maps a <see cref="DateTimeOffset"/> value to a PostgreSQL <c>timestamptz</c> column.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="helper">The builder being configured.</param>
    /// <param name="columnName">The name of the target column.</param>
    /// <param name="propertyGetter">Reads the value from an entity; <see langword="null"/> is written as SQL <c>NULL</c>.</param>
    /// <returns>The same <paramref name="helper"/>, so that calls can be chained.</returns>
    public static BulkContextBuilder<TEntity> MapTimeStampTz<TEntity>(this BulkContextBuilder<TEntity> helper, string columnName, Func<TEntity, DateTimeOffset?> propertyGetter)
        where TEntity : class
    {
        return helper.Map(columnName, propertyGetter, NpgsqlDbType.TimestampTz);
    }

    /// <summary>
    /// Maps a <see cref="TimeSpan"/> value to a PostgreSQL <c>interval</c> column.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="helper">The builder being configured.</param>
    /// <param name="columnName">The name of the target column.</param>
    /// <param name="propertyGetter">Reads the value from an entity; <see langword="null"/> is written as SQL <c>NULL</c>.</param>
    /// <returns>The same <paramref name="helper"/>, so that calls can be chained.</returns>
    public static BulkContextBuilder<TEntity> MapInterval<TEntity>(this BulkContextBuilder<TEntity> helper, string columnName, Func<TEntity, TimeSpan?> propertyGetter)
        where TEntity : class
    {
        return helper.Map(columnName, propertyGetter, NpgsqlDbType.Interval);
    }

    /// <summary>
    /// Maps a <see cref="DateTimeOffset"/> value to a PostgreSQL <c>timetz</c> column.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="helper">The builder being configured.</param>
    /// <param name="columnName">The name of the target column.</param>
    /// <param name="propertyGetter">Reads the value from an entity; <see langword="null"/> is written as SQL <c>NULL</c>.</param>
    /// <returns>The same <paramref name="helper"/>, so that calls can be chained.</returns>
    public static BulkContextBuilder<TEntity> MapTimeTz<TEntity>(this BulkContextBuilder<TEntity> helper, string columnName, Func<TEntity, DateTimeOffset?> propertyGetter)
        where TEntity : class
    {
        return helper.Map(columnName, propertyGetter, NpgsqlDbType.TimeTz);
    }
}
