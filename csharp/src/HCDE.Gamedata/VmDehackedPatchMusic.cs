namespace HCDE.Gamedata;

/// <summary><c>PatchMusic</c> key construction from <c>d_dehacked.cpp</c>.</summary>
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
}
