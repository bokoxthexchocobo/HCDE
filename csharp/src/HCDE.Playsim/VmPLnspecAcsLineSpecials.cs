namespace HCDE.Playsim;

/// <summary>Native ACS/FS and combined elevator specials from <c>p_lnspec.cpp</c> (1905–2046).</summary>
public static class VmPLnspecAcsLineSpecials
{
    public enum ElevatorKind
    {
        LowerBoth,
        RaiseBoth,
        MoveToFloor,
        RaiseToNearest,
        LowerToNearest,
    }

    public const int BoomBrokenEmuYear = 1998;

    public readonly record struct AcsScriptInvoke(
        int Script,
        string? MapName,
        int[] Args,
        int ArgCount,
        int Flags,
        bool FoundMap);

    public static AcsScriptInvoke BuildExecute(
        int script,
        int mapArg,
        string currentMap,
        int arg2,
        int arg3,
        int arg4,
        bool backSide,
        bool always,
        bool wantResult,
        Func<int, string?> findLevelByNum)
    {
        var flags = VmPLnspecArgMacros.AcsBacksideFlag(backSide);
        if (always)
            flags |= VmPLnspecArgMacros.AcsAlwaysFlag;
        if (wantResult)
            flags |= VmPLnspecArgMacros.AcsWantResultFlag;

        if (wantResult)
        {
            var args = new[] { arg2, arg3, arg4, 0 };
            return new(script, currentMap, args, 4, flags, true);
        }

        var execArgs = new[] { arg2, arg3, arg4 };
        if (mapArg == 0)
            return new(script, currentMap, execArgs, 3, flags, true);

        var name = findLevelByNum(mapArg);
        return name == null
            ? new(script, null, execArgs, 3, flags, false)
            : new(script, name, execArgs, 3, flags, true);
    }

    public static AcsScriptInvoke ResolveMapForExecute(int mapArg, string currentMap, Func<int, string?> findLevelByNum) =>
        BuildExecute(0, mapArg, currentMap, 0, 0, 0, false, false, false, findLevelByNum);

    public static AcsScriptInvoke AcsExecute(int script, int mapArg, string currentMap, int a2, int a3, int a4, bool backSide, Func<int, string?> findLevelByNum) =>
        BuildExecute(script, mapArg, currentMap, a2, a3, a4, backSide, false, false, findLevelByNum);

    public static AcsScriptInvoke AcsExecuteAlways(int script, int mapArg, string currentMap, int a2, int a3, int a4, bool backSide, Func<int, string?> findLevelByNum) =>
        BuildExecute(script, mapArg, currentMap, a2, a3, a4, backSide, true, false, findLevelByNum);

    public static AcsScriptInvoke AcsExecuteWithResult(int script, string currentMap, int a1, int a2, int a3, int a4, bool backSide)
    {
        var flags = VmPLnspecArgMacros.AcsBacksideFlag(backSide)
            | VmPLnspecArgMacros.AcsAlwaysFlag
            | VmPLnspecArgMacros.AcsWantResultFlag;
        return new(script, currentMap, new[] { a1, a2, a3, a4 }, 4, flags, true);
    }

    public static bool LockedExecuteRequiresKey(int lockArg, bool hasKeys) => lockArg != 0 && !hasKeys;

    public static bool LockedExecuteDoorRequiresKey(int lockArg, bool hasKeys) => lockArg != 0 && !hasKeys;

    public static bool ShouldDeferToExecuteAfterKeyCheck(int lockArg, bool hasKeys) =>
        lockArg == 0 || hasKeys;

    public readonly record struct AcsControlInvoke(int Script, string? MapName, bool UseCurrentMap);

    public static AcsControlInvoke ResolveSuspend(int script, int mapArg, string currentMap, Func<int, string?> findLevelByNum)
    {
        if (mapArg == 0)
            return new(script, currentMap, true);
        var name = findLevelByNum(mapArg);
        return new(script, name, false);
    }

    public static AcsControlInvoke ResolveTerminate(int script, int mapArg, string currentMap, Func<int, string?> findLevelByNum) =>
        ResolveSuspend(script, mapArg, currentMap, findLevelByNum);

    public static bool SuspendUsesCurrentMap(int mapArg) => mapArg == 0;

    public static bool TerminateUsesCurrentMap(int mapArg) => mapArg == 0;

    public static bool FsExecuteRejectBackside(bool firstSideOnly, bool backSide, bool hasLine) =>
        firstSideOnly && hasLine && backSide;

    public static bool FsExecuteRequiresKey(int lockArg, bool hasKeys) => lockArg != 0 && !hasKeys;

    public static bool FsExecuteMsgTypeIsDoor(int msgType) => msgType != 0;

    public static bool FsExecuteUseDoorKeyCheck(int msgType) => FsExecuteMsgTypeIsDoor(msgType);

    public readonly record struct CombinedElevatorInvoke(ElevatorKind Kind, double Speed, int Height, int Tag);

    public static CombinedElevatorInvoke LowerBoth(int tag, int speed, int height) =>
        new(ElevatorKind.LowerBoth, VmPLnspecArgMacros.Speed(speed), height, tag);

    public static CombinedElevatorInvoke RaiseBoth(int tag, int speed, int height) =>
        new(ElevatorKind.RaiseBoth, VmPLnspecArgMacros.Speed(speed), height, tag);

    public static bool ShouldRunBrokenBoomFloor(int boomEmu, bool ceilingOk) =>
        boomEmu != BoomBrokenEmuYear || !ceilingOk;

    public static bool CombineLowerRaiseResults(bool ceilingResult, bool floorResult, int boomEmu) =>
        ceilingResult | (ShouldRunBrokenBoomFloor(boomEmu, ceilingResult) && floorResult);

    public static ElevatorKind ResolveElevatorKind(int opcode) => opcode switch
    {
        0 => ElevatorKind.MoveToFloor,
        1 => ElevatorKind.RaiseToNearest,
        _ => ElevatorKind.LowerToNearest,
    };

    public static CombinedElevatorInvoke MoveToFloor(int tag, int speed) =>
        new(ElevatorKind.MoveToFloor, VmPLnspecArgMacros.Speed(speed), 0, tag);

    public static CombinedElevatorInvoke RaiseToNearest(int tag, int speed) =>
        new(ElevatorKind.RaiseToNearest, VmPLnspecArgMacros.Speed(speed), 0, tag);

    public static CombinedElevatorInvoke LowerToNearest(int tag, int speed) =>
        new(ElevatorKind.LowerToNearest, VmPLnspecArgMacros.Speed(speed), 0, tag);
}
