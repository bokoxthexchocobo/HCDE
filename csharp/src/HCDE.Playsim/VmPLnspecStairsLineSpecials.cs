namespace HCDE.Playsim;

/// <summary>Native <c>LS_Stairs_*</c> and <c>LS_Generic_Stairs</c> from <c>p_lnspec.cpp</c> (569–650).</summary>
public static class VmPLnspecStairsLineSpecials
{
    public const int StairUseSpecials = 1;
    public const int StairSync = 2;
    public const int StairCrush = 4;

    public enum BuildDirection
    {
        Down,
        Up,
    }

    public readonly record struct StairInvoke(
        int Tag,
        BuildDirection Direction,
        int StepHeight,
        double Speed,
        int DelayTics,
        int Reset,
        int Flags);

    public static StairInvoke FloorStop(int tag) =>
        new(tag, BuildDirection.Down, 0, 0, 0, 0, 0);

    public static StairInvoke BuildDown(int tag, int speed, int height, int delay, int reset) =>
        new(tag, BuildDirection.Down, height, VmPLnspecArgMacros.Speed(speed), VmPLnspecArgMacros.Tics(delay), reset, StairUseSpecials);

    public static StairInvoke BuildUp(int tag, int speed, int height, int delay, int reset) =>
        new(tag, BuildDirection.Up, height, VmPLnspecArgMacros.Speed(speed), VmPLnspecArgMacros.Tics(delay), reset, StairUseSpecials);

    public static StairInvoke BuildDownSync(int tag, int speed, int height, int reset) =>
        new(tag, BuildDirection.Down, height, VmPLnspecArgMacros.Speed(speed), 0, reset, StairUseSpecials | StairSync);

    public static StairInvoke BuildUpSync(int tag, int speed, int height, int reset) =>
        new(tag, BuildDirection.Up, height, VmPLnspecArgMacros.Speed(speed), 0, reset, StairUseSpecials | StairSync);

    public static StairInvoke BuildUpDoom(int tag, int speed, int height, int delay, int reset) =>
        new(tag, BuildDirection.Up, height, VmPLnspecArgMacros.Speed(speed), VmPLnspecArgMacros.Tics(delay), reset, 0);

    public static StairInvoke BuildUpDoomCrush(int tag, int speed, int height, int delay, int reset) =>
        new(tag, BuildDirection.Up, height, VmPLnspecArgMacros.Speed(speed), VmPLnspecArgMacros.Tics(delay), reset, StairCrush);

    public static StairInvoke BuildDownDoom(int tag, int speed, int height, int delay, int reset) =>
        new(tag, BuildDirection.Down, height, VmPLnspecArgMacros.Speed(speed), VmPLnspecArgMacros.Tics(delay), reset, 0);

    public static StairInvoke BuildDownDoomSync(int tag, int speed, int height, int reset) =>
        new(tag, BuildDirection.Down, height, VmPLnspecArgMacros.Speed(speed), 0, reset, StairSync);

    public static StairInvoke BuildUpDoomSync(int tag, int speed, int height, int reset) =>
        new(tag, BuildDirection.Up, height, VmPLnspecArgMacros.Speed(speed), 0, reset, StairSync);

    public static (StairInvoke invoke, bool toggleDirection) GenericStairs(int tag, int speed, int step, int dirFlags, int reset)
    {
        var buildUp = (dirFlags & 1) != 0;
        var invoke = new StairInvoke(tag, buildUp ? BuildDirection.Up : BuildDirection.Down, step,
            VmPLnspecArgMacros.Speed(speed), 0, reset, dirFlags & 2);
        return (invoke, true);
    }

    public static int ToggleGenericStairsDirection(int dirFlags) => dirFlags ^ 1;

    public static bool ShouldToggleRepeatableLine(bool success, bool repeatSpecial, int specialId, int genericStairsId) =>
        success && repeatSpecial && specialId == genericStairsId;
}
