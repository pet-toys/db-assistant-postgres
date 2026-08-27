namespace PetToys.DbAssistant.Postgres.Benchmarks;

/// <summary>
/// One destination column: what it is called and what PostgreSQL type it holds.
/// </summary>
/// <remarks>
/// A benchmark class declares its columns once, in the order both arms write them, and the
/// destination table and the hand-written arm's <c>COPY</c> command are both generated from that
/// declaration. Spelling either of them out separately is how a reordered column ends up writing
/// one value into another column of the same type, with both arms still succeeding and no longer
/// measuring the same work.
/// </remarks>
/// <param name="Name">The column name, unquoted.</param>
/// <param name="DataType">The column's PostgreSQL type, as it appears in <c>CREATE TABLE</c>.</param>
public sealed record ColumnSpec(string Name, string DataType);
