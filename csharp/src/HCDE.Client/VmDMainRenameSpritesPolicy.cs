namespace HCDE.Client;

/// <summary><c>RenameSprites</c> gametype tables from <c>d_main.cpp</c> (2926–2999).</summary>
public static class VmDMainRenameSpritesPolicy
{
    public const int GameDoom = 0;
    public const int GameHeretic = 1;
    public const int GameHexen = 2;
    public const int GameStrife = 3;

    public static string ResolveAltBigFont(int gametype) =>
        gametype == GameStrife ? "SBIGFONT" : IsRavenGame(gametype) ? "HBIGFONT" : "DBIGFONT";

    public static bool IsRavenGame(int gametype) => gametype == GameHeretic || gametype == GameHexen;

    public static (uint[]? table, int pairCount) ResolveRenameTable(int gametype) =>
        gametype switch
        {
            GameDoom => (null, 0),
            GameHeretic => (HereticRenames, HereticRenames.Length / 2),
            GameHexen => (HexenRenames, HexenRenames.Length / 2),
            GameStrife => (StrifeRenames, StrifeRenames.Length / 2),
            _ => (null, 0)
        };

    public static bool ShouldRenameSprites(int gametype) => gametype != GameDoom;

    public static uint DecodeSpriteId(char a, char b, char c, char d) =>
        (uint)(a | (b << 8) | (c << 16) | (d << 24));

    private static readonly uint[] HereticRenames =
    {
        DecodeSpriteId('H', 'E', 'A', 'D'), DecodeSpriteId('L', 'I', 'C', 'H')
    };

    private static readonly uint[] HexenRenames =
    {
        DecodeSpriteId('B', 'A', 'R', 'L'), DecodeSpriteId('Z', 'B', 'A', 'R'),
        DecodeSpriteId('A', 'R', 'M', '1'), DecodeSpriteId('A', 'R', '_', '1'),
    };

    private static readonly uint[] StrifeRenames =
    {
        DecodeSpriteId('M', 'I', 'S', 'L'), DecodeSpriteId('S', 'M', 'I', 'S'),
        DecodeSpriteId('A', 'R', 'M', '1'), DecodeSpriteId('A', 'R', 'M', '3'),
    };

    public static bool IsSpriteNamespace(int ns, int spriteNs) => ns == spriteNs;

    public static bool IsMinotaurZFrame(uint dword, uint mntrId) =>
        dword == mntrId;

    public static bool ShouldRenameAllSprites(bool oldSpritesArg, bool noSpriteRename) =>
        oldSpritesArg || noSpriteRename;

    public static bool ShouldRenameIwadSprite(bool renameAll, int container, int iwadNum) =>
        renameAll || container == iwadNum;

    public static bool ShouldPatchMinotaurFrames(bool mntrZFound) => !mntrZFound;

    public static bool ShouldShiftMinotaurFrame(char frame) => frame >= 'F' && frame <= 'K';

    public static char ShiftMinotaurFrame(char frame) => (char)(frame + ('U' - 'F'));
}

