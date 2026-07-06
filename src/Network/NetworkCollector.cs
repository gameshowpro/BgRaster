// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Network;

internal static class NetworkCollector
{
    /// <summary>IANA IF Type integer → (shortName, longName).</summary>
    internal static readonly FrozenDictionary<int, (string Short, string Long)> s_ianaIfTypes = new Dictionary<int, (string, string)>
    {
        [1] = ("other", "other"),
        [6] = ("ethernetCsmacd", "ethernetCsmacd"),
        [9] = ("iso88025TokenRing", "iso88025TokenRing"),
        [15] = ("fddi", "fddi"),
        [23] = ("ppp", "ppp"),
        [24] = ("softwareLoopback", "softwareLoopback"),
        [28] = ("slip", "slip"),
        [32] = ("frameRelayService", "frameRelayService"),
        [37] = ("atm", "atm"),
        [53] = ("propVirtual", "proprietary virtual/internal"),
        [62] = ("fastEther", "fastEther"),
        [69] = ("gigabitEthernet", "gigabitEthernet"),
        [71] = ("ieee80211", "ieee80211"),
        [117] = ("gigabitEthernet", "gigabitEthernet"),
        [131] = ("tunnel", "tunnel"),
        [144] = ("l2vlan", "l2vlan"),
        [161] = ("ieee8023adLag", "ieee8023adLag"),
        [237] = ("l3ipvlan", "l3ipvlan"),
    }.ToFrozenDictionary();

    /// <summary>.NET NetworkInterfaceType enum name → IANA integer.</summary>
    internal static readonly FrozenDictionary<string, int> s_dotNetTypeToIana = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
        ["Ethernet"] = 6,
        ["TokenRing"] = 9,
        ["Fddi"] = 15,
        ["Ppp"] = 23,
        ["Loopback"] = 24,
        ["Slip"] = 28,
        ["Atm"] = 37,
        ["FastEthernetFx"] = 62,
        ["FastEthernetT"] = 62,
        ["GigabitEthernet"] = 117,
        ["Wireless80211"] = 71,
        ["Tunnel"] = 131,
        ["Wman"] = 237,
        ["Wwanpp"] = 243,
        ["Wwanpp2"] = 244,
    }.ToFrozenDictionary();

    /// <summary>IANA short name → integer.</summary>
    internal static readonly FrozenDictionary<string, int> s_ianaShortToId = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
        ["other"] = 1,
        ["ethernetCsmacd"] = 6,
        ["iso88025TokenRing"] = 9,
        ["fddi"] = 15,
        ["ppp"] = 23,
        ["softwareLoopback"] = 24,
        ["slip"] = 28,
        ["frameRelayService"] = 32,
        ["atm"] = 37,
        ["propVirtual"] = 53,
        ["fastEther"] = 62,
        ["gigabitEthernet"] = 117,
        ["ieee80211"] = 71,
        ["tunnel"] = 131,
        ["l2vlan"] = 144,
        ["ieee8023adLag"] = 161,
        ["l3ipvlan"] = 237,
    }.ToFrozenDictionary();

    /// <summary>Parse an adapter type value (enum name, IANA short, or integer) to its IANA type ID.</summary>
    internal static bool TryParseAdapterType(string value, out int ianaId)
    {
        if (int.TryParse(value, out ianaId) && s_ianaIfTypes.ContainsKey(ianaId))
        {
            return true;
        }

        if (s_dotNetTypeToIana.TryGetValue(value, out ianaId))
        {
            return true;
        }

        if (s_ianaShortToId.TryGetValue(value, out ianaId))
        {
            return true;
        }

        return false;
    }

    private static string FormatOrigin(PrefixOrigin prefix, SuffixOrigin suffix)
    {
        string p = prefix switch
        {
            PrefixOrigin.Dhcp => "DHCP",
            PrefixOrigin.Manual => "Manual",
            PrefixOrigin.WellKnown => "WellKnown",
            PrefixOrigin.RouterAdvertisement => "Router advertisement",
            _ => "Unknown",
        };
        string s = suffix switch
        {
            SuffixOrigin.OriginDhcp => "DHCP",
            SuffixOrigin.Manual => "Manual",
            SuffixOrigin.WellKnown => "WellKnown",
            SuffixOrigin.LinkLayerAddress => "Link layer",
            SuffixOrigin.Random => "Random",
            _ => "Unknown",
        };
        if (string.Equals(p, s, StringComparison.Ordinal))
        {
            return p;
        }

        return $"{p}-{s}";
    }

    private static string FormatSpeed(long bitsPerSecond)
    {
        if (bitsPerSecond <= 0)
        {
            return "N/A";
        }

        string[] units = ["b/s", "kb/s", "Mb/s", "Gb/s", "Tb/s"];
        double value = bitsPerSecond;
        int unitIdx = 0;
        while (value >= 1000 && unitIdx < units.Length - 1)
        {
            value /= 1000;
            unitIdx++;
        }
        return $"{value:0.#}{units[unitIdx]}";
    }

    private static string GetIanaTypeString(int ifType)
    {
        if (s_ianaIfTypes.TryGetValue(ifType, out (string Short, string Long) names))
        {
            return names.Short;
        }

        return ifType.ToString(CultureInfo.InvariantCulture);
    }

    private static string GetIanaTypeLong(int ifType)
    {
        if (s_ianaIfTypes.TryGetValue(ifType, out (string Short, string Long) names))
        {
            return names.Long;
        }

        return ifType.ToString(CultureInfo.InvariantCulture);
    }

    internal static ImmutableArray<AdapterInfo> Collect()
    {
        if (!OperatingSystem.IsWindows())
        {
            return [];
        }

        ImmutableArray<AdapterInfo>.Builder adapters = ImmutableArray.CreateBuilder<AdapterInfo>();

        foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            int ifType = GetInterfaceTypeNumber(ni.NetworkInterfaceType);

            // Collect IP addresses
            IPInterfaceProperties? props = ni.GetIPProperties();
            ImmutableArray<AdapterIpAddress>.Builder ips = ImmutableArray.CreateBuilder<AdapterIpAddress>();

            if (props is not null)
            {
                foreach (UnicastIPAddressInformation ip in props.UnicastAddresses)
                {
                    int cidrBits = ip.IPv4Mask is not null ? CountBits(ip.IPv4Mask) : ip.PrefixLength;
                    string origin = FormatOrigin(ip.PrefixOrigin, ip.SuffixOrigin);
                    ips.Add(new AdapterIpAddress
                    {
                        Address = ip.Address.ToString(),
                        CidrBits = ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 ? ip.PrefixLength : cidrBits,
                        Origin = origin,
                    });
                }
            }

            adapters.Add(new AdapterInfo
            {
                Name = ni.Name,
                Description = ni.Description,
                Id = ni.Id,
                Type = GetIanaTypeString(ifType),
                TypeLong = GetIanaTypeLong(ifType),
                Status = ni.OperationalStatus == OperationalStatus.Up ? "Up" : "Down",
                Speed = FormatSpeed(ni.Speed),
                MacAddress = BitConverter.ToString(ni.GetPhysicalAddress().GetAddressBytes()).Replace("-", ":"),
                IpAddresses = ips.ToImmutable(),
            });
        }

        return adapters.ToImmutable();
    }

    private static int GetInterfaceTypeNumber(NetworkInterfaceType type) =>
        s_dotNetTypeToIana.TryGetValue(type.ToString(), out int id) ? id : (int)type;

    private static int CountBits(IPAddress mask)
    {
        byte[] bytes = mask.GetAddressBytes();
        int bits = 0;
        foreach (byte b in bytes)
        {
            for (int i = 7; i >= 0; i--)
            {
                if ((b & (1 << i)) != 0)
                {
                    bits++;
                }
                else
                {
                    return bits;
                }
            }
        }
        return bits;
    }
}