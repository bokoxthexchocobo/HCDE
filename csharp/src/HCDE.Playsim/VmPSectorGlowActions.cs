namespace HCDE.Playsim;

/// <summary>Force-field removal, floor clip, glow, sector actions, and overlap from <c>p_sectors.cpp</c> (1167–1362).</summary>
public static class VmPSectorGlowActions
{
    public const int ForceFieldSpecial = 33;
    public const uint MlBlocking = 0x1;
    public const uint MlBlockeverything = 0x8000000;
    public const int Mf2Floorclip = 1 << 5;
    public const int Mf4Standstill = 1 << 0;
    public const uint SecmfOverlapping = 512;
    public const int Floor = 0;
    public const int Ceiling = 1;
    public const int NullTextureId = -1;

    public sealed class MapLine
    {
        public MapSector? BackSector { get; set; }
        public int Special { get; set; }
        public uint Flags { get; set; }
        public SideDef[] Sidedef { get; } = { new(), new() };
    }

    public sealed class SideDef
    {
        public int MidTexture { get; set; }
        public void SetTexture(int side, int tex) => MidTexture = tex;
    }

    public sealed class MapSector
    {
        public List<MapLine> Lines { get; } = new();
        public PlaneSlot[] Planes { get; } = { new(), new() };
        public Plane FloorPlane { get; } = new();
        public Plane CeilingPlane { get; } = new();
        public uint MoreFlags { get; set; }
        public object? SecActTarget { get; set; }
        public int Sectornum { get; set; }

        public int GetTexture(int pos) => Planes[pos].Texture;
    }

    public sealed class Plane
    {
        public bool IsSlope { get; set; }
        public Func<double, double, double> ZAtPoint { get; set; } = (_, _) => 0;
    }

    public sealed class PlaneSlot
    {
        public int Texture { get; set; }
        public uint GlowColor { get; set; }
        public float GlowHeight { get; set; }
        public double TexZ { get; set; }
    }

    public sealed class MsecNode
    {
        public ThingActor? Thing { get; set; }
        public MsecNode? Next { get; set; }
    }

    public sealed class ThingActor
    {
        public int Flags2 { get; set; }
        public Action? AdjustFloorClip { get; set; }
    }

    public sealed class SectorActionActor
    {
        public int Flags4 { get; set; }
        public SectorActionActor? Tracer { get; set; }
        public bool Destroyed { get; set; }
    }

    public sealed class GlowTexture
    {
        public bool IsGlowing { get; set; }
        public bool IsAutoGlowing { get; set; }
        public float GlowHeight { get; set; }
        public float[] GlowColor { get; } = new float[4];

        public void GetGlowColor(float[] dest)
        {
            Array.Copy(GlowColor, dest, 4);
        }
    }

    public static void RemoveForceField(MapSector sector)
    {
        foreach (var line in sector.Lines)
        {
            if (line.BackSector is null || line.Special != ForceFieldSpecial)
                continue;
            line.Flags &= ~(MlBlocking | MlBlockeverything);
            line.Special = 0;
            line.Sidedef[0].SetTexture(1, NullTextureId);
            line.Sidedef[1].SetTexture(1, NullTextureId);
        }
    }

    public static void AdjustFloorClip(MapSector sector, MsecNode? touchingList)
    {
        for (var node = touchingList; node is not null; node = node.Next)
        {
            if (node.Thing is not null && (node.Thing.Flags2 & Mf2Floorclip) != 0)
                node.Thing.AdjustFloorClip?.Invoke();
        }
    }

    public static int CheckSpriteGlow(
        MapSector sector,
        int lightlevel,
        double posX,
        double posY,
        double posZ,
        Func<int, bool, GlowTexture?>? getGameTexture = null)
    {
        var bottomglowcolor = new float[4];
        bottomglowcolor[3] = 0;
        var c = sector.Planes[Floor].GlowColor;
        if (c == 0)
        {
            var tex = getGameTexture?.Invoke(sector.GetTexture(Floor), false);
            if (tex is not null && tex.IsGlowing)
            {
                if (!tex.IsAutoGlowing)
                    tex = getGameTexture?.Invoke(sector.GetTexture(Floor), true);
                if (tex is not null && tex.IsGlowing)
                {
                    tex.GetGlowColor(bottomglowcolor);
                    bottomglowcolor[3] = tex.GlowHeight;
                }
            }
        }
        else if (c != uint.MaxValue)
        {
            bottomglowcolor[0] = ((c >> 16) & 0xff) / 255f;
            bottomglowcolor[1] = ((c >> 8) & 0xff) / 255f;
            bottomglowcolor[2] = (c & 0xff) / 255f;
            bottomglowcolor[3] = sector.Planes[Floor].GlowHeight;
        }

        if (bottomglowcolor[3] > 0)
        {
            var floordiff = posZ - sector.FloorPlane.ZAtPoint(posX, posY);
            if (floordiff < bottomglowcolor[3])
            {
                var maxlight = (255 + lightlevel) >> 1;
                var lightfrac = floordiff / bottomglowcolor[3];
                if (lightfrac < 0)
                    lightfrac = 0;
                lightlevel = (int)(lightfrac * lightlevel + maxlight * (1 - lightfrac));
            }
        }

        return lightlevel;
    }

    public static bool GetWallGlow(
        MapSector sector,
        float[] topglowcolor,
        float[] bottomglowcolor,
        Func<int, bool, GlowTexture?>? getGameTexture = null)
    {
        var ret = false;
        bottomglowcolor[3] = topglowcolor[3] = 0;
        var c = sector.Planes[Ceiling].GlowColor;
        if (c == 0)
        {
            var tex = getGameTexture?.Invoke(sector.GetTexture(Ceiling), false);
            if (tex is not null && tex.IsGlowing)
            {
                if (!tex.IsAutoGlowing)
                    tex = getGameTexture?.Invoke(sector.GetTexture(Ceiling), true);
                if (tex is not null && tex.IsGlowing)
                {
                    ret = true;
                    tex.GetGlowColor(topglowcolor);
                    topglowcolor[3] = tex.GlowHeight;
                }
            }
        }
        else if (c != uint.MaxValue)
        {
            topglowcolor[0] = ((c >> 16) & 0xff) / 255f;
            topglowcolor[1] = ((c >> 8) & 0xff) / 255f;
            topglowcolor[2] = (c & 0xff) / 255f;
            topglowcolor[3] = sector.Planes[Ceiling].GlowHeight;
            ret = topglowcolor[3] > 0;
        }

        c = sector.Planes[Floor].GlowColor;
        if (c == 0)
        {
            var tex = getGameTexture?.Invoke(sector.GetTexture(Floor), false);
            if (tex is not null && tex.IsGlowing)
            {
                if (!tex.IsAutoGlowing)
                    tex = getGameTexture?.Invoke(sector.GetTexture(Floor), true);
                if (tex is not null && tex.IsGlowing)
                {
                    ret = true;
                    tex.GetGlowColor(bottomglowcolor);
                    bottomglowcolor[3] = tex.GlowHeight;
                }
            }
        }
        else if (c != uint.MaxValue)
        {
            bottomglowcolor[0] = ((c >> 16) & 0xff) / 255f;
            bottomglowcolor[1] = ((c >> 8) & 0xff) / 255f;
            bottomglowcolor[2] = (c & 0xff) / 255f;
            bottomglowcolor[3] = sector.Planes[Floor].GlowHeight;
            ret = bottomglowcolor[3] > 0;
        }

        return ret;
    }

    public static bool TriggerSectorActions(
        MapSector sector,
        object? thing,
        int activation,
        Func<SectorActionActor, object?, int, bool>? triggerAction = null,
        Action<int, int, bool>? netDiagTrace = null)
    {
        var act = sector.SecActTarget as SectorActionActor;
        var res = false;

        while (act is not null)
        {
            var next = act.Tracer;
            if (triggerAction?.Invoke(act, thing, activation) == true)
            {
                if ((act.Flags4 & Mf4Standstill) != 0)
                    act.Destroyed = true;
                netDiagTrace?.Invoke(sector.Sectornum, activation, true);
                res = true;
            }

            act = next;
        }

        return res;
    }

    public static void CheckOverlap(MapSector sector)
    {
        if (sector.Planes[Floor].TexZ > sector.Planes[Ceiling].TexZ
            && !sector.FloorPlane.IsSlope
            && !sector.CeilingPlane.IsSlope)
        {
            sector.MoreFlags |= SecmfOverlapping;
        }
        else
        {
            sector.MoreFlags &= ~SecmfOverlapping;
        }
    }
}
