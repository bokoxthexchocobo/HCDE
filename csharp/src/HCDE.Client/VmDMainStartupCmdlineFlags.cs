namespace HCDE.Client;

/// <summary><c>CheckCmdLine</c> dmflags and gametype branches from <c>d_main.cpp</c> (2594–2680).</summary>
public static class VmDMainStartupCmdlineFlags
{
    public const int DfNoMonsters = 0x00000001;
    public const int DfMonstersRespawn = 0x00000002;
    public const int DfFastMonsters = 0x00000004;
    public const int DfWeaponsStay = 0x00000010;
    public const int DfItemsRespawn = 0x00000020;
    public const int DfNoCoopWeaponSpawn = 0x02000000;
    public const int Df3NoPlayerClip = 0x00000001;
    public const int Df3CoopShareKeys = 0x00000002;
    public const int Df3RememberLastWeap = 0x00000004;
    public const int Df3LocalItems = 0x00000008;

    public static int ApplyMonsterFlags(int flags, bool noMonsters, bool respawn, bool fast)
    {
        if (noMonsters)
            flags |= DfNoMonsters;
        if (respawn)
            flags |= DfMonstersRespawn;
        if (fast)
            flags |= DfFastMonsters;
        return flags;
    }

    public static bool IsExplicitInvasionMode(bool hasExplicitGametype, int explicitGametype) =>
        hasExplicitGametype && explicitGametype == 4;

    public static bool IsExplicitMultiplayerMode(bool altDeath, bool deathmatch, bool coop) =>
        altDeath || deathmatch || coop;

    public static (int flags, int flags3) ApplyAltDeathMode(int flags, int flags3) =>
        (flags | DfItemsRespawn, flags3);

    public static (int flags, int flags3) ApplyDeathmatchMode(int flags, int flags3) =>
        (flags | DfWeaponsStay | DfItemsRespawn, flags3);

    public static (int flags, int flags3) ApplyCoopMode(int flags, int flags3, bool isHexen)
    {
        flags |= DfNoCoopWeaponSpawn;
        flags3 |= Df3NoPlayerClip | Df3CoopShareKeys | Df3RememberLastWeap;
        if (!isHexen)
            flags3 |= Df3LocalItems;
        return (flags, flags3);
    }

    public static bool ShouldApplyCoopBranch(bool coop, bool explicitInvasion) => coop && !explicitInvasion;

    public static bool ShouldApplySoloGametypeReset(
        bool explicitMultiplayer,
        bool hasExplicitGametype,
        bool launchingServer) =>
        !explicitMultiplayer && !hasExplicitGametype && !launchingServer;

    public static string FormatLumpFilterPrefix(string autoname) => $"{autoname}.";

    public static int AdvanceLumpFilterSegment(string filter, int lastPos)
    {
        var idx = filter.IndexOf('.', lastPos + 1);
        return idx;
    }

    public static string BuildAutoloadConfigName(string filter, int dotIndex) =>
        dotIndex > 0 ? filter[..dotIndex] + ".Autoload" : string.Empty;
}
