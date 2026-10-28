namespace HCDE.Playsim;

/// <summary>
/// <c>SF_SetColor</c> loop tail through <c>SaveCurrentScript</c> from <c>fragglescript/t_func.cpp</c> (**3695–3844**); chunk **941**.
/// </summary>
public static class VmFScriptParserShotLine941Slice
{
    public const int Mf3IsMonster = 1 << 4;
    public const uint LineFlagMonstersCanActivate = 1u << 0;
    public const uint LineFlagRepeatSpecial = 1u << 1;
    public const uint LineFlagSpacMask = 1u << 2;
    public const uint LineFlagFirstSideOnly = 1u << 3;

    public static uint LineFlagPreserveMask =>
        LineFlagMonstersCanActivate | LineFlagRepeatSpecial | LineFlagSpacMask | LineFlagFirstSideOnly;

    public static bool SetColorLoopAppliesToSector(int sectorIndex) => sectorIndex >= 0;

    public static bool SpawnShot2RequiresTwoArgs(int argc) => argc >= 2;

    public static object? ResolveSpawnShot2Source(bool secondArgIsInt, int secondArgInt, object? actorValue, object? trigger) =>
        secondArgIsInt && secondArgInt < 0 ? trigger : actorValue;

    public static bool SpawnShot2UsesOptionalZ(int argc) => argc > 2;

    public static bool SpawnShot2AbortsWithoutSource(object? source) => source is null;

    public static bool SpawnShot2ClearsMissileOnFailedSpawn(bool spawnOk) => !spawnOk;

    public static object? SpawnShot2ReturnMobj(object? spawned, bool checkSpawnOk)
    {
        if (spawned == null)
            return null;
        if (SpawnShot2ClearsMissileOnFailedSpawn(checkSpawnOk))
            return null;
        return spawned;
    }

    public readonly record struct SpawnShot2Invoke(object Source, double ZOffset);

    public static SpawnShot2Invoke? BuildSpawnShot2Invoke(
        int argc,
        bool secondArgIsInt,
        int secondArgInt,
        object? actorValue,
        object? trigger,
        double zArg)
    {
        if (!SpawnShot2RequiresTwoArgs(argc))
            return null;
        var source = ResolveSpawnShot2Source(secondArgIsInt, secondArgInt, actorValue, trigger);
        if (SpawnShot2AbortsWithoutSource(source))
            return null;
        var z = SpawnShot2UsesOptionalZ(argc) ? zArg : 0;
        return new(source!, z);
    }

    public static bool KillInSectorRequiresOneArg(int argc) => argc >= 1;

    public static bool KillInSectorShouldMassacre(bool flags3HasMonster, bool sectorHasTag) =>
        flags3HasMonster && sectorHasTag;

    public static bool SetLineTriggerRequiresTwoArgs(int argc) => argc >= 2;

    public static int SetLineTriggerTagFromArgc(int argc, int tagArg) => argc > 2 ? tagArg : 0;

    public static bool SetLineTriggerIteratorActive(int lineIndex) => lineIndex >= 0;

    public static uint MergeSetLineTriggerFlags(uint translatedCurrent, uint previous) =>
        (translatedCurrent & LineFlagPreserveMask) | (previous & ~LineFlagPreserveMask);

    public static bool RunLineSpecialRequiresMinArgs(int argc, int minArgs) => argc >= minArgs;

    public static int RunLineSpecialFillArg(int argc, int index, int argvValue) =>
        argc > index ? argvValue : 0;

    public static void FillRunLineSpecialArgs(int argc, ReadOnlySpan<int> argv, Span<int> argsOut)
    {
        for (var i = 0; i < argsOut.Length; i++)
            argsOut[i] = RunLineSpecialFillArg(argc, i, i < argv.Length ? argv[i] : 0);
    }

    public static bool SaveCurrentScriptHasThinker(object? fraggleThinker) => fraggleThinker != null;

    public readonly record struct SaveCurrentScriptInvoke(object Trigger, object Script, int RoverIndex);

    public static SaveCurrentScriptInvoke? BuildSaveCurrentScript(object? thinker, object? trigger, object? script, int roverIndex)
    {
        if (!SaveCurrentScriptHasThinker(thinker) || trigger is null || script is null)
            return null;
        return new(trigger, script, roverIndex);
    }
}
