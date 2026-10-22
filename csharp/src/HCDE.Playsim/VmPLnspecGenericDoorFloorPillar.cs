namespace HCDE.Playsim;

/// <summary>
/// Generic door/floor, door wait, texture floors, waggle/transfer/donut, pillars from <c>p_lnspec.cpp</c>
/// (278–289, 305–334, 336–364, 415–419, 472–483, 502–573, 652–669).
/// </summary>
public static class VmPLnspecGenericDoorFloorPillar
{
    public enum DoorKind { Raise, Open, CloseWaitOpen, Close }

    public sealed class LineContext
    {
        public Func<DoorKind, int, double, int, int, int, bool, bool>? EvDoDoor { get; init; }
        public Func<int, int, double, int, int, int, bool, bool>? EvDoFloor { get; init; }
        public Func<int, int, double, int, int, int, int, bool>? EvDoPillar { get; init; }
        public Func<int, int, int, int, int, bool, bool>? EvStartWaggle { get; init; }
        public Func<int, bool, bool>? EvDoChange { get; init; }
        public Func<int, bool>? EvStopFloor { get; init; }
        public Func<int, double, double, bool>? EvDoDonut { get; init; }
        public Func<int, bool>? EvFloorCrushStop { get; init; }
    }

    public static bool DoorWaitRaise(int tag, int speed, int delay, int wait, int lightTag, LineContext ctx) =>
        ctx.EvDoDoor?.Invoke(DoorKind.Raise, tag, VmPLnspecArgMacros.Speed(speed), VmPLnspecArgMacros.Tics(delay), 0, lightTag, false) == true;

    public static bool DoorWaitClose(int tag, int speed, int wait, int lightTag, LineContext ctx) =>
        ctx.EvDoDoor?.Invoke(DoorKind.Close, tag, VmPLnspecArgMacros.Speed(speed), VmPLnspecArgMacros.Tics(wait), 0, lightTag, false) == true;

    public static bool GenericDoor(int arg0, int arg1, int arg2, int arg3, int arg4, LineContext ctx)
    {
        var kindBits = arg2 & 63;
        var kind = kindBits switch
        {
            0 => DoorKind.Raise,
            1 => DoorKind.Open,
            2 => DoorKind.CloseWaitOpen,
            3 => DoorKind.Close,
            _ => (DoorKind)(-1),
        };
        if ((int)kind < 0)
            return false;
        var boomGen = (arg2 & 64) != 0;
        var localLight = (arg2 & 128) != 0;
        var tag = localLight ? 0 : arg0;
        var lightTag = localLight ? arg0 : 0;
        return ctx.EvDoDoor?.Invoke(kind, tag, VmPLnspecArgMacros.Speed(arg1), VmPLnspecArgMacros.Octics(arg3), arg4, lightTag, boomGen) == true;
    }

    public static bool FloorLowerByValue(int tag, int speed, int height, int change, LineContext ctx) =>
        ctx.EvDoFloor?.Invoke(0, tag, VmPLnspecArgMacros.Speed(speed), height, -1, VmPLnspecArgMacros.Change(change), false) == true;

    public static bool FloorLowerToLowest(int tag, int speed, int change, LineContext ctx) =>
        ctx.EvDoFloor?.Invoke(1, tag, VmPLnspecArgMacros.Speed(speed), 0, -1, VmPLnspecArgMacros.Change(change), false) == true;

    public static bool FloorLowerToHighest(int tag, int speed, int adjust, bool hereticLower, LineContext ctx) =>
        ctx.EvDoFloor?.Invoke(2, tag, VmPLnspecArgMacros.Speed(speed), adjust - 128, -1, 0, hereticLower) == true;

    public static bool FloorLowerToNearest(int tag, int speed, int change, LineContext ctx) =>
        ctx.EvDoFloor?.Invoke(3, tag, VmPLnspecArgMacros.Speed(speed), 0, -1, VmPLnspecArgMacros.Change(change), false) == true;

    public static bool FloorCrushStop(int tag, LineContext ctx) =>
        ctx.EvFloorCrushStop?.Invoke(tag) == true;

    public static bool FloorRaiseByTexture(int tag, int speed, int change, int crush, LineContext ctx) =>
        ctx.EvDoFloor?.Invoke(10, tag, VmPLnspecArgMacros.Speed(speed), 0, VmPLnspecArgMacros.Crush(crush), VmPLnspecArgMacros.Change(change), true) == true;

    public static bool FloorLowerByTexture(int tag, int speed, int change, LineContext ctx) =>
        ctx.EvDoFloor?.Invoke(11, tag, VmPLnspecArgMacros.Speed(speed), 0, -1, VmPLnspecArgMacros.Change(change), false) == true;

    public static bool FloorWaggle(int tag, int a1, int a2, int a3, int a4, LineContext ctx) =>
        ctx.EvStartWaggle?.Invoke(tag, a1, a2, a3, a4, false) == true;

    public static bool CeilingWaggle(int tag, int a1, int a2, int a3, int a4, LineContext ctx) =>
        ctx.EvStartWaggle?.Invoke(tag, a1, a2, a3, a4, true) == true;

    public static bool FloorTransferTrigger(int tag, LineContext ctx) =>
        ctx.EvDoChange?.Invoke(tag, false) == true;

    public static bool FloorTransferNumeric(int tag, LineContext ctx) =>
        ctx.EvDoChange?.Invoke(tag, true) == true;

    public static bool FloorDonut(int pillarTag, int pillarSpeed, int slimeSpeed, LineContext ctx) =>
        ctx.EvDoDonut?.Invoke(pillarTag, VmPLnspecArgMacros.Speed(pillarSpeed), VmPLnspecArgMacros.Speed(slimeSpeed)) == true;

    public static bool GenericFloor(int tag, int speed, int height, int target, int flags, LineContext ctx)
    {
        var invoke = VmPLnspecFloorLineSpecials.GenericFloor(tag, speed, height, target, flags);
        return ctx.EvDoFloor?.Invoke((int)invoke.Kind, invoke.Tag, invoke.Speed, invoke.Height, invoke.Crush, invoke.Change, invoke.DirectionUp) == true;
    }

    public static bool FloorStop(int tag, LineContext ctx) =>
        ctx.EvStopFloor?.Invoke(tag) == true;

    public static bool PillarBuild(int tag, int speed, int height, LineContext ctx) =>
        ctx.EvDoPillar?.Invoke(0, tag, VmPLnspecArgMacros.Speed(speed), height, 0, -1, 0) == true;

    public static bool PillarBuildAndCrush(int tag, int speed, int height, int crush, int crushType, LineContext ctx) =>
        ctx.EvDoPillar?.Invoke(0, tag, VmPLnspecArgMacros.Speed(speed), height, 0, crush, crushType) == true;

    public static bool PillarOpen(int tag, int speed, int floorHeight, int ceilHeight, LineContext ctx) =>
        ctx.EvDoPillar?.Invoke(1, tag, VmPLnspecArgMacros.Speed(speed), floorHeight, ceilHeight, -1, 0) == true;
}
