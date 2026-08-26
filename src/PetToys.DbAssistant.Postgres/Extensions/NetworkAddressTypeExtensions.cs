using System;
using System.Net;
using System.Net.NetworkInformation;
using NpgsqlTypes;

namespace PetToys.DbAssistant.Postgres.Extensions;

/// <summary>
/// Maps entity properties to PostgreSQL network address columns.
/// </summary>
public static class NetworkAddressTypeExtensions
{
    /// <summary>
    /// Maps an <see cref="IPAddress"/> value to a PostgreSQL <c>inet</c> column.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="builder">The builder being configured.</param>
    /// <param name="columnName">The name of the target column.</param>
    /// <param name="getter">Reads the value from an entity; <see langword="null"/> is written as SQL <c>NULL</c>.</param>
    /// <returns>The same <paramref name="builder"/>, so that calls can be chained.</returns>
    public static BulkContextBuilder<TEntity> MapInetAddress<TEntity>(this BulkContextBuilder<TEntity> builder, string columnName, Func<TEntity, IPAddress?> getter)
        where TEntity : class
    {
        return builder.Map(columnName, getter, NpgsqlDbType.Inet);
    }

    /// <summary>
    /// Maps a <see cref="PhysicalAddress"/> value to a PostgreSQL <c>macaddr</c> column.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="builder">The builder being configured.</param>
    /// <param name="columnName">The name of the target column.</param>
    /// <param name="getter">Reads the value from an entity; <see langword="null"/> is written as SQL <c>NULL</c>.</param>
    /// <returns>The same <paramref name="builder"/>, so that calls can be chained.</returns>
    public static BulkContextBuilder<TEntity> MapMacAddress<TEntity>(this BulkContextBuilder<TEntity> builder, string columnName, Func<TEntity, PhysicalAddress?> getter)
        where TEntity : class
    {
        return builder.Map(columnName, getter, NpgsqlDbType.MacAddr);
    }
}
