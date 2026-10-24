namespace HCDE.Playsim;

/// <summary>Elevator lifecycle and waggle thinkers from <c>a_floor.cpp</c> (878–939, 1167–1353).</summary>
public static class VmAFloorWaggleElevator
{
    public const int WglstateExpand = 1;
    public const int WglstateStable = 2;
    public const int WglstateReduce = 3;
    public const int Ticrate = 35;

    public sealed class FloorSector
    {
        public object? FloorData { get; set; }
        public object? CeilingData { get; set; }
        public double FloorPlaneD { get; set; }
        public double CeilingPlaneD { get; set; }
        public Func<bool>? PlaneMovingFloor { get; set; }
        public Func<bool>? PlaneMovingCeiling { get; set; }
        public Action<object?>? SetFloorInterp { get; set; }
        public Action<object?>? SetCeilingInterp { get; set; }
    }

    public sealed class ElevatorThinker
    {
        public required FloorSector Sector { get; init; }
        public object? InterpFloor { get; set; }
        public object? InterpCeiling { get; set; }
        public int Type { get; set; }
        public int Direction { get; set; }
        public double FloorDestDist { get; set; }
        public double CeilingDestDist { get; set; }
        public double Speed { get; set; }
    }

    public sealed class WaggleThinker
    {
        public required FloorSector Sector { get; init; }
        public bool Ceiling { get; set; }
        public object? Interpolation { get; set; }
        public double OriginalDist { get; set; }
        public double Accumulator { get; set; }
        public double AccDelta { get; set; }
        public double TargetScale { get; set; }
        public double Scale { get; set; }
        public double ScaleDelta { get; set; }
        public int Ticker { get; set; }
        public int State { get; set; } = WglstateExpand;
        public bool Destroyed { get; set; }
    }

    public sealed class ElevatorLifecycleContext
    {
        public Action<object?>? DelRef { get; init; }
    }

    public sealed class WaggleTickContext
    {
        public Func<double, double>? BobSin { get; init; }
        public Func<WaggleThinker, double, double>? GetPlaneD { get; init; }
        public Action<WaggleThinker, double>? SetPlaneD { get; init; }
        public Func<WaggleThinker, double, double>? HeightDiff { get; init; }
        public Action<WaggleThinker, double>? ChangePlaneTexZ { get; init; }
        public Action<WaggleThinker, int, double, bool>? Scroll3dMidtex { get; init; }
        public Action<WaggleThinker, int, double, bool>? MoveLinkedSectors { get; init; }
        public Action<WaggleThinker, int, double, bool, bool>? ChangeSector { get; init; }
        public Action<WaggleThinker>? Destroy { get; init; }
    }

    public sealed class StartWaggleContext
    {
        public Func<int, IEnumerable<int>>? SectorTagIterator { get; init; }
        public Func<int, FloorSector?>? SectorByIndex { get; init; }
        public Func<FloorSector, WaggleThinker>? CreateFloorWaggle { get; init; }
        public Func<FloorSector, WaggleThinker>? CreateCeilingWaggle { get; init; }
    }

    public static void ElevatorConstruct(ElevatorThinker elev, object? floorInterp, object? ceilingInterp)
    {
        elev.Sector.FloorData = elev;
        elev.Sector.CeilingData = elev;
        elev.InterpFloor = floorInterp;
        elev.InterpCeiling = ceilingInterp;
        elev.Sector.SetFloorInterp?.Invoke(floorInterp);
        elev.Sector.SetCeilingInterp?.Invoke(ceilingInterp);
    }

    public static void ElevatorSerialize(ElevatorThinker elev, Action<string, object?> writeField)
    {
        writeField("type", elev.Type);
        writeField("direction", elev.Direction);
        writeField("floordestdist", elev.FloorDestDist);
        writeField("ceilingdestdist", elev.CeilingDestDist);
        writeField("speed", elev.Speed);
        writeField("interp_floor", elev.InterpFloor);
        writeField("interp_ceiling", elev.InterpCeiling);
    }

    public static void ElevatorOnDestroy(ElevatorThinker elev, ElevatorLifecycleContext ctx)
    {
        if (elev.InterpCeiling != null)
        {
            ctx.DelRef?.Invoke(elev.InterpCeiling);
            elev.InterpCeiling = null;
        }
        if (elev.InterpFloor != null)
        {
            ctx.DelRef?.Invoke(elev.InterpFloor);
            elev.InterpFloor = null;
        }
    }

    public static void WaggleBaseConstruct(WaggleThinker w) { }

    public static void WaggleBaseSerialize(WaggleThinker w, Action<string, object?> writeField)
    {
        writeField("originaldist", w.OriginalDist);
        writeField("accumulator", w.Accumulator);
        writeField("accdelta", w.AccDelta);
        writeField("targetscale", w.TargetScale);
        writeField("scale", w.Scale);
        writeField("scaledelta", w.ScaleDelta);
        writeField("ticker", w.Ticker);
        writeField("state", w.State);
    }

    public static void FloorWaggleConstruct(WaggleThinker w, object? interp)
    {
        w.Sector.FloorData = w;
        w.Interpolation = interp;
        w.Sector.SetFloorInterp?.Invoke(interp);
    }

    public static void CeilingWaggleConstruct(WaggleThinker w, object? interp)
    {
        w.Sector.CeilingData = w;
        w.Interpolation = interp;
        w.Sector.SetCeilingInterp?.Invoke(interp);
    }

    public static void FloorWaggleTick(WaggleThinker w, WaggleTickContext ctx) => DoWaggle(w, false, ctx);

    public static void CeilingWaggleTick(WaggleThinker w, WaggleTickContext ctx) => DoWaggle(w, true, ctx);

    public static void DoWaggle(WaggleThinker w, bool ceiling, WaggleTickContext ctx)
    {
        switch (w.State)
        {
            case WglstateExpand:
                w.Scale += w.ScaleDelta;
                if (w.Scale >= w.TargetScale)
                {
                    w.Scale = w.TargetScale;
                    w.State = WglstateStable;
                }
                break;
            case WglstateReduce:
                w.Scale -= w.ScaleDelta;
                if (w.Scale <= 0)
                {
                    var planeD = ctx.GetPlaneD?.Invoke(w, w.OriginalDist) ?? w.OriginalDist;
                    var dist = (w.OriginalDist - planeD) / 1.0;
                    ctx.ChangePlaneTexZ?.Invoke(w, -ctx.HeightDiff?.Invoke(w, w.OriginalDist) ?? 0);
                    ctx.SetPlaneD?.Invoke(w, w.OriginalDist);
                    ctx.ChangeSector?.Invoke(w, 1, dist, ceiling, false);
                    if (ceiling)
                        w.Sector.CeilingData = null;
                    else
                        w.Sector.FloorData = null;
                    w.Destroyed = true;
                    ctx.Destroy?.Invoke(w);
                    return;
                }
                break;
            case WglstateStable:
                if (w.Ticker != -1 && --w.Ticker == 0)
                    w.State = WglstateReduce;
                break;
        }

        w.Accumulator += w.AccDelta;
        var oldD = ctx.GetPlaneD?.Invoke(w, 0) ?? 0;
        var bob = (ctx.BobSin?.Invoke(w.Accumulator) ?? 0) * w.Scale;
        ctx.SetPlaneD?.Invoke(w, w.OriginalDist + bob);
        ctx.ChangePlaneTexZ?.Invoke(w, ctx.HeightDiff?.Invoke(w, oldD) ?? 0);
        var delta = ctx.HeightDiff?.Invoke(w, oldD) ?? 0;
        ctx.Scroll3dMidtex?.Invoke(w, 1, delta, ceiling);
        ctx.MoveLinkedSectors?.Invoke(w, 1, delta, ceiling);
        ctx.ChangeSector?.Invoke(w, 1, delta, ceiling, false);
    }

    public static bool EvStartWaggle(
        StartWaggleContext ctx,
        int tag,
        int height,
        int speed,
        int offset,
        int timer,
        bool ceiling)
    {
        var ret = false;
        foreach (var secnum in ctx.SectorTagIterator?.Invoke(tag) ?? Array.Empty<int>())
        {
            var sector = ctx.SectorByIndex?.Invoke(secnum);
            if (sector == null)
                continue;
            if ((!ceiling && sector.PlaneMovingFloor?.Invoke() == true) ||
                (ceiling && sector.PlaneMovingCeiling?.Invoke() == true))
                continue;

            ret = true;
            var waggle = ceiling
                ? ctx.CreateCeilingWaggle?.Invoke(sector)
                : ctx.CreateFloorWaggle?.Invoke(sector);
            if (waggle == null)
                continue;

            waggle.Ceiling = ceiling;
            waggle.OriginalDist = ceiling ? sector.CeilingPlaneD : sector.FloorPlaneD;
            waggle.Accumulator = offset;
            waggle.AccDelta = speed / 64.0;
            waggle.Scale = 0;
            waggle.TargetScale = height / 64.0;
            waggle.ScaleDelta = waggle.TargetScale / (Ticrate + (3 * Ticrate * height) / 255.0);
            waggle.Ticker = timer > 0 ? timer * Ticrate : -1;
            waggle.State = WglstateExpand;
        }
        return ret;
    }
}
