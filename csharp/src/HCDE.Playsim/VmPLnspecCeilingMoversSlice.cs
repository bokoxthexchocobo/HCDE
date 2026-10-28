namespace HCDE.Playsim;

/// <summary>
/// Ceiling crush/mover specials through <c>LS_Generic_Ceiling</c> from <c>p_lnspec.cpp</c> (**701–913**).
/// </summary>
public static class VmPLnspecCeilingMoversSlice
{
    public enum CeilingOp
    {
        LowerAndCrush,
        CrushRaiseAndStay,
        CrushAndRaise,
        MoveToValue,
        LowerToHighestFloor,
        LowerInstant,
        RaiseInstant,
        RaiseToNearest,
        RaiseToHighest,
        RaiseToLowest,
        RaiseToHighestFloor,
        RaiseByTexture,
        LowerToLowest,
        LowerToNearest,
        LowerToHighest,
        RaiseToFloor,
        LowerToFloor,
        LowerByTexture,
        RaiseByValue,
        LowerByValue,
        LowerToFloorGeneric,
        LowerByTextureGeneric,
    }

    public readonly record struct CeilingCall(
        CeilingOp Op,
        int Tag,
        double DownSpeed,
        double UpSpeed,
        int Height,
        int Crush,
        int Silent,
        int Change,
        bool CrushTypeHexen);

    public sealed class LineContext
    {
        public Func<CeilingCall, bool>? EvDoCeiling { get; init; }
        public Func<int, bool, bool>? EvCeilingCrushStop { get; init; }
        public Func<int, bool>? EvStopCeiling { get; init; }
        public int GameTypeHexen { get; init; }
    }

    static bool CrushTypeBool(int mode, bool withSlowdown, LineContext ctx) =>
        (int)VmPLnspecPolyobjLineSpecials.CeilingCrushType(mode, withSlowdown, ctx.GameTypeHexen) != 0;

    static bool DoCeiling(CeilingCall call, LineContext ctx) =>
        ctx.EvDoCeiling?.Invoke(call) == true;

    static bool Call(
        CeilingOp op,
        int tag,
        double down,
        double up,
        int height,
        int crush,
        int silent,
        int change,
        bool crushHexen,
        LineContext ctx) =>
        DoCeiling(new CeilingCall(op, tag, down, up, height, crush, silent, change, crushHexen), ctx);

    public static bool CeilingLowerAndCrush(int tag, int speed, int crush, int crushMode, LineContext ctx)
    {
        var spd = VmPLnspecArgMacros.Speed(speed);
        return Call(CeilingOp.LowerAndCrush, tag, spd, spd, 8, crush, 0, 0, CrushTypeBool(crushMode, speed == 8, ctx), ctx);
    }

    public static bool CeilingLowerAndCrushDist(int tag, int speed, int crush, int dist, int crushMode, LineContext ctx)
    {
        var spd = VmPLnspecArgMacros.Speed(speed);
        return Call(CeilingOp.LowerAndCrush, tag, spd, spd, dist, crush, 0, 0, CrushTypeBool(crushMode, speed == 8, ctx), ctx);
    }

    public static bool CeilingCrushStop(int tag, int arg1, LineContext ctx)
    {
        var remove = arg1 switch
        {
            1 => false,
            2 => true,
            _ => ctx.GameTypeHexen == VmPLnspecPolyobjLineSpecials.GameHexen,
        };
        return ctx.EvCeilingCrushStop?.Invoke(tag, remove) == true;
    }

    public static bool CeilingCrushRaiseAndStay(int tag, int speed, int crush, int crushMode, LineContext ctx)
    {
        var spd = VmPLnspecArgMacros.Speed(speed);
        return Call(CeilingOp.CrushRaiseAndStay, tag, spd, spd / 2, 8, crush, 0, 0, CrushTypeBool(crushMode, false, ctx), ctx);
    }

    public static bool CeilingMoveToValueTimes8(int tag, int speed, int height, bool negative, int change, LineContext ctx) =>
        Call(CeilingOp.MoveToValue, tag, VmPLnspecArgMacros.Speed(speed), 0,
            height * 8 * (negative ? -1 : 1), -1, 0, VmPLnspecArgMacros.Change(change), false, ctx);

    public static bool CeilingMoveToValue(int tag, int speed, int height, bool negative, int change, LineContext ctx) =>
        Call(CeilingOp.MoveToValue, tag, VmPLnspecArgMacros.Speed(speed), 0,
            height * (negative ? -1 : 1), -1, 0, VmPLnspecArgMacros.Change(change), false, ctx);

    public static bool CeilingMoveToValueAndCrush(int tag, int speed, int height, int crush, int crushMode, LineContext ctx) =>
        Call(CeilingOp.MoveToValue, tag, VmPLnspecArgMacros.Speed(speed), 0, height,
            VmPLnspecArgMacros.Crush(crush), 0, 0, CrushTypeBool(crushMode, speed == 8, ctx), ctx);

    public static bool CeilingLowerToHighestFloor(int tag, int speed, int change, int crush, int gap, LineContext ctx) =>
        Call(CeilingOp.LowerToHighestFloor, tag, VmPLnspecArgMacros.Speed(speed), 0, gap,
            VmPLnspecArgMacros.Crush(crush), 0, VmPLnspecArgMacros.Change(change), false, ctx);

    public static bool CeilingLowerInstant(int tag, int height, int change, int crush, LineContext ctx) =>
        Call(CeilingOp.LowerInstant, tag, 0, 0, height * 8, VmPLnspecArgMacros.Crush(crush), 0,
            VmPLnspecArgMacros.Change(change), false, ctx);

    public static bool CeilingRaiseInstant(int tag, int height, int change, LineContext ctx) =>
        Call(CeilingOp.RaiseInstant, tag, 0, 0, height * 8, -1, 0, VmPLnspecArgMacros.Change(change), false, ctx);

    public static bool CeilingCrushRaiseAndStayA(int tag, int downSpeed, int upSpeed, int damage, int crushMode, LineContext ctx) =>
        Call(CeilingOp.CrushRaiseAndStay, tag, VmPLnspecArgMacros.Speed(downSpeed), VmPLnspecArgMacros.Speed(upSpeed),
            0, damage, 0, 0, CrushTypeBool(crushMode, false, ctx), ctx);

    public static bool CeilingCrushRaiseAndStaySilA(int tag, int downSpeed, int upSpeed, int damage, int crushMode, LineContext ctx) =>
        Call(CeilingOp.CrushRaiseAndStay, tag, VmPLnspecArgMacros.Speed(downSpeed), VmPLnspecArgMacros.Speed(upSpeed),
            0, damage, 1, 0, CrushTypeBool(crushMode, false, ctx), ctx);

    public static bool CeilingCrushAndRaiseA(int tag, int downSpeed, int upSpeed, int damage, int crushMode, LineContext ctx) =>
        Call(CeilingOp.CrushAndRaise, tag, VmPLnspecArgMacros.Speed(downSpeed), VmPLnspecArgMacros.Speed(upSpeed),
            0, damage, 0, 0, CrushTypeBool(crushMode, downSpeed == 8 && upSpeed == 8, ctx), ctx);

    public static bool CeilingCrushAndRaiseDist(int tag, int dist, int speed, int damage, int crushMode, LineContext ctx)
    {
        var spd = VmPLnspecArgMacros.Speed(speed);
        return Call(CeilingOp.CrushAndRaise, tag, spd, spd, dist, damage, 0, 0, CrushTypeBool(crushMode, speed == 8, ctx), ctx);
    }

    public static bool CeilingCrushAndRaiseSilentA(int tag, int downSpeed, int upSpeed, int damage, int crushMode, LineContext ctx) =>
        Call(CeilingOp.CrushAndRaise, tag, VmPLnspecArgMacros.Speed(downSpeed), VmPLnspecArgMacros.Speed(upSpeed),
            0, damage, 1, 0, CrushTypeBool(crushMode, downSpeed == 8 && upSpeed == 8, ctx), ctx);

    public static bool CeilingCrushAndRaiseSilentDist(int tag, int dist, int speed, int damage, int crushMode, LineContext ctx)
    {
        var spd = VmPLnspecArgMacros.Speed(speed);
        return Call(CeilingOp.CrushAndRaise, tag, spd, spd, dist, damage, 1, 0, CrushTypeBool(crushMode, speed == 8, ctx), ctx);
    }

    public static bool CeilingRaiseToNearest(int tag, int speed, int change, LineContext ctx) =>
        Call(CeilingOp.RaiseToNearest, tag, VmPLnspecArgMacros.Speed(speed), 0, 0, -1, 0,
            VmPLnspecArgMacros.Change(change), false, ctx);

    public static bool CeilingRaiseToHighest(int tag, int speed, int change, LineContext ctx) =>
        Call(CeilingOp.RaiseToHighest, tag, VmPLnspecArgMacros.Speed(speed), 0, 0, -1, 0,
            VmPLnspecArgMacros.Change(change), false, ctx);

    public static bool CeilingRaiseToLowest(int tag, int speed, int change, LineContext ctx) =>
        Call(CeilingOp.RaiseToLowest, tag, VmPLnspecArgMacros.Speed(speed), 0, 0, -1, 0,
            VmPLnspecArgMacros.Change(change), false, ctx);

    public static bool CeilingRaiseToHighestFloor(int tag, int speed, int change, LineContext ctx) =>
        Call(CeilingOp.RaiseToHighestFloor, tag, VmPLnspecArgMacros.Speed(speed), 0, 0, -1, 0,
            VmPLnspecArgMacros.Change(change), false, ctx);

    public static bool CeilingRaiseByTexture(int tag, int speed, int change, LineContext ctx) =>
        Call(CeilingOp.RaiseByTexture, tag, VmPLnspecArgMacros.Speed(speed), 0, 0, -1, 0,
            VmPLnspecArgMacros.Change(change), false, ctx);

    public static bool CeilingLowerToLowest(int tag, int speed, int change, int crush, LineContext ctx) =>
        Call(CeilingOp.LowerToLowest, tag, VmPLnspecArgMacros.Speed(speed), 0, 0,
            VmPLnspecArgMacros.Crush(crush), 0, VmPLnspecArgMacros.Change(change), false, ctx);

    public static bool CeilingLowerToNearest(int tag, int speed, int change, int crush, LineContext ctx) =>
        Call(CeilingOp.LowerToNearest, tag, VmPLnspecArgMacros.Speed(speed), 0, 0,
            VmPLnspecArgMacros.Crush(crush), 0, VmPLnspecArgMacros.Change(change), false, ctx);

    public static bool CeilingToHighestInstant(int tag, int change, int crush, LineContext ctx) =>
        Call(CeilingOp.LowerToHighest, tag, 2, 0, 0, VmPLnspecArgMacros.Crush(crush), 0,
            VmPLnspecArgMacros.Change(change), false, ctx);

    public static bool CeilingToFloorInstant(int tag, int change, int crush, int gap, LineContext ctx) =>
        Call(CeilingOp.RaiseToFloor, tag, 2, 0, gap, VmPLnspecArgMacros.Crush(crush), 0,
            VmPLnspecArgMacros.Change(change), false, ctx);

    public static bool CeilingLowerToFloor(int tag, int speed, int change, int crush, int gap, LineContext ctx) =>
        Call(CeilingOp.LowerToFloor, tag, VmPLnspecArgMacros.Speed(speed), 0, gap,
            VmPLnspecArgMacros.Crush(crush), 0, VmPLnspecArgMacros.Change(change), false, ctx);

    public static bool CeilingLowerByTexture(int tag, int speed, int change, int crush, LineContext ctx) =>
        Call(CeilingOp.LowerByTexture, tag, VmPLnspecArgMacros.Speed(speed), 0, 0,
            VmPLnspecArgMacros.Crush(crush), 0, VmPLnspecArgMacros.Change(change), false, ctx);

    public static bool CeilingStop(int tag, LineContext ctx) =>
        ctx.EvStopCeiling?.Invoke(tag) == true;

    static CeilingOp ResolveGenericCeilingKind(int target, bool raise) =>
        raise ? target switch
        {
            1 => CeilingOp.RaiseToHighest,
            2 => CeilingOp.RaiseToLowest,
            3 => CeilingOp.RaiseToNearest,
            4 => CeilingOp.RaiseToHighestFloor,
            5 => CeilingOp.RaiseToFloor,
            6 => CeilingOp.RaiseByTexture,
            _ => CeilingOp.RaiseByValue,
        } : target switch
        {
            1 => CeilingOp.LowerToHighest,
            2 => CeilingOp.LowerToLowest,
            3 => CeilingOp.LowerToNearest,
            4 => CeilingOp.LowerToHighestFloor,
            5 => CeilingOp.LowerToFloorGeneric,
            6 => CeilingOp.LowerByTextureGeneric,
            _ => CeilingOp.LowerByValue,
        };

    public static bool GenericCeiling(int tag, int speed, int height, int target, int flags, LineContext ctx)
    {
        var raise = (flags & 8) != 0;
        var kind = ResolveGenericCeilingKind(target, raise);
        var crush = (flags & 16) != 0 ? 20 : -1;
        var change = flags & 7;
        var spd = VmPLnspecArgMacros.Speed(speed);
        return Call(kind, tag, spd, spd, height, crush, 0, change, false, ctx);
    }
}
