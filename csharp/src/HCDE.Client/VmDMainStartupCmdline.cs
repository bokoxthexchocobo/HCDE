namespace HCDE.Client;

/// <summary><c>D_DoomInit</c>, <c>AddAutoloadFiles</c>, and <c>CheckCmdLine</c> opening from <c>d_main.cpp</c> (2488–2680).</summary>
public static class VmDMainStartupCmdline
{
    public static readonly string[] CollectFileExtensions =
    {
        ".deh", ".bex", ".cfg", ".lmp"
    };

    public static bool ShouldLoadLightsPk3(int startupLoadLights, bool autoloadLights, bool nolightsArg, bool shareware) =>
        !shareware && (startupLoadLights == 1 || (startupLoadLights != 0 && autoloadLights)) && !nolightsArg;

    public static bool ShouldLoadBrightmapsPk3(int startupBrightmaps, bool autoloadBrightmaps, bool nobrightmapsArg, bool shareware) =>
        !shareware && (startupBrightmaps == 1 || (startupBrightmaps != 0 && autoloadBrightmaps)) && !nobrightmapsArg;

    public static bool ShouldLoadWidescreenPk3(int startupWidescreen, bool autoloadWidescreen, bool nowidescreenArg, bool shareware) =>
        !shareware && (startupWidescreen == 1 || (startupWidescreen != 0 && autoloadWidescreen)) && !nowidescreenArg;

    public static bool IsJoiningNetworkGame(bool join, bool dedicatedJoin, bool joinDedicated) =>
        join || dedicatedJoin || joinDedicated;

    public static bool ShouldDisableAutoload(
        bool shareware,
        bool noAutoload,
        bool disableAutoload,
        bool host,
        bool joiningNetwork,
        bool dedicatedServer) =>
        shareware || noAutoload || disableAutoload || host || joiningNetwork || dedicatedServer;

    public static bool TryParseExplicitGametype(IReadOnlyList<string> args, out int gametype)
    {
        gametype = -1;
        for (var i = 1; i < args.Count; i++)
        {
            var a = args[i];
            if (a.StartsWith('+') && a[1..].Equals("sv_gametype", StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 < args.Count && int.TryParse(args[i + 1], out gametype))
                    return true;
            }

            if (a.StartsWith('+') && a[1..].Equals("set", StringComparison.OrdinalIgnoreCase)
                && i + 2 < args.Count
                && args[i + 1].Equals("sv_gametype", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(args[i + 2], out gametype))
                return true;
        }

        return false;
    }

    public static int ApplyDeathmatchFlags(int flags, bool altDeath, bool deathmatch, bool coop, bool explicitInvasion)
    {
        if (altDeath)
            return flags | 0x20;
        if (deathmatch)
            return flags | 0x10 | 0x20;
        if (coop && !explicitInvasion)
            return flags | 0x2000000;
        return flags;
    }

    public static bool ShouldResetSoloGametype(bool hasExplicitGametype, bool launchingServer) =>
        !hasExplicitGametype && !launchingServer;

    public static bool ShouldApplyMonsterFlags(bool noMonsters, bool respawn, bool fast) =>
        noMonsters || respawn || fast;

    public static bool ShouldSkipExtrasPk3(bool shareware, bool noExtrasArg) =>
        shareware || noExtrasArg;

    public static bool ShouldLoadZvoxAutoload(bool shareware, bool disableAutoload) =>
        !shareware && !disableAutoload;

    public static IReadOnlyList<string> EnumerateSkinGlobPatterns() => new[] { "*.wad", "*.pk3" };

    public static string CombineSkinDirectory(string baseDir, string subdir = "skins") =>
        baseDir.EndsWith('/') || baseDir.EndsWith('\\') ? baseDir + subdir : baseDir + "/" + subdir;
}
