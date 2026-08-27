using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using AwesomeAssertions;
using Npgsql;
using PetToys.DbAssistant.Postgres.Extensions;
using PetToys.DbAssistant.Postgres.Test.Entities;
using Xunit;

namespace PetToys.DbAssistant.Postgres.Test.Bulk;

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

    private static readonly Faker<RoundTripEntity> FakeRoundTripEntity = new Faker<RoundTripEntity>()
        .StrictMode(true)
        .RuleFor(e => e.Id, f => f.IndexFaker)
        .RuleFor(e => e.Label, f => f.Lorem.Word().OrNull(f, .1f))
        .RuleFor(e => e.Flag, f => f.Random.Bool())
        .RuleFor(e => e.Amount, f => f.Random.Decimal())
        .RuleFor(e => e.Identifier, f => f.Random.Guid())
        .RuleFor(e => e.Payload, f => f.Random.Bytes(16).OrNull(f, .1f))
        .RuleFor(e => e.CreatedAt, f => f.Date.Future());

    [DockerRequiredFact]
    public Task Binary_BulkInsert_CopiesEveryRow() =>
        RunBulkAsync(
            FakeBinaryEntity.Generate(BatchSize),
            builder => builder
                .MapByteArray("bytes", entity => entity.Bytes)
                .MapByteArray("nullable_bytes", entity => entity.NullableBytes));

    [DockerRequiredFact]
    public Task Boolean_BulkInsert_CopiesEveryRow() =>
        RunBulkAsync(
            FakeBooleanEntity.Generate(BatchSize),
            builder => builder
                .MapBoolean("boolean", entity => entity.Boolean)
                .MapBoolean("nullable_boolean", entity => entity.NullableBoolean));

    [DockerRequiredFact]
    public Task DateTime_BulkInsert_CopiesEveryRow() =>
        RunBulkAsync(
            FakeDateTimeEntity.Generate(BatchSize),
            builder => builder
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
                .MapTimeTz("nullable_datetime_offset_time_tz", entity => entity.NullableDateTimeOffsetToTimeTz));

    [DockerRequiredFact]
    public Task Json_BulkInsert_CopiesEveryRow() =>
        RunBulkAsync(
            FakeJsonEntity.Generate(BatchSize),
            builder => builder
                .MapJson("json", entity => entity.Json)
                .MapJson("nullable_json", entity => entity.NullableJson)
                .MapJsonb("jsonb", entity => entity.Jsonb)
                .MapJsonb("nullable_jsonb", entity => entity.NullableJsonb));

    [DockerRequiredFact]
    public Task Monetary_BulkInsert_CopiesEveryRow() =>
        RunBulkAsync(
            FakeMonetaryEntity.Generate(BatchSize),
            builder => builder
                .MapMoney("money", entity => entity.Money)
                .MapMoney("nullable_money", entity => entity.NullableMoney));

    [DockerRequiredFact]
    public Task NetworkAddress_BulkInsert_CopiesEveryRow() =>
        RunBulkAsync(
            FakeNetworkAddressEntity.Generate(BatchSize),
            builder => builder
                .MapInetAddress("ip_address", entity => entity.IpAddress)
                .MapInetAddress("nullable_ip_address", entity => entity.NullableIpAddress)
                .MapMacAddress("mac_addr", entity => entity.MacAddress)
                .MapMacAddress("nullable_mac_addr", entity => entity.NullableMacAddress));

    [DockerRequiredFact]
    public Task Numeric_BulkInsert_CopiesEveryRow() =>
        RunBulkAsync(
            FakeNumericEntity.Generate(BatchSize),
            builder => builder
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
                .MapDouble("nullable_double", entity => entity.NullableDouble));

    [DockerRequiredFact]
    public Task String_BulkInsert_CopiesEveryRow() =>
        RunBulkAsync(
            FakeStringEntity.Generate(BatchSize),
            builder => builder
                .MapVarchar("string_varchar", entity => entity.StringToVarchar)
                .MapVarchar("nullable_string_varchar", entity => entity.NullableStringToVarchar)

                .MapCharacter("string_char", entity => entity.StringToChar)
                .MapCharacter("nullable_string_char", entity => entity.NullableStringToChar)

                .MapText("text", entity => entity.Text)
                .MapText("nullable_text", entity => entity.NullableText));

    [DockerRequiredFact]
    public Task UUID_BulkInsert_CopiesEveryRow() =>
        RunBulkAsync(
            FakeUUIDEntity.Generate(BatchSize),
            builder => builder
                .MapUUID("uuid", entity => entity.UUID)
                .MapUUID("nullable_uuid", entity => entity.NullableUUID));

    [DockerRequiredFact]
    public async Task BulkInsert_RoundTrips_ValuesAndNulls()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var tableName = TableName<RoundTripEntity>();
        var createdAt = new DateTime(2026, 6, 14, 10, 30, 0, DateTimeKind.Unspecified);
        var identifier = new Guid("11112222-3333-4444-5555-666677778888");
        var rows = new List<RoundTripEntity>
        {
            new() { Id = 1, Label = "alpha", Flag = true, Amount = 123.45m, Identifier = identifier, Payload = [1, 2, 3], CreatedAt = createdAt },
            new() { Id = 2, Label = null, Flag = false, Amount = -0.01m, Identifier = Guid.Empty, Payload = null, CreatedAt = createdAt },
        };

        await using var connection = await OpenConnectionAndCreateTableAsync<RoundTripEntity>(tableName);
        var written = await MapRoundTrip(connection.CreateBulkContext<RoundTripEntity>(tableName))
            .WriteDataAsync(rows, cancellationToken);
        written.Should().Be((ulong)rows.Count);

        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT id, label, flag, amount, identifier, payload, created_at FROM {tableName.QuoteIdentifier()} ORDER BY id;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        (await reader.ReadAsync(cancellationToken)).Should().BeTrue();
        reader.GetInt32(0).Should().Be(1);
        reader.GetString(1).Should().Be("alpha");
        reader.GetBoolean(2).Should().BeTrue();
        reader.GetDecimal(3).Should().Be(123.45m);
        reader.GetGuid(4).Should().Be(identifier);
        reader.GetFieldValue<byte[]>(5).Should().Equal(new byte[] { 1, 2, 3 });
        reader.GetDateTime(6).Should().Be(createdAt);

        (await reader.ReadAsync(cancellationToken)).Should().BeTrue();
        reader.GetInt32(0).Should().Be(2);
        reader.IsDBNull(1).Should().BeTrue();
        reader.GetBoolean(2).Should().BeFalse();
        reader.GetDecimal(3).Should().Be(-0.01m);
        reader.GetGuid(4).Should().Be(Guid.Empty);
        reader.IsDBNull(5).Should().BeTrue();
        reader.GetDateTime(6).Should().Be(createdAt);

        (await reader.ReadAsync(cancellationToken)).Should().BeFalse();
    }

    [DockerRequiredFact]
    public async Task BulkInsert_EmptyCollection_WritesNothing()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var tableName = TableName<RoundTripEntity>();

        await using var connection = await OpenConnectionAndCreateTableAsync<RoundTripEntity>(tableName);
        var written = await MapRoundTrip(connection.CreateBulkContext<RoundTripEntity>(tableName))
            .WriteDataAsync(Array.Empty<RoundTripEntity>(), cancellationToken);

        written.Should().Be(0UL);
        (await ExecuteCountAsync(connection, tableName, cancellationToken)).Should().Be(0);
    }

    [DockerRequiredFact]
    public async Task BulkInsert_AsyncEnumerableSource_CopiesEveryRow()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var tableName = TableName<RoundTripEntity>();
        var rows = FakeRoundTripEntity.Generate(2_000);

        await using var connection = await OpenConnectionAndCreateTableAsync<RoundTripEntity>(tableName);
        var written = await MapRoundTrip(connection.CreateBulkContext<RoundTripEntity>(tableName))
            .WriteDataAsync(ToAsyncEnumerable(rows), cancellationToken);

        written.Should().Be((ulong)rows.Count);
        (await ExecuteCountAsync(connection, tableName, cancellationToken)).Should().Be(rows.Count);
    }

    [DockerRequiredFact]
    public async Task BulkInsert_BusyConnection_FailsFastNamingTheState()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var tableName = TableName<RoundTripEntity>();
        var rows = FakeRoundTripEntity.Generate(1);

        await using var connection = await OpenConnectionAndCreateTableAsync<RoundTripEntity>(tableName);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT generate_series(1, 1000);";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        (await reader.ReadAsync(cancellationToken)).Should().BeTrue();

        // Which flags Npgsql reports for a half-read reader is its business; what
        // this test is about is that the state is one the copy has to refuse, and
        // that the error says which one it was.
        var busyState = connection.FullState;
        busyState.Should().NotBe(ConnectionState.Open);
        busyState.Should().NotBe(ConnectionState.Closed);

        var act = async () => await MapRoundTrip(connection.CreateBulkContext<RoundTripEntity>(tableName))
            .WriteDataAsync(rows, cancellationToken);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage($"*{busyState}*");
    }

    [DockerRequiredFact]
    public async Task BulkInsert_BrokenConnection_IsOpenedForTheCopyAndClosedAgain()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        // A permanent table, not the temporary one every other test uses: the
        // session that owns a temporary table dies with the backend this test
        // terminates on purpose.
        var tableName = TableName<RoundTripEntity>() + "_broken";
        var rows = FakeRoundTripEntity.Generate(10);

        await using var connection = await OpenConnectionAndCreateTableAsync<RoundTripEntity>(tableName, temporary: false);
        await BreakAsync(connection, cancellationToken);
        connection.FullState.Should().Be(ConnectionState.Broken);

        var written = await MapRoundTrip(connection.CreateBulkContext<RoundTripEntity>(tableName))
            .WriteDataAsync(rows, cancellationToken);

        written.Should().Be((ulong)rows.Count);
        connection.FullState.Should().Be(ConnectionState.Closed);

        await connection.OpenAsync(cancellationToken);
        (await ExecuteCountAsync(connection, tableName, cancellationToken)).Should().Be(rows.Count);
        await using var drop = connection.CreateCommand();
        drop.CommandText = $"DROP TABLE {tableName.QuoteIdentifier()};";
        await drop.ExecuteNonQueryAsync(cancellationToken);
    }

    [DockerRequiredFact]
    public async Task TimeStampTz_NonUtcDateTime_FailsNamingColumnAndUtcRequirement()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var tableName = TableName<TimestampTzEntity>();
        var rows = new[]
        {
            new TimestampTzEntity { CreatedAt = new DateTime(2026, 6, 14, 10, 30, 0, DateTimeKind.Local) },
        };

        await using var connection = await OpenConnectionAndCreateTableAsync<TimestampTzEntity>(tableName);
        var act = async () => await connection.CreateBulkContext<TimestampTzEntity>(tableName)
            .MapTimeStampTz("created_at", entity => entity.CreatedAt)
            .WriteDataAsync(rows, cancellationToken);

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .WithMessage("*created_at*")
            .WithMessage("*DateTimeKind.Utc*");
    }

    /// <summary>
    /// Drives the connection into <see cref="ConnectionState.Broken"/>. Having the
    /// session terminate its own backend is the one way to get there on demand:
    /// stopping the container would take the shared fixture down with it, and
    /// disposing the connection only closes it.
    /// </summary>
    private static async Task BreakAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT pg_terminate_backend(pg_backend_pid());";

        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (NpgsqlException)
        {
            // Expected: the server drops the connection mid-command, which is
            // exactly what leaves it broken.
        }
    }

    private static BulkContextBuilder<RoundTripEntity> MapRoundTrip(BulkContextBuilder<RoundTripEntity> builder) =>
        builder
            .MapInteger("id", entity => entity.Id)
            .MapText("label", entity => entity.Label)
            .MapBoolean("flag", entity => entity.Flag)
            .MapNumeric("amount", entity => entity.Amount)
            .MapUUID("identifier", entity => entity.Identifier)
            .MapByteArray("payload", entity => entity.Payload)
            .MapTimeStamp("created_at", entity => entity.CreatedAt);

    private async Task RunBulkAsync<TEntity>(
        IReadOnlyCollection<TEntity> data,
        Func<BulkContextBuilder<TEntity>, BulkContextBuilder<TEntity>> configure)
        where TEntity : class, new()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var tableName = TableName<TEntity>();
        await using var connection = await OpenConnectionAndCreateTableAsync<TEntity>(tableName);

        var watch = Stopwatch.StartNew();
        var result = await configure(connection.CreateBulkContext<TEntity>(tableName))
            .WriteDataAsync(data, cancellationToken);
        watch.Stop();

        result.Should().Be((ulong)data.Count);
        output.WriteLine("Inserted {0:N0} rows. Elapsed time: {1:N0} ms.", result, watch.ElapsedMilliseconds);
        var count = await ExecuteCountAsync(connection, tableName, cancellationToken);
        count.Should().Be(data.Count);
    }

    private async Task<NpgsqlConnection> OpenConnectionAndCreateTableAsync<TEntity>(string tableName, bool temporary = true)
        where TEntity : class, new()
    {
        var columnIdentifiers = typeof(TEntity)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.GetCustomAttribute<DbColumnAttribute>())
            .Where(a => a is not null)
            .Select(a => a!.DbCreateColumnStatement)
            .ToList();

        // A temporary table belongs to its session and is gone with it. A permanent
        // one outlives the test that asked for it, including a failing one, so it
        // is dropped first rather than left to fail the next run in setup.
        var create = temporary
            ? "CREATE TEMPORARY TABLE"
            : $"DROP TABLE IF EXISTS {tableName.QuoteIdentifier()}; CREATE TABLE";
        var sb = new StringBuilder($"{create} {tableName.QuoteIdentifier()} (")
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
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {tableName.QuoteIdentifier()};";
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return (long?)result ?? 0;
    }

    private static async IAsyncEnumerable<TEntity> ToAsyncEnumerable<TEntity>(IEnumerable<TEntity> source)
    {
        foreach (var item in source)
        {
            yield return item;
            await Task.Yield();
        }
    }

    private static string TableName<TEntity>() where TEntity : class, new() => SnakeCaseNameRewriter.RewriteName(typeof(TEntity).Name);
}
