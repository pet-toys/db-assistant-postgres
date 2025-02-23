using System;
using NpgsqlTypes;

namespace PetToys.DbAssistant.Postgres.Test.Entities;

internal sealed class UUIDEntity
{
    [DbColumn("uuid", NpgsqlDbType.Uuid)]
    public Guid UUID { get; init; } = Guid.NewGuid();

    [DbColumn("nullable_uuid", NpgsqlDbType.Uuid, true)]
    public Guid? NullableUUID { get; init; }

    [DbColumn("nullable_uuid_has_value", NpgsqlDbType.Uuid, true)]
    public Guid? NullableUUIDHasValue { get; init; } = Guid.NewGuid();
}
