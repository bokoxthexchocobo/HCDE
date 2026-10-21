namespace HCDE.Net.Core;

/// <summary><c>Net_DoCommand</c> summon family (<c>d_net.cpp</c>).</summary>
public static class VmDNetDoCommandSummon
{
    public const ulong MfMissile = 1;
    public const ulong MfFriendly = 1;
    public const ulong Mf3NoBlockMonsters = 1;
    public const ulong MtfConsoleThing = 1;

    public static bool SummonRequiresSourcePawnNative(object? source) => source is not null;

    public static bool SummonTypeResolvedNative(object? typeInfo) => typeInfo is not null;

    public static bool ShouldSpawnPlayerMissileNative(ulong mobjFlags, ulong mfMissile) => (mobjFlags & mfMissile) != 0;

    public static bool ShouldApplySummonFriendFlagsNative(int cmd, int demSummonFriend, int demSummonFriend2, int demSummonMbf) =>
        cmd == demSummonFriend || cmd == demSummonFriend2 || cmd == demSummonMbf;

    public static bool ShouldClearSummonFriendFlagsNative(int cmd, int demSummonFoe, int demSummonFoe2) =>
        cmd == demSummonFoe || cmd == demSummonFoe2;

    public static bool Summon2ReadsExtendedFieldsNative(int cmd, int demSummon2, int demSummonFoe2) =>
        cmd >= demSummon2 && cmd <= demSummonFoe2;

    public static bool ShouldRunNamedScriptNative(int gameState, int levelState) => gameState == levelState;

    public static double SummonSpawnOffsetRadiusNative(double defRadius, double sourceRadius) =>
        defRadius * 2 + sourceRadius;

    public static bool ShouldMarkConsoleSpawnedNative(ulong spawnFlags, ulong consoleThingFlag) =>
        (spawnFlags & consoleThingFlag) == 0;

    public static int SummonFriendPlayerIndexNative(int player) => player + 1;

    public static bool ShouldApplyMbfNoBlockMonstersNative(int cmd, int demSummonMbf, ulong flags3, ulong mf3NoBlockMonsters) =>
        cmd == demSummonMbf && (flags3 & mf3NoBlockMonsters) == 0;

    public static bool ShouldAssignSummon2YawNative(int cmd, int demSummon2, int demSummonFoe2) =>
        cmd >= demSummon2 && cmd <= demSummonFoe2;

    public static bool ShouldAssignSummon2ArgsNative(int cmd, int demSummon2, int demSummonFoe2, int tid) =>
        cmd >= demSummon2 && cmd <= demSummonFoe2 && tid != 0;

    public static int ComputeSummon2YawDegreesNative(int sourceYawDegrees, int angleOffsetDegrees) =>
        sourceYawDegrees - angleOffsetDegrees;

    public static bool ShouldOfferAutosaveNative(
        bool netgame,
        bool demoplayback,
        int disableAutosave,
        int autosaveCount,
        int playerState,
        int liveState,
        bool deathmatch) =>
        !netgame && !demoplayback && disableAutosave < 2 && autosaveCount != 0
        && playerState == liveState && !deathmatch;

    public static bool VisualThinkerClassAllowedNative(object? typeInfo, bool isVisualThinkerDescendant) =>
        typeInfo is not null && isVisualThinkerDescendant;

    public static bool ShouldRunSprayOnLevelNative(object? mobj) => mobj is not null;

    public static bool ShouldRunMdkCheatNative(object? player) => player is not null;

    public static bool FovChangedNative(float desiredFov, float newFov) => newFov != desiredFov;

    public static bool ShouldBroadcastFovToClientsNative(bool authoritySlot) => authoritySlot;

    public static bool RunScriptUsesAlwaysFlagNative(int cmd, int demRunScript2) => cmd == demRunScript2;

    public static int RunNamedScriptIndexFromNameNative(int nameIndex) => -nameIndex;

    public static bool RunNamedScriptAlwaysFlagNative(int argnByte) => (argnByte & 128) != 0;

    public static int RunNamedScriptArgCountNative(int argnByte) => argnByte & 127;

    public static bool ShouldClampRunSpecialArgNative(int index, int maxArgs) =>
        index >= 0 && index < maxArgs;

    public static bool ShouldExecuteRunSpecialNative(bool checkCheatMode, bool isConsolePlayer) =>
        !checkCheatMode || isConsolePlayer;
}
