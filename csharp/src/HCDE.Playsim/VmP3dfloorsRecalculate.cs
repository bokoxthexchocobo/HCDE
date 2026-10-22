namespace HCDE.Playsim;

/// <summary>Faithful port of <c>P_Recalculate3DFloors</c> (<c>p_3dfloors.cpp</c> 291–535).</summary>
public static class VmP3dfloorsRecalculate
{
    public const int FfDynamic = 1 << 0;
    public const int FfClipped = 1 << 1;
    public const int FfExists = 1 << 2;
    public const int FfThisinside = 1 << 3;
    public const int FfSwimmable = 1 << 4;
    public const int FfTranslucent = 1 << 5;
    public const int FfRenderall = 1 << 6;
    public const int FfNoshade = 1 << 7;
    public const int FfDoubleshadow = 1 << 8;
    public const int FfReset = 1 << 9;

    public sealed class PlaneModel
    {
        public Func<double, double, double> ZAtPoint { get; set; } = (_, _) => 0;

        public void CopyFrom(PlaneModel other) => ZAtPoint = other.ZAtPoint;
    }

    public sealed class FFloorModel
    {
        public int Flags { get; set; }
        public PlaneModel Top { get; } = new();
        public PlaneModel Bottom { get; } = new();
        public int TopLightLevel { get; set; }
        public object? Colormap { get; set; }
        public int Blend { get; set; }
    }

    public sealed class LightListEntry
    {
        public PlaneModel Plane { get; } = new();
        public int LightLevel { get; set; }
        public FFloorModel? Caster { get; set; }
        public FFloorModel? LightSource { get; set; }
        public object? ExtraColormap { get; set; }
        public int Blend { get; set; }
        public int Flags { get; set; }
    }

    public sealed class SectorRecalcState
    {
        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public PlaneModel CeilingPlane { get; } = new();
        public PlaneModel FloorPlane { get; } = new();
        public int SectorLightLevel { get; set; }
        public object? SectorColormap { get; set; }
        public List<FFloorModel> FFloors { get; } = new();
        public List<LightListEntry> LightList { get; } = new();
        public Func<FFloorModel, FFloorModel> CloneFloor { get; set; } = f => new FFloorModel
        {
            Flags = f.Flags,
            TopLightLevel = f.TopLightLevel,
            Colormap = f.Colormap,
            Blend = f.Blend,
        };
    }

    static double ZAtCenter(PlaneModel plane, SectorRecalcState sector) =>
        plane.ZAtPoint(sector.CenterX, sector.CenterY);

    static void CopyFloor(FFloorModel dst, FFloorModel src)
    {
        dst.Flags = src.Flags;
        dst.TopLightLevel = src.TopLightLevel;
        dst.Colormap = src.Colormap;
        dst.Blend = src.Blend;
        dst.Top.ZAtPoint = src.Top.ZAtPoint;
        dst.Bottom.ZAtPoint = src.Bottom.ZAtPoint;
    }

    public static void Recalculate3DFloors(SectorRecalcState sector)
    {
        var ffloors = sector.FFloors;
        var lightlist = sector.LightList;

        if (ffloors.Count > 1)
        {
            var oldlist = new List<FFloorModel>(ffloors);
            ffloors.Clear();

            for (var i = 0; i < oldlist.Count; i++)
            {
                var rover = oldlist[i];
                if ((rover.Flags & FfDynamic) != 0)
                {
                    oldlist.RemoveAt(i);
                    i--;
                    continue;
                }

                if ((rover.Flags & FfClipped) != 0)
                {
                    rover.Flags &= ~FfClipped;
                    rover.Flags |= FfExists;
                }
            }

            FFloorModel? clipped = null;
            FFloorModel? solid = null;
            double solidBottom = 0;
            double clippedTop = 0;
            double clippedBottom = 0;

            while (oldlist.Count > 0)
            {
                var pick = oldlist[0];
                var height = ZAtCenter(pick.Top, sector);
                var pickIndex = 0;
                for (var j = 1; j < oldlist.Count; j++)
                {
                    var h2 = ZAtCenter(oldlist[j].Top, sector);
                    if (h2 > height)
                    {
                        pick = oldlist[j];
                        pickIndex = j;
                        height = h2;
                    }
                }

                oldlist.RemoveAt(pickIndex);
                var pickBottom = ZAtCenter(pick.Bottom, sector);

                if ((pick.Flags & FfThisinside) != 0)
                {
                    ffloors.Add(pick);
                }
                else if (((pick.Flags & (FfSwimmable | FfTranslucent)) != 0 || (pick.Flags & FfRenderall) == 0)
                         && (pick.Flags & FfExists) != 0)
                {
                    if (solid != null && solidBottom < height)
                    {
                        ffloors.Add(pick);
                        if (solidBottom < pickBottom)
                        {
                            pick.Flags |= FfClipped;
                            pick.Flags &= ~FfExists;
                        }
                        else
                        {
                            var dyn = sector.CloneFloor(pick);
                            CopyFloor(dyn, pick);
                            pick.Flags |= FfClipped;
                            pick.Flags &= ~FfExists;
                            dyn.Flags |= FfDynamic;
                            dyn.Top.CopyFrom(solid.Bottom);
                            ffloors.Add(dyn);
                            clipped = dyn;
                            clippedTop = solidBottom;
                            clippedBottom = pickBottom;
                        }
                    }
                    else if (pickBottom > height)
                    {
                        var dyn = sector.CloneFloor(pick);
                        CopyFloor(dyn, pick);
                        pick.Flags |= FfClipped;
                        pick.Flags &= ~FfExists;
                        dyn.Flags |= FfDynamic;
                        dyn.Bottom.CopyFrom(pick.Top);
                        ffloors.Add(pick);
                        ffloors.Add(dyn);
                    }
                    else
                    {
                        clipped = pick;
                        clippedTop = height;
                        clippedBottom = pickBottom;
                        ffloors.Add(pick);
                    }
                }
                else if (clipped != null && clippedBottom < height)
                {
                    var dyn = sector.CloneFloor(clipped);
                    CopyFloor(dyn, clipped);
                    clipped.Flags |= FfClipped;
                    clipped.Flags &= ~FfExists;
                    dyn.Flags |= FfDynamic;
                    dyn.Bottom.CopyFrom(pick.Top);
                    ffloors.Add(dyn);
                    ffloors.Add(pick);

                    if (pickBottom <= clippedBottom)
                        clipped = null;
                    else
                    {
                        dyn = sector.CloneFloor(clipped);
                        CopyFloor(dyn, clipped);
                        dyn.Flags |= FfDynamic | FfExists;
                        dyn.Top.CopyFrom(pick.Bottom);
                        ffloors.Add(dyn);
                        clipped = dyn;
                        clippedTop = pickBottom;
                    }

                    solid = pick;
                    solidBottom = pickBottom;
                }
                else
                {
                    clipped = null;
                    if (solid == null || solidBottom > pickBottom)
                    {
                        solid = pick;
                        solidBottom = pickBottom;
                    }

                    ffloors.Add(pick);
                }
            }
        }

        if (ffloors.Count == 0)
            return;

        lightlist.Clear();
        lightlist.Add(new LightListEntry());
        var head = lightlist[0];
        head.Plane.CopyFrom(sector.CeilingPlane);
        head.LightLevel = sector.SectorLightLevel;
        head.Caster = null;
        head.LightSource = null;
        head.ExtraColormap = sector.SectorColormap;
        head.Blend = 0;
        head.Flags = 0;

        var resetlight = new LightListEntry
        {
            LightLevel = head.LightLevel,
            LightSource = head.LightSource,
            ExtraColormap = head.ExtraColormap,
            Blend = head.Blend,
        };
        resetlight.Plane.CopyFrom(head.Plane);

        var maxheight = ZAtCenter(sector.CeilingPlane, sector);
        var minheight = ZAtCenter(sector.FloorPlane, sector);

        for (var i = 0; i < ffloors.Count; i++)
        {
            var rover = ffloors[i];
            if ((rover.Flags & FfExists) == 0 || (rover.Flags & FfNoshade) != 0)
                continue;

            var ffTop = ZAtCenter(rover.Top, sector);
            if (ffTop < minheight)
                break;
            if (ffTop < maxheight)
            {
                var newlight = new LightListEntry();
                newlight.Plane.CopyFrom(rover.Top);
                newlight.LightLevel = rover.TopLightLevel;
                newlight.Caster = rover;
                newlight.LightSource = rover;
                newlight.ExtraColormap = rover.Colormap;
                newlight.Blend = rover.Blend;
                newlight.Flags = rover.Flags;
                lightlist.Add(newlight);
            }
            else
            {
                var ffBottom = ZAtCenter(rover.Bottom, sector);
                if (ffBottom < maxheight)
                {
                    head.LightLevel = rover.TopLightLevel;
                    head.Caster = rover;
                    head.LightSource = rover;
                    head.ExtraColormap = rover.Colormap;
                    head.Blend = rover.Blend;
                    head.Flags = rover.Flags;
                }
            }

            if ((rover.Flags & (FfDoubleshadow | FfReset)) == 0)
            {
                var last = lightlist[^1];
                resetlight.LightLevel = last.LightLevel;
                resetlight.LightSource = last.LightSource;
                resetlight.ExtraColormap = last.ExtraColormap;
                resetlight.Blend = last.Blend;
                resetlight.Plane.CopyFrom(last.Plane);
            }
            else if ((rover.Flags & FfReset) != 0)
            {
                resetlight.LightLevel = sector.SectorLightLevel;
                resetlight.LightSource = null;
                resetlight.ExtraColormap = sector.SectorColormap;
                resetlight.Blend = 0;
            }

            if ((rover.Flags & FfDoubleshadow) != 0)
            {
                var ffBottom = ZAtCenter(rover.Bottom, sector);
                if (ffBottom < maxheight && ffBottom > minheight)
                {
                    var newlight = new LightListEntry
                    {
                        Caster = rover,
                        LightSource = resetlight.LightSource,
                        LightLevel = resetlight.LightLevel,
                        ExtraColormap = resetlight.ExtraColormap,
                        Blend = resetlight.Blend,
                        Flags = rover.Flags,
                    };
                    newlight.Plane.CopyFrom(rover.Bottom);
                    lightlist.Add(newlight);
                }
            }
        }
    }
}
