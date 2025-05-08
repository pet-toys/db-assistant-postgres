using System;
using NpgsqlTypes;

namespace PetToys.DbAssistant.Postgres.Test.Entities;

internal sealed class MonetaryEntity
{
    [DbColumn("money", NpgsqlDbType.Money)]
    public decimal Money { get; init; } = Math.Round(decimal.MaxValue / 1_000_000_000_000, 2);

    [DbColumn("nullable_money", NpgsqlDbType.Money, true)]
    public decimal? NullableMoney { get; init; }
}
