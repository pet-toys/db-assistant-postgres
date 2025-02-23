using System;
using NpgsqlTypes;

namespace PetToys.DbAssistant.Postgres.Test.Entities;

internal sealed class DateTimeEntity
{
    private static readonly TimeZoneInfo Tz = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles");

    // DateTime => Date
    [DbColumn("datetime_date", NpgsqlDbType.Date)]
    public DateTime DateTimeToDate { get; init; } = DateTime.Now;

    [DbColumn("nullable_datetime_date", NpgsqlDbType.Date, true)]
    public DateTime? NullableDateTimeToDate { get; init; }

    [DbColumn("nullable_datetime_date_has_value", NpgsqlDbType.Date, true)]
    public DateTime? NullableDateTimeToDateHasValue { get; init; } = DateTime.UtcNow;

    // TimeSpan => Time
    [DbColumn("timespan_time", NpgsqlDbType.Time)]
    public TimeSpan TimeSpanToTime { get; init; } = DateTime.Now.TimeOfDay;

    [DbColumn("nullable_timespan_time", NpgsqlDbType.Time, true)]
    public TimeSpan? NullableTimeSpanToTime { get; init; }

    [DbColumn("nullable_timespan_time_has_value", NpgsqlDbType.Time, true)]
    public TimeSpan? NullableTimeSpanToTimeHasValue { get; init; } = DateTime.UtcNow.TimeOfDay;

    // DateTime => Timestamp
    [DbColumn("datetime_timestamp", NpgsqlDbType.Timestamp)]
    public DateTime DateTimeToTimestamp { get; init; } = DateTime.Now;

    [DbColumn("nullable_datetime_timestamp", NpgsqlDbType.Timestamp, true)]
    public DateTime? NullableDateTimeToTimestamp { get; init; }

    [DbColumn("nullable_datetime_timestamp_has_value", NpgsqlDbType.Timestamp, true)]
    public DateTime? NullableDateTimeToTimestampHasValue { get; init; } = DateTime.Now;

    // DateTime => TimestampTz
    [DbColumn("datetime_timestamp_tz", NpgsqlDbType.TimestampTz)]
    public DateTime DateTimeToTimestampTz { get; init; } = DateTime.UtcNow;

    [DbColumn("nullable_datetime_timestamp_tz", NpgsqlDbType.TimestampTz, true)]
    public DateTime? NullableDateTimeToTimestampTz { get; init; }

    [DbColumn("nullable_datetime_timestamp_tz_has_value", NpgsqlDbType.TimestampTz, true)]
    public DateTime? NullableDateTimeToTimestampTzHasValue { get; init; } = DateTime.UtcNow;

    // DateTimeOffset => TimestampTz
    [DbColumn("datetime_offset_timestamp_tz", NpgsqlDbType.TimestampTz)]
    public DateTimeOffset DateTimeOffsetToTimestampTz { get; init; } = DateTimeOffset.UtcNow;

    [DbColumn("nullable_datetime_offset_timestamp_tz", NpgsqlDbType.TimestampTz, true)]
    public DateTimeOffset? NullableDateTimeOffsetToTimestampTz { get; init; }

    [DbColumn("nullable_datetime_offset_timestamp_tz_has_value", NpgsqlDbType.TimestampTz, true)]
    public DateTimeOffset? NullableDateTimeOffsetToTimestampTzHasValue { get; init; } = DateTimeOffset.UtcNow;

    // TimeSpan => Interval
    [DbColumn("timespan_interval", NpgsqlDbType.Interval)]
    public TimeSpan TimeSpanToInterval { get; init; } = DateTime.Now.TimeOfDay;

    [DbColumn("nullable_timespan_interval", NpgsqlDbType.Interval, true)]
    public TimeSpan? NullableTimeSpanToInterval { get; init; }

    [DbColumn("nullable_timespan_interval_has_value", NpgsqlDbType.Interval, true)]
    public TimeSpan? NullableTimeSpanToIntervalHasValue { get; init; } = DateTime.UtcNow.TimeOfDay;

    // DateTimeOffset => TimeTz
    [DbColumn("datetime_offset_time_tz", NpgsqlDbType.TimeTz)]
    public DateTimeOffset DateTimeOffsetToTimeTz { get; init; } = DateTimeOffset.UtcNow;

    [DbColumn("nullable_datetime_offset_time_tz", NpgsqlDbType.TimeTz, true)]
    public DateTimeOffset? NullableDateTimeOffsetToTimeTz { get; init; }

    [DbColumn("nullable_datetime_offset_time_tz_has_value", NpgsqlDbType.TimeTz, true)]
    public DateTimeOffset? NullableDateTimeOffsetToTimeTzHasValue { get; init; } = DateTimeOffset.UtcNow;
}