using System;
using NpgsqlTypes;

namespace PetToys.DbAssistant.Postgres.Test.Entities;

/// <summary>
/// A single <c>timestamptz</c> column, used to exercise the UTC requirement of
/// the <see cref="DateTime"/> overload of <c>MapTimeStampTz</c>.
/// </summary>
internal sealed class TimestampTzEntity
{
    [DbColumn("created_at", NpgsqlDbType.TimestampTz)]
    public DateTime CreatedAt { get; init; }
}
