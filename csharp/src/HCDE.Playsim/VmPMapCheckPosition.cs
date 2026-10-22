namespace HCDE.Playsim;

/// <summary>Faithful port of <c>P_CheckPosition</c> (<c>p_map.cpp</c> 1817–2034).</summary>
public static class VmPMapCheckPosition
{
    public const int MfNoclip = 1;
    public const int MfSkullfly = 1 << 1;
    public const int MfFloat = 1 << 2;
    public const int MfMissile = 1 << 3;
    public const int Mf2Thruactors = 1;
    public const int FfSolid = 1;
    public const int FfExists = 1 << 1;
    public const int CompatfNoPassmobj = 1;
    public const int FfcfRestrictedportal = 1;

    public sealed class F3dFloor
    {
        public int Flags { get; init; }
        public double BottomZ { get; init; }
        public double TopZ { get; init; }
        public object? FloorPic { get; init; }
        public object? CeilingPic { get; init; }
        public int FloorTerrain { get; init; }
    }

    public sealed class SectorSample
    {
        public bool PortalBlocksCeiling { get; init; }
        public bool PortalBlocksFloor { get; init; }
        public double FloorZ { get; init; }
        public double CeilingZ { get; init; }
        public object? FloorPic { get; init; }
        public object? CeilingPic { get; init; }
        public object? FloorSector { get; init; }
        public object? CeilingSector { get; init; }
        public int FloorTerrain { get; init; }
        public IReadOnlyList<F3dFloor> Floors3d { get; init; } = Array.Empty<F3dFloor>();
    }

    public sealed class CheckPositionTm
    {
        public object? Thing { get; set; }
        public double PosX { get; set; }
        public double PosY { get; set; }
        public double PosZ { get; set; }
        public object? Sector { get; set; }
        public object? CeilingLine { get; set; }
        public double FloorZ { get; set; }
        public double DropoffZ { get; set; }
        public bool DropoffIsPortal { get; set; }
        public object? FloorPic { get; set; }
        public double CeilingZ { get; set; }
        public object? CeilingPic { get; set; }
        public object? FloorSector { get; set; }
        public object? CeilingSector { get; set; }
        public int FloorTerrain { get; set; }
        public bool TouchMidtex { get; set; }
        public bool AboveMidtex { get; set; }
        public object? StepThing { get; set; }
    }

    public sealed class ThingIteratorResult
    {
        public object? Thing { get; init; }
        public int PortalFlags { get; init; }
    }

    public sealed class LineIteratorResult
    {
        public object? Line { get; init; }
        public bool PassablePortal { get; init; }
        public bool LinkedPortal { get; init; }
    }

    public sealed class BlockingThingProbe
    {
        public double Z { get; init; }
        public double Top { get; init; }
        public bool IsPlayer { get; init; }
    }

    public sealed class CheckPositionState
    {
        public required CheckPositionTm Tm { get; init; }
        public double PosX { get; set; }
        public double PosY { get; set; }
        public bool ActorsOnly { get; set; }
        public int ThingFlags { get; set; }
        public int ThingFlags2 { get; set; }
        public double ThingZ { get; set; }
        public double ThingHeight { get; set; }
        public double ThingTop => ThingHeight > 0 ? ThingZ + ThingHeight : ThingZ + 1;
        public double ThingRadius { get; set; }
        public double MaxStepHeight { get; set; }
        public bool IsNoClip2 { get; set; }
        public bool HasPlayer { get; set; }
        public int LevelCompatFlags { get; set; }
        public object? BlockingMobj { get; set; }
        public object? BlockingLine { get; set; }
        public object? ThingBlocker { get; set; }
        public Action<int>? BumpValidCount { get; init; }
        public Action? ClearSpechit { get; init; }
        public Action? ClearPortalhit { get; init; }
        public Func<double, double, SectorSample?>? PointInSector { get; init; }
        public Action<CheckPositionTm, int>? GetFloorCeilingZ { get; init; }
        public Func<SectorSample, double, double, (double floor, double ceiling, object? floorSec, object? ceilSec)>?
            LowestHighestAt { get; init; }
        public Func<IEnumerable<ThingIteratorResult>>? IterateThings { get; init; }
        public Func<ThingIteratorResult, bool>? PitCheckThing { get; init; }
        public Func<IEnumerable<LineIteratorResult>>? IterateLines { get; init; }
        public Func<LineIteratorResult, bool, bool>? PitCheckLine { get; init; }
        public Func<LineIteratorResult, bool>? PitCheckPortal { get; init; }
    }

    public static bool CheckPosition(CheckPositionState s)
    {
        var realHeight = s.ThingHeight;
        s.Tm.PosX = s.PosX;
        s.Tm.PosY = s.PosY;
        s.Tm.PosZ = s.ThingZ;
        s.BlockingLine = null;
        s.Tm.CeilingLine = null;

        var newsec = s.PointInSector?.Invoke(s.PosX, s.PosY);
        if (newsec == null)
            return false;

        s.Tm.Sector = newsec.FloorSector ?? newsec;

        if (!s.IsNoClip2)
        {
            if (!newsec.PortalBlocksCeiling || !newsec.PortalBlocksFloor)
                s.GetFloorCeilingZ?.Invoke(s.Tm, 0);
            else
            {
                s.Tm.FloorZ = s.Tm.DropoffZ = newsec.FloorZ;
                s.Tm.DropoffIsPortal = false;
                s.Tm.FloorPic = newsec.FloorPic;
                s.Tm.CeilingZ = newsec.CeilingZ;
                s.Tm.CeilingPic = newsec.CeilingPic;
                s.Tm.FloorSector = newsec.FloorSector;
                s.Tm.CeilingSector = newsec.CeilingSector;
                s.Tm.FloorTerrain = newsec.FloorTerrain;
            }

            var thingtop = s.ThingTop;
            foreach (var rover in newsec.Floors3d)
            {
                if ((rover.Flags & FfSolid) == 0 || (rover.Flags & FfExists) == 0)
                    continue;
                var mid = rover.BottomZ + (rover.TopZ - rover.BottomZ) / 2;
                var delta1 = s.ThingZ - mid;
                var delta2 = thingtop - mid;
                if (rover.TopZ > s.Tm.FloorZ && Math.Abs(delta1) < Math.Abs(delta2))
                {
                    s.Tm.FloorZ = s.Tm.DropoffZ = rover.TopZ;
                    s.Tm.DropoffIsPortal = false;
                    s.Tm.FloorPic = rover.FloorPic;
                    s.Tm.FloorTerrain = rover.FloorTerrain;
                }

                if (rover.BottomZ < s.Tm.CeilingZ && Math.Abs(delta1) >= Math.Abs(delta2))
                {
                    s.Tm.CeilingZ = rover.BottomZ;
                    s.Tm.CeilingPic = rover.CeilingPic;
                }
            }
        }
        else
        {
            var lh = s.LowestHighestAt?.Invoke(newsec, s.PosX, s.PosY);
            if (lh != null)
            {
                s.Tm.FloorZ = s.Tm.DropoffZ = lh.Value.floor;
                s.Tm.CeilingZ = lh.Value.ceiling;
                s.Tm.FloorSector = lh.Value.floorSec;
                s.Tm.CeilingSector = lh.Value.ceilSec;
            }

            s.Tm.FloorPic = newsec.FloorPic;
            s.Tm.FloorTerrain = newsec.FloorTerrain;
            s.Tm.CeilingPic = newsec.CeilingPic;
        }

        s.Tm.TouchMidtex = false;
        s.Tm.AboveMidtex = false;
        s.BumpValidCount?.Invoke(1);
        s.ClearSpechit?.Invoke();
        s.ClearPortalhit?.Invoke();

        if ((s.ThingFlags & MfNoclip) != 0 && (s.ThingFlags & MfSkullfly) == 0)
            return true;

        s.BlockingMobj = null;
        s.ThingBlocker = null;
        if (s.HasPlayer)
            s.ThingHeight = realHeight + s.MaxStepHeight;

        s.Tm.StepThing = null;

        if ((s.ThingFlags2 & Mf2Thruactors) == 0)
        {
            foreach (var tcres in s.IterateThings?.Invoke() ?? Array.Empty<ThingIteratorResult>())
            {
                if (s.PitCheckThing?.Invoke(tcres) != true)
                {
                    var blocking = s.BlockingMobj;
                    if (blocking == null || (s.LevelCompatFlags & CompatfNoPassmobj) != 0 ||
                        (tcres.PortalFlags & FfcfRestrictedportal) != 0)
                    {
                        s.ThingHeight = realHeight;
                        return false;
                    }

                    if (blocking is BlockingThingProbe bt && !bt.IsPlayer &&
                        (s.ThingFlags & (MfFloat | MfMissile | MfSkullfly)) == 0 &&
                        bt.Top - s.ThingZ <= s.MaxStepHeight)
                    {
                        if (s.ThingBlocker == null || bt.Z > ((BlockingThingProbe)s.ThingBlocker).Z)
                            s.ThingBlocker = blocking;
                        s.BlockingMobj = null;
                    }
                    else if (s.HasPlayer && blocking is BlockingThingProbe btp &&
                             s.ThingTop - btp.Z <= s.MaxStepHeight)
                    {
                        if (s.ThingBlocker != null)
                        {
                            s.ThingHeight = realHeight;
                            return false;
                        }

                        s.BlockingMobj = null;
                    }
                    else
                    {
                        s.ThingHeight = realHeight;
                        return false;
                    }
                }
            }
        }

        s.BumpValidCount?.Invoke(1);
        s.ClearSpechit?.Invoke();
        s.ClearPortalhit?.Invoke();
        s.BlockingMobj = null;
        s.ThingHeight = realHeight;

        if (s.ActorsOnly || (s.ThingFlags & MfNoclip) != 0)
            return s.ThingBlocker == null;

        var thingdropoffz = s.Tm.FloorZ;
        s.Tm.FloorZ = s.Tm.DropoffZ;
        var good = true;
        foreach (var lcres in s.IterateLines?.Invoke() ?? Array.Empty<LineIteratorResult>())
        {
            var thisresult = s.PitCheckLine?.Invoke(lcres, good) == true;
            good &= thisresult;
            if (thisresult && lcres.PassablePortal && !lcres.LinkedPortal &&
                s.PitCheckPortal?.Invoke(lcres) == true)
                s.BlockingLine = lcres.Line;
        }

        if (!good || s.Tm.CeilingZ - s.Tm.FloorZ < realHeight)
            return false;

        if (s.Tm.TouchMidtex)
            s.Tm.DropoffZ = s.Tm.FloorZ;
        else if (s.Tm.StepThing != null)
            s.Tm.DropoffZ = thingdropoffz;

        s.BlockingMobj = s.ThingBlocker;
        return s.ThingBlocker == null;
    }

    public static bool ShouldDealDamage(int damage, int flags6, int flags7) =>
        damage > 0 || (flags6 & 1) != 0 || (flags7 & 1) != 0;

    public static int HealFromNegativeDamage(int damage) => -damage;
}
