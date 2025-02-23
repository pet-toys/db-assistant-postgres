using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using PetToys.DbAssistant.Postgres.Extensions;
using PetToys.DbAssistant.Postgres.Test.Entities;
using Xunit;
using Xunit.Abstractions;

namespace PetToys.DbAssistant.Postgres.Test;

[Trait("Category", "Integration")]
public sealed class BulkInsertTest(ITestOutputHelper output) : DatabaseTestBase
{
    private const int BatchSize = 1_000;

    [Fact]
    public async Task Binary_Test()
    {
        var data = Enumerable.Repeat(new BinaryEntity(), BatchSize).ToList();
        var tableName = await ReCreateTableAsync<BinaryEntity>();
        await using var connection = GetConnection();
        var watch = Stopwatch.StartNew();
        var result = await connection.CreateBulkContext<BinaryEntity>(tableName)
            .MapByteArray("bytes", entity => entity.Bytes)
            .MapByteArray("nullable_bytes_has_value", entity => entity.NullableBytesHasValue)
            .MapByteArray("nullable_bytes", entity => entity.NullableBytes)
            .WriteDataAsync(data);
        watch.Stop();
        result.Should().Be((ulong)data.Count);
        output.WriteLine("Inserted {0:N0} rows. Elapsed time: {1:N0} ms.", result, watch.ElapsedMilliseconds);
    }

    [Fact]
    public async Task Boolean_Test()
    {
        var data = Enumerable.Repeat(new BooleanEntity(), BatchSize).ToList();
        var tableName = await ReCreateTableAsync<BooleanEntity>();
        await using var connection = GetConnection();
        var watch = Stopwatch.StartNew();
        var result = await connection.CreateBulkContext<BooleanEntity>(tableName)
            .MapBoolean("boolean", entity => entity.Boolean)
            .MapBoolean("nullable_boolean_has_value", entity => entity.NullableBooleanHasValue)
            .MapBoolean("nullable_boolean", entity => entity.NullableBoolean)
            .WriteDataAsync(data);
        watch.Stop();
        result.Should().Be((ulong)data.Count);
        output.WriteLine("Inserted {0:N0} rows. Elapsed time: {1:N0} ms.", result, watch.ElapsedMilliseconds);
    }

    [Fact]
    public async Task DateTime_Test()
    {
        var data = Enumerable.Repeat(new DateTimeEntity(), BatchSize).ToList();
        var tableName = await ReCreateTableAsync<DateTimeEntity>();
        await using var connection = GetConnection();
        var watch = Stopwatch.StartNew();
        var result = await connection.CreateBulkContext<DateTimeEntity>(tableName)
            .MapDate("datetime_date", entity => entity.DateTimeToDate)
            .MapDate("nullable_datetime_date", entity => entity.NullableDateTimeToDate)
            .MapDate("nullable_datetime_date_has_value", entity => entity.NullableDateTimeToDateHasValue)

            .MapTime("timespan_time", entity => entity.TimeSpanToTime)
            .MapTime("nullable_timespan_time", entity => entity.NullableTimeSpanToTime)
            .MapTime("nullable_timespan_time_has_value", entity => entity.NullableTimeSpanToTimeHasValue)

            .MapTimeStamp("datetime_timestamp", entity => entity.DateTimeToTimestamp)
            .MapTimeStamp("nullable_datetime_timestamp", entity => entity.NullableDateTimeToTimestamp)
            .MapTimeStamp("nullable_datetime_timestamp_has_value", entity => entity.NullableDateTimeToTimestampHasValue)

            .MapTimeStampTz("datetime_timestamp_tz", entity => entity.DateTimeToTimestampTz)
            .MapTimeStampTz("nullable_datetime_timestamp_tz", entity => entity.NullableDateTimeToTimestampTz)
            .MapTimeStampTz("nullable_datetime_timestamp_tz_has_value", entity => entity.NullableDateTimeToTimestampTzHasValue)

            .MapTimeStampTz("datetime_offset_timestamp_tz", entity => entity.DateTimeOffsetToTimestampTz)
            .MapTimeStampTz("nullable_datetime_offset_timestamp_tz", entity => entity.NullableDateTimeOffsetToTimestampTz)
            .MapTimeStampTz("nullable_datetime_offset_timestamp_tz_has_value", entity => entity.NullableDateTimeOffsetToTimestampTzHasValue)

            .MapInterval("timespan_interval", entity => entity.TimeSpanToInterval)
            .MapInterval("nullable_timespan_interval", entity => entity.NullableTimeSpanToInterval)
            .MapInterval("nullable_timespan_interval_has_value", entity => entity.NullableTimeSpanToIntervalHasValue)

            .MapTimeTz("datetime_offset_time_tz", entity => entity.DateTimeOffsetToTimeTz)
            .MapTimeTz("nullable_datetime_offset_time_tz", entity => entity.NullableDateTimeOffsetToTimeTz)
            .MapTimeTz("nullable_datetime_offset_time_tz_has_value", entity => entity.NullableDateTimeOffsetToTimeTzHasValue)

            .WriteDataAsync(data);
        watch.Stop();
        result.Should().Be((ulong)data.Count);
        output.WriteLine("Inserted {0:N0} rows. Elapsed time: {1:N0} ms.", result, watch.ElapsedMilliseconds);
    }

    [Fact]
    public async Task Json_Test()
    {
        var data = Enumerable.Repeat(new JsonEntity(), BatchSize).ToList();
        var tableName = await ReCreateTableAsync<JsonEntity>();
        await using var connection = GetConnection();
        var watch = Stopwatch.StartNew();
        var result = await connection.CreateBulkContext<JsonEntity>(tableName)
            .MapJson("json", entity => entity.Json)
            .MapJson("nullable_json", entity => entity.NullableJson)
            .MapJson("nullable_json_has_value", entity => entity.NullableJsonHasValue)
            .MapJsonb("jsonb", entity => entity.Jsonb)
            .MapJsonb("nullable_jsonb", entity => entity.NullableJsonb)
            .MapJsonb("nullable_jsonb_has_value", entity => entity.NullableJsonbHasValue)
            .WriteDataAsync(data);
        watch.Stop();
        result.Should().Be((ulong)data.Count);
        output.WriteLine("Inserted {0:N0} rows. Elapsed time: {1:N0} ms.", result, watch.ElapsedMilliseconds);
    }

    [Fact]
    public async Task Monetary_Test()
    {
        var data = Enumerable.Repeat(new MonetaryEntity(), BatchSize).ToList();
        var tableName = await ReCreateTableAsync<MonetaryEntity>();
        await using var connection = GetConnection();
        var watch = Stopwatch.StartNew();
        var result = await connection.CreateBulkContext<MonetaryEntity>(tableName)
            .MapMoney("money", entity => entity.Money)
            .MapMoney("nullable_money", entity => entity.NullableMoney)
            .MapMoney("nullable_money_has_value", entity => entity.NullableMoneyHasValue)
            .WriteDataAsync(data);
        watch.Stop();
        result.Should().Be((ulong)data.Count);
        output.WriteLine("Inserted {0:N0} rows. Elapsed time: {1:N0} ms.", result, watch.ElapsedMilliseconds);
    }

    [Fact]
    public async Task NetworkAddress_Test()
    {
        var data = Enumerable.Repeat(new NetworkAddressEntity(), BatchSize).ToList();
        var tableName = await ReCreateTableAsync<NetworkAddressEntity>();
        await using var connection = GetConnection();
        var watch = Stopwatch.StartNew();
        var result = await connection.CreateBulkContext<NetworkAddressEntity>(tableName)
            .MapInetAddress("ip_address", entity => entity.IpAddress)
            .MapInetAddress("nullable_ip_address", entity => entity.NullableIpAddress)
            .MapInetAddress("nullable_ip_address_has_value", entity => entity.NullableIpAddressHasValue)
            .MapMacAddress("mac_addr", entity => entity.MacAddress)
            .MapMacAddress("nullable_mac_addr", entity => entity.NullableMacAddress)
            .MapMacAddress("nullable_mac_addr_has_value", entity => entity.NullableMacAddressHasValue)
            .WriteDataAsync(data);
        watch.Stop();
        result.Should().Be((ulong)data.Count);
        output.WriteLine("Inserted {0:N0} rows. Elapsed time: {1:N0} ms.", result, watch.ElapsedMilliseconds);
    }

    [Fact]
    public async Task Numeric_Test()
    {
        var data = Enumerable.Repeat(new NumericEntity(), BatchSize).ToList();
        var tableName = await ReCreateTableAsync<NumericEntity>();
        await using var connection = GetConnection();
        var watch = Stopwatch.StartNew();
        var result = await connection.CreateBulkContext<NumericEntity>(tableName)
            .MapSmallInt("smallint", entity => entity.Smallint)
            .MapSmallInt("nullable_smallint", entity => entity.NullableSmallint)
            .MapSmallInt("nullable_smallint_has_value", entity => entity.NullableSmallintHasValue)

            .MapInteger("integer", entity => entity.IntegerCol)
            .MapInteger("nullable_integer", entity => entity.NullableInteger)
            .MapInteger("nullable_integer_has_value", entity => entity.NullableIntegerHasValue)

            .MapBigInt("bigint", entity => entity.Bigint)
            .MapBigInt("nullable_bigint", entity => entity.NullableBigint)
            .MapBigInt("nullable_bigint_has_value", entity => entity.NullableBigintHasValue)

            .MapNumeric("numeric", entity => entity.Numeric)
            .MapNumeric("nullable_numeric", entity => entity.NullableNumeric)
            .MapNumeric("nullable_numeric_has_value", entity => entity.NullableNumericHasValue)

            .MapReal("real", entity => entity.Real)
            .MapReal("nullable_real", entity => entity.NullableReal)
            .MapReal("nullable_real_has_value", entity => entity.NullableRealHasValue)

            .MapDouble("double", entity => entity.DoubleCol)
            .MapDouble("nullable_double", entity => entity.NullableDouble)
            .MapDouble("nullable_double_has_value", entity => entity.NullableDoubleHasValue)

            .WriteDataAsync(data);
        watch.Stop();
        result.Should().Be((ulong)data.Count);
        output.WriteLine("Inserted {0:N0} rows. Elapsed time: {1:N0} ms.", result, watch.ElapsedMilliseconds);
    }

    [Fact]
    public async Task String_Test()
    {
        var data = Enumerable.Repeat(new StringEntity(), BatchSize).ToList();
        var tableName = await ReCreateTableAsync<StringEntity>();
        await using var connection = GetConnection();
        var watch = Stopwatch.StartNew();
        var result = await connection.CreateBulkContext<StringEntity>(tableName)
            .MapVarchar("string_varchar", entity => entity.StringToVarchar)
            .MapVarchar("nullable_string_varchar", entity => entity.NullableStringToVarchar)
            .MapVarchar("nullable_string_varchar_has_value", entity => entity.NullableStringToVarcharHasValue)

            .MapCharacter("string_char", entity => entity.StringToChar)
            .MapCharacter("nullable_string_char", entity => entity.NullableStringToChar)
            .MapCharacter("nullable_string_char_has_value", entity => entity.NullableStringToCharHasValue)

            .MapText("text", entity => entity.Text)
            .MapText("nullable_text", entity => entity.NullableText)
            .MapText("nullable_text_has_value", entity => entity.NullableTextHasValue)
            .WriteDataAsync(data);
        watch.Stop();
        result.Should().Be((ulong)data.Count);
        output.WriteLine("Inserted {0:N0} rows. Elapsed time: {1:N0} ms.", result, watch.ElapsedMilliseconds);
    }

    [Fact]
    public async Task UUID_Test()
    {
        var data = Enumerable.Repeat(new UUIDEntity(), BatchSize).ToList();

        var tableName = await ReCreateTableAsync<UUIDEntity>();
        await using var connection = GetConnection();
        var watch = Stopwatch.StartNew();
        var result = await connection.CreateBulkContext<UUIDEntity>(tableName)
            .MapUUID("uuid", entity => entity.UUID)
            .MapUUID("nullable_uuid", entity => entity.NullableUUID)
            .MapUUID("nullable_uuid_has_value", entity => entity.NullableUUIDHasValue)
            .WriteDataAsync(data);
        watch.Stop();
        result.Should().Be((ulong)data.Count);
        output.WriteLine("Inserted {0:N0} rows. Elapsed time: {1:N0} ms.", result, watch.ElapsedMilliseconds);
    }
}
