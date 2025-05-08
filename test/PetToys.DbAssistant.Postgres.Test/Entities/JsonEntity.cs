using NpgsqlTypes;

namespace PetToys.DbAssistant.Postgres.Test.Entities;

internal sealed class JsonEntity
{
    [DbColumn("json", NpgsqlDbType.Json)]
    public string Json { get; init; } = "{\"key\": \"value\"}";

    [DbColumn("nullable_json", NpgsqlDbType.Json, true)]
    public string? NullableJson { get; init; }

    [DbColumn("jsonb", NpgsqlDbType.Jsonb)]
    public string Jsonb { get; init; } = "{\"key\": \"value\"}";

    [DbColumn("nullable_jsonb", NpgsqlDbType.Jsonb, true)]
    public string? NullableJsonb { get; init; }
}
