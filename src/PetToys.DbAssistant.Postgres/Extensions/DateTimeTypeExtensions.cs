using System;
using NpgsqlTypes;

namespace PetToys.DbAssistant.Postgres.Extensions;

public static class DateTimeTypeExtensions
{
    public static BulkContextBuilder<TEntity> MapDate<TEntity>(this BulkContextBuilder<TEntity> helper, string columnName, Func<TEntity, DateTime?> propertyGetter)
        where TEntity : class
    {
        return helper.Map(columnName, propertyGetter, NpgsqlDbType.Date);
    }

    public static BulkContextBuilder<TEntity> MapTime<TEntity>(this BulkContextBuilder<TEntity> helper, string columnName, Func<TEntity, TimeSpan?> propertyGetter)
        where TEntity : class
    {
        return helper.Map(columnName, propertyGetter, NpgsqlDbType.Time);
    }

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
    public static BulkContextBuilder<TEntity> MapTimeStampTz<TEntity>(this BulkContextBuilder<TEntity> helper, string columnName, Func<TEntity, DateTime?> propertyGetter)
        where TEntity : class
    {
        return helper.MapUtcTimeStampTz(columnName, propertyGetter);
    }

    public static BulkContextBuilder<TEntity> MapTimeStampTz<TEntity>(this BulkContextBuilder<TEntity> helper, string columnName, Func<TEntity, DateTimeOffset?> propertyGetter)
        where TEntity : class
    {
        return helper.Map(columnName, propertyGetter, NpgsqlDbType.TimestampTz);
    }

    public static BulkContextBuilder<TEntity> MapInterval<TEntity>(this BulkContextBuilder<TEntity> helper, string columnName, Func<TEntity, TimeSpan?> propertyGetter)
        where TEntity : class
    {
        return helper.Map(columnName, propertyGetter, NpgsqlDbType.Interval);
    }

    public static BulkContextBuilder<TEntity> MapTimeTz<TEntity>(this BulkContextBuilder<TEntity> helper, string columnName, Func<TEntity, DateTimeOffset?> propertyGetter)
        where TEntity : class
    {
        return helper.Map(columnName, propertyGetter, NpgsqlDbType.TimeTz);
    }
}
