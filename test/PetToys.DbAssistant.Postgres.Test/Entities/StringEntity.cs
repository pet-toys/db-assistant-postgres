using NpgsqlTypes;

namespace PetToys.DbAssistant.Postgres.Test.Entities;

internal sealed class StringEntity
{
    // String => Varchar
    [DbColumn("string_varchar", NpgsqlDbType.Varchar, Length = 500)]
    public string StringToVarchar { get; init; } = "varchar";

    [DbColumn("nullable_string_varchar", NpgsqlDbType.Varchar, true, Length = 500)]
    public string? NullableStringToVarchar { get; init; }

    // String => Char
    [DbColumn("string_char", NpgsqlDbType.Char, Length = 50)]
    public string StringToChar { get; init; } = "char";

    [DbColumn("nullable_string_char", NpgsqlDbType.Char, true, Length = 50)]
    public string? NullableStringToChar { get; init; }

    // String => Text
    [DbColumn("text", NpgsqlDbType.Text)]
    public string Text { get; init; } = "Text";

    [DbColumn("nullable_text", NpgsqlDbType.Text, true)]
    public string? NullableText { get; init; }
}
