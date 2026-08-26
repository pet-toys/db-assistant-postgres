using System;
using NpgsqlTypes;

namespace PetToys.DbAssistant.Postgres.Test;

[AttributeUsage(AttributeTargets.Property)]
internal sealed class DbColumnAttribute(string columnName, NpgsqlDbType dbType, bool nullable = false)
    : Attribute
{
    public string DbCreateColumnStatement => dbType switch
    {
        NpgsqlDbType.Double => CreateStatement("double precision"),
        NpgsqlDbType.Varchar => CreateStatement("character varying"),
        _ => CreateStatement(dbType.ToString().ToLowerInvariant()),
    };

    public int Length { get; set; }

    private string PrecisionStatement => Length > 0 ? $"({Length:D})" : string.Empty;

    private string CreateStatement(string type) => $"{columnName.QuoteIdentifier()} {type}{PrecisionStatement} " + (nullable ? "NULL" : "NOT NULL");
}
