namespace HCDE.Playsim;

/// <summary>
/// `CCMD(map)`, `recordmap`, and `open` from <c>g_level.cpp</c> (**301–450**).
/// </summary>
public static class VmGLevelMapCommands1012Slice
{
    public const int GaNewGame2 = 3;
    public const int GaRecordGame = 4;
    public const int UnsetSkill = -1;
    public const int MapCommandMinArgc = 2;
    public const int RecordMapCommandMinArgc = 3;
    public const int OpenCommandMinArgc = 2;

    public enum SpOnlyCommandRejectReason
    {
        None,
        NetGame,
    }

    public enum MultiplayerNextMode
    {
        Single,
        Coop,
        Deathmatch,
    }

    public readonly struct MultiplayerNextFlags
    {
        public bool Deathmatch { get; init; }
        public bool MultiplayerNext { get; init; }
    }

    public readonly struct OpenMapDeferredSetup
    {
        public string MapName { get; init; }
        public int Skill { get; init; }
        public int TraceGameAction { get; init; }
    }

    public static SpOnlyCommandRejectReason ResolveSpOnlyCommandRejectReason(bool netGame) =>
        netGame ? SpOnlyCommandRejectReason.NetGame : SpOnlyCommandRejectReason.None;

    public static bool HasMapCommandTarget(int argc) => argc >= MapCommandMinArgc;

    public static bool HasRecordMapCommandTarget(int argc) => argc >= RecordMapCommandMinArgc;

    public static bool HasOpenCommandTarget(int argc) => argc >= OpenCommandMinArgc;

    public static string ResolveMapNameArgument(string mapArg, string primaryMapName) =>
        mapArg == "*" ? primaryMapName : mapArg;

    public static MultiplayerNextMode ResolveMultiplayerNextMode(string? modeArg)
    {
        if (string.IsNullOrEmpty(modeArg))
            return MultiplayerNextMode.Single;
        if (string.Equals(modeArg, "coop", StringComparison.OrdinalIgnoreCase))
            return MultiplayerNextMode.Coop;
        if (string.Equals(modeArg, "dm", StringComparison.OrdinalIgnoreCase))
            return MultiplayerNextMode.Deathmatch;
        return MultiplayerNextMode.Single;
    }

    public static MultiplayerNextFlags ResolveMultiplayerNextFlags(MultiplayerNextMode mode) =>
        mode switch
        {
            MultiplayerNextMode.Coop => new MultiplayerNextFlags { Deathmatch = false, MultiplayerNext = true },
            MultiplayerNextMode.Deathmatch => new MultiplayerNextFlags { Deathmatch = true, MultiplayerNext = true },
            _ => new MultiplayerNextFlags { Deathmatch = false, MultiplayerNext = false },
        };

    public static bool ShouldDeferNewGameForValidMap(bool mapDataValid, bool rejectedAsNetGame) =>
        mapDataValid && !rejectedAsNetGame;

    public static int ResolveRecordMapTraceAction() => GaRecordGame;

    public static string BuildOpenMapPath(string fileArg) => $"file:{fileArg}";

    public static OpenMapDeferredSetup ResolveOpenMapDeferredSetup(string fileArg, bool mapDataValid) =>
        new()
        {
            MapName = BuildOpenMapPath(fileArg),
            Skill = UnsetSkill,
            TraceGameAction = GaNewGame2,
        };
}
