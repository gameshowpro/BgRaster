// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

using System.IO.Enumeration;

namespace GameshowPro.BgRaster.Network;

static class NetworkFilter
{
    internal static ImmutableArray<AdapterInfo> Apply(ImmutableArray<AdapterInfo> adapters, Models.NetworkOptions options)
    {
        IEnumerable<AdapterInfo> result = adapters;

        if (options.RequireUp)
            result = result.Where(a => a.Status == "Up");

        if (options.RequireAdapterType.Length > 0)
        {
            HashSet<int> allowed = [.. options.RequireAdapterType
                .Select(v => TryParseAdapterType(v, out int id) ? id : -1)
                .Where(id => id >= 0)];
            result = result.Where(a =>
            {
                int thisId = GetAdapterTypeId(a.Type);
                return allowed.Contains(thisId);
            });
        }
        else if (options.ExcludeAdapterType.Length > 0)
        {
            HashSet<int> excluded = [.. options.ExcludeAdapterType
                .Select(v => TryParseAdapterType(v, out int id) ? id : -1)
                .Where(id => id >= 0)];
            result = result.Where(a =>
            {
                int thisId = GetAdapterTypeId(a.Type);
                return !excluded.Contains(thisId);
            });
        }

        if (options.RequireMacAddress.Length > 0)
        {
            HashSet<string> macs = new(options.RequireMacAddress.Select(m => NormalizeMac(m)), StringComparer.OrdinalIgnoreCase);
            result = result.Where(a => macs.Contains(NormalizeMac(a.MacAddress)));
        }

        if (options.RequireName.Length > 0)
            result = result.Where(a => options.RequireName.Any(pattern => MatchWildcard(a.Name, pattern)));

        if (options.RequireDescription.Length > 0)
            result = result.Where(a => options.RequireDescription.Any(pattern => MatchWildcard(a.Description, pattern)));

        // Per-adapter IP filtering
        ImmutableArray<AdapterInfo>.Builder filtered = ImmutableArray.CreateBuilder<AdapterInfo>();
        foreach (AdapterInfo adapter in result)
        {
            IEnumerable<AdapterIpAddress> ips = adapter.IpAddresses;

            if (!string.IsNullOrEmpty(options.RequireFamily))
            {
                bool isV6 = string.Equals(options.RequireFamily, "IPv6", StringComparison.OrdinalIgnoreCase);
                ips = ips.Where(ip => ip.Address.Contains(':') == isV6);
            }

            if (options.RequireSubnet.Length > 0)
            {
                List<(IPAddress Net, int Bits)> subnets = [];
                foreach (string s in options.RequireSubnet)
                {
                    if (TryParseCidr(s, out IPAddress? net, out int bits) && net is not null)
                        subnets.Add((net, bits));
                }
                ips = ips.Where(ip =>
                {
                    if (!IPAddress.TryParse(ip.Address, out IPAddress? addr))
                        return false;
                    return subnets.Any(s => IsInSubnet(addr, s.Net, s.Bits));
                });
            }

            ImmutableArray<AdapterIpAddress> finalIps = ips.ToImmutableArray();
            if (finalIps.Length >= options.MinimumAddressCount)
            {
                filtered.Add(adapter with { IpAddresses = finalIps });
            }
        }

        return filtered.ToImmutable();
    }

    static int GetAdapterTypeId(string typeShort) =>
        NetworkCollector.IanaShortToId.TryGetValue(typeShort, out int id) ? id : 0;

    static bool TryParseAdapterType(string value, out int ianaId)
    {
        if (int.TryParse(value, out ianaId) && NetworkCollector.IanaIfTypes.ContainsKey(ianaId))
            return true;
        if (NetworkCollector.DotNetTypeToIana.TryGetValue(value, out ianaId))
            return true;
        if (NetworkCollector.IanaShortToId.TryGetValue(value, out ianaId))
            return true;
        return false;
    }

    static string NormalizeMac(string mac) => mac.Replace(":", "").Replace("-", "").Replace(".", "").ToUpperInvariant();

    static bool MatchWildcard(string input, string pattern) =>
        FileSystemName.MatchesSimpleExpression(pattern, input);

    static bool TryParseCidr(string cidr, out IPAddress? net, out int bits)
    {
        net = null;
        bits = -1;
        int slash = cidr.LastIndexOf('/');
        if (slash < 0) return false;
        string addrPart = cidr[..slash];
        string bitsPart = cidr[(slash + 1)..];
        if (!IPAddress.TryParse(addrPart, out net)) return false;
        if (!int.TryParse(bitsPart, out bits)) return false;
        return bits >= 0 && bits <= (net.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 ? 128 : 32);
    }

    static bool IsInSubnet(IPAddress addr, IPAddress net, int bits)
    {
        if (addr.AddressFamily != net.AddressFamily) return false;
        byte[] a = addr.GetAddressBytes();
        byte[] n = net.GetAddressBytes();
        int byteCount = bits / 8;
        for (int i = 0; i < byteCount; i++)
            if (a[i] != n[i]) return false;
        int remainingBits = bits % 8;
        if (remainingBits > 0)
        {
            int mask = (byte)(0xFF << (8 - remainingBits));
            if ((a[byteCount] & mask) != (n[byteCount] & mask)) return false;
        }
        return true;
    }
}
