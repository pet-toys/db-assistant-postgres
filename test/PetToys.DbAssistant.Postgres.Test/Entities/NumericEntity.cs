using NpgsqlTypes;

namespace PetToys.DbAssistant.Postgres.Test.Entities;

internal sealed class NumericEntity
{
    // Smallint
    [DbColumn("smallint", NpgsqlDbType.Smallint)]
    public short Smallint { get; init; } = short.MaxValue;

    [DbColumn("nullable_smallint", NpgsqlDbType.Smallint, true)]
    public short? NullableSmallint { get; init; }

    // Integer
    [DbColumn("integer", NpgsqlDbType.Integer)]
    public int IntegerCol { get; init; } = int.MaxValue;

    [DbColumn("nullable_integer", NpgsqlDbType.Integer, true)]
    public int? NullableInteger { get; init; }

    // Bigint
    [DbColumn("bigint", NpgsqlDbType.Bigint)]
    public long Bigint { get; init; } = long.MaxValue;

    [DbColumn("nullable_bigint", NpgsqlDbType.Bigint, true)]
    public long? NullableBigint { get; init; }

    // Numeric
    [DbColumn("numeric", NpgsqlDbType.Numeric)]
    public decimal Numeric { get; init; } = decimal.MaxValue;

    [DbColumn("nullable_numeric", NpgsqlDbType.Numeric, true)]
    public decimal? NullableNumeric { get; init; }

    // Real
    [DbColumn("real", NpgsqlDbType.Real)]
    public float Real { get; init; } = float.MaxValue;

    [DbColumn("nullable_real", NpgsqlDbType.Real, true)]
    public float? NullableReal { get; init; }

    // Double
    [DbColumn("double", NpgsqlDbType.Double)]
    public double DoubleCol { get; init; } = double.MaxValue;

    [DbColumn("nullable_double", NpgsqlDbType.Double, true)]
    public double? NullableDouble { get; init; }
}
