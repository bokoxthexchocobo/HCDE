namespace HCDE.Playsim;

/// <summary>
/// Exit transitions and early <c>CreateIntermission</c> from <c>g_level.cpp</c> (**901–1050**).
/// </summary>
public static class VmGLevelExitIntermission1013Slice
{
    public const int ChangeLevelKeepFacing = 1;
    public const int Level3ExitNormalUsed = 0x00000020;
    public const int Level3ExitSecretUsed = 0x00000040;
    public const uint LevelChangeMapCheat = 0x40000000;
    public const uint Level2NoClusterText = 0x00200000;

    public const string EndSequencePrefix = "enDSeQ";
    public const string ExitTextKeyNormal = "Normal";
    public const string ExitTextKeySecret = "Secret";
    public const string InterStrife = "Inter_Strife";
    public const string InterStrifeGood = "Inter_Strife_Good";
    public const string InterStrifeSad = "Inter_Strife_Sad";

    [Flags]
    public enum ExitTextDefined : short
    {
        None = 0,
        Text = 1,
        Music = 2,
        Backdrop = 4,
        Lookup = 8,
        Pic = 16,
    }

    public readonly struct ChangeLevelVmArgs
    {
        public string LevelName { get; init; }
        public int Position { get; init; }
        public int InFlags { get; init; }
        public int NextSkill { get; init; }
    }

    public readonly struct ExitLevelTransition
    {
        public uint Flags3After { get; init; }
        public string NextMap { get; init; }
        public int Position { get; init; }
        public int ChangeFlags { get; init; }
    }

    public readonly struct SecretExitTransition
    {
        public uint Flags3After { get; init; }
        public string ResolvedMap { get; init; }
        public int Position { get; init; }
    }

    public readonly struct FinaleLaunchPlan
    {
        public bool ShouldLaunch { get; init; }
        public bool EndgameSequence { get; init; }
        public string EndSequenceName { get; init; }
        public string ExitTextKey { get; init; }
        public ExitTextDefined ExitDefined { get; init; }
    }

    public static bool HasChangeLevelActionTarget(int paramCountAfterSelf) =>
        paramCountAfterSelf >= 4;

    public static ChangeLevelVmArgs PackChangeLevelVmArgs(
        string levelName,
        int position,
        int inFlags,
        int nextSkill) =>
        new()
        {
            LevelName = levelName,
            Position = position,
            InFlags = inFlags,
            NextSkill = nextSkill,
        };

    public static bool MatchesEndSequenceMapPrefix(string? mapName) =>
        !string.IsNullOrEmpty(mapName)
        && mapName.Length >= EndSequencePrefix.Length
        && string.Compare(mapName, 0, EndSequencePrefix, 0, EndSequencePrefix.Length, StringComparison.Ordinal) == 0;

    public static string ResolveSecretExitMap(string primaryNextMap, string? secretMap, bool secretMapDataValid)
    {
        var next = primaryNextMap;
        if (string.IsNullOrEmpty(secretMap))
            return next;
        if (MatchesEndSequenceMapPrefix(secretMap) || secretMapDataValid)
            next = secretMap;
        return next;
    }

    public static ExitLevelTransition PlanNormalExit(
        uint flags3,
        string nextMap,
        int position,
        bool keepFacing) =>
        new()
        {
            Flags3After = flags3 | Level3ExitNormalUsed,
            NextMap = nextMap,
            Position = position,
            ChangeFlags = keepFacing ? ChangeLevelKeepFacing : 0,
        };

    public static SecretExitTransition PlanSecretExit(
        uint flags3,
        string resolvedMap,
        int position) =>
        new()
        {
            Flags3After = flags3 | Level3ExitSecretUsed,
            ResolvedMap = resolvedMap,
            Position = position,
        };

    public static bool ShouldSkipCreateIntermissionForCheat(uint levelFlags) =>
        (levelFlags & LevelChangeMapCheat) != 0;

    public static bool TryParseEndgameSequenceName(string nextLevel, out string sequenceName)
    {
        sequenceName = string.Empty;
        if (!MatchesEndSequenceMapPrefix(nextLevel) || nextLevel.Length <= EndSequencePrefix.Length)
            return false;
        var hex = nextLevel[EndSequencePrefix.Length..];
        if (hex.Length == 0 || !long.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out var value))
            return false;
        sequenceName = $"#{value}";
        return true;
    }

    public static string ResolveStrifeEndingSequence(
        string parsedSequenceToken,
        bool playerHasQuestItem25,
        bool playerHasQuestItem28)
    {
        if (!string.Equals(parsedSequenceToken, InterStrife, StringComparison.Ordinal))
            return parsedSequenceToken;
        return playerHasQuestItem25 || playerHasQuestItem28 ? InterStrifeGood : InterStrifeSad;
    }

    public static string ResolveEndgameExitTextKey(uint flags3) =>
        (flags3 & Level3ExitSecretUsed) != 0 ? ExitTextKeySecret : ExitTextKeyNormal;

    public static string? ResolveNonEndgameExitTextKey(uint flags3, string nextLevel)
    {
        if ((flags3 & Level3ExitSecretUsed) != 0)
            return ExitTextKeySecret;
        if ((flags3 & Level3ExitNormalUsed) != 0)
            return ExitTextKeyNormal;
        return nextLevel;
    }

    public static bool ShouldUseClusterEndgameText(uint levelInfoFlags2) =>
        (levelInfoFlags2 & Level2NoClusterText) == 0;

    public static bool HasDefinedExitText(ExitTextDefined defined) =>
        (defined & ExitTextDefined.Text) != 0;

    public static bool ShouldRunMapExitIntermission(bool deathmatch, uint levelFlags) =>
        !deathmatch && !ShouldSkipCreateIntermissionForCheat(levelFlags);

    public static FinaleLaunchPlan PlanEndgameFinaleFromExitText(
        uint flags3,
        ExitTextDefined defined,
        string parsedEndSequence) =>
        new()
        {
            ShouldLaunch = HasDefinedExitText(defined),
            EndgameSequence = true,
            EndSequenceName = parsedEndSequence,
            ExitTextKey = ResolveEndgameExitTextKey(flags3),
            ExitDefined = defined,
        };

    public static FinaleLaunchPlan PlanMapExitFinaleFromExitText(
        uint flags3,
        string nextLevel,
        ExitTextDefined defined) =>
        new()
        {
            ShouldLaunch = HasDefinedExitText(defined),
            EndgameSequence = false,
            EndSequenceName = string.Empty,
            ExitTextKey = ResolveNonEndgameExitTextKey(flags3, nextLevel) ?? nextLevel,
            ExitDefined = defined,
        };

    public static bool ShouldReturnEarlyAfterMapExitFinale(FinaleLaunchPlan plan) =>
        plan.ShouldLaunch && !plan.EndgameSequence;
}
