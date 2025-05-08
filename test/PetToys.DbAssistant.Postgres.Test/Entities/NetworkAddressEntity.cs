using System.Net;
using System.Net.NetworkInformation;
using NpgsqlTypes;

namespace PetToys.DbAssistant.Postgres.Test.Entities;

internal sealed class NetworkAddressEntity
{
    [DbColumn("ip_address", NpgsqlDbType.Inet)]
    public IPAddress IpAddress { get; init; } = IPAddress.Parse("127.0.0.1");

    [DbColumn("nullable_ip_address", NpgsqlDbType.Inet, true)]
    public IPAddress? NullableIpAddress { get; init; }

    [DbColumn("mac_addr", NpgsqlDbType.MacAddr)]
    public PhysicalAddress MacAddress { get; init; } = PhysicalAddress.Parse("00-00-00-00-00-00");

    [DbColumn("nullable_mac_addr", NpgsqlDbType.MacAddr, true)]
    public PhysicalAddress? NullableMacAddress { get; init; }
}