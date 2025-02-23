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

    public static BulkContextBuilder<TEntity> MapTimeStampTz<TEntity>(this BulkContextBuilder<TEntity> helper, string columnName, Func<TEntity, DateTime?> propertyGetter)
        where TEntity : class
    {
        return helper.Map(columnName, propertyGetter, NpgsqlDbType.TimestampTz);
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

    public static BulkContextBuilder<TEntity> MapTimeTz<TEntity>(this BulkContextBuilder<TEntity> helper, string columnName, Func<TEntity, TimeSpan?> propertyGetter)
        where TEntity : class
    {
        return helper.Map(columnName, propertyGetter, NpgsqlDbType.TimeTz);
    }
}
