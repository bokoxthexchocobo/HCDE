namespace HCDE.Playsim;

/// <summary>DPolyAction / thinker serialize+construct, <c>FPolyObj</c> ctor, <c>GetMirror</c>, <c>RotatePt</c> from <c>po_man.cpp</c> (147–280, 715–730, 738–741, 918–927).</summary>
public static class VmPoManPolyThinkerBands
{
    public sealed class PolyActionState
    {
        public object? PolyObj { get; set; }
        public double Speed { get; set; }
        public double Dist { get; set; }
        public object? Interpolation { get; set; }
        public object? SpecialDataOnPoly { get; set; }
    }

    public sealed class PolyActionSerializeContext
    {
        public PolyActionState State { get; } = new();
        public Action<string, object?>? WriteField { get; init; }
        public Action? SerializeSuper { get; init; }
    }

    public static void PolyActionSerialize(PolyActionSerializeContext ctx)
    {
        ctx.SerializeSuper?.Invoke();
        ctx.WriteField?.Invoke("polyobj", ctx.State.PolyObj);
        ctx.WriteField?.Invoke("speed", ctx.State.Speed);
        ctx.WriteField?.Invoke("dist", ctx.State.Dist);
        ctx.WriteField?.Invoke("interpolation", ctx.State.Interpolation);
    }

    public sealed class PolyActionConstructContext
    {
        public PolyActionState State { get; } = new();
        public Action? SetInterpolation { get; init; }
    }

    public static void PolyActionConstruct(PolyActionConstructContext ctx, object? polyObj)
    {
        ctx.State.PolyObj = polyObj;
        ctx.State.Speed = 0;
        ctx.State.Dist = 0;
        ctx.SetInterpolation?.Invoke();
    }

    public static void PolyActionOnDestroy(PolyActionState state, Action? stopInterpolation, Action? destroySuper)
    {
        if (ReferenceEquals(state.SpecialDataOnPoly, state))
            state.SpecialDataOnPoly = null;
        stopInterpolation?.Invoke();
        destroySuper?.Invoke();
    }

    public static void PolyActionStop(Action? stopSequence, Action? destroy) => stopSequence?.Invoke();

    public static void PolyActionSetInterpolation(PolyActionState state, Func<object?>? createInterpolation)
    {
        if (createInterpolation is not null)
            state.Interpolation = createInterpolation();
    }

    public static void PolyActionStopInterpolation(PolyActionState state, Action<object?>? releaseInterpolation)
    {
        if (state.Interpolation is null)
            return;
        releaseInterpolation?.Invoke(state.Interpolation);
        state.Interpolation = null;
    }

    public static void RotatePolyConstruct(PolyActionConstructContext ctx, object? poly) =>
        PolyActionConstruct(ctx, poly);

    public sealed class MovePolyState : PolyActionState
    {
        public double AngleDegrees { get; set; }
        public double SpeedVx { get; set; }
        public double SpeedVy { get; set; }
    }

    public static void MovePolySerialize(PolyActionSerializeContext ctx, MovePolyState move)
    {
        PolyActionSerialize(ctx);
        ctx.WriteField?.Invoke("angle", move.AngleDegrees);
        ctx.WriteField?.Invoke("speedv", (move.SpeedVx, move.SpeedVy));
    }

    public static void MovePolyConstruct(PolyActionConstructContext ctx, object? poly, MovePolyState move)
    {
        PolyActionConstruct(ctx, poly);
        move.AngleDegrees = 0;
        move.SpeedVx = 0;
        move.SpeedVy = 0;
    }

    public sealed class MovePolyToState : PolyActionState
    {
        public double SpeedVx { get; set; }
        public double SpeedVy { get; set; }
        public double TargetX { get; set; }
        public double TargetY { get; set; }
    }

    public static void MovePolyToSerialize(PolyActionSerializeContext ctx, MovePolyToState move)
    {
        PolyActionSerialize(ctx);
        ctx.WriteField?.Invoke("speedv", (move.SpeedVx, move.SpeedVy));
        ctx.WriteField?.Invoke("target", (move.TargetX, move.TargetY));
    }

    public static void MovePolyToConstruct(PolyActionConstructContext ctx, object? poly, MovePolyToState move)
    {
        PolyActionConstruct(ctx, poly);
        move.SpeedVx = move.SpeedVy = move.TargetX = move.TargetY = 0;
    }

    public sealed class PolyDoorState : PolyActionState
    {
        public int DoorType { get; set; }
        public double DirectionDegrees { get; set; }
        public double TotalDist { get; set; }
        public int Tics { get; set; }
        public int WaitTics { get; set; }
        public bool Close { get; set; }
    }

    public static void PolyDoorSerialize(PolyActionSerializeContext ctx, PolyDoorState door)
    {
        PolyActionSerialize(ctx);
        ctx.WriteField?.Invoke("type", door.DoorType);
        ctx.WriteField?.Invoke("direction", door.DirectionDegrees);
        ctx.WriteField?.Invoke("totaldist", door.TotalDist);
        ctx.WriteField?.Invoke("tics", door.Tics);
        ctx.WriteField?.Invoke("waittics", door.WaitTics);
        ctx.WriteField?.Invoke("close", door.Close);
    }

    public static void PolyDoorConstruct(PolyActionConstructContext ctx, object? poly, PolyDoorState door, int doorType)
    {
        PolyActionConstruct(ctx, poly);
        door.DoorType = doorType;
        door.DirectionDegrees = 0;
        door.TotalDist = 0;
        door.Tics = 0;
        door.WaitTics = 0;
        door.Close = false;
    }

    public sealed class FPolyObjState
    {
        public double StartSpotX { get; set; }
        public double StartSpotY { get; set; }
        public double AngleDegrees { get; set; }
        public int Tag { get; set; }
        public int[] Bbox { get; } = new int[4];
        public int ValidCount { get; set; }
        public int Crush { get; set; }
        public bool HurtOnTouch { get; set; }
        public int SeqType { get; set; }
        public int Size { get; set; }
        public bool Blocked { get; set; }
        public object? SubsectorLinks { get; set; }
        public object? SpecialData { get; set; }
        public object? Interpolation { get; set; }
        public int MirrorNum { get; set; }
    }

    public static void FPolyObjConstruct(FPolyObjState poly)
    {
        poly.StartSpotX = 0;
        poly.StartSpotY = 0;
        poly.AngleDegrees = 0;
        poly.Tag = 0;
        Array.Clear(poly.Bbox, 0, poly.Bbox.Length);
        poly.ValidCount = 0;
        poly.Crush = 0;
        poly.HurtOnTouch = false;
        poly.SeqType = 0;
        poly.Size = 0;
        poly.Blocked = false;
        poly.SubsectorLinks = null;
        poly.SpecialData = null;
        poly.Interpolation = null;
    }

    public static int FPolyObjGetMirror(FPolyObjState poly) => poly.MirrorNum;

    public static void RotatePt(double angleDegrees, ref double outX, ref double outY, double startX, double startY)
    {
        var trX = outX;
        var trY = outY;
        var rad = angleDegrees * Math.PI / 180.0;
        var s = Math.Sin(rad);
        var c = Math.Cos(rad);
        outX = trX * c - trY * s + startX;
        outY = trX * s + trY * c + startY;
    }
}
