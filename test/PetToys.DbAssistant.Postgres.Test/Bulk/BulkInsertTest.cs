using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Npgsql;
using PetToys.DbAssistant.Postgres.Extensions;
using PetToys.DbAssistant.Postgres.Test.Entities;
using Xunit;

namespace PetToys.DbAssistant.Postgres.Test.Bulk;

[Trait("Category", "Integration")]
public sealed class BulkInsertTest(PostgresFixture fixture, ITestOutputHelper output)
    : IClassFixture<PostgresFixture>
{
    private const int BatchSize = 1_000_000;

    private static readonly Faker<BinaryEntity> FakeBinaryEntity = new Faker<BinaryEntity>()
        .StrictMode(true)
        .RuleFor(e => e.Bytes, f => f.Random.Bytes(f.Random.Number(500)))
        .RuleFor(e => e.NullableBytes, f => f.Random.Bytes(f.Random.Number(500)).OrNull(f, .1f));

    private static readonly Faker<BooleanEntity> FakeBooleanEntity = new Faker<BooleanEntity>()
        .StrictMode(true)
        .RuleFor(e => e.Boolean, f => f.Random.Bool())
        .RuleFor(e => e.NullableBoolean, f => f.Random.Bool().OrNull(f, .1f));

    private static readonly Faker<DateTimeEntity> FakeDateTimeEntity = new Faker<DateTimeEntity>()
        .StrictMode(true)
        .RuleFor(e => e.DateTimeToDate, f => f.Date.Future().ToUniversalTime())
        .RuleFor(e => e.NullableDateTimeToDate, f => f.Date.Future().ToUniversalTime().OrNull(f, .1f))

        .RuleFor(e => e.TimeSpanToTime, f => f.Date.Timespan(TimeSpan.FromHours(24)))
        .RuleFor(e => e.NullableTimeSpanToTime, f => f.Date.Timespan(TimeSpan.FromHours(24)).OrNull(f, .1f))

        .RuleFor(e => e.DateTimeToTimestamp, f => f.Date.Future())
        .RuleFor(e => e.NullableDateTimeToTimestamp, f => f.Date.Future().OrNull(f, .1f))

        .RuleFor(e => e.DateTimeToTimestampTz, f => f.Date.Future().ToUniversalTime())
        .RuleFor(e => e.NullableDateTimeToTimestampTz, f => f.Date.Future().ToUniversalTime().OrNull(f, .1f))

        .RuleFor(e => e.DateTimeOffsetToTimestampTz, f => f.Date.FutureOffset().ToUniversalTime())
        .RuleFor(e => e.NullableDateTimeOffsetToTimestampTz, f => f.Date.FutureOffset().ToUniversalTime().OrNull(f, .1f))

        .RuleFor(e => e.TimeSpanToInterval, f => f.Date.Timespan(TimeSpan.FromHours(24)))
        .RuleFor(e => e.NullableTimeSpanToInterval, f => f.Date.Timespan(TimeSpan.FromHours(24)).OrNull(f, .1f))

        .RuleFor(e => e.DateTimeOffsetToTimeTz, f => f.Date.FutureOffset().ToUniversalTime())
        .RuleFor(e => e.NullableDateTimeOffsetToTimeTz, f => f.Date.FutureOffset().ToUniversalTime().OrNull(f, .1f));

    private static readonly Faker<JsonEntity> FakeJsonEntity = new Faker<JsonEntity>()
        .StrictMode(true)
        .RuleFor(e => e.Json, f => f.Random.Replace("{\"key\": \"*******\"}"))
        .RuleFor(e => e.NullableJson, f => f.Random.Replace("{\"key\": \"*******\"}").OrNull(f, .1f))
        .RuleFor(e => e.Jsonb, f => f.Random.Replace("{\"key\": \"*******\"}"))
        .RuleFor(e => e.NullableJsonb, f => f.Random.Replace("{\"key\": \"*******\"}").OrNull(f, .1f));

    private static readonly Faker<MonetaryEntity> FakeMonetaryEntity = new Faker<MonetaryEntity>()
        .StrictMode(true)
        .RuleFor(e => e.Money, f => f.Random.Decimal())
        .RuleFor(e => e.NullableMoney, f => f.Random.Decimal().OrNull(f, .1f));

    private static readonly Faker<NetworkAddressEntity> FakeNetworkAddressEntity = new Faker<NetworkAddressEntity>()
        .StrictMode(true)
        .RuleFor(e => e.IpAddress, f => f.Internet.IpAddress())
        .RuleFor(e => e.NullableIpAddress, f => f.Internet.IpAddress().OrNull(f, .1f))
        .RuleFor(e => e.MacAddress, f => PhysicalAddress.Parse(f.Internet.Mac()))
        .RuleFor(e => e.NullableMacAddress, f => PhysicalAddress.Parse(f.Internet.Mac()).OrNull(f, .1f));

    private static readonly Faker<NumericEntity> FakeNumericEntity = new Faker<NumericEntity>()
        .StrictMode(true)
        .RuleFor(e => e.Smallint, f => f.Random.Short())
        .RuleFor(e => e.NullableSmallint, f => f.Random.Short().OrNull(f, .1f))

        .RuleFor(e => e.IntegerCol, f => f.Random.Int())
        .RuleFor(e => e.NullableInteger, f => f.Random.Int().OrNull(f, .1f))

        .RuleFor(e => e.Bigint, f => f.Random.Long())
        .RuleFor(e => e.NullableBigint, f => f.Random.Long().OrNull(f, .1f))

        .RuleFor(e => e.Numeric, f => f.Random.Decimal())
        .RuleFor(e => e.NullableNumeric, f => f.Random.Decimal().OrNull(f, .1f))

        .RuleFor(e => e.Real, f => f.Random.Float())
        .RuleFor(e => e.NullableReal, f => f.Random.Float().OrNull(f, .1f))

        .RuleFor(e => e.DoubleCol, f => f.Random.Double())
        .RuleFor(e => e.NullableDouble, f => f.Random.Double().OrNull(f, .1f));

    private static readonly Faker<StringEntity> FakeStringEntity = new Faker<StringEntity>()
        .StrictMode(true)
        .RuleFor(e => e.StringToVarchar, f => f.Lorem.Sentence())
        .RuleFor(e => e.NullableStringToVarchar, f => f.Lorem.Sentence().OrNull(f, .1f))
        .RuleFor(e => e.StringToChar, f => f.Lorem.Word())
        .RuleFor(e => e.NullableStringToChar, f => f.Lorem.Word().OrNull(f, .1f))
        .RuleFor(e => e.Text, f => f.Lorem.Paragraph())
        .RuleFor(e => e.NullableText, f => f.Lorem.Paragraph().OrNull(f, .1f));

    private static readonly Faker<UUIDEntity> FakeUUIDEntity = new Faker<UUIDEntity>()
        .StrictMode(true)
        .RuleFor(e => e.UUID, f => f.Random.Guid())
        .RuleFor(e => e.NullableUUID, f => f.Random.Guid().OrNull(f, .1f));

    [Fact]
    public async Task Binary_Test()
    {
        var data = FakeBinaryEntity.Generate(BatchSize);
        var tableName = TableName<BinaryEntity>();
        await using var connection = await OpenConnectionAndCreateTableAsync<BinaryEntity>(tableName);
        var watch = Stopwatch.StartNew();
        var result = await connection.CreateBulkContext<BinaryEntity>(tableName)
            .MapByteArray("bytes", entity => entity.Bytes)
            .MapByteArray("nullable_bytes", entity => entity.NullableBytes)
            .WriteDataAsync(data, TestContext.Current.CancellationToken);
        watch.Stop();
        result.Should().Be((ulong)data.Count);
        output.WriteLine("Inserted {0:N0} rows. Elapsed time: {1:N0} ms.", result, watch.ElapsedMilliseconds);
        var count = await ExecuteCountAsync(connection, tableName, TestContext.Current.CancellationToken);
        count.Should().Be(data.Count);
        await connection.CloseAsync();
    }

    [Fact]
    public async Task Boolean_Test()
    {
        var data = FakeBooleanEntity.Generate(BatchSize);
        var tableName = TableName<BooleanEntity>();
        await using var connection = await OpenConnectionAndCreateTableAsync<BooleanEntity>(tableName);
        var watch = Stopwatch.StartNew();
        var result = await connection.CreateBulkContext<BooleanEntity>(tableName)
            .MapBoolean("boolean", entity => entity.Boolean)
            .MapBoolean("nullable_boolean", entity => entity.NullableBoolean)
            .WriteDataAsync(data, TestContext.Current.CancellationToken);
        watch.Stop();
        result.Should().Be((ulong)data.Count);
        output.WriteLine("Inserted {0:N0} rows. Elapsed time: {1:N0} ms.", result, watch.ElapsedMilliseconds);
        var count = await ExecuteCountAsync(connection, tableName, TestContext.Current.CancellationToken);
        count.Should().Be(data.Count);
        await connection.CloseAsync();
    }

    [Fact]
    public async Task DateTime_Test()
    {
        var data = FakeDateTimeEntity.Generate(BatchSize);
        var tableName = TableName<DateTimeEntity>();
        await using var connection = await OpenConnectionAndCreateTableAsync<DateTimeEntity>(tableName);
        var watch = Stopwatch.StartNew();
        var result = await connection.CreateBulkContext<DateTimeEntity>(tableName)
            .MapDate("datetime_date", entity => entity.DateTimeToDate)
            .MapDate("nullable_datetime_date", entity => entity.NullableDateTimeToDate)

            .MapTime("timespan_time", entity => entity.TimeSpanToTime)
            .MapTime("nullable_timespan_time", entity => entity.NullableTimeSpanToTime)

            .MapTimeStamp("datetime_timestamp", entity => entity.DateTimeToTimestamp)
            .MapTimeStamp("nullable_datetime_timestamp", entity => entity.NullableDateTimeToTimestamp)

            .MapTimeStampTz("datetime_timestamp_tz", entity => entity.DateTimeToTimestampTz)
            .MapTimeStampTz("nullable_datetime_timestamp_tz", entity => entity.NullableDateTimeToTimestampTz)

            .MapTimeStampTz("datetime_offset_timestamp_tz", entity => entity.DateTimeOffsetToTimestampTz)
            .MapTimeStampTz("nullable_datetime_offset_timestamp_tz", entity => entity.NullableDateTimeOffsetToTimestampTz)

            .MapInterval("timespan_interval", entity => entity.TimeSpanToInterval)
            .MapInterval("nullable_timespan_interval", entity => entity.NullableTimeSpanToInterval)

            .MapTimeTz("datetime_offset_time_tz", entity => entity.DateTimeOffsetToTimeTz)
            .MapTimeTz("nullable_datetime_offset_time_tz", entity => entity.NullableDateTimeOffsetToTimeTz)

            .WriteDataAsync(data, TestContext.Current.CancellationToken);
        watch.Stop();
        result.Should().Be((ulong)data.Count);
        output.WriteLine("Inserted {0:N0} rows. Elapsed time: {1:N0} ms.", result, watch.ElapsedMilliseconds);
        var count = await ExecuteCountAsync(connection, tableName, TestContext.Current.CancellationToken);
        count.Should().Be(data.Count);
        await connection.CloseAsync();
    }

    [Fact]
    public async Task Json_Test()
    {
        var data = FakeJsonEntity.Generate(BatchSize);
        var tableName = TableName<JsonEntity>();
        await using var connection = await OpenConnectionAndCreateTableAsync<JsonEntity>(tableName);
        var watch = Stopwatch.StartNew();
        var result = await connection.CreateBulkContext<JsonEntity>(tableName)
            .MapJson("json", entity => entity.Json)
            .MapJson("nullable_json", entity => entity.NullableJson)
            .MapJsonb("jsonb", entity => entity.Jsonb)
            .MapJsonb("nullable_jsonb", entity => entity.NullableJsonb)
            .WriteDataAsync(data, TestContext.Current.CancellationToken);
        watch.Stop();
        result.Should().Be((ulong)data.Count);
        output.WriteLine("Inserted {0:N0} rows. Elapsed time: {1:N0} ms.", result, watch.ElapsedMilliseconds);
        var count = await ExecuteCountAsync(connection, tableName, TestContext.Current.CancellationToken);
        count.Should().Be(data.Count);
        await connection.CloseAsync();
    }

    [Fact]
    public async Task Monetary_Test()
    {
        var data = FakeMonetaryEntity.Generate(BatchSize);
        var tableName = TableName<MonetaryEntity>();
        await using var connection = await OpenConnectionAndCreateTableAsync<MonetaryEntity>(tableName);
        var watch = Stopwatch.StartNew();
        var result = await connection.CreateBulkContext<MonetaryEntity>(tableName)
            .MapMoney("money", entity => entity.Money)
            .MapMoney("nullable_money", entity => entity.NullableMoney)
            .WriteDataAsync(data, TestContext.Current.CancellationToken);
        watch.Stop();
        result.Should().Be((ulong)data.Count);
        output.WriteLine("Inserted {0:N0} rows. Elapsed time: {1:N0} ms.", result, watch.ElapsedMilliseconds);
        var count = await ExecuteCountAsync(connection, tableName, TestContext.Current.CancellationToken);
        count.Should().Be(data.Count);
        await connection.CloseAsync();
    }

    [Fact]
    public async Task NetworkAddress_Test()
    {
        var data = FakeNetworkAddressEntity.Generate(BatchSize);
        var tableName = TableName<NetworkAddressEntity>();
        await using var connection = await OpenConnectionAndCreateTableAsync<NetworkAddressEntity>(tableName);
        var watch = Stopwatch.StartNew();
        var result = await connection.CreateBulkContext<NetworkAddressEntity>(tableName)
            .MapInetAddress("ip_address", entity => entity.IpAddress)
            .MapInetAddress("nullable_ip_address", entity => entity.NullableIpAddress)
            .MapMacAddress("mac_addr", entity => entity.MacAddress)
            .MapMacAddress("nullable_mac_addr", entity => entity.NullableMacAddress)
            .WriteDataAsync(data, TestContext.Current.CancellationToken);
        watch.Stop();
        result.Should().Be((ulong)data.Count);
        output.WriteLine("Inserted {0:N0} rows. Elapsed time: {1:N0} ms.", result, watch.ElapsedMilliseconds);
        var count = await ExecuteCountAsync(connection, tableName, TestContext.Current.CancellationToken);
        count.Should().Be(data.Count);
        await connection.CloseAsync();
    }

    [Fact]
    public async Task Numeric_Test()
    {
        var data = FakeNumericEntity.Generate(BatchSize);
        var tableName = TableName<NumericEntity>();
        await using var connection = await OpenConnectionAndCreateTableAsync<NumericEntity>(tableName);
        var watch = Stopwatch.StartNew();
        var result = await connection.CreateBulkContext<NumericEntity>(tableName)
            .MapSmallInt("smallint", entity => entity.Smallint)
            .MapSmallInt("nullable_smallint", entity => entity.NullableSmallint)

            .MapInteger("integer", entity => entity.IntegerCol)
            .MapInteger("nullable_integer", entity => entity.NullableInteger)

            .MapBigInt("bigint", entity => entity.Bigint)
            .MapBigInt("nullable_bigint", entity => entity.NullableBigint)

            .MapNumeric("numeric", entity => entity.Numeric)
            .MapNumeric("nullable_numeric", entity => entity.NullableNumeric)

            .MapReal("real", entity => entity.Real)
            .MapReal("nullable_real", entity => entity.NullableReal)

            .MapDouble("double", entity => entity.DoubleCol)
            .MapDouble("nullable_double", entity => entity.NullableDouble)

            .WriteDataAsync(data, TestContext.Current.CancellationToken);
        watch.Stop();
        result.Should().Be((ulong)data.Count);
        output.WriteLine("Inserted {0:N0} rows. Elapsed time: {1:N0} ms.", result, watch.ElapsedMilliseconds);
        var count = await ExecuteCountAsync(connection, tableName, TestContext.Current.CancellationToken);
        count.Should().Be(data.Count);
        await connection.CloseAsync();
    }

    [Fact]
    public async Task String_Test()
    {
        var data = FakeStringEntity.Generate(BatchSize);
        var tableName = TableName<StringEntity>();
        await using var connection = await OpenConnectionAndCreateTableAsync<StringEntity>(tableName);
        var watch = Stopwatch.StartNew();
        var result = await connection.CreateBulkContext<StringEntity>(tableName)
            .MapVarchar("string_varchar", entity => entity.StringToVarchar)
            .MapVarchar("nullable_string_varchar", entity => entity.NullableStringToVarchar)

            .MapCharacter("string_char", entity => entity.StringToChar)
            .MapCharacter("nullable_string_char", entity => entity.NullableStringToChar)

            .MapText("text", entity => entity.Text)
            .MapText("nullable_text", entity => entity.NullableText)
            .WriteDataAsync(data, TestContext.Current.CancellationToken);
        watch.Stop();
        result.Should().Be((ulong)data.Count);
        output.WriteLine("Inserted {0:N0} rows. Elapsed time: {1:N0} ms.", result, watch.ElapsedMilliseconds);
        var count = await ExecuteCountAsync(connection, tableName, TestContext.Current.CancellationToken);
        count.Should().Be(data.Count);
        await connection.CloseAsync();
    }

    [Fact]
    public async Task UUID_Test()
    {
        var data = FakeUUIDEntity.Generate(BatchSize);
        var tableName = TableName<UUIDEntity>();
        await using var connection = await OpenConnectionAndCreateTableAsync<UUIDEntity>(tableName);
        var watch = Stopwatch.StartNew();
        var result = await connection.CreateBulkContext<UUIDEntity>(tableName)
            .MapUUID("uuid", entity => entity.UUID)
            .MapUUID("nullable_uuid", entity => entity.NullableUUID)
            .WriteDataAsync(data, TestContext.Current.CancellationToken);
        watch.Stop();
        result.Should().Be((ulong)data.Count);
        output.WriteLine("Inserted {0:N0} rows. Elapsed time: {1:N0} ms.", result, watch.ElapsedMilliseconds);
        var count = await ExecuteCountAsync(connection, tableName, TestContext.Current.CancellationToken);
        count.Should().Be(data.Count);
        await connection.CloseAsync();
    }

    private async Task<NpgsqlConnection> OpenConnectionAndCreateTableAsync<TEntity>(string tableName)
        where TEntity : class, new()
    {
        var columnIdentifiers = typeof(TEntity)!
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.GetCustomAttribute<DbColumnAttribute>())
            .Where(a => a is not null)
            .Select(a => a!.DbCreateColumnStatement)
            .ToList();

        var sb = new StringBuilder($"CREATE TEMPORARY TABLE {tableName.QuoteIdentifier()} (")
            .AppendJoin(", ", columnIdentifiers)
            .Append(");");
        var connection = (NpgsqlConnection)fixture.CreateConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sb.ToString();
        await command.ExecuteNonQueryAsync();
        return connection;
    }

    private static async ValueTask<long> ExecuteCountAsync(NpgsqlConnection connection, string tableName, CancellationToken cancellationToken = default)
    {
        var query = $"SELECT COUNT(*) FROM {tableName};";
        await using var command = connection.CreateCommand();
        command.CommandText = query;
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return (long?)result ?? 0;
    }

    private static string TableName<TEntity>() where TEntity : class, new() => SnakeCaseNameRewriter.RewriteName(typeof(TEntity).Name);
}
