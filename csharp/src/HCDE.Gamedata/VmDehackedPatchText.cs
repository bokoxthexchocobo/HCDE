namespace HCDE.Gamedata;

/// <summary><c>PatchText</c> helpers from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedPatchText
{
    public static bool IsDoomMusicHack(string oldText) =>
        oldText.Equals("Doom", StringComparison.OrdinalIgnoreCase);

    public static string ResolveMusicDoomKey() => "MUSIC_DOOM";

    public static bool TryReplaceSpriteName(int oldSize, string oldText, string newText, out string spriteOld, out string spriteNew)
    {
        spriteOld = "";
        spriteNew = "";
        if (oldSize != 4 || oldText.Length < 4 || newText.Length < 4) return false;
        spriteOld = oldText[..4];
        spriteNew = newText[..4];
        return true;
    }

    public static string NormalizeEnglishLookupKey(string oldText) =>
        oldText.Replace(' ', ' ').Trim();
}
