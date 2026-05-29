namespace GameshowPro.BgRaster.Resolution;

record SubstitutionContext(
    string MachineName,
    int OutputWidth,
    int OutputHeight,
    int OutputIndex,
    string OutputName,
    int SliceWidth,
    int SliceHeight,
    int SliceIndex = 0);

static class FieldSubstitutor
{
    internal static string Substitute(string template, SubstitutionContext ctx)
    {
        string outputIndex = ctx.OutputIndex.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string outputIndexPlusOne = (ctx.OutputIndex + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
        string outputLetter = ToLetterToken(ctx.OutputIndex);
        string sliceIndex = ctx.SliceIndex.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string sliceIndexPlusOne = (ctx.SliceIndex + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
        string sliceLetter = ToLetterToken(ctx.SliceIndex);

        string result = template
            .Replace("${MachineName}", ctx.MachineName)
            .Replace("${OutputWidth}", ctx.OutputWidth.ToString(System.Globalization.CultureInfo.InvariantCulture))
            .Replace("${OutputHeight}", ctx.OutputHeight.ToString(System.Globalization.CultureInfo.InvariantCulture))
            .Replace("${OutputIndexPlusOne}", outputIndexPlusOne)
            .Replace("${OutputIndex}", outputIndex)
            .Replace("${OutputLetter}", outputLetter)
            .Replace("${SliceWidth}", ctx.SliceWidth.ToString(System.Globalization.CultureInfo.InvariantCulture))
            .Replace("${SliceHeight}", ctx.SliceHeight.ToString(System.Globalization.CultureInfo.InvariantCulture))
            .Replace("${SliceIndexPlusOne}", sliceIndexPlusOne)
            .Replace("${SliceIndex}", sliceIndex)
            .Replace("${SliceLetter}", sliceLetter)
            .Replace("${OutputName}", ctx.OutputName);

        return result;
    }

    static string ToLetterToken(int zeroBasedIndex)
    {
        if (zeroBasedIndex < 0)
            return "?";

        int value = zeroBasedIndex;
        StringBuilder builder = new();
        while (value >= 0)
        {
            int remainder = value % 26;
            builder.Insert(0, (char)('A' + remainder));
            value = (value / 26) - 1;
        }

        return builder.ToString();
    }
}
