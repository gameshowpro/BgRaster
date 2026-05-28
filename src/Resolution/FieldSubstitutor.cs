namespace GameshowPro.BgRaster.Resolution;

record SubstitutionContext(
    string MachineName,
    int Width,
    int Height,
    int Index,
    string OutputName,
    int? ParentIndex = null);

static class FieldSubstitutor
{
    internal static string Substitute(string template, SubstitutionContext ctx)
    {
        string result = template
            .Replace("${MachineName}", ctx.MachineName)
            .Replace("${Width}", ctx.Width.ToString(System.Globalization.CultureInfo.InvariantCulture))
            .Replace("${Height}", ctx.Height.ToString(System.Globalization.CultureInfo.InvariantCulture))
            .Replace("${Index}", ctx.Index.ToString(System.Globalization.CultureInfo.InvariantCulture))
            .Replace("${IndexPlusOne}", (ctx.Index + 1).ToString(System.Globalization.CultureInfo.InvariantCulture))
            .Replace("${OutputName}", ctx.OutputName);

        if (ctx.ParentIndex.HasValue)
            result = result.Replace("${ParentIndex}", ctx.ParentIndex.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));

        return result;
    }
}
