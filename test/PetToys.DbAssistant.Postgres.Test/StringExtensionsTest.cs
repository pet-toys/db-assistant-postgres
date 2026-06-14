using AwesomeAssertions;
using Xunit;

namespace PetToys.DbAssistant.Postgres.Test;

public sealed class StringExtensionsTest
{
    [Fact]
    public void QuoteIdentifier_PlainName_IsWrappedInDoubleQuotes()
    {
        "orders".QuoteIdentifier().Should().Be("\"orders\"");
    }

    [Fact]
    public void QuoteIdentifier_EmbeddedDoubleQuote_IsDoubled()
    {
        "My\"Table".QuoteIdentifier().Should().Be("\"My\"\"Table\"");
    }

    [Theory]
    [InlineData("Order Items.v2", "\"Order Items.v2\"")]
    [InlineData("with space", "\"with space\"")]
    [InlineData("Dotted.Name", "\"Dotted.Name\"")]
    [InlineData("MixedCase", "\"MixedCase\"")]
    public void QuoteIdentifier_SpecialCharacters_ContentIsUnchangedInsideQuotes(string input, string expected)
    {
        input.QuoteIdentifier().Should().Be(expected);
    }

    [Fact]
    public void QuoteIdentifier_PreQuotedInput_IsTreatedAsRawAndEscaped()
    {
        "\"name\"".QuoteIdentifier().Should().Be("\"\"\"name\"\"\"");
    }
}
