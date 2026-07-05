// SPDX-License-Identifier: MIT
// Copyright © 2026 Barjonas LLC

namespace GameshowPro.BgRaster.Network;

static class NetworkFormatter
{
    internal static string Format(ImmutableArray<AdapterInfo> adapters, Models.NetworkOptions options)
    {
        if (adapters.Length == 0)
            return string.Empty;

        StringBuilder sb = new();
        foreach (AdapterInfo adapter in adapters)
        {
            string ipBlock = FormatIpAddresses(adapter.IpAddresses, options);
            string adapterBlock = FormatAdapter(adapter, options.AdapterFormat)
                .Replace("${IpAddresses}", ipBlock);
            sb.Append(adapterBlock);
        }
        return sb.ToString();
    }

    internal static string FormatAdapter(AdapterInfo adapter, string formatTemplate) =>
        formatTemplate
            .Replace("${Name}", adapter.Name)
            .Replace("${Description}", adapter.Description)
            .Replace("${ID}", adapter.Id)
            .Replace("${Type}", adapter.Type)
            .Replace("${TypeLong}", adapter.TypeLong)
            .Replace("${Status}", adapter.Status)
            .Replace("${Speed}", adapter.Speed)
            .Replace("${PhysicalAddress}", adapter.MacAddress)
            .Replace("${MacAddress}", adapter.MacAddress);

    internal static string FormatIpAddresses(ImmutableArray<AdapterIpAddress> ips, Models.NetworkOptions options)
    {
        StringBuilder sb = new();
        foreach (AdapterIpAddress ip in ips)
            sb.Append(FormatIpAddress(ip, options.IpAddressFormat));
        return sb.ToString();
    }

    internal static string FormatIpAddress(AdapterIpAddress ip, string formatTemplate) =>
        formatTemplate
            .Replace("${Address}", ip.Address)
            .Replace("${CidrBits}", ip.CidrBits.ToString(System.Globalization.CultureInfo.InvariantCulture))
            .Replace("${Origin}", ip.Origin);
}
