namespace PetToys.DbAssistant.Postgres;

internal static class StringExtensions
{
    private const char QuoteChar = '"';

    public static string QuoteIdentifier(this string value) =>
        (value.StartsWith(QuoteChar) ? string.Empty : QuoteChar) + value + (value.EndsWith(QuoteChar) ? string.Empty : QuoteChar);
}
