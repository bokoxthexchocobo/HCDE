namespace HCDE.Gamedata;

/// <summary><c>PatchMusic</c> from <c>d_dehacked.cpp</c> (2872–2891).</summary>
public static class VmDehackedPatchMusic
{
    public static string BuildMusicKey(string line1) => $"MUSIC_{line1}";

    public static bool TryParseMusicLine(string line1, string line2, out string key, out string newName)
    {
        key = "";
        newName = "";
        if (string.IsNullOrWhiteSpace(line1)) return false;
        key = BuildMusicKey(line1.Trim());
        newName = line2.Trim();
        return newName.Length > 0;
    }

    public static int RunPatchMusicNative(
        Func<(int Result, string Line1, string Line2)> getLine,
        Action<string, string> insertMusicString)
    {
        var result = 0;
        while (true)
        {
            (result, var line1, var line2) = getLine();
            if (result != 1)
                return result;

            if (!TryParseMusicLine(line1, line2, out var key, out var newName))
                continue;

            insertMusicString(key, newName);
        }
    }
}
