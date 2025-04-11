using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace PetToys.DbAssistant.Postgres.Test;

public abstract class DatabaseTestBase
{
    private readonly string _connectionString;

    protected DatabaseTestBase()
    {
        AssertionConfiguration.Current.Equivalency.Modify(options => options.WithStrictOrdering());

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddUserSecrets<DatabaseTestBase>()
            .Build();
        var builder = new NpgsqlConnectionStringBuilder(configuration.GetConnectionString("TestPgConnection"));
        _connectionString = builder.ConnectionString;
    }

    protected async Task<string> ReCreateTableAsync<TEntity>()
        where TEntity : class, new()
    {
        var tableName = SnakeCaseNameRewriter.RewriteName(typeof(TEntity).Name);
        var columnIdentifiers = typeof(TEntity)!
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.GetCustomAttribute<DbColumnAttribute>())
            .Where(a => a is not null)
            .Select(a => a!.DbCreateColumnStatement)
            .ToList();

        var sb = new StringBuilder($"""
                                    DROP TABLE IF EXISTS {tableName.QuoteIdentifier()};
                                    CREATE TABLE {tableName.QuoteIdentifier()} (
                                    """)
            .AppendJoin(", ", columnIdentifiers)
            .Append(");");
        var c = sb.ToString();
        await using var connection = GetConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sb.ToString();
        await command.ExecuteNonQueryAsync();
        return tableName;
    }

    protected NpgsqlConnection GetConnection() => new(_connectionString);
}