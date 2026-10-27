namespace HCDE.Playsim;

/// <summary>
/// Door movers and floor height specials from <c>p_lnspec.cpp</c> (**241–470**).
/// </summary>
public static class VmPLnspecDoorFloorLineSpecials
{
    public enum DoorOp
    {
        Close,
        Open,
        Raise,
        CloseWaitOpen,
        WaitRaise,
        WaitClose,
    }

    public enum FloorOp
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
        RaiseByValueTimes8,
        LowerByValueTimes8,
        CrushStop,
        LowerInstant,
        RaiseInstant,
        ToCeilingInstant,
        MoveToValueTimes8,
        MoveToValue,
        MoveToValueAndCrush,
        RaiseToLowestCeiling,
        LowerToLowestCeiling,
    }

    public enum SlidingDoorMode
    {
        OpenClose,
        Close,
    }

    public sealed class LineContext
    {
        public Func<DoorOp, int, double, int, int, int, bool, int, bool>? EvDoDoor { get; init; }
        public Func<FloorOp, int, double, int, int, int, bool, bool>? EvDoFloor { get; init; }
        public Func<int, int, int, int, int, SlidingDoorMode, bool>? EvSlidingDoor { get; init; }
        public Func<int, bool, bool>? CheckKeys { get; init; }
        public Func<int, bool>? EvFloorCrushStop { get; init; }
        public int GameTypeHexen { get; init; }
    }

    static bool DoDoor(
        DoorOp op,
        int tag,
        int speed,
        int delayTics,
        int lockArg,
        int lightTag,
        bool boomGen,
        int waitTics,
        LineContext ctx) =>
        ctx.EvDoDoor?.Invoke(op, tag, VmPLnspecArgMacros.Speed(speed), delayTics, lockArg, lightTag, boomGen, waitTics) == true;

    public static bool DoorClose(int tag, int speed, int lightTag, LineContext ctx) =>
        DoDoor(DoorOp.Close, tag, speed, 0, 0, lightTag, false, 0, ctx);

    public static bool DoorOpen(int tag, int speed, int lightTag, LineContext ctx) =>
        DoDoor(DoorOp.Open, tag, speed, 0, 0, lightTag, false, 0, ctx);

    public static bool DoorRaise(int tag, int speed, int delay, int lightTag, LineContext ctx) =>
        DoDoor(DoorOp.Raise, tag, speed, VmPLnspecArgMacros.Tics(delay), 0, lightTag, false, 0, ctx);

    public static bool DoorLockedRaise(int tag, int speed, int delay, int lockArg, int lightTag, LineContext ctx)
    {
        var op = delay != 0 ? DoorOp.Raise : DoorOp.Open;
        return DoDoor(op, tag, speed, VmPLnspecArgMacros.Tics(delay), lockArg, lightTag, false, 0, ctx);
    }

    public static bool DoorCloseWaitOpen(int tag, int speed, int delay, int lightTag, LineContext ctx) =>
        DoDoor(DoorOp.CloseWaitOpen, tag, speed, VmPLnspecArgMacros.Octics(delay), 0, lightTag, false, 0, ctx);

    public static bool DoorWaitRaise(int tag, int speed, int delay, int wait, int lightTag, LineContext ctx) =>
        ctx.EvDoDoor?.Invoke(
            DoorOp.WaitRaise,
            tag,
            VmPLnspecArgMacros.Speed(speed),
            VmPLnspecArgMacros.Tics(delay),
            0,
            lightTag,
            false,
            VmPLnspecArgMacros.Tics(wait)) == true;

    public static bool DoorWaitClose(int tag, int speed, int wait, int lightTag, LineContext ctx) =>
        ctx.EvDoDoor?.Invoke(
            DoorOp.WaitClose,
            tag,
            VmPLnspecArgMacros.Speed(speed),
            0,
            0,
            lightTag,
            false,
            VmPLnspecArgMacros.Tics(wait)) == true;

    public static bool DoorAnimated(int tag, int speed, int delay, int lockArg, bool hasKeys, LineContext ctx)
    {
        if (lockArg != 0 && ctx.CheckKeys?.Invoke(lockArg, tag != 0) != true)
            return false;
        return ctx.EvSlidingDoor?.Invoke(tag, speed, delay, lockArg, -1, SlidingDoorMode.OpenClose) == true;
    }

    public static bool DoorAnimatedClose(int tag, int speed, LineContext ctx) =>
        ctx.EvSlidingDoor?.Invoke(tag, speed, -1, 0, -1, SlidingDoorMode.Close) == true;

    public static bool GenericDoor(int arg0, int arg1, int arg2, int arg3, int arg4, LineContext ctx)
    {
        var kindBits = arg2 & 63;
        var op = kindBits switch
        {
            0 => DoorOp.Raise,
            1 => DoorOp.Open,
            2 => DoorOp.CloseWaitOpen,
            3 => DoorOp.Close,
            _ => (DoorOp)(-1),
        };
        if ((int)op < 0)
            return false;
        var boomGen = (arg2 & 64) != 0;
        var localLight = (arg2 & 128) != 0;
        var tag = localLight ? 0 : arg0;
        var lightTag = localLight ? arg0 : 0;
        return ctx.EvDoDoor?.Invoke(
            op,
            tag,
            VmPLnspecArgMacros.Speed(arg1),
            VmPLnspecArgMacros.Octics(arg3),
            arg4,
            lightTag,
            boomGen,
            0) == true;
    }

    static bool DoFloor(
        FloorOp op,
        int tag,
        double speed,
        int height,
        int crush,
        int change,
        bool directionUp,
        LineContext ctx) =>
        ctx.EvDoFloor?.Invoke(op, tag, speed, height, crush, change, directionUp) == true;

    static bool CrushTypeBool(int mode, LineContext ctx) =>
        VmPLnspecPolyobjLineSpecials.CrushTypeDoomHexen(mode, ctx.GameTypeHexen);

    public static bool FloorLowerByValue(int tag, int speed, int height, int change, LineContext ctx) =>
        DoFloor(FloorOp.LowerByValue, tag, VmPLnspecArgMacros.Speed(speed), height, -1, VmPLnspecArgMacros.Change(change), false, ctx);

    public static bool FloorLowerToLowest(int tag, int speed, int change, LineContext ctx) =>
        DoFloor(FloorOp.LowerToLowest, tag, VmPLnspecArgMacros.Speed(speed), 0, -1, VmPLnspecArgMacros.Change(change), false, ctx);

    public static bool FloorLowerToHighest(int tag, int speed, int adjust, bool hereticLower, LineContext ctx) =>
        DoFloor(FloorOp.LowerToHighest, tag, VmPLnspecArgMacros.Speed(speed), adjust - 128, -1, 0, hereticLower, ctx);

    public static bool FloorLowerToHighestEe(int tag, int speed, int change, LineContext ctx) =>
        DoFloor(FloorOp.LowerToHighest, tag, VmPLnspecArgMacros.Speed(speed), 0, -1, VmPLnspecArgMacros.Change(change), false, ctx);

    public static bool FloorLowerToNearest(int tag, int speed, int change, LineContext ctx) =>
        DoFloor(FloorOp.LowerToNearest, tag, VmPLnspecArgMacros.Speed(speed), 0, -1, VmPLnspecArgMacros.Change(change), false, ctx);

    public static bool FloorRaiseByValue(int tag, int speed, int height, int change, int crush, LineContext ctx) =>
        DoFloor(FloorOp.RaiseByValue, tag, VmPLnspecArgMacros.Speed(speed), height, VmPLnspecArgMacros.Crush(crush), VmPLnspecArgMacros.Change(change), true, ctx);

    public static bool FloorRaiseToHighest(int tag, int speed, int change, int crush, LineContext ctx) =>
        DoFloor(FloorOp.RaiseToHighest, tag, VmPLnspecArgMacros.Speed(speed), 0, VmPLnspecArgMacros.Crush(crush), VmPLnspecArgMacros.Change(change), true, ctx);

    public static bool FloorRaiseToNearest(int tag, int speed, int change, int crush, LineContext ctx) =>
        DoFloor(FloorOp.RaiseToNearest, tag, VmPLnspecArgMacros.Speed(speed), 0, VmPLnspecArgMacros.Crush(crush), VmPLnspecArgMacros.Change(change), true, ctx);

    public static bool FloorRaiseToLowest(int tag, int change, int crush, LineContext ctx) =>
        DoFloor(FloorOp.RaiseToLowest, tag, 2.0, 0, VmPLnspecArgMacros.Crush(crush), VmPLnspecArgMacros.Change(change), true, ctx);

    public static bool FloorRaiseAndCrush(int tag, int speed, int crush, int crushMode, LineContext ctx) =>
        DoFloor(FloorOp.RaiseAndCrush, tag, VmPLnspecArgMacros.Speed(speed), 0, crush, 0, CrushTypeBool(crushMode, ctx), ctx);

    public static bool FloorRaiseAndCrushDoom(int tag, int speed, int crush, int crushMode, LineContext ctx) =>
        DoFloor(FloorOp.RaiseAndCrushDoom, tag, VmPLnspecArgMacros.Speed(speed), 0, crush, 0, CrushTypeBool(crushMode, ctx), ctx);

    public static bool FloorRaiseByValueTimes8(int tag, int speed, int height, int change, int crush, LineContext ctx) =>
        DoFloor(FloorOp.RaiseByValueTimes8, tag, VmPLnspecArgMacros.Speed(speed), height * 8, VmPLnspecArgMacros.Crush(crush), VmPLnspecArgMacros.Change(change), true, ctx);

    public static bool FloorLowerByValueTimes8(int tag, int speed, int height, int change, LineContext ctx) =>
        DoFloor(FloorOp.LowerByValueTimes8, tag, VmPLnspecArgMacros.Speed(speed), height * 8, -1, VmPLnspecArgMacros.Change(change), false, ctx);

    public static bool FloorCrushStop(int tag, LineContext ctx) =>
        ctx.EvFloorCrushStop?.Invoke(tag) == true;

    public static bool FloorLowerInstant(int tag, int height, int change, LineContext ctx) =>
        DoFloor(FloorOp.LowerInstant, tag, 0.0, height * 8, -1, VmPLnspecArgMacros.Change(change), false, ctx);

    public static bool FloorRaiseInstant(int tag, int height, int change, int crush, LineContext ctx) =>
        DoFloor(FloorOp.RaiseInstant, tag, 0.0, height * 8, VmPLnspecArgMacros.Crush(crush), VmPLnspecArgMacros.Change(change), true, ctx);

    public static bool FloorToCeilingInstant(int tag, int change, int crush, int gap, LineContext ctx) =>
        DoFloor(FloorOp.ToCeilingInstant, tag, 0.0, gap, VmPLnspecArgMacros.Crush(crush), VmPLnspecArgMacros.Change(change), true, ctx);

    public static bool FloorMoveToValueTimes8(int tag, int speed, int height, bool negative, int change, LineContext ctx) =>
        DoFloor(
            FloorOp.MoveToValueTimes8,
            tag,
            VmPLnspecArgMacros.Speed(speed),
            height * 8 * (negative ? -1 : 1),
            -1,
            VmPLnspecArgMacros.Change(change),
            false,
            ctx);

    public static bool FloorMoveToValue(int tag, int speed, int height, bool negative, int change, LineContext ctx) =>
        DoFloor(
            FloorOp.MoveToValue,
            tag,
            VmPLnspecArgMacros.Speed(speed),
            height * (negative ? -1 : 1),
            -1,
            VmPLnspecArgMacros.Change(change),
            false,
            ctx);

    public static bool FloorMoveToValueAndCrush(int tag, int speed, int height, int crush, int crushMode, LineContext ctx) =>
        DoFloor(
            FloorOp.MoveToValueAndCrush,
            tag,
            VmPLnspecArgMacros.Speed(speed),
            height,
            VmPLnspecArgMacros.Crush(crush),
            0,
            CrushTypeBool(crushMode, ctx),
            ctx);

    public static bool FloorRaiseToLowestCeiling(int tag, int speed, int change, int crush, LineContext ctx) =>
        DoFloor(FloorOp.RaiseToLowestCeiling, tag, VmPLnspecArgMacros.Speed(speed), 0, VmPLnspecArgMacros.Crush(crush), VmPLnspecArgMacros.Change(change), true, ctx);

    public static bool FloorLowerToLowestCeiling(int tag, int speed, int change, int arg4, LineContext ctx) =>
        DoFloor(FloorOp.LowerToLowestCeiling, tag, VmPLnspecArgMacros.Speed(speed), arg4, -1, VmPLnspecArgMacros.Change(change), false, ctx);
}
