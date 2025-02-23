using NpgsqlTypes;

namespace PetToys.DbAssistant.Postgres.Test.Entities;

internal sealed class NumericEntity
{
    // Smallint
    [DbColumn("smallint", NpgsqlDbType.Smallint)]
    public short Smallint { get; init; } = short.MaxValue;

    [DbColumn("nullable_smallint", NpgsqlDbType.Smallint, true)]
    public short? NullableSmallint { get; init; }

    [DbColumn("nullable_smallint_has_value", NpgsqlDbType.Smallint, true)]
    public short? NullableSmallintHasValue { get; init; } = short.MinValue;

    // Integer
    [DbColumn("integer", NpgsqlDbType.Integer)]
    public int IntegerCol { get; init; } = int.MaxValue;

    [DbColumn("nullable_integer", NpgsqlDbType.Integer, true)]
    public int? NullableInteger { get; init; }

    [DbColumn("nullable_integer_has_value", NpgsqlDbType.Integer, true)]
    public int? NullableIntegerHasValue { get; init; } = int.MinValue;

    // Bigint
    [DbColumn("bigint", NpgsqlDbType.Bigint)]
    public long Bigint { get; init; } = long.MaxValue;

    [DbColumn("nullable_bigint", NpgsqlDbType.Bigint, true)]
    public long? NullableBigint { get; init; }

    [DbColumn("nullable_bigint_has_value", NpgsqlDbType.Bigint, true)]
    public long? NullableBigintHasValue { get; init; } = long.MinValue;

    // Numeric
    [DbColumn("numeric", NpgsqlDbType.Numeric)]
    public decimal Numeric { get; init; } = decimal.MaxValue;

    [DbColumn("nullable_numeric", NpgsqlDbType.Numeric, true)]
    public decimal? NullableNumeric { get; init; }

    [DbColumn("nullable_numeric_has_value", NpgsqlDbType.Numeric, true)]
    public decimal? NullableNumericHasValue { get; init; } = decimal.MinValue;

    // Real
    [DbColumn("real", NpgsqlDbType.Real)]
    public float Real { get; init; } = float.MaxValue;

    [DbColumn("nullable_real", NpgsqlDbType.Real, true)]
    public float? NullableReal { get; init; }

    [DbColumn("nullable_real_has_value", NpgsqlDbType.Real, true)]
    public float? NullableRealHasValue { get; init; } = float.MinValue;

    // Double
    [DbColumn("double", NpgsqlDbType.Double)]
    public double DoubleCol { get; init; } = double.MaxValue;

    [DbColumn("nullable_double", NpgsqlDbType.Double, true)]
    public double? NullableDouble { get; init; }

    [DbColumn("nullable_double_has_value", NpgsqlDbType.Double, true)]
    public double? NullableDoubleHasValue { get; init; } = double.MinValue;
}