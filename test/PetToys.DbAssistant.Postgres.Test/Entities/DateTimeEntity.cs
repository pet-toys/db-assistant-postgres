using System;
using NpgsqlTypes;

namespace PetToys.DbAssistant.Postgres.Test.Entities;

internal sealed class DateTimeEntity
{
    // DateTime => Date
    [DbColumn("datetime_date", NpgsqlDbType.Date)]
    public DateTime DateTimeToDate { get; init; } = DateTime.Now;

    [DbColumn("nullable_datetime_date", NpgsqlDbType.Date, true)]
    public DateTime? NullableDateTimeToDate { get; init; }

    // TimeSpan => Time
    [DbColumn("timespan_time", NpgsqlDbType.Time)]
    public TimeSpan TimeSpanToTime { get; init; } = DateTime.Now.TimeOfDay;

    [DbColumn("nullable_timespan_time", NpgsqlDbType.Time, true)]
    public TimeSpan? NullableTimeSpanToTime { get; init; }

    // DateTime => Timestamp
    [DbColumn("datetime_timestamp", NpgsqlDbType.Timestamp)]
    public DateTime DateTimeToTimestamp { get; init; } = DateTime.Now;

    [DbColumn("nullable_datetime_timestamp", NpgsqlDbType.Timestamp, true)]
    public DateTime? NullableDateTimeToTimestamp { get; init; }

    // DateTime => TimestampTz
    [DbColumn("datetime_timestamp_tz", NpgsqlDbType.TimestampTz)]
    public DateTime DateTimeToTimestampTz { get; init; } = DateTime.UtcNow;

    [DbColumn("nullable_datetime_timestamp_tz", NpgsqlDbType.TimestampTz, true)]
    public DateTime? NullableDateTimeToTimestampTz { get; init; }

    // DateTimeOffset => TimestampTz
    [DbColumn("datetime_offset_timestamp_tz", NpgsqlDbType.TimestampTz)]
    public DateTimeOffset DateTimeOffsetToTimestampTz { get; init; } = DateTimeOffset.UtcNow;

    [DbColumn("nullable_datetime_offset_timestamp_tz", NpgsqlDbType.TimestampTz, true)]
    public DateTimeOffset? NullableDateTimeOffsetToTimestampTz { get; init; }

    // TimeSpan => Interval
    [DbColumn("timespan_interval", NpgsqlDbType.Interval)]
    public TimeSpan TimeSpanToInterval { get; init; } = DateTime.Now.TimeOfDay;

    [DbColumn("nullable_timespan_interval", NpgsqlDbType.Interval, true)]
    public TimeSpan? NullableTimeSpanToInterval { get; init; }

    // DateTimeOffset => TimeTz
    [DbColumn("datetime_offset_time_tz", NpgsqlDbType.TimeTz)]
    public DateTimeOffset DateTimeOffsetToTimeTz { get; init; } = DateTimeOffset.UtcNow;

    [DbColumn("nullable_datetime_offset_time_tz", NpgsqlDbType.TimeTz, true)]
    public DateTimeOffset? NullableDateTimeOffsetToTimeTz { get; init; }
}
