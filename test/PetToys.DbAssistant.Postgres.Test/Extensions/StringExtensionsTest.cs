using AwesomeAssertions;
using Xunit;

namespace PetToys.DbAssistant.Postgres.Test.Extensions;

public sealed class StringExtensionsTest
{
    [Theory]
    [InlineData("orders", "\"orders\"")]                  // plain name is wrapped
    [InlineData("MixedCase", "\"MixedCase\"")]            // case is preserved
    [InlineData("with space", "\"with space\"")]          // spaces are kept inside the quotes
    [InlineData("Dotted.Name", "\"Dotted.Name\"")]        // a dot is part of the identifier, not a separator
    [InlineData("", "\"\"")]                              // empty input becomes an empty quoted identifier
    [InlineData("  ", "\"  \"")]                          // whitespace is wrapped, not trimmed
    [InlineData("My\"Table", "\"My\"\"Table\"")]          // an embedded quote is doubled
    [InlineData("\"name\"", "\"\"\"name\"\"\"")]          // surrounding quotes are escaped, not stripped
    [InlineData("a\"\"b", "\"a\"\"\"\"b\"")]              // each quote is doubled independently
    public void QuoteIdentifier_WrapsAndDoublesEmbeddedQuotes(string value, string expected)
        => value.QuoteIdentifier().Should().Be(expected);
}
