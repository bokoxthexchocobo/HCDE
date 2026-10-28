namespace HCDE.Gamedata;

/// <summary><c>D_LoadDehLumps</c> / patch sniffing from <c>d_dehacked.cpp</c> (3212–3375).</summary>
public static class VmDehackedLumpLoader
{
    public static bool IsRootDehOrBex(string fullName)
    {
        if (fullName.Contains('/'))
            return false;
        var ext = Path.GetExtension(fullName);
        return ext.Equals(".deh", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".bex", StringComparison.OrdinalIgnoreCase);
    }

    public static bool ShouldSkipIwadLump(bool fromPwadSource, int fileContainer, int maxIwadNum) =>
        fromPwadSource && fileContainer <= maxIwadNum;

    public static bool IsSupportedDehackedHeader(ReadOnlySpan<char> header) =>
        header.StartsWith("Patch File for DeHackEd v".AsSpan(), StringComparison.Ordinal)
        && header.Length > 25
        && (header[25] >= '3' || (header[25] >= '2' && header.Length > 27 && header[27] >= '3'));
}
