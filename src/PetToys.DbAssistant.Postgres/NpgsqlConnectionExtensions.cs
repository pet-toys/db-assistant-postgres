using System;
using Npgsql;

namespace PetToys.DbAssistant.Postgres;

/// <summary>
/// Starts a bulk-copy configuration on an <see cref="NpgsqlConnection"/>.
/// </summary>
public static class NpgsqlConnectionExtensions
{
    /// <summary>
    /// Creates a <see cref="BulkContextBuilder{TEntity}"/> targeting the given table.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being copied.</typeparam>
    /// <param name="connection">The connection the <c>COPY</c> runs on.</param>
    /// <param name="tableName">
    /// The name of the destination table. It is quoted as a PostgreSQL
    /// identifier, so it is matched case-sensitively and needs no quoting of its
    /// own.
    /// </param>
    /// <param name="schemaName">
    /// The schema the table lives in, quoted the same way. Omitted, the table is
    /// resolved through the connection's <c>search_path</c>.
    /// </param>
    /// <returns>A builder to map columns on.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="tableName"/>, or a supplied <paramref name="schemaName"/>,
    /// is empty or whitespace.
    /// </exception>
    public static BulkContextBuilder<TEntity> CreateBulkContext<TEntity>(this NpgsqlConnection connection, string tableName, string? schemaName = null)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(connection);

        return new BulkContextBuilder<TEntity>(connection, tableName, schemaName);
    }
}
