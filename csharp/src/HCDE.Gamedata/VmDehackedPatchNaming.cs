namespace HCDE.Gamedata;

/// <summary>Sanitized dehacked VM function names from <c>SetDehParams</c> in <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedPatchNaming
{
    private static readonly char[] UnsafeChars = [':', '.', '-', ' ', '\\', '/', '*', '?', '"'];

    public static string SanitizePatchName(string patchName)
    {
        var chars = patchName.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (Array.IndexOf(UnsafeChars, chars[i]) >= 0) chars[i] = '_';
        }

        return new string(chars);
    }

    public static bool IsCodePointerIndexInRange(int index, int aliasCount, int factoryCount) =>
        index >= 0 && index < aliasCount && index < factoryCount;
}
