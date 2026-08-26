using System;
using NpgsqlTypes;

namespace PetToys.DbAssistant.Postgres.Extensions;

/// <summary>
/// Maps entity properties to PostgreSQL UUID columns.
/// </summary>
public static class UUIDTypeExtensions
{
    /// <summary>
    /// Maps a <see cref="Guid"/> value to a PostgreSQL <c>uuid</c> column.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="builder">The builder being configured.</param>
    /// <param name="columnName">The name of the target column.</param>
    /// <param name="getter">Reads the value from an entity; <see langword="null"/> is written as SQL <c>NULL</c>.</param>
    /// <returns>The same <paramref name="builder"/>, so that calls can be chained.</returns>
    public static BulkContextBuilder<TEntity> MapUUID<TEntity>(this BulkContextBuilder<TEntity> builder, string columnName, Func<TEntity, Guid?> getter)
        where TEntity : class
    {
        return builder.Map(columnName, getter, NpgsqlDbType.Uuid);
    }
}
