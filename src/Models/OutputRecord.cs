namespace GameshowPro.BgRaster.Models;

record OutputRecord
{
    internal string Id { get; init; } = "";
    internal int Index { get; init; }
    internal int DesktopX { get; init; }
    internal int DesktopY { get; init; }
    internal int WidthPx { get; init; }
    internal int HeightPx { get; init; }
    internal int DpiX { get; init; }
    internal int DpiY { get; init; }
    internal int Rotation { get; init; }
    internal int RefreshRateHz { get; init; }
    internal string AdapterName { get; init; } = "";
    internal string FriendlyName { get; init; } = "";
}
