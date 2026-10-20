namespace HCDE.Playsim;

/// <summary>Native <c>LS_Floor_*</c> argument wiring from <c>p_lnspec.cpp</c> (336–531).</summary>
public static class VmPLnspecFloorLineSpecials
{
    public enum FloorKind
    {
        LowerByValue,
        LowerToLowest,
        LowerToHighest,
        LowerToNearest,
        RaiseByValue,
        RaiseToHighest,
        RaiseToNearest,
        RaiseToLowest,
        RaiseAndCrush,
        RaiseAndCrushDoom,
        LowerByValueTimes8,
        RaiseByValueTimes8,
        CrushStop,
        LowerInstant,
        RaiseInstant,
        ToCeilingInstant,
        MoveToValueTimes8,
        MoveToValue,
        MoveToValueAndCrush,
        RaiseToLowestCeiling,
        LowerToLowestCeiling,
        RaiseByTexture,
        LowerByTexture,
        RaiseToCeiling,
        RaiseByValueTxTy,
        LowerToLowestTxTy,
        Waggle,
        TransferTrigger,
        TransferNumeric,
        Donut,
        Stop,
    }

    public readonly record struct FloorInvoke(
        FloorKind Kind,
        int Tag,
        double Speed,
        int Height,
        int Crush,
        int Change,
        bool DirectionUp,
        bool HereticLower,
        int Gap,
        bool NegativeMove);

    public static FloorInvoke LowerByValue(int tag, int speed, int height, int change) =>
        new(FloorKind.LowerByValue, tag, VmPLnspecArgMacros.Speed(speed), height, -1, VmPLnspecArgMacros.Change(change), false, false, 0, false);

    public static FloorInvoke LowerToLowest(int tag, int speed, int change) =>
        new(FloorKind.LowerToLowest, tag, VmPLnspecArgMacros.Speed(speed), 0, -1, VmPLnspecArgMacros.Change(change), false, false, 0, false);

    public static FloorInvoke LowerToHighest(int tag, int speed, int adjust, bool hereticLower) =>
        new(FloorKind.LowerToHighest, tag, VmPLnspecArgMacros.Speed(speed), adjust - 128, -1, 0, false, hereticLower, 0, false);

    public static FloorInvoke LowerToNearest(int tag, int speed, int change) =>
        new(FloorKind.LowerToNearest, tag, VmPLnspecArgMacros.Speed(speed), 0, -1, VmPLnspecArgMacros.Change(change), false, false, 0, false);

    public static FloorInvoke RaiseByValue(int tag, int speed, int height, int change, int crush) =>
        new(FloorKind.RaiseByValue, tag, VmPLnspecArgMacros.Speed(speed), height, VmPLnspecArgMacros.Crush(crush), VmPLnspecArgMacros.Change(change), true, false, 0, false);

    public static FloorInvoke RaiseToHighest(int tag, int speed, int change, int crush) =>
        new(FloorKind.RaiseToHighest, tag, VmPLnspecArgMacros.Speed(speed), 0, VmPLnspecArgMacros.Crush(crush), VmPLnspecArgMacros.Change(change), true, false, 0, false);

    public static FloorInvoke RaiseAndCrush(int tag, int speed, int crush, int crushMode) =>
        new(FloorKind.RaiseAndCrush, tag, VmPLnspecArgMacros.Speed(speed), 0, crush, 0, true, false, 0, false);

    public static FloorInvoke LowerByValueTimes8(int tag, int speed, int height, int change) =>
        new(FloorKind.LowerByValueTimes8, tag, VmPLnspecArgMacros.Speed(speed), height * 8, -1, VmPLnspecArgMacros.Change(change), false, false, 0, false);

    public static FloorInvoke RaiseByValueTimes8(int tag, int speed, int height, int change, int crush) =>
        new(FloorKind.RaiseByValueTimes8, tag, VmPLnspecArgMacros.Speed(speed), height * 8, VmPLnspecArgMacros.Crush(crush), VmPLnspecArgMacros.Change(change), true, false, 0, false);

    public static FloorInvoke LowerInstant(int tag, int height, int change) =>
        new(FloorKind.LowerInstant, tag, 0, height * 8, -1, VmPLnspecArgMacros.Change(change), false, false, 0, false);

    public static FloorInvoke RaiseInstant(int tag, int height, int change, int crush) =>
        new(FloorKind.RaiseInstant, tag, 0, height * 8, VmPLnspecArgMacros.Crush(crush), VmPLnspecArgMacros.Change(change), true, false, 0, false);

    public static FloorInvoke ToCeilingInstant(int tag, int change, int crush, int gap) =>
        new(FloorKind.ToCeilingInstant, tag, 0, gap, VmPLnspecArgMacros.Crush(crush), VmPLnspecArgMacros.Change(change), false, false, gap, false);

    public static FloorInvoke MoveToValueTimes8(int tag, int speed, int height, bool negative, int change) =>
        new(FloorKind.MoveToValueTimes8, tag, VmPLnspecArgMacros.Speed(speed), height * 8 * (negative ? -1 : 1), -1, VmPLnspecArgMacros.Change(change), !negative, false, 0, negative);

    public static FloorInvoke MoveToValue(int tag, int speed, int height, bool negative, int change) =>
        new(FloorKind.MoveToValue, tag, VmPLnspecArgMacros.Speed(speed), height * (negative ? -1 : 1), -1, VmPLnspecArgMacros.Change(change), !negative, false, 0, negative);

    public static FloorInvoke RaiseToLowestCeiling(int tag, int speed, int change, int crush) =>
        new(FloorKind.RaiseToLowestCeiling, tag, VmPLnspecArgMacros.Speed(speed), 0, VmPLnspecArgMacros.Crush(crush), VmPLnspecArgMacros.Change(change), true, false, 0, false);

    public static FloorInvoke LowerByTexture(int tag, int speed, int change) =>
        new(FloorKind.LowerByTexture, tag, VmPLnspecArgMacros.Speed(speed), 0, -1, VmPLnspecArgMacros.Change(change), false, false, 0, false);

    public static FloorInvoke RaiseToCeiling(int tag, int speed, int change, int crush, int gap) =>
        new(FloorKind.RaiseToCeiling, tag, VmPLnspecArgMacros.Speed(speed), gap, VmPLnspecArgMacros.Crush(crush), VmPLnspecArgMacros.Change(change), true, false, gap, false);

    public static FloorInvoke RaiseByValueTxTy(int tag, int speed, int height) =>
        new(FloorKind.RaiseByValueTxTy, tag, VmPLnspecArgMacros.Speed(speed), height, -1, 0, true, false, 0, false);

    public static FloorInvoke LowerToLowestTxTy(int tag, int speed, int height) =>
        new(FloorKind.LowerToLowestTxTy, tag, VmPLnspecArgMacros.Speed(speed), height, -1, 0, false, false, 0, false);

    public static FloorInvoke Waggle(int tag, int amplitude, int frequency, int delay, int time) =>
        new(FloorKind.Waggle, tag, amplitude, frequency, delay, time, false, false, 0, false);

    public static (int pillarTag, double pillarSpeed, double slimeSpeed) Donut(int pillarTag, int pillarSpeed, int slimeSpeed) =>
        (pillarTag, VmPLnspecArgMacros.Speed(pillarSpeed), VmPLnspecArgMacros.Speed(slimeSpeed));

    public static FloorKind ResolveGenericFloorKind(int target, bool raise) =>
        raise ? target switch
        {
            1 => FloorKind.RaiseToHighest,
            2 => FloorKind.RaiseToLowest,
            3 => FloorKind.RaiseToNearest,
            4 => FloorKind.RaiseToLowestCeiling,
            5 => FloorKind.RaiseToCeiling,
            6 => FloorKind.RaiseByTexture,
            _ => FloorKind.RaiseByValue,
        } : target switch
        {
            1 => FloorKind.LowerToHighest,
            2 => FloorKind.LowerToLowest,
            3 => FloorKind.LowerToNearest,
            4 => FloorKind.LowerToLowestCeiling,
            5 => FloorKind.ToCeilingInstant,
            6 => FloorKind.LowerByTexture,
            _ => FloorKind.LowerByValue,
        };

    public static FloorInvoke GenericFloor(int tag, int speed, int height, int target, int flags)
    {
        var raise = (flags & 8) != 0;
        var kind = ResolveGenericFloorKind(target, raise);
        var crush = (flags & 16) != 0 ? 20 : -1;
        var change = flags & 7;
        return new(kind, tag, VmPLnspecArgMacros.Speed(speed), height, crush, change, raise, false, 0, false);
    }
}
