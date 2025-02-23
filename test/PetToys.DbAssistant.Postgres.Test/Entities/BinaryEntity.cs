using NpgsqlTypes;

namespace PetToys.DbAssistant.Postgres.Test.Entities;

internal sealed class BinaryEntity
{
    [DbColumn("bytes", NpgsqlDbType.Bytea)]
    public byte[] Bytes { get; init; } = [0x01, 0x02, 0x03, 0x04, 0x05];

    [DbColumn("nullable_bytes", NpgsqlDbType.Bytea, true)]
    public byte[]? NullableBytes { get; init; }

    [DbColumn("nullable_bytes_has_value", NpgsqlDbType.Bytea, true)]
    public byte[]? NullableBytesHasValue { get; init; } = [0x01, 0x02, 0x03, 0x04, 0x05];
}
