namespace PetToys.DbAssistant.Postgres;

internal static class StringExtensions
{
    private const string Quote = "\"";
    private const string EscapedQuote = "\"\"";

    public static string QuoteIdentifier(this string value) =>
        Quote + value.Replace(Quote, EscapedQuote) + Quote;
}
