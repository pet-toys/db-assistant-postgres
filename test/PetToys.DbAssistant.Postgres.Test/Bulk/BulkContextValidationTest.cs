using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using AwesomeAssertions;
using Npgsql;
using PetToys.DbAssistant.Postgres.Extensions;
using Xunit;

namespace PetToys.DbAssistant.Postgres.Test.Bulk;

public sealed class BulkContextValidationTest
{
    private sealed class Sample
    {
        public string? Name { get; init; }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void CreateBulkContext_NullOrWhitespaceTableName_ThrowsForTableName(string? tableName)
    {
        using var connection = new NpgsqlConnection();

        var act = () => connection.CreateBulkContext<Sample>(tableName!);

        act.Should().Throw<ArgumentException>().WithParameterName(nameof(tableName));
    }

    [Fact]
    public void CreateBulkContext_ValidTableName_ReturnsBuilder()
    {
        using var connection = new NpgsqlConnection();

        var builder = connection.CreateBulkContext<Sample>("orders");

        builder.Should().NotBeNull();
    }

    [Fact]
    public void CreateBulkContext_NullConnection_ThrowsForConnection()
    {
        NpgsqlConnection connection = null!;

        var act = () => connection.CreateBulkContext<Sample>("orders");

        act.Should().Throw<ArgumentNullException>().WithParameterName("connection");
    }

    [Fact]
    public void CreateBulkContext_NullSchemaName_IsAllowed()
    {
        using var connection = new NpgsqlConnection();

        var builder = connection.CreateBulkContext<Sample>("orders", schemaName: null);

        builder.Should().NotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void CreateBulkContext_WhitespaceSchemaName_ThrowsForSchemaName(string schemaName)
    {
        using var connection = new NpgsqlConnection();

        var act = () => connection.CreateBulkContext<Sample>("orders", schemaName);

        act.Should().Throw<ArgumentException>().WithParameterName(nameof(schemaName));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Map_NullOrWhitespaceColumnName_ThrowsForColumnName(string? columnName)
    {
        using var connection = new NpgsqlConnection();
        var builder = connection.CreateBulkContext<Sample>("orders");

        var act = () => builder.MapText(columnName!, entity => entity.Name);

        act.Should().Throw<ArgumentException>().WithParameterName(nameof(columnName));
    }

    [Fact]
    public void Map_DuplicateColumnName_ThrowsNamingTheColumn()
    {
        using var connection = new NpgsqlConnection();
        var builder = connection.CreateBulkContext<Sample>("orders")
            .MapText("name", entity => entity.Name);

        var act = () => builder.MapVarchar("name", entity => entity.Name);

        act.Should().Throw<InvalidOperationException>().WithMessage("*name*");
    }

    [Fact]
    public void Map_CaseDifferentColumnNames_AreDistinct()
    {
        using var connection = new NpgsqlConnection();

        var act = () => connection.CreateBulkContext<Sample>("orders")
            .MapText("Name", entity => entity.Name)
            .MapText("name", entity => entity.Name);

        act.Should().NotThrow();
    }

    [Fact]
    public void Map_ReturnsSameBuilderInstance_ForFluentChaining()
    {
        using var connection = new NpgsqlConnection();
        var builder = connection.CreateBulkContext<Sample>("orders");

        var chained = builder.MapText("name", entity => entity.Name);

        chained.Should().BeSameAs(builder);
    }

    [Fact]
    public async Task WriteDataAsync_NoColumnsMapped_ReturnsZeroWithoutOpeningConnection()
    {
        using var connection = new NpgsqlConnection();
        var context = connection.CreateBulkContext<Sample>("orders");

        var written = await context.WriteDataAsync(new[] { new Sample() }, TestContext.Current.CancellationToken);

        written.Should().Be(0UL);
        connection.State.Should().Be(ConnectionState.Closed);
    }

    [Fact]
    public async Task WriteDataAsync_NoColumnsMapped_AsyncEnumerable_ReturnsZeroWithoutOpeningConnection()
    {
        using var connection = new NpgsqlConnection();
        var context = connection.CreateBulkContext<Sample>("orders");

        var written = await context.WriteDataAsync(AsyncSamples(), TestContext.Current.CancellationToken);

        written.Should().Be(0UL);
        connection.State.Should().Be(ConnectionState.Closed);

        static async IAsyncEnumerable<Sample> AsyncSamples()
        {
            await Task.CompletedTask;
            yield return new Sample();
        }
    }

    [Fact]
    public async Task WriteDataAsync_NullEntities_ThrowsForEntities_AndLeavesConnectionClosed()
    {
        using var connection = new NpgsqlConnection();
        var context = connection.CreateBulkContext<Sample>("orders")
            .MapText("name", entity => entity.Name);

        var act = async () => await context.WriteDataAsync((IEnumerable<Sample>)null!, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("entities");
        connection.State.Should().Be(ConnectionState.Closed);
    }

    [Fact]
    public async Task WriteDataAsync_NullAsyncEntities_ThrowsForEntities_AndLeavesConnectionClosed()
    {
        using var connection = new NpgsqlConnection();
        var context = connection.CreateBulkContext<Sample>("orders")
            .MapText("name", entity => entity.Name);

        var act = async () => await context.WriteDataAsync((IAsyncEnumerable<Sample>)null!, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("entities");
        connection.State.Should().Be(ConnectionState.Closed);
    }

    [Fact]
    public async Task WriteDataAsync_NullEntities_NoColumnsMapped_ThrowsInsteadOfReturningZero()
    {
        using var connection = new NpgsqlConnection();
        var context = connection.CreateBulkContext<Sample>("orders");

        var act = async () => await context.WriteDataAsync((IEnumerable<Sample>)null!, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("entities");
    }
}
