using System;
using System.Net;

namespace PetToys.DbAssistant.Postgres.Benchmarks;

/// <summary>
/// A twelve-column row spanning the numeric, string, JSON, binary, UUID, network-address and
/// date/time families - one column from each of the <c>Map*</c> groups the library offers.
/// </summary>
public sealed class WideRow
{
    public required int Id { get; init; }

    public required string Code { get; init; }

    public required string Name { get; init; }

    public required decimal Amount { get; init; }

    public required double Ratio { get; init; }

    public required bool Flag { get; init; }

    public required Guid Identifier { get; init; }

    public required byte[] Payload { get; init; }

    public required string Document { get; init; }

    public required IPAddress Address { get; init; }

    public required DateTime CreatedAt { get; init; }

    public required TimeSpan Duration { get; init; }
}
