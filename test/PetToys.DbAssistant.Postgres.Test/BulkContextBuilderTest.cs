using System;
using AwesomeAssertions;
using Npgsql;
using PetToys.DbAssistant.Postgres.Extensions;
using Xunit;

namespace PetToys.DbAssistant.Postgres.Test;

public sealed class BulkContextBuilderTest
{
    private sealed class Sample
    {
        public string? Name { get; init; }
    }

    private static NpgsqlConnection Connection() => new();

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void CreateBulkContext_BlankTableName_Throws(string tableName)
    {
        var act = () => Connection().CreateBulkContext<Sample>(tableName);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateBulkContext_NullTableName_Throws()
    {
        var act = () => Connection().CreateBulkContext<Sample>(null!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateBulkContext_ValidTableName_ReturnsBuilder()
    {
        var builder = Connection().CreateBulkContext<Sample>("orders");

        builder.Should().NotBeNull();
    }

    [Fact]
    public void CreateBulkContext_NullSchemaName_ReturnsBuilder()
    {
        var builder = Connection().CreateBulkContext<Sample>("orders", schemaName: null);

        builder.Should().NotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void CreateBulkContext_BlankSchemaName_Throws(string schemaName)
    {
        var act = () => Connection().CreateBulkContext<Sample>("orders", schemaName);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Map_BlankColumnName_Throws(string columnName)
    {
        var builder = Connection().CreateBulkContext<Sample>("orders");

        var act = () => builder.MapText(columnName, e => e.Name);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Map_DuplicateColumnName_Throws()
    {
        var builder = Connection().CreateBulkContext<Sample>("orders")
            .MapText("name", e => e.Name);

        var act = () => builder.MapText("name", e => e.Name);

        act.Should().Throw<InvalidOperationException>().WithMessage("*name*");
    }

    [Fact]
    public void Map_DuplicateColumnName_DifferentType_Throws()
    {
        var builder = Connection().CreateBulkContext<Sample>("orders")
            .MapText("value", e => e.Name);

        var act = () => builder.MapVarchar("value", e => e.Name);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Map_CaseDifferentColumnNames_AreDistinct()
    {
        var builder = Connection().CreateBulkContext<Sample>("orders");

        var act = () => builder
            .MapText("Name", e => e.Name)
            .MapText("name", e => e.Name);

        act.Should().NotThrow();
    }

    [Fact]
    public void Map_DistinctColumnNames_MapNormally()
    {
        var builder = Connection().CreateBulkContext<Sample>("orders");

        var act = () => builder
            .MapText("first", e => e.Name)
            .MapText("second", e => e.Name)
            .MapVarchar("third", e => e.Name);

        act.Should().NotThrow();
    }
}
