namespace HCDE.Playsim;

public static class VmP3DFloorSupport
{
    public const int FfExists = 0x1;
    public const int FfSolid = 0x2;
    public const int FfDynamic = 0x800000;
    public const int FfClipped = 0x1000000;
    public const double EqualEpsilon = 1.0 / 65536.0;

    public sealed class SecPlane
    {
        public Func<double, double, double> ZAtPoint { get; set; } = (_, _) => 0;
        public double CoeffC { get; set; } = 1;

        public double FC() => CoeffC;

        public void FlipVert() => CoeffC = -CoeffC;

        public void CopyFrom(SecPlane other)
        {
            ZAtPoint = other.ZAtPoint;
            CoeffC = other.CoeffC;
        }
    }

    public sealed class FFloor
    {
        public int Flags { get; set; }
        public SecPlane Top { get; } = new();
        public SecPlane Bottom { get; } = new();
        public object? BottomTexture { get; set; }
        public object? TopTexture { get; set; }
        public SectorTerrainModel? Model { get; set; }
        public int LastLight { get; set; }
        public int Alpha { get; set; }

        public object? Colormap { get; set; }
    }

    public sealed class SectorTerrainModel
    {
        public Func<FFloor, int, int>? GetTerrain { get; set; }
    }

    public sealed class LightListEntry
    {
        public SecPlane Plane { get; } = new();
        public int? LightLevelRef { get; set; }
        public object? ExtraColormap { get; set; }
        public int Blend { get; set; }
        public int Flags { get; set; }
        public FFloor? LightSource { get; set; }
        public FFloor? Caster { get; set; }
    }

    public sealed class SectorXFloor
    {
        public List<FFloor> FFloors { get; } = new();
        public List<SectorState> Attached { get; } = new();
        public List<LightListEntry> LightList { get; } = new();
    }

    public sealed class SectorState
    {
        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public SecPlane FloorPlane { get; } = new();
        public SecPlane CeilingPlane { get; } = new();
        public object? Colormap { get; set; }
        public SectorXFloor XFloor { get; } = new();
    }

    public sealed class LevelState
    {
        public List<SectorState> Sectors { get; } = new();
    }

    public sealed class LineDef
    {
        public SectorState? FrontSector { get; set; }
        public SectorState? BackSector { get; set; }
    }

    public sealed class LineOpeningState
    {
        public double Top { get; set; }
        public double Bottom { get; set; }
        public double LowFloor { get; set; }
        public int LowFloorThroughPortal { get; set; }
        public object? TopSec { get; set; }
        public object? BottomSec { get; set; }
        public object? CeilingPic { get; set; }
        public object? FloorPic { get; set; }
        public int FloorTerrain { get; set; } = -1;
        public SecPlane FrontFloorPlane { get; } = new();
        public SecPlane BackFloorPlane { get; } = new();
        public FFloor? TopFFloor { get; set; }
        public FFloor? BottomFFloor { get; set; }
    }

    public sealed class ThingActor
    {
        public double Z { get; set; }
        public double Height { get; set; } = 56;
        public double MaxStepHeight { get; set; } = 24;

    }

    public static void ClearDynamic3DFloorData(LevelState level, Action<FFloor>? destroyFloor = null)
    {
        foreach (var sec in level.Sectors)
        {
            var ffloors = sec.XFloor.FFloors;
            for (var i = 0; i < ffloors.Count; i++)
            {
                var rover = ffloors[i];
                if ((rover.Flags & FfDynamic) != 0)
                {
                    destroyFloor?.Invoke(rover);
                    ffloors.RemoveAt(i);
                    i--;
                    continue;
                }

                if ((rover.Flags & FfClipped) != 0)
                {
                    rover.Flags &= ~FfClipped;
                    rover.Flags |= FfExists;
                }
            }
        }
    }

    public static void RecalculateAttached3DFloors(SectorState sec, Action<SectorState> recalculate3DFloors)
    {
        var x = sec.XFloor;
        for (var i = 0; i < x.Attached.Count; i++)
            recalculate3DFloors(x.Attached[i]);
        recalculate3DFloors(sec);
    }

    public static void RecalculateLights(SectorState sector)
    {
        var lightlist = sector.XFloor.LightList;
        for (var i = 0; i < lightlist.Count; i++)
        {
            var ll = lightlist[i];
            if (ll.LightSource is not null)
            {
                ll.ExtraColormap = ll.LightSource.Colormap;
                ll.Blend = ll.LightSource.Alpha;
            }
            else
            {
                ll.ExtraColormap = sector.Colormap;
                ll.Blend = 0;
            }
        }
    }

    public static void RecalculateAttachedLights(SectorState sector)
    {
        var x = sector.XFloor;
        for (var i = 0; i < x.Attached.Count; i++)
            RecalculateLights(x.Attached[i]);
        RecalculateLights(sector);
    }

    public static LightListEntry? GetPlaneLight(SectorState sector, SecPlane plane, bool underside)
    {
        var lightlist = sector.XFloor.LightList;
        if (lightlist.Count == 0)
            return null;

        var planeHeight = plane.ZAtPoint(sector.CenterX, sector.CenterY);
        if (underside)
            planeHeight -= EqualEpsilon;

        for (var i = 1; i < lightlist.Count; i++)
        {
            if (lightlist[i].Plane.ZAtPoint(sector.CenterX, sector.CenterY) <= planeHeight)
                return lightlist[i - 1];
        }

        return lightlist[^1];
    }

    public static void LineOpeningXFloors(
        LineOpeningState open,
        ThingActor? thing,
        LineDef linedef,
        double x,
        double y,
        bool restrict)
    {
        if (thing is null)
            return;

        var thingBot = thing.Z;
        var thingTop = thing.Z + thing.Height;

        var xf = new[] { linedef.FrontSector!.XFloor, linedef.BackSector!.XFloor };

        if (xf[0].FFloors.Count == 0 && xf[1].FFloors.Count == 0)
            return;

        var lowestCeiling = open.Top;
        var highestFloor = open.Bottom;
        var lowestFloor = new[] { linedef.FrontSector!.FloorPlane.ZAtPoint(x, y), linedef.BackSector!.FloorPlane.ZAtPoint(x, y) };
        var lowestFloorSet = new[] { false, false };
        object? highestFloorPic = null;
        var highestFloorTerrain = -1;
        object? lowestCeilingPic = null;
        SectorState? lowestCeilingSec = null;
        SectorState? highestFloorSec = null;
        SecPlane?[] highestFloorPlanes = { open.FrontFloorPlane, open.BackFloorPlane };
        FFloor? lowestCeilingFFloor = null;
        FFloor? highestFloorFFloor = null;

        for (var j = 0; j < 2; j++)
        {
            var sector = j == 0 ? linedef.FrontSector! : linedef.BackSector!;
            for (var i = 0; i < xf[j].FFloors.Count; i++)
            {
                var rover = xf[j].FFloors[i];
                if ((rover.Flags & FfExists) == 0)
                    continue;
                if ((rover.Flags & FfSolid) == 0)
                    continue;

                var ffBottom = rover.Bottom.ZAtPoint(x, y);
                var ffTop = rover.Top.ZAtPoint(x, y);

                var delta1 = Math.Abs(thingBot - ((ffBottom + ffTop) / 2));
                var delta2 = Math.Abs(thingTop - ((ffBottom + ffTop) / 2));

                if (ffBottom < lowestCeiling && delta1 > delta2)
                {
                    lowestCeiling = ffBottom;
                    lowestCeilingPic = rover.BottomTexture;
                    lowestCeilingSec = sector;
                    lowestCeilingFFloor = rover;
                }

                if (delta1 <= delta2 && (!restrict || thing.Z >= ffTop))
                {
                    if (ffTop > highestFloor)
                    {
                        highestFloor = ffTop;
                        highestFloorPic = rover.TopTexture;
                        highestFloorTerrain = rover.Model?.GetTerrain?.Invoke(rover, 1) ?? -1;
                        highestFloorSec = sector;
                        highestFloorFFloor = rover;
                    }

                    if (ffTop > highestFloorPlanes[j]!.ZAtPoint(x, y))
                        highestFloorPlanes[j] = rover.Top;
                }

                if (ffTop > lowestFloor[j] && ffTop <= thing.Z + thing.MaxStepHeight)
                {
                    lowestFloor[j] = ffTop;
                    lowestFloorSet[j] = true;
                }
            }
        }

        if (highestFloor > open.Bottom)
        {
            open.Bottom = highestFloor;
            open.FloorPic = highestFloorPic;
            open.FloorTerrain = highestFloorTerrain;
            open.BottomSec = highestFloorSec;
            open.BottomFFloor = highestFloorFFloor;
        }

        if (!ReferenceEquals(highestFloorPlanes[0], open.FrontFloorPlane))
        {
            open.FrontFloorPlane.CopyFrom(highestFloorPlanes[0]!);
            if (open.FrontFloorPlane.FC() < 0)
                open.FrontFloorPlane.FlipVert();
        }

        if (!ReferenceEquals(highestFloorPlanes[1], open.BackFloorPlane))
        {
            open.BackFloorPlane.CopyFrom(highestFloorPlanes[1]!);
            if (open.BackFloorPlane.FC() < 0)
                open.BackFloorPlane.FlipVert();
        }

        if (lowestCeiling < open.Top)
        {
            open.Top = lowestCeiling;
            open.CeilingPic = lowestCeilingPic;
            open.TopSec = lowestCeilingSec;
            open.TopFFloor = lowestCeilingFFloor;
        }

        if ((open.LowFloorThroughPortal & 1) != 0 && lowestFloorSet[0])
            open.LowFloorThroughPortal &= ~1;
        if ((open.LowFloorThroughPortal & 2) != 0 && lowestFloorSet[1])
            open.LowFloorThroughPortal &= ~2;

        var low1 = (open.LowFloorThroughPortal & 1) != 0 ? open.LowFloor : lowestFloor[0];
        var low2 = (open.LowFloorThroughPortal & 2) != 0 ? open.LowFloor : lowestFloor[1];
        open.LowFloor = Math.Min(low1, low2);
    }

    public static SecPlane FindFloorPlane(SectorState? sector, double posX, double posY, double posZ)
    {
        var retplane = new SecPlane();
        if (sector is null)
            return retplane;

        retplane.CopyFrom(sector.FloorPlane);

        foreach (var rover in sector.XFloor.FFloors)
        {
            if ((rover.Flags & FfSolid) == 0 || (rover.Flags & FfExists) == 0)
                continue;

            if (rover.Top.ZAtPoint(posX, posY) == posZ)
            {
                retplane.CopyFrom(rover.Top);
                if (retplane.FC() < 0)
                    retplane.FlipVert();
                break;
            }
        }

        return retplane;
    }

    public static int Find3DFloor(SectorState? sec, double posX, double posY, double posZ, bool above, bool floor, out double cmpZ)
    {
        cmpZ = 0;
        if (sec is null)
            return -1;

        cmpZ = sec.CeilingPlane.ZAtPoint(posX, posY);
        if (posZ >= cmpZ)
            return -1;

        cmpZ = sec.FloorPlane.ZAtPoint(posX, posY);
        if (posZ <= cmpZ)
            return -1;

        for (var i = 0; i < sec.XFloor.FFloors.Count; ++i)
        {
            var rover = sec.XFloor.FFloors[i];
            if ((rover.Flags & FfSolid) == 0 || (rover.Flags & FfExists) == 0)
                continue;

            if (above)
            {
                if (floor && posZ >= (cmpZ = rover.Top.ZAtPoint(posX, posY)))
                    return i - 1;
                if (posZ >= (cmpZ = rover.Bottom.ZAtPoint(posX, posY)))
                    return i - 1;
            }
            else
            {
                if (!floor && posZ <= (cmpZ = rover.Bottom.ZAtPoint(posX, posY)))
                    return i;
                if (posZ <= (cmpZ = rover.Top.ZAtPoint(posX, posY)))
                    return i;
            }
        }

        return -1;
    }
}
