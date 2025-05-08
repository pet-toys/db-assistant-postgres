using Npgsql;
using Testcontainers.PostgreSql;
using Testcontainers.Xunit;
using Xunit.Sdk;

namespace PetToys.DbAssistant.Postgres.Test.Bulk;

public sealed class PostgresFixture(IMessageSink messageSink)
    : DbContainerFixture<PostgreSqlBuilder, PostgreSqlContainer>(messageSink)
{
    public override NpgsqlFactory DbProviderFactory => NpgsqlFactory.Instance;
}
