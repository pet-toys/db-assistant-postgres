using System;
using System.Net;
using System.Net.NetworkInformation;
using NpgsqlTypes;

namespace PetToys.DbAssistant.Postgres.Extensions;

public static class NetworkAddressTypeExtensions
{
    public static BulkContextBuilder<TEntity> MapInetAddress<TEntity>(this BulkContextBuilder<TEntity> builder, string columnName, Func<TEntity, IPAddress?> getter)
        where TEntity : class
    {
        return builder.Map(columnName, getter, NpgsqlDbType.Inet);
    }

    public static BulkContextBuilder<TEntity> MapMacAddress<TEntity>(this BulkContextBuilder<TEntity> builder, string columnName, Func<TEntity, PhysicalAddress?> getter)
        where TEntity : class
    {
        return builder.Map(columnName, getter, NpgsqlDbType.MacAddr);
    }
}