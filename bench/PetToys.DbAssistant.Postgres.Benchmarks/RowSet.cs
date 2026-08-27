using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;

namespace PetToys.DbAssistant.Postgres.Benchmarks;

/// <summary>
/// The rows every benchmark copies.
/// </summary>
/// <remarks>
/// Everything here is drawn from one <see cref="Random"/> constructed with a constant seed, and
/// nothing reads the clock, the environment or a new <see cref="Guid"/>. Two runs of the same
/// revision therefore copy identical bytes, which is the only reason their durations can be
/// compared at all. The generators are called from <c>[GlobalSetup]</c>, so no part of building a
/// row lands inside a measured region.
/// </remarks>
public static class RowSet
{
    private const int Seed = 20260827;

    /// <summary>
    /// The instant every timestamp is an offset from. A literal, not <see cref="DateTime.UtcNow"/>:
    /// a run's own start time would be one more thing that differs between two runs.
    /// </summary>
    private static readonly DateTime Epoch = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static readonly string[] Names = BuildNames();

    private static readonly string[] Documents =
    [
        """{"kind":"order","priority":1}""",
        """{"kind":"order","priority":2,"tags":["bulk","copy"]}""",
        """{"kind":"invoice","priority":3,"lines":12}""",
        """{"kind":"shipment","priority":1,"carrier":"ups","weight":18.25}""",
    ];

    private static readonly IPAddress[] Addresses =
    [
        IPAddress.Parse("10.0.0.1"),
        IPAddress.Parse("192.168.13.240"),
        IPAddress.Parse("172.16.4.9"),
        IPAddress.Parse("2001:db8::7334"),
    ];

    /// <summary>Builds the narrow row set.</summary>
    /// <param name="count">How many rows to build.</param>
    public static IReadOnlyList<NarrowRow> Narrow(int count)
    {
        var random = new Random(Seed);
        var rows = new List<NarrowRow>(count);

        for (var index = 0; index < count; index++)
        {
            rows.Add(new NarrowRow
            {
                Id = index,
                Name = Names[random.Next(Names.Length)],
                CreatedAt = Epoch.AddSeconds(random.Next(0, 31_536_000)),
                Active = random.Next(2) == 0,
            });
        }

        return rows;
    }

    /// <summary>Builds the wide row set.</summary>
    /// <param name="count">How many rows to build.</param>
    public static IReadOnlyList<WideRow> Wide(int count)
    {
        var random = new Random(Seed);
        var payloads = BuildPayloads(random);
        var identifiers = BuildIdentifiers(random);
        var rows = new List<WideRow>(count);

        for (var index = 0; index < count; index++)
        {
            rows.Add(new WideRow
            {
                Id = index,
                Code = index.ToString("D8", CultureInfo.InvariantCulture),
                Name = Names[random.Next(Names.Length)],
                Amount = Math.Round((decimal)random.NextDouble() * 10_000m, 2),
                Ratio = random.NextDouble(),
                Flag = random.Next(2) == 0,
                Identifier = identifiers[random.Next(identifiers.Length)],
                Payload = payloads[random.Next(payloads.Length)],
                Document = Documents[random.Next(Documents.Length)],
                Address = Addresses[random.Next(Addresses.Length)],
                CreatedAt = Epoch.AddSeconds(random.Next(0, 31_536_000)),
                Duration = TimeSpan.FromSeconds(random.Next(0, 86_400)),
            });
        }

        return rows;
    }

    /// <summary>
    /// A pool of names of differing lengths. Text is written by length, so a single fixed string
    /// would measure one length and call it the text column.
    /// </summary>
    private static string[] BuildNames()
    {
        var names = new string[16];

        for (var index = 0; index < names.Length; index++)
        {
            names[index] = string.Create(
                CultureInfo.InvariantCulture,
                $"row-{index:D2}-{new string((char)('a' + index), 4 + (index * 3))}");
        }

        return names;
    }

    /// <summary>
    /// A pool of payloads rather than one array per row: at a hundred thousand rows the
    /// per-row arrays would be most of the set's memory, and what the copy writes is the bytes,
    /// not the reference.
    /// </summary>
    private static byte[][] BuildPayloads(Random random)
    {
        var payloads = new byte[8][];

        for (var index = 0; index < payloads.Length; index++)
        {
            var payload = new byte[32 + (index * 16)];
            random.NextBytes(payload);
            payloads[index] = payload;
        }

        return payloads;
    }

    private static Guid[] BuildIdentifiers(Random random)
    {
        var identifiers = new Guid[16];
        var bytes = new byte[16];

        for (var index = 0; index < identifiers.Length; index++)
        {
            random.NextBytes(bytes);
            identifiers[index] = new Guid(bytes);
        }

        return identifiers;
    }
}
