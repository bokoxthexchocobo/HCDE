namespace HCDE.Playsim;

/// <summary>Native <c>LS_Ceiling_*</c> argument wiring from <c>p_lnspec.cpp</c> (670–835).</summary>
public static class VmPLnspecCeilingLineSpecials
{
    public enum CeilingKind
    {
        LowerByValue,
        RaiseByValue,
        LowerByValueTimes8,
        RaiseByValueTimes8,
        CrushAndRaise,
        LowerAndCrush,
        CrushStop,
        CrushRaiseAndStay,
        MoveToValueTimes8,
        MoveToValue,
        MoveToValueAndCrush,
        LowerToHighestFloor,
        LowerInstant,
        RaiseInstant,
        CrushRaiseAndStayA,
        CrushRaiseAndStaySilA,
        CrushAndRaiseA,
        CrushAndRaiseDist,
        CrushAndRaiseSilentA,
        CrushAndRaiseSilentDist,
        RaiseToNearest,
        RaiseToHighest,
        RaiseToLowest,
        RaiseToHighestFloor,
        RaiseByTexture,
        LowerToLowest,
    }

    public readonly record struct CeilingInvoke(
        CeilingKind Kind,
        int Tag,
        double DownSpeed,
        double UpSpeed,
        int Height,
        int Crush,
        int Silent,
        int Change,
        int Dist);

    public static CeilingInvoke LowerByValue(int tag, int speed, int height, int change, int crush) =>
        new(CeilingKind.LowerByValue, tag, VmPLnspecArgMacros.Speed(speed), 0, height, VmPLnspecArgMacros.Crush(crush), 0, VmPLnspecArgMacros.Change(change), 0);

    public static CeilingInvoke RaiseByValue(int tag, int speed, int height, int change) =>
        new(CeilingKind.RaiseByValue, tag, VmPLnspecArgMacros.Speed(speed), 0, height, -1, 0, VmPLnspecArgMacros.Change(change), 0);

    public static CeilingInvoke LowerByValueTimes8(int tag, int speed, int height, int change) =>
        new(CeilingKind.LowerByValueTimes8, tag, VmPLnspecArgMacros.Speed(speed), 0, height * 8, -1, 0, VmPLnspecArgMacros.Change(change), 0);

    public static CeilingInvoke CrushAndRaise(int tag, int speed, int crush, int crushType) =>
        new(CeilingKind.CrushAndRaise, tag, VmPLnspecArgMacros.Speed(speed), VmPLnspecArgMacros.Speed(speed) / 2, 8, crush, 0, 0, 0);

    public static CeilingInvoke LowerAndCrush(int tag, int speed, int crush, int dist, bool useDist) =>
        new(useDist ? CeilingKind.LowerAndCrush : CeilingKind.LowerAndCrush, tag,
            VmPLnspecArgMacros.Speed(speed), VmPLnspecArgMacros.Speed(speed), useDist ? dist : 8, crush, 0, 0, dist);

    public static CeilingInvoke CrushStop(int tag, int arg1, int gameHexen) =>
        new(CeilingKind.CrushStop, tag, 0, 0, 0, 0, 0, 0, VmPLnspecArgMacros.CeilingCrushStopRemove(arg1, gameHexen));

    public static CeilingInvoke MoveToValueTimes8(int tag, int speed, int height, bool negative, int change) =>
        new(CeilingKind.MoveToValueTimes8, tag, VmPLnspecArgMacros.Speed(speed), 0, height * 8 * (negative ? -1 : 1), -1, 0, VmPLnspecArgMacros.Change(change), 0);

    public static CeilingInvoke MoveToValue(int tag, int speed, int height, bool negative, int change) =>
        new(CeilingKind.MoveToValue, tag, VmPLnspecArgMacros.Speed(speed), 0, height * (negative ? -1 : 1), -1, 0, VmPLnspecArgMacros.Change(change), 0);

    public static CeilingInvoke LowerInstant(int tag, int height, int change, int crush) =>
        new(CeilingKind.LowerInstant, tag, 0, 0, height * 8, VmPLnspecArgMacros.Crush(crush), 0, VmPLnspecArgMacros.Change(change), 0);

    public static CeilingInvoke RaiseInstant(int tag, int height, int change) =>
        new(CeilingKind.RaiseInstant, tag, 0, 0, height * 8, -1, 0, VmPLnspecArgMacros.Change(change), 0);

    public static CeilingInvoke CrushRaiseAndStay(int tag, int downSpeed, int upSpeed, int damage, int silent) =>
        new(CeilingKind.CrushRaiseAndStay, tag, VmPLnspecArgMacros.Speed(downSpeed), VmPLnspecArgMacros.Speed(upSpeed), 0, damage, silent, 0, 0);

    public static CeilingInvoke CrushAndRaiseDist(int tag, int dist, int speed, int damage, int silent) =>
        new(silent != 0 ? CeilingKind.CrushAndRaiseSilentDist : CeilingKind.CrushAndRaiseDist,
            tag, VmPLnspecArgMacros.Speed(speed), VmPLnspecArgMacros.Speed(speed), dist, damage, silent, 0, dist);

    public static CeilingInvoke RaiseToNearest(int tag, int speed, int change) =>
        new(CeilingKind.RaiseToNearest, tag, VmPLnspecArgMacros.Speed(speed), 0, 0, -1, 0, VmPLnspecArgMacros.Change(change), 0);
}
