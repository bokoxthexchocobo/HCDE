namespace HCDE.Playsim;

/// <summary>
/// Heal, missile, map-thing, and <c>SF_ObjState</c> head from <c>fragglescript/t_func.cpp</c> (**2985–3134**); chunk **939**.
/// </summary>
public static class VmFScriptParserCombatMap939Slice
{
    public const int ObjStateCount = 9;
    public const int MfCorpse = 1 << 4;

    public static int TestLocationReturnInt(bool valid) => valid ? 1 : 0;

    public static object? OptionalActor(int argc, object? arg0, object? trigger) =>
        argc > 0 ? arg0 : trigger;

    public enum HealMode
    {
        InvalidArgs,
        RestoreDefault,
        AddAmount,
    }

    public static HealMode HealModeFromArgc(int argc) => argc switch
    {
        < 2 => HealMode.RestoreDefault,
        2 => HealMode.AddAmount,
        _ => HealMode.InvalidArgs,
    };

    public static bool HealSyncsPlayer(bool hasPlayer) => hasPlayer;

    public static int ObjDeadReturn(int health, int flags) =>
        health <= 0 || (flags & MfCorpse) != 0 ? 1 : 0;

    public static bool SpawnMissileRequiresThreeArgs(int argc) => argc >= 3;

    public static bool SpawnMissileCanRun(object? shooter, object? target) =>
        shooter != null && target != null;

    public static bool MapThingIndexInRange(int index, int spawnedCount) =>
        index >= 0 && index < spawnedCount;

    public static int MapThingsCountReturn(int spawnedCount) => spawnedCount;

    public static bool ObjStateRequiresOneArg(int argc) => argc >= 1;

    public static bool ObjStateValid(int state) => state >= 1 && state <= ObjStateCount;

    public static int ObjStateNameIndex(int state) => state - 1;

    public static readonly string[] ObjStateNames =
    [
        "Spawn",
        "See",
        "Missile",
        "Melee",
        "Pain",
        "Death",
        "Raise",
        "XDeath",
        "Crash",
    ];

    public static string? ObjStateNameAt(int state) =>
        ObjStateValid(state) ? ObjStateNames[ObjStateNameIndex(state)] : null;

    public readonly record struct ObjStateInvoke(object? Actor, int State, string? StateName);

    public static ObjStateInvoke? BuildObjState(int argc, object? arg0, object? trigger, int stateArg0, int stateArg1)
    {
        if (!ObjStateRequiresOneArg(argc))
            return null;
        object? mo;
        int state;
        if (argc == 1)
        {
            mo = trigger;
            state = stateArg0;
        }
        else if (argc == 2)
        {
            mo = arg0;
            state = stateArg1;
        }
        else
            return null;

        if (mo == null || !ObjStateValid(state))
            return null;
        return new(mo, state, ObjStateNameAt(state));
    }

    public readonly record struct MapThingExistInvoke(int ReturnValue, bool Exists);

    public static MapThingExistInvoke BuildMapThingExist(int index, int spawnedCount, object? entry, bool isMapActor)
    {
        if (!MapThingIndexInRange(index, spawnedCount) || entry == null)
            return new(0, false);
        return new(isMapActor ? 1 : 0, true);
    }
}
