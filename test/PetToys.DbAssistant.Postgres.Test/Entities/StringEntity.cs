using NpgsqlTypes;

namespace PetToys.DbAssistant.Postgres.Test.Entities;

internal sealed class StringEntity
{
    // String => Varchar
    [DbColumn("string_varchar", NpgsqlDbType.Varchar, Length = 50)]
    public string StringToVarchar { get; init; } = "varchar";

    [DbColumn("nullable_string_varchar", NpgsqlDbType.Varchar, true, Length = 50)]
    public string? NullableStringToVarchar { get; init; }

    [DbColumn("nullable_string_varchar_has_value", NpgsqlDbType.Varchar, true, Length = 50)]
    public string? NullableStringToVarcharHasValue { get; init; } = "varchar";

    // String => Char
    [DbColumn("string_char", NpgsqlDbType.Char, Length = 50)]
    public string StringToChar { get; init; } = "char";

    [DbColumn("nullable_string_char", NpgsqlDbType.Char, true, Length = 50)]
    public string? NullableStringToChar { get; init; }

    [DbColumn("nullable_string_char_has_value", NpgsqlDbType.Char, true, Length = 4)]
    public string? NullableStringToCharHasValue { get; init; } = "char";

    // String => Text
    [DbColumn("text", NpgsqlDbType.Text)]
    public string Text { get; init; } = "Text";

    [DbColumn("nullable_text", NpgsqlDbType.Text, true)]
    public string? NullableText { get; init; }

    [DbColumn("nullable_text_has_value", NpgsqlDbType.Text, true)]
    public string? NullableTextHasValue { get; init; } = "Text";
}