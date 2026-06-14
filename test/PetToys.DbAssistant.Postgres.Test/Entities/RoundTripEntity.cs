using System;
using NpgsqlTypes;

namespace PetToys.DbAssistant.Postgres.Test.Entities;

/// <summary>
/// A compact cross-section entity used by the value round-trip test: one column
/// per representative mapping family (integer, text, boolean, numeric, uuid,
/// bytea, timestamp), with nullable columns to exercise the null path.
/// </summary>
internal sealed class RoundTripEntity
{
    [DbColumn("id", NpgsqlDbType.Integer)]
    public int Id { get; init; }

    [DbColumn("label", NpgsqlDbType.Text, true)]
    public string? Label { get; init; }

    [DbColumn("flag", NpgsqlDbType.Boolean)]
    public bool Flag { get; init; }

    [DbColumn("amount", NpgsqlDbType.Numeric)]
    public decimal Amount { get; init; }

    [DbColumn("identifier", NpgsqlDbType.Uuid)]
    public Guid Identifier { get; init; }

    [DbColumn("payload", NpgsqlDbType.Bytea, true)]
    public byte[]? Payload { get; init; }

    [DbColumn("created_at", NpgsqlDbType.Timestamp)]
    public DateTime CreatedAt { get; init; }
}
