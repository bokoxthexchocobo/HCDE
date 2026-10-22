namespace HCDE.Playsim;

/// <summary>3D-floor colormap, add, and hit checks from <c>p_3dfloors.cpp</c> (48–280).</summary>
public static class VmP3DFloorColormapAdd
{
    public const int FfExists = 0x1;
    public const int FfSolid = 0x2;
    public const int FfThisinside = 0x40000000;
    public const int FfThinFloor = 0x80000;
    public const int FfFix = 0x200000;
    public const int FfSwimmable = 0x10;
    public const int FfInvertSector = 0x400000;
    public const int FfBothplanes = 0x200;
    public const int FfRenderplanes = 0x8;
    public const int FfNoshade = 0x20;
    public const int FfSeethrough = 0x2000000;
    public const int FfShootthrough = 0x4000000;
    public const int FfTranslucent = 0x800;
    public const int FfAdditivetrans = 0x10000000;
    public const int FfFadewalls = 0x8000000;
    public const int FfFog = 0x1000;
    public const int FfNodamage = 0x100000;
    public const int SecspacHitFloor = 1;
    public const int SecspacHitCeiling = 2;
    public const double EqualEpsilon = 1.0 / 65536.0;

    public sealed class Colormap
    {
        public int LightColor { get; set; }
        public int FadeColor { get; set; }
        public int Desaturation { get; set; }
        public int BlendFactor { get; set; }
        public int FogDensity { get; set; }

        public bool FadeIsBlack() => FadeColor == 0;
    }

    public sealed class SecPlane
    {
        public bool IsSlope { get; set; }
        public Func<double, double, double> ZAtPoint { get; set; } = (_, _) => 0;
    }

    public sealed class FloorSide
    {
        public SecPlane Plane { get; set; } = null!;
        public object? Texture { get; set; }
        public int Isceiling { get; set; }
        public MapSector? Model { get; set; }
        public bool Copied { get; set; }
        public int Vindex { get; set; } = -1;
    }

    public sealed class FFloor
    {
        public int Flags { get; set; }
        public int Alpha { get; set; }
        public object? Master { get; set; }
        public MapSector Target { get; set; } = null!;
        public MapSector Model { get; set; } = null!;
        public FloorSide Top { get; } = new();
        public FloorSide Bottom { get; } = new();
        public int? TopLightLevelRef { get; set; }
    }

    public sealed class SectorXFloor
    {
        public List<FFloor> FFloors { get; } = new();
        public List<MapSector> Attached { get; } = new();
    }

    public sealed class MapSector
    {
        public SectorXFloor XFloor { get; } = new();
        public SecPlane FloorPlane { get; } = new();
        public SecPlane CeilingPlane { get; } = new();
        public int LightLevel { get; set; }
        public Colormap Colormap { get; } = new();
        public PlaneSlot[] Planes { get; } = { new(), new() };
        public object? SecActTarget { get; set; }
        public int[] TerrainNum { get; } = { -1, -1 };

        public int GetTexture(int pos) => Planes[pos].TextureIndex;

        public int GetTerrain(int pos) =>
            TerrainNum[pos] >= 0 ? TerrainNum[pos] : TerrainTypeTable[GetTexture(pos)];
    }

    public static int[] TerrainTypeTable { get; set; } = Array.Empty<int>();

    public sealed class PlaneSlot
    {
        public object? Texture { get; set; }
        public int TextureIndex { get; set; }
    }

    public sealed class MapActor
    {
        public MapSector? Sector { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double Top => Z + Height;
        public double Height { get; set; } = 41;
        public MapSector? BlockingFloor { get; set; }
        public MapSector? BlockingCeiling { get; set; }
        public MapSector? Blocking3DFloor { get; set; }
        public PlayerState? Player { get; set; }
    }

    public sealed class PlayerState
    {
        public int Cheats { get; set; }
    }

    public const int CfPredicting = 1;

    public static Colormap GetColormap(FFloor floor)
    {
        if ((floor.Target.Colormap.FadeIsBlack() && floor.Model.Colormap.FadeIsBlack())
            || (floor.Flags & (FfFadewalls | FfFog)) != 0)
        {
            return floor.Model.Colormap;
        }

        return new Colormap
        {
            LightColor = floor.Model.Colormap.LightColor,
            FadeColor = floor.Target.Colormap.FadeColor,
            Desaturation = floor.Model.Colormap.Desaturation,
            BlendFactor = floor.Model.Colormap.BlendFactor,
            FogDensity = floor.Target.Colormap.FogDensity,
        };
    }

    public static int GetBlend(FFloor floor)
    {
        if ((floor.Flags & FfFadewalls) == 0 && floor.Target.Colormap.FadeColor != floor.Model.Colormap.FadeColor)
            return floor.Model.Colormap.FadeColor;
        return 0;
    }

    public static void UpdateColormap(FFloor floor, Colormap map)
    {
        if ((floor.Target.Colormap.FadeIsBlack() && floor.Model.Colormap.FadeIsBlack())
            || (floor.Flags & FfFadewalls) != 0
            || floor.Target.Colormap.FadeColor == floor.Model.Colormap.FadeColor)
        {
            CopyColormap(map, floor.Model.Colormap);
        }
        else
        {
            map.LightColor = floor.Model.Colormap.LightColor;
            map.FadeColor = floor.Target.Colormap.FadeColor;
            map.Desaturation = floor.Model.Colormap.Desaturation;
            map.BlendFactor = floor.Model.Colormap.BlendFactor;
            map.FogDensity = floor.Target.Colormap.FogDensity;
        }
    }

    static void CopyColormap(Colormap dst, Colormap src)
    {
        dst.LightColor = src.LightColor;
        dst.FadeColor = src.FadeColor;
        dst.Desaturation = src.Desaturation;
        dst.BlendFactor = src.BlendFactor;
        dst.FogDensity = src.FogDensity;
    }

    public static void Add3DFloor(
        MapSector sec,
        MapSector sec2,
        object? master,
        int flags,
        int alpha,
        Action<MapSector, MapSector, object?, int, int>? recursiveAdd = null)
    {
        if ((flags & FfThisinside) == 0)
        {
            foreach (var attached in sec2.XFloor.Attached)
            {
                if (attached == sec)
                    return;
            }

            sec2.XFloor.Attached.Add(sec);
        }

        var ffloor = new FFloor
        {
            Target = sec,
            Model = sec2,
            Master = master,
        };
        ffloor.Top.Model = ffloor.Bottom.Model = sec2;

        if ((flags & FfThinFloor) == 0)
        {
            ffloor.Bottom.Plane = sec2.FloorPlane;
            ffloor.Bottom.Texture = sec2.Planes[0].Texture;
            ffloor.Bottom.Isceiling = 0;
        }
        else
        {
            ffloor.Bottom.Plane = sec2.CeilingPlane;
            ffloor.Bottom.Texture = sec2.Planes[1].Texture;
            ffloor.Bottom.Isceiling = 1;
        }

        if ((flags & FfFix) == 0)
        {
            ffloor.Top.Plane = sec2.CeilingPlane;
            ffloor.Top.Texture = sec2.Planes[1].Texture;
            ffloor.TopLightLevelRef = sec2.LightLevel;
            ffloor.Top.Isceiling = 1;
        }
        else
        {
            ffloor.Top.Plane = sec.FloorPlane;
            ffloor.Top.Texture = sec2.Planes[0].Texture;
            ffloor.TopLightLevelRef = sec.LightLevel;
            ffloor.Top.Isceiling = 0;
            ffloor.Top.Model = sec;
        }

        if ((flags & FfInvertSector) != 0)
        {
            SwapSides(ffloor);
            if ((flags & FfSwimmable) != 0)
            {
                ffloor.Bottom.Plane = sec.FloorPlane;
                ffloor.Bottom.Model = sec;
                ffloor.Bottom.Isceiling = 0;
            }
        }

        ffloor.Flags = flags;
        ffloor.Alpha = Math.Clamp(alpha, 0, 255);

        if (ffloor.Top.Plane.IsSlope || ffloor.Bottom.Plane.IsSlope)
        {
            ffloor.Alpha = 255;
            ffloor.Flags &= ~FfAdditivetrans;
        }

        if ((flags & FfThisinside) != 0)
            SwapSides(ffloor);

        sec.XFloor.FFloors.Add(ffloor);

        if ((flags & FfBothplanes) != 0)
        {
            var add = recursiveAdd ?? ((a, b, c, d, e) => Add3DFloor(a, b, c, d, e));
            add(
                sec,
                sec2,
                master,
                FfExists | FfThisinside | FfRenderplanes | FfNoshade | FfSeethrough | FfShootthrough
                    | (flags & (FfInvertSector | FfTranslucent | FfAdditivetrans)),
                alpha);
        }
    }

    public static void ActorOnSpecial3DFloor(
        MapActor victim,
        Func<MapActor, MapSector, bool>? checkForSpecialSector = null,
        Action<MapActor, MapSector, FFloor?>? actorInSpecialSector = null,
        Action<MapActor, int>? actorOnSpecialFlat = null)
    {
        foreach (var rover in victim.Sector!.XFloor.FFloors)
        {
            if ((rover.Flags & FfExists) == 0)
                continue;
            if ((rover.Flags & FfFix) != 0)
                continue;
            if (checkForSpecialSector?.Invoke(victim, rover.Model) != true)
                continue;

            if ((rover.Flags & FfSolid) != 0)
            {
                if (victim.Z != rover.Top.Plane.ZAtPoint(victim.X, victim.Y))
                    continue;
            }
            else if ((rover.Flags & FfNodamage) != 0
                     || victim.Z > rover.Top.Plane.ZAtPoint(victim.X, victim.Y)
                     || victim.Top < rover.Bottom.Plane.ZAtPoint(victim.X, victim.Y))
            {
                continue;
            }

            actorInSpecialSector?.Invoke(victim, rover.Model, rover);
            actorOnSpecialFlat?.Invoke(victim, rover.Model.GetTerrain(rover.Top.Isceiling));
            break;
        }
    }

    static void SwapSides(FFloor ffloor)
    {
        var tmpPlane = ffloor.Top.Plane;
        var tmpTex = ffloor.Top.Texture;
        var tmpIceil = ffloor.Top.Isceiling;
        var tmpModel = ffloor.Top.Model;
        var tmpCopied = ffloor.Top.Copied;
        var tmpVindex = ffloor.Top.Vindex;

        ffloor.Top.Plane = ffloor.Bottom.Plane;
        ffloor.Top.Texture = ffloor.Bottom.Texture;
        ffloor.Top.Isceiling = ffloor.Bottom.Isceiling;
        ffloor.Top.Model = ffloor.Bottom.Model;
        ffloor.Top.Copied = ffloor.Bottom.Copied;
        ffloor.Top.Vindex = ffloor.Bottom.Vindex;

        ffloor.Bottom.Plane = tmpPlane;
        ffloor.Bottom.Texture = tmpTex;
        ffloor.Bottom.Isceiling = tmpIceil;
        ffloor.Bottom.Model = tmpModel;
        ffloor.Bottom.Copied = tmpCopied;
        ffloor.Bottom.Vindex = tmpVindex;
    }

    public static bool CheckFor3DFloorHit(
        MapActor mo,
        double z,
        bool trigger,
        Action<MapActor, int>? triggerSectorActions = null)
    {
        if (mo.Player is not null && (mo.Player.Cheats & CfPredicting) != 0)
            return false;

        foreach (var rover in mo.Sector!.XFloor.FFloors)
        {
            if ((rover.Flags & FfExists) == 0)
                continue;
            if ((rover.Flags & FfSolid) != 0 && rover.Model.SecActTarget is not null)
            {
                if (Math.Abs(z - rover.Top.Plane.ZAtPoint(mo.X, mo.Y)) < EqualEpsilon)
                {
                    mo.BlockingFloor = rover.Model;
                    mo.Blocking3DFloor = rover.Model;
                    if (trigger)
                        triggerSectorActions?.Invoke(mo, SecspacHitFloor);
                    return true;
                }
            }
        }

        return false;
    }

    public static bool CheckFor3DCeilingHit(
        MapActor mo,
        double z,
        bool trigger,
        Action<MapActor, int>? triggerSectorActions = null)
    {
        if (mo.Player is not null && (mo.Player.Cheats & CfPredicting) != 0)
            return false;

        foreach (var rover in mo.Sector!.XFloor.FFloors)
        {
            if ((rover.Flags & FfExists) == 0)
                continue;
            if ((rover.Flags & FfSolid) != 0 && rover.Model.SecActTarget is not null)
            {
                if (Math.Abs(z - rover.Bottom.Plane.ZAtPoint(mo.X, mo.Y)) < EqualEpsilon)
                {
                    mo.BlockingCeiling = rover.Model;
                    mo.Blocking3DFloor = rover.Model;
                    if (trigger)
                        triggerSectorActions?.Invoke(mo, SecspacHitCeiling);
                    return true;
                }
            }
        }

        return false;
    }
}
