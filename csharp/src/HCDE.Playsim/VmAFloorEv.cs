namespace HCDE.Playsim;

/// <summary>Floor line specials in <c>a_floor.cpp</c> (796–863, 1113–1154); EV floor/stair band on chunk **919** <see cref="VmAFloorEvLineFloor919Slice"/>.</summary>
public static class VmAFloorEv
{
    public const int BuildUp = 0;
    public const int BuildDown = 1;
    public const int StairUseSpecials = 1;
    public const int StairSync = 2;
    public const int StairCrush = 4;
    public const int CompatfStairindex = 1;
    public const int Compatf2Floormove = 1;
    public const int TrigChangeOnly = 0;
    public const int NumChangeOnly = 1;
    public const int StairsSpecial1 = 26;
    public const int StairsSpecial2 = 27;

    public sealed class EvLine
    {
        public StairSector? FrontSector { get; set; }
    }

    public sealed class StairLine
    {
        public bool TwoSided { get; set; }
        public StairSector? FrontSector { get; set; }
        public StairSector? BackSector { get; set; }
    }

    public sealed class StairSector
    {
        public int SectorNum { get; set; }
        public int Stairlock { get; set; }
        public int Prevsec { get; set; } = -1;
        public int Nextsec { get; set; } = -1;
        public int Validcount { get; set; }
        public string FloorTexture { get; set; } = "";
        public double FloorPlaneD { get; set; }
        public VmAFloorTickElevator.VertexSpot Centerspot { get; } = new();
        public int Special { get; set; }
        public List<StairLine> Lines { get; } = new();
        public object? FloorData { get; set; }
        public Func<double, double, double, double>? PointToDist { get; set; }
        public double CenterFloor { get; set; }
    }

    public sealed class FloorEvContext
    {
        public int CompatFlags { get; init; }
        public int CompatFlags2 { get; init; }
        public Func<int, EvLine?, IEnumerable<int>>? SectorTagIterator { get; init; }
        public Func<int, StairSector?>? SectorByIndex { get; init; }
        public Func<int, int, IEnumerable<int>>? SectorTagIteratorCompat { get; init; }
        public VmAFloorCreate.CreateFloorContext? CreateFloor { get; init; }
        public Func<StairSector, VmAFloorCreate.FloorSector?>? ToCreateSector { get; init; }
        public Func<StairSector, bool>? PlaneMovingFloor { get; init; }
        public Func<StairSector, VmAFloorTickElevator.FloorThinker>? CreateStairThinker { get; init; }
        public Action<VmAFloorTickElevator.FloorThinker>? StartFloorSound { get; init; }
        public Func<StairSector, int, StairSector?>? NextSpecialSector { get; init; }
        public Func<StairSector, int>? BumpValidcount { get; init; }
        public Func<StairSector, VmAFloorTickElevator.VertexSpot, double>? FindHighestFloorPoint { get; init; }
        public Func<StairSector, StairLine, StairSector, StairSector?>? GetNextSector { get; init; }
        public Func<StairSector, bool>? IsFloorThinker { get; init; }
        public Func<StairSector, int>? GetFloorThinkerType { get; init; }
        public Action<StairSector>? StopSequence { get; init; }
        public Action<StairSector>? DestroyFloorData { get; init; }
        public Action<StairSector, string>? SetFloorTexture { get; init; }
        public Action<StairSector, StairSector>? TransferSpecial { get; init; }
        public Func<StairSector, double, StairSector?>? FindModelFloorSector { get; init; }
    }

    public static bool EvDoChange(FloorEvContext ctx, EvLine? line, int changeType, int tag)
    {
        var rtn = false;
        foreach (var secnum in ctx.SectorTagIterator?.Invoke(tag, null) ?? Array.Empty<int>())
        {
            var sec = ctx.SectorByIndex?.Invoke(secnum);
            if (sec == null)
                continue;
            rtn = true;
            switch (changeType)
            {
                case TrigChangeOnly:
                    if (line?.FrontSector != null)
                    {
                        ctx.SetFloorTexture?.Invoke(sec, line.FrontSector.FloorTexture);
                        ctx.TransferSpecial?.Invoke(sec, line.FrontSector);
                    }
                    break;
                case NumChangeOnly:
                    var model = ctx.FindModelFloorSector?.Invoke(sec, sec.CenterFloor);
                    if (model != null)
                    {
                        ctx.SetFloorTexture?.Invoke(sec, model.FloorTexture);
                        ctx.TransferSpecial?.Invoke(sec, model);
                    }
                    break;
            }
        }
        return rtn;
    }

    public static bool EvDoDonut(FloorEvContext ctx, int tag, EvLine? line, double pillarspeed, double slimespeed)
    {
        var rtn = false;
        foreach (var secnum in ctx.SectorTagIterator?.Invoke(tag, line) ?? Array.Empty<int>())
        {
            var s1 = ctx.SectorByIndex?.Invoke(secnum);
            if (s1 == null || ctx.PlaneMovingFloor?.Invoke(s1) == true)
                continue;
            rtn = true;
            if (s1.Lines.Count == 0)
                continue;
            var s2 = ctx.GetNextSector?.Invoke(s1, s1.Lines[0], s1);
            if (s2 == null)
                continue;
            if ((ctx.CompatFlags2 & Compatf2Floormove) == 0 && ctx.PlaneMovingFloor?.Invoke(s2) == true)
                continue;

            foreach (var ln in s2.Lines)
            {
                if (ln.BackSector == null || ln.BackSector == s1)
                    continue;
                var s3 = ln.BackSector;
                var slime = ctx.CreateStairThinker?.Invoke(s2);
                if (slime != null)
                {
                    slime.Type = VmAFloorTickElevator.DonutRaise;
                    slime.Crush = -1;
                    slime.Direction = 1;
                    slime.Speed = slimespeed;
                    slime.Texture = s3.FloorTexture;
                    var spot = new VmAFloorTickElevator.VertexSpot();
                    var height = ctx.FindHighestFloorPoint?.Invoke(s3, spot) ?? 0;
                    slime.FloorDestDist = s2.PointToDist?.Invoke(spot.X, spot.Y, height) ?? 0;
                    ctx.StartFloorSound?.Invoke(slime);
                }
                var pillar = ctx.CreateStairThinker?.Invoke(s1);
                if (pillar != null)
                {
                    pillar.Type = VmAFloorCreate.FloorLowerToNearest;
                    pillar.Crush = -1;
                    pillar.Direction = -1;
                    pillar.Speed = pillarspeed;
                    var spot = new VmAFloorTickElevator.VertexSpot();
                    var height = ctx.FindHighestFloorPoint?.Invoke(s3, spot) ?? 0;
                    pillar.FloorDestDist = s1.PointToDist?.Invoke(spot.X, spot.Y, height) ?? 0;
                    ctx.StartFloorSound?.Invoke(pillar);
                }
                break;
            }
        }
        return rtn;
    }

}
