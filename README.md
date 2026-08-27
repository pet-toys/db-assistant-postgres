# Database Assistant for PostgreSQL

[![Unit Test][test-badge]][test-url] [![NuGet Version][nuget-v-badge]][nuget-url] [![NuGet Downloads][nuget-dt-badge]][nuget-url] [![Target frameworks][dotnet-badge]][nuget-url] [![License][license-badge]][license-url]

![Database Assistant for PostgreSQL](https://raw.githubusercontent.com/pet-toys/db-assistant-postgres/refs/heads/dev/assets/promotion.png)

> Pour millions of rows into PostgreSQL at binary-`COPY` speed - a fluent,
> strongly-typed mapping API over [Npgsql][npgsql], no `DataTable`, no
> hand-rolled writers, just `COPY ... FROM STDIN BINARY` doing what it does
> best.

A small, focused wrapper around Npgsql's [binary import][binary-import]. Map
each entity property to a PostgreSQL column with a type-specific `Map*` method,
then stream an `IEnumerable<TEntity>` - or an `IAsyncEnumerable<TEntity>` -
straight into the server. The library writes the binary `COPY` protocol for
you, quotes every identifier, and leaves the connection exactly as it found it.

## Why

`COPY ... FROM STDIN BINARY` is the fastest way to load many rows into
PostgreSQL, but Npgsql exposes it as a low-level writer: you open the importer,
start each row, and push every value with its `NpgsqlDbType` by hand. That is
fast but easy to get wrong - one mismatched type or forgotten null and the copy
fails mid-stream. This library closes that gap:

- **Stream, don't stage.** Rows flow through the binary importer one at a time,
  so a million-row insert never materializes a million-row buffer in memory.
- **Map to PostgreSQL types, not magic strings.** `MapJsonb`, `MapMoney`,
  `MapUUID`, `MapTimeStampTz`, `MapInetAddress` - each method binds the right
  `NpgsqlDbType`, so the wire format matches the column.
- **Let nulls just work.** A mapped getter that returns `null` writes a SQL
  `NULL`; everything else is written with its declared type. Nullable value
  types map naturally.
- **Stay safe and in control.** Every table, schema, and column name is quoted
  per PostgreSQL rules, duplicate columns are rejected up front, and a
  `CancellationToken` flows all the way through the copy.

## Features

- **Fluent builder** - `CreateBulkContext` → `Map*` → `WriteDataAsync`.
- **Type-specific column mapping** for the PostgreSQL type families: text
  (`MapText`, `MapVarchar`, `MapCharacter`), numeric (`MapSmallInt`,
  `MapInteger`, `MapBigInt`, `MapNumeric`, `MapReal`, `MapDouble`), monetary
  (`MapMoney`), boolean (`MapBoolean`), binary (`MapByteArray`), JSON
  (`MapJson`, `MapJsonb`), UUID (`MapUUID`), date/time (`MapDate`, `MapTime`,
  `MapTimeTz`, `MapTimeStamp`, `MapTimeStampTz`, `MapInterval`), and network
  addresses (`MapInetAddress`, `MapMacAddress`).
- **Synchronous and asynchronous sources** - `WriteDataAsync` accepts both
  `IEnumerable<TEntity>` and `IAsyncEnumerable<TEntity>`.
- **Null-aware writes** - a getter returning `null` emits a SQL `NULL`; no
  sentinel values, no special casing.
- **Safe identifier quoting** - table, schema, and column names are wrapped and
  escaped per PostgreSQL rules, so names with special characters or mixed case
  work and identifier injection does not.
- **Managed connection lifecycle** - a closed connection is opened for the copy
  and closed again afterwards, leaving it as it was found; one that is neither
  open nor closed is rejected before the copy starts.
- **Cancellation** - pass a `CancellationToken` to `WriteDataAsync`; it reaches
  every `await` along the copy.
- **Multi-targets** `net8.0`, `net9.0`, and `net10.0`.

## Installation

```sh
dotnet add package PetToys.DbAssistant.Postgres
```

## Getting started

Describe how each entity property maps to a column, then write the data:

```csharp
using Npgsql;
using PetToys.DbAssistant.Postgres;
using PetToys.DbAssistant.Postgres.Extensions;

await using var connection = new NpgsqlConnection(connectionString);

ulong rowsCopied = await connection.CreateBulkContext<BusinessEntity>("records")
    .MapInteger("id", e => e.Id)
    .MapText("name", e => e.Name)
    .MapJsonb("payload", e => e.Payload)
    .MapMoney("price", e => e.Price)
    .MapTimeStampTz("created_at", e => e.CreatedAt)
    .WriteDataAsync(entities);
```

`WriteDataAsync` opens the connection if it is closed, runs the binary `COPY`,
closes the connection again if it opened it, and returns the number of rows
written.

## Usage

### Mapping columns

Each `Map*` call adds one column to the copy. The first argument is the column
name; the lambda projects the value out of the entity. Pick the method that
matches the destination column's PostgreSQL type:

```csharp
await connection.CreateBulkContext<BusinessEntity>("records")
    .MapInteger("id", e => e.Id)         // integer
    .MapText("name", e => e.Name)        // text
    .MapNumeric("amount", e => e.Amount) // numeric
    .MapUUID("ref", e => e.Reference)    // uuid
    .WriteDataAsync(entities);
```

A column name may be mapped only once. Mapping the same column twice throws an
`InvalidOperationException` naming the duplicated column. Comparison is
case-sensitive, matching PostgreSQL quoted-identifier semantics, so `"Name"`
and `"name"` are two distinct columns.

### Nullable values

Make the mapped getter return a nullable type. When it yields `null`, the
column receives a SQL `NULL`; otherwise the value is written with its mapped
type:

```csharp
await connection.CreateBulkContext<BusinessEntity>("records")
    .MapInteger("id", e => e.Id)
    .MapText("note", e => e.Note)        // string? -> NULL when the note is null
    .MapByteArray("blob", e => e.Blob)   // byte[]? -> NULL when absent
    .WriteDataAsync(entities);
```

### Streaming an async source

When rows arrive from an asynchronous producer - a paged query, a channel, a
stream - pass the `IAsyncEnumerable<TEntity>` directly. Nothing is buffered:

```csharp
async IAsyncEnumerable<BusinessEntity> ReadAsync() { /* yield rows */ }

ulong rowsCopied = await connection.CreateBulkContext<BusinessEntity>("records")
    .MapInteger("id", e => e.Id)
    .MapText("name", e => e.Name)
    .WriteDataAsync(ReadAsync(), cancellationToken);
```

### Schema-qualified tables

Pass a schema name to target a table outside the default search path; both the
schema and the table are quoted independently:

```csharp
await connection.CreateBulkContext<BusinessEntity>("records", schemaName: "analytics")
    .MapInteger("id", e => e.Id)
    .WriteDataAsync(entities);   // COPY "analytics"."records"(...) FROM STDIN BINARY
```

### Cancellation

`WriteDataAsync` takes a `CancellationToken` that flows through every step of
the copy:

```csharp
await connection.CreateBulkContext<BusinessEntity>("records")
    .MapInteger("id", e => e.Id)
    .WriteDataAsync(entities, cancellationToken);
```

## Good to know

- **The connection is left as it was found.** A connection that is already open
  stays open; a closed one is opened for the copy and closed again afterwards.
  Any other state - a broken connection, or one busy with another operation -
  fails with an `InvalidOperationException` naming the state, instead of
  surfacing as an error from inside the copy.
- **Arguments are validated up front.** `CreateBulkContext` throws
  `ArgumentNullException` for a null connection and `ArgumentException` for a
  null or whitespace table or schema name; `WriteDataAsync` throws
  `ArgumentNullException` for a null collection - before the connection is ever
  opened.
- **A copy needs at least one mapped column.** If no `Map*` call was made,
  `WriteDataAsync` throws `InvalidOperationException` without touching the
  connection, rather than reporting a silent no-op as `0` rows written.
- **Identifiers are quoted, not sanitized.** Names are wrapped in double quotes
  and embedded quotes are doubled, so `My"Table` becomes `"My""Table"`. Supply
  the raw name; the library never strips quoting you add yourself.
- **`timestamptz` from a `DateTime` must be UTC.** `MapTimeStampTz` with a
  `DateTime` getter accepts only `DateTimeKind.Utc`; a `Local` or `Unspecified`
  value fails the copy with an error naming the column. Convert to UTC
  (`DateTime.ToUniversalTime()`), or map from a `DateTimeOffset` - the
  `MapTimeStampTz(..., Func<TEntity, DateTimeOffset?>)` overload carries the
  offset for you.
- **Load into a staging table for the best throughput.** Copy into an unindexed
  temporary or staging table first, then insert from there into the indexed
  target. This keeps the copy itself as cheap as possible.
- **The mapping costs a few percent.** Measured against the hand-written
  `NpgsqlBinaryImporter` loop it replaces, a copy of 100,000 rows runs at 1.04x
  on a four-column row and 1.06x on a twelve-column one, and the extra
  allocation is a couple of kilobytes per copy rather than per row. The
  [benchmarks][benchmarks-url] carry the numbers, the machine they were taken
  on, and what makes a comparison against them valid.

More runnable examples live in the [unit tests][tests-url].

## Roadmap

- High-performance binary import/export between a table and a stream.
- CSV import/export.
- Whatever the next real use case calls for.

This package is built for its author's own needs; feature requests and pull
requests are welcome. See [Contributing][contributing] to get started.

## License

Provided under the [Apache License, Version 2.0][license-url].

[test-badge]: https://img.shields.io/github/actions/workflow/status/pet-toys/db-assistant-postgres/test.yml?branch=dev&style=flat-square&logo=github&label=test
[test-url]: https://github.com/pet-toys/db-assistant-postgres/actions?query=workflow%3Atest+branch%3Adev
[nuget-v-badge]: https://img.shields.io/nuget/v/PetToys.DbAssistant.Postgres?style=flat-square&logo=nuget&label=version
[nuget-dt-badge]: https://img.shields.io/nuget/dt/PetToys.DbAssistant.Postgres?style=flat-square&logo=nuget
[nuget-url]: https://www.nuget.org/packages/PetToys.DbAssistant.Postgres/
[dotnet-badge]: https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-512BD4?style=flat-square&logo=dotnet
[license-badge]: https://img.shields.io/github/license/pet-toys/db-assistant-postgres?style=flat-square&color=blue
[license-url]: https://www.apache.org/licenses/LICENSE-2.0
[contributing]: https://github.com/pet-toys/db-assistant-postgres/blob/dev/docs/CONTRIBUTING.md
[npgsql]: https://www.nuget.org/packages/Npgsql/
[binary-import]: https://www.npgsql.org/doc/copy.html#binary-copy
[tests-url]: https://github.com/pet-toys/db-assistant-postgres/tree/dev/test/PetToys.DbAssistant.Postgres.Test
[benchmarks-url]: https://github.com/pet-toys/db-assistant-postgres/tree/dev/bench/PetToys.DbAssistant.Postgres.Benchmarks
