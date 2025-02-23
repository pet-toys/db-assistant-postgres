using FluentAssertions;
using Xunit;

namespace PetToys.DbAssistant.Postgres.Test;

public sealed class EmptyTest
{
    [Fact]
    public void Empty_Test()
    {
        true.Should().BeTrue();
    }
}
