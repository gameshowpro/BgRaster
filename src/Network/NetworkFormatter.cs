// SPDX-License-Identifier: MIT
// Copyright (C) 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Network;

internal static class NetworkFormatter
{
    internal static string Format(ImmutableArray<AdapterInfo> adapters, NetworkOptions options)
    {
        if (adapters.Length == 0)
        {
            return string.Empty;
        }

        StringBuilder sb = new();
        foreach (AdapterInfo adapter in adapters)
        {
            string ipBlock = FormatIpAddresses(adapter.IpAddresses, options.IpAddressFormat);
            string adapterBlock = FormatAdapter(adapter, options.AdapterFormat)
                .Replace("${IpAddresses}", ipBlock);
            _ = sb.Append(adapterBlock);
        }
        return sb.ToString();
    }

    internal static string FormatAdapter(AdapterInfo adapter, ImmutableArray<string> formatTemplates)
    {
        StringBuilder sb = new();
        foreach (string template in formatTemplates)
        {
            _ = sb.Append(ApplyAdapterSubstitutions(template, adapter));
        }
        return sb.ToString();
    }

    internal static string FormatIpAddresses(ImmutableArray<AdapterIpAddress> ips, ImmutableArray<string> formatTemplates)
    {
        StringBuilder sb = new();
        foreach (AdapterIpAddress ip in ips)
        {
            _ = sb.Append(FormatIpAddress(ip, formatTemplates));
        }
        return sb.ToString();
    }

    internal static string FormatIpAddress(AdapterIpAddress ip, ImmutableArray<string> formatTemplates)
    {
        StringBuilder sb = new();
        foreach (string template in formatTemplates)
        {
            _ = sb.Append(ApplyIpSubstitutions(template, ip));
        }
        return sb.ToString();
    }

    private static string ApplyAdapterSubstitutions(string template, AdapterInfo adapter) =>
        template
            .Replace("${Name}", adapter.Name)
            .Replace("${Description}", adapter.Description)
            .Replace("${ID}", adapter.Id)
            .Replace("${Type}", adapter.Type)
            .Replace("${TypeLong}", adapter.TypeLong)
            .Replace("${Status}", adapter.Status)
            .Replace("${Speed}", adapter.Speed)
            .Replace("${PhysicalAddress}", adapter.MacAddress)
            .Replace("${MacAddress}", adapter.MacAddress);

    private static string ApplyIpSubstitutions(string template, AdapterIpAddress ip) =>
        template
            .Replace("${Address}", ip.Address)
            .Replace("${CidrBits}", ip.CidrBits.ToString(CultureInfo.InvariantCulture))
            .Replace("${Origin}", ip.Origin);
}