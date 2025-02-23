using NpgsqlTypes;

namespace PetToys.DbAssistant.Postgres.Test.Entities;

internal sealed class BooleanEntity
{
    [DbColumn("boolean", NpgsqlDbType.Boolean)]
    public bool Boolean { get; init; } = true;

    [DbColumn("nullable_boolean", NpgsqlDbType.Boolean, true)]
    public bool? NullableBoolean { get; init; }

    [DbColumn("nullable_boolean_has_value", NpgsqlDbType.Boolean, true)]
    public bool? NullableBooleanHasValue { get; init; } = false;
}