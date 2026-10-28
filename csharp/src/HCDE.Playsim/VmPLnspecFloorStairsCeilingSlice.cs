namespace HCDE.Playsim;

/// <summary>
/// Texture floors, waggle/transfer/donut, generic floor, stairs, pillars, and ceiling movers from
/// <c>p_lnspec.cpp</c> (**472–700**).
/// </summary>
public static class VmPLnspecFloorStairsCeilingSlice
{
    public enum FloorKind
    {
        RaiseByTexture,
        LowerByTexture,
        RaiseToCeiling,
        RaiseAndChange,
        LowerAndChange,
        RaiseByValue,
        RaiseToHighest,
        RaiseToLowest,
        RaiseToNearest,
        RaiseToLowestCeiling,
        LowerToHighest,
        LowerToLowest,
        LowerToNearest,
        LowerToLowestCeiling,
        LowerToCeiling,
        LowerByValue,
        LowerByTextureGeneric,
    }

    public enum StairBuild
    {
        Down,
        Up,
    }

    public enum PillarKind
    {
        Build,
        Open,
    }

    public enum CeilingKind
    {
        LowerByValue,
        RaiseByValue,
        LowerByValueTimes8,
        RaiseByValueTimes8,
        CrushAndRaise,
    }

    public sealed class LineContext
    {
        public Func<FloorKind, int, double, int, int, int, bool, bool>? EvDoFloor { get; init; }
        public Func<int, int, int, int, int, bool, bool>? EvStartWaggle { get; init; }
        public Func<int, bool, bool>? EvDoChange { get; init; }
        public Func<int, double, double, bool>? EvDoDonut { get; init; }
        public Func<int, bool>? EvStopFloor { get; init; }
        public Func<int, StairBuild, int, double, int, int, int, bool>? EvBuildStairs { get; init; }
        public Func<PillarKind, int, double, int, int, int, bool, bool>? EvDoPillar { get; init; }
        public Func<CeilingKind, int, double, double, int, int, int, bool, bool>? EvDoCeiling { get; init; }
        public bool RepeatSpecial { get; init; }
        public int LineSpecialId { get; init; }
        public int GenericStairsSpecialId { get; init; }
        public Action? ToggleGenericStairsDirection { get; init; }
        public int GameTypeHexen { get; init; }
    }

    public static bool FloorRaiseByTexture(int tag, int speed, int change, int crush, LineContext ctx) =>
        ctx.EvDoFloor?.Invoke(FloorKind.RaiseByTexture, tag, VmPLnspecArgMacros.Speed(speed), 0,
            VmPLnspecArgMacros.Crush(crush), VmPLnspecArgMacros.Change(change), true) == true;

    public static bool FloorLowerByTexture(int tag, int speed, int change, LineContext ctx) =>
        ctx.EvDoFloor?.Invoke(FloorKind.LowerByTexture, tag, VmPLnspecArgMacros.Speed(speed), 0, -1,
            VmPLnspecArgMacros.Change(change), true) == true;

    public static bool FloorRaiseToCeiling(int tag, int speed, int change, int crush, int gap, LineContext ctx) =>
        ctx.EvDoFloor?.Invoke(FloorKind.RaiseToCeiling, tag, VmPLnspecArgMacros.Speed(speed), gap,
            VmPLnspecArgMacros.Crush(crush), VmPLnspecArgMacros.Change(change), true) == true;

    public static bool FloorRaiseByValueTxTy(int tag, int speed, int height, LineContext ctx) =>
        ctx.EvDoFloor?.Invoke(FloorKind.RaiseAndChange, tag, VmPLnspecArgMacros.Speed(speed), height, -1, 0, false) == true;

    public static bool FloorLowerToLowestTxTy(int tag, int speed, int height, LineContext ctx) =>
        ctx.EvDoFloor?.Invoke(FloorKind.LowerAndChange, tag, VmPLnspecArgMacros.Speed(speed), height, -1, 0, false) == true;

    public static bool FloorWaggle(int tag, int a1, int a2, int a3, int a4, LineContext ctx) =>
        ctx.EvStartWaggle?.Invoke(tag, a1, a2, a3, a4, false) == true;

    public static bool CeilingWaggle(int tag, int a1, int a2, int a3, int a4, LineContext ctx) =>
        ctx.EvStartWaggle?.Invoke(tag, a1, a2, a3, a4, true) == true;

    public static bool FloorTransferTrigger(LineContext ctx, int tag) =>
        ctx.EvDoChange?.Invoke(tag, false) == true;

    public static bool FloorTransferNumeric(LineContext ctx, int tag) =>
        ctx.EvDoChange?.Invoke(tag, true) == true;

    public static bool FloorDonut(int pillarTag, int pillarSpeed, int slimeSpeed, LineContext ctx) =>
        ctx.EvDoDonut?.Invoke(pillarTag, VmPLnspecArgMacros.Speed(pillarSpeed), VmPLnspecArgMacros.Speed(slimeSpeed)) == true;

    static FloorKind ResolveGenericFloorKind(int target, bool raise) =>
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
            5 => FloorKind.LowerToCeiling,
            6 => FloorKind.LowerByTextureGeneric,
            _ => FloorKind.LowerByValue,
        };

    public static bool GenericFloor(int tag, int speed, int height, int target, int flags, LineContext ctx)
    {
        var raise = (flags & 8) != 0;
        var kind = ResolveGenericFloorKind(target, raise);
        var crush = (flags & 16) != 0 ? 20 : -1;
        var change = flags & 7;
        return ctx.EvDoFloor?.Invoke(kind, tag, VmPLnspecArgMacros.Speed(speed), height, crush, change, true) == true;
    }

    public static bool FloorStop(int tag, LineContext ctx) =>
        ctx.EvStopFloor?.Invoke(tag) == true;

    const int StairUseSpecials = 1;
    const int StairSync = 2;
    const int StairCrush = 4;

    static bool BuildStairs(
        int tag,
        StairBuild dir,
        int height,
        int speed,
        int delayTics,
        int reset,
        int flags,
        LineContext ctx) =>
        ctx.EvBuildStairs?.Invoke(tag, dir, height, VmPLnspecArgMacros.Speed(speed), delayTics, reset, flags) == true;

    public static bool StairsBuildDown(int tag, int speed, int height, int delay, int reset, LineContext ctx) =>
        BuildStairs(tag, StairBuild.Down, height, speed, VmPLnspecArgMacros.Tics(delay), reset, StairUseSpecials, ctx);

    public static bool StairsBuildUp(int tag, int speed, int height, int delay, int reset, LineContext ctx) =>
        BuildStairs(tag, StairBuild.Up, height, speed, VmPLnspecArgMacros.Tics(delay), reset, StairUseSpecials, ctx);

    public static bool StairsBuildDownSync(int tag, int speed, int height, int reset, LineContext ctx) =>
        BuildStairs(tag, StairBuild.Down, height, speed, 0, reset, StairUseSpecials | StairSync, ctx);

    public static bool StairsBuildUpSync(int tag, int speed, int height, int reset, LineContext ctx) =>
        BuildStairs(tag, StairBuild.Up, height, speed, 0, reset, StairUseSpecials | StairSync, ctx);

    public static bool StairsBuildUpDoom(int tag, int speed, int height, int delay, int reset, LineContext ctx) =>
        BuildStairs(tag, StairBuild.Up, height, speed, VmPLnspecArgMacros.Tics(delay), reset, 0, ctx);

    public static bool StairsBuildUpDoomCrush(int tag, int speed, int height, int delay, int reset, LineContext ctx) =>
        BuildStairs(tag, StairBuild.Up, height, speed, VmPLnspecArgMacros.Tics(delay), reset, StairCrush, ctx);

    public static bool StairsBuildDownDoom(int tag, int speed, int height, int delay, int reset, LineContext ctx) =>
        BuildStairs(tag, StairBuild.Down, height, speed, VmPLnspecArgMacros.Tics(delay), reset, 0, ctx);

    public static bool StairsBuildDownDoomSync(int tag, int speed, int height, int reset, LineContext ctx) =>
        BuildStairs(tag, StairBuild.Down, height, speed, 0, reset, StairSync, ctx);

    public static bool StairsBuildUpDoomSync(int tag, int speed, int height, int reset, LineContext ctx) =>
        BuildStairs(tag, StairBuild.Up, height, speed, 0, reset, StairSync, ctx);

    public static bool GenericStairs(int tag, int speed, int step, int dirFlags, int reset, LineContext ctx)
    {
        var buildUp = (dirFlags & 1) != 0;
        var dir = buildUp ? StairBuild.Up : StairBuild.Down;
        var res = BuildStairs(tag, dir, step, speed, 0, reset, dirFlags & 2, ctx);
        if (res && ctx.RepeatSpecial && ctx.LineSpecialId == ctx.GenericStairsSpecialId)
            ctx.ToggleGenericStairsDirection?.Invoke();
        return res;
    }

    public static bool PillarBuild(int tag, int speed, int height, LineContext ctx) =>
        ctx.EvDoPillar?.Invoke(PillarKind.Build, tag, VmPLnspecArgMacros.Speed(speed), height, 0, -1, false) == true;

    public static bool PillarBuildAndCrush(int tag, int speed, int height, int crush, int crushMode, LineContext ctx) =>
        ctx.EvDoPillar?.Invoke(PillarKind.Build, tag, VmPLnspecArgMacros.Speed(speed), height, 0, crush,
            VmPLnspecPolyobjLineSpecials.CrushTypeDoomHexen(crushMode, ctx.GameTypeHexen)) == true;

    public static bool PillarOpen(int tag, int speed, int floorHeight, int ceilHeight, LineContext ctx) =>
        ctx.EvDoPillar?.Invoke(PillarKind.Open, tag, VmPLnspecArgMacros.Speed(speed), floorHeight, ceilHeight, -1, false) == true;

    public static bool CeilingLowerByValue(int tag, int speed, int height, int change, int crush, LineContext ctx) =>
        ctx.EvDoCeiling?.Invoke(CeilingKind.LowerByValue, tag, VmPLnspecArgMacros.Speed(speed), 0, height,
            VmPLnspecArgMacros.Crush(crush), VmPLnspecArgMacros.Change(change), false) == true;

    public static bool CeilingRaiseByValue(int tag, int speed, int height, int change, LineContext ctx) =>
        ctx.EvDoCeiling?.Invoke(CeilingKind.RaiseByValue, tag, VmPLnspecArgMacros.Speed(speed), 0, height, -1,
            VmPLnspecArgMacros.Change(change), false) == true;

    public static bool CeilingLowerByValueTimes8(int tag, int speed, int height, int change, LineContext ctx) =>
        ctx.EvDoCeiling?.Invoke(CeilingKind.LowerByValueTimes8, tag, VmPLnspecArgMacros.Speed(speed), 0, height * 8, -1,
            VmPLnspecArgMacros.Change(change), false) == true;

    public static bool CeilingRaiseByValueTimes8(int tag, int speed, int height, int change, LineContext ctx) =>
        ctx.EvDoCeiling?.Invoke(CeilingKind.RaiseByValueTimes8, tag, VmPLnspecArgMacros.Speed(speed), 0, height * 8, -1,
            VmPLnspecArgMacros.Change(change), false) == true;

    public static bool CeilingCrushAndRaise(int tag, int speed, int crush, int crushMode, LineContext ctx)
    {
        var spd = VmPLnspecArgMacros.Speed(speed);
        return ctx.EvDoCeiling?.Invoke(CeilingKind.CrushAndRaise, tag, spd, spd / 2, 8, crush, 0,
            VmPLnspecPolyobjLineSpecials.CrushTypeDoomHexen(crushMode, ctx.GameTypeHexen)) == true;
    }
}
