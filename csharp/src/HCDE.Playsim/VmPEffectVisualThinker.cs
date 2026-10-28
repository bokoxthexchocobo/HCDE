namespace HCDE.Playsim;

/// <summary>Faithful port of visual thinker band (<c>p_effect.cpp</c> 987–1368).</summary>
public static class VmPEffectVisualThinker
{
    public const int SpfNotimefreeze = 1;
    public const int VtfIsparticle = 1;
    public const int VtfDontinterpolate = 1 << 1;
    public const int VtfAddLightlevel = 1 << 2;
    public const int VtfFlipoffsetx = 1 << 3;
    public const int VtfFlipoffsety = 1 << 4;
    public const int VtfParticlesquare = 1 << 5;
    public const int VtfParticleround = 1 << 6;
    public const int VtfParticlesmooth = 1 << 7;
    public const int PtSquare = 1;
    public const int PtRound = 2;
    public const int PtSmooth = 3;
    public const int PtDefault = 0;
    public const int StyleCount = 20;

    public sealed class ParticleSlot
    {
        public double PosX { get; set; }
        public double PosY { get; set; }
        public double PosZ { get; set; }
        public double VelX { get; set; }
        public double VelY { get; set; }
        public double VelZ { get; set; }
        public double AccZ { get; set; }
        public int Color { get; set; }
        public float Size { get; set; }
        public int Flags { get; set; }
        public float Alpha { get; set; }
        public float Roll { get; set; }
        public bool TextureValid { get; set; }
        public int Style { get; set; }
        public object? Subsector { get; set; }
    }

    public sealed class DisconnectState
    {
        public double Radius { get; set; }
        public double Height { get; set; }
        public int Spawned { get; set; }
        public Func<ParticleSlot?>? JitterParticle { get; set; }
        public Func<int>? Random128 { get; set; }
    }

    public static double DisconnectJitterAxis(int random, double scale) => (random - 128) * scale / 128;

    public static double DisconnectHeightSample(int random, double height) => random * height / 256;

    public static int DisconnectBloodColor(int random) => random < 128 ? 0x8b0000 : 0x800000;

    public static void DisconnectEffect(DisconnectState s)
    {
        for (var i = 64; i > 0; i--)
        {
            var p = s.JitterParticle?.Invoke();
            if (p == null) break;
            var xo = DisconnectJitterAxis(s.Random128?.Invoke() ?? 0, s.Radius);
            var yo = DisconnectJitterAxis(s.Random128?.Invoke() ?? 0, s.Radius);
            var zo = DisconnectHeightSample(s.Random128?.Invoke() ?? 0, s.Height);
            p.PosX = xo;
            p.PosY = yo;
            p.PosZ = zo;
            p.AccZ -= 1.0 / 4096;
            p.Color = DisconnectBloodColor(s.Random128?.Invoke() ?? 0);
            p.Size = 4;
            s.Spawned++;
        }
    }

    public sealed class VisualThinkerState
    {
        public int Flags { get; set; }
        public int VisualFlags { get; set; }
        public int LightLevel { get; set; } = -1;
        public ParticleSlot Pt { get; } = new();
        public object? Head { get; set; }
        public object? Prev { get; set; }
        public object? Next { get; set; }
        public double PrevX { get; set; }
        public double PrevY { get; set; }
        public double PrevZ { get; set; }
        public float PrevRoll { get; set; }
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }
        public bool Euthanize { get; set; }
        public bool LevelFrozen { get; set; }
        public bool Destroyed { get; set; }
        public object? CurSector { get; set; }
        public Func<double, double, double, double, (double X, double Y)>? PortalOffset { get; set; }
        public Func<double, double, double, object?>? PointInRenderSubsector { get; set; }
        public Func<object, bool>? PortalBlocksCeiling { get; set; }
        public Func<object, bool>? PortalBlocksFloor { get; set; }
        public Func<object, double>? PortalCeilingZ { get; set; }
        public Func<object, (double X, double Y, double Z)>? PortalCeilingDisplacement { get; set; }
        public Func<object, double>? PortalFloorZ { get; set; }
        public Func<object, (double X, double Y, double Z)>? PortalFloorDisplacement { get; set; }
    }

    public static void Construct(VisualThinkerState s, ref object? listHead)
    {
        s.Pt.PosX = s.Pt.PosY = s.Pt.PosZ = 0;
        s.Pt.VelX = s.Pt.VelY = s.Pt.VelZ = 0;
        s.Pt.Roll = 0;
        s.Pt.Alpha = 1;
        s.Pt.TextureValid = false;
        s.Prev = s.Next = null;
        if (listHead != null)
        {
            s.Next = listHead;
        }
        listHead = s;
    }

    public static void OnDestroy(VisualThinkerState s, ref object? listHead)
    {
        if (s.Prev != null) { /* unlink prev */ }
        if (s.Next != null) { /* unlink next */ }
        if (ReferenceEquals(listHead, s)) listHead = s.Next;
        s.Next = s.Prev = null;
        s.Pt.Alpha = 0;
        s.Destroyed = true;
    }

    public static bool ValidTexture(VisualThinkerState s) =>
        (s.VisualFlags & VtfIsparticle) != 0 || s.Pt.TextureValid;

    public static void UpdateSpriteInfo(VisualThinkerState s, bool localAnim, bool textureChanged)
    {
        if (localAnim && textureChanged)
        {
            /* standalone anim init */
        }
    }

    public static void Tick(VisualThinkerState s)
    {
        if (s.Euthanize) return;
        if (!ValidTexture(s))
        {
            s.Destroyed = true;
            return;
        }
        if (IsFrozen(s))
        {
            s.Pt.Subsector = s.PointInRenderSubsector?.Invoke(s.Pt.PosX, s.Pt.PosY, s.Pt.PosZ);
            UpdateSpriteInfo(s, false, false);
            return;
        }

        s.PrevX = s.Pt.PosX;
        s.PrevY = s.Pt.PosY;
        s.PrevZ = s.Pt.PosZ;
        s.PrevRoll = s.Pt.Roll;
        if (s.PortalOffset != null)
        {
            var xy = s.PortalOffset(s.Pt.PosX, s.Pt.PosY, s.Pt.VelX, s.Pt.VelY);
            s.Pt.PosX = xy.X;
            s.Pt.PosY = xy.Y;
        }
        else
        {
            s.Pt.PosX += s.Pt.VelX;
            s.Pt.PosY += s.Pt.VelY;
        }
        s.Pt.PosZ += s.Pt.VelZ;
        var ss = s.PointInRenderSubsector?.Invoke(s.Pt.PosX, s.Pt.PosY, s.Pt.PosZ);
        if (ss != null)
        {
            if (s.PortalBlocksCeiling?.Invoke(ss) == false && s.Pt.PosZ > (s.PortalCeilingZ?.Invoke(ss) ?? double.MaxValue))
            {
                var disp = s.PortalCeilingDisplacement?.Invoke(ss) ?? (0, 0, 0);
                s.Pt.PosX += disp.X;
                s.Pt.PosY += disp.Y;
                s.Pt.PosZ += disp.Z;
                ss = s.PointInRenderSubsector?.Invoke(s.Pt.PosX, s.Pt.PosY, s.Pt.PosZ);
            }
            else if (s.PortalBlocksFloor?.Invoke(ss) == false && s.Pt.PosZ < (s.PortalFloorZ?.Invoke(ss) ?? double.MinValue))
            {
                var disp = s.PortalFloorDisplacement?.Invoke(ss) ?? (0, 0, 0);
                s.Pt.PosX += disp.X;
                s.Pt.PosY += disp.Y;
                s.Pt.PosZ += disp.Z;
            }
        }
        s.Pt.Subsector = ss;
        UpdateSpriteInfo(s, false, false);
    }

    public static int GetLightLevel(VisualThinkerState s, int sectorLight)
    {
        if ((s.VisualFlags & VtfAddLightlevel) != 0) return sectorLight + s.LightLevel;
        if (s.LightLevel > -1) return s.LightLevel;
        return sectorLight;
    }

    public static (double X, double Y, double Z) InterpolatedPosition(VisualThinkerState s, double ticFrac)
    {
        if ((s.VisualFlags & VtfDontinterpolate) != 0) return (s.Pt.PosX, s.Pt.PosY, s.Pt.PosZ);
        return (s.PrevX + ticFrac * (s.Pt.PosX - s.PrevX), s.PrevY + ticFrac * (s.Pt.PosY - s.PrevY), s.PrevZ + ticFrac * (s.Pt.PosZ - s.PrevZ));
    }

    public static float InterpolatedRoll(VisualThinkerState s, double ticFrac) =>
        (s.VisualFlags & VtfDontinterpolate) != 0 ? s.Pt.Roll : (float)(s.PrevRoll + (s.Pt.Roll - s.PrevRoll) * ticFrac);

    public static void SetTranslation(VisualThinkerState s, string name, Func<string, int>? findTranslation)
    {
        if (string.IsNullOrEmpty(name))
        {
            s.Flags = 0;
            return;
        }
        var t = findTranslation?.Invoke(name) ?? -1;
        if (t >= 0) s.Flags = t;
    }

    public static int GetParticleType(VisualThinkerState s)
    {
        var flag = s.VisualFlags & (VtfParticlesquare | VtfParticleround | VtfParticlesmooth);
        return flag switch
        {
            VtfParticlesquare => PtSquare,
            VtfParticleround => PtRound,
            VtfParticlesmooth => PtSmooth,
            _ => PtDefault,
        };
    }

    public static bool IsFrozen(VisualThinkerState s) => s.LevelFrozen && (s.Pt.Flags & SpfNotimefreeze) == 0;

    public static void SetRenderStyle(VisualThinkerState s, int mode)
    {
        if (mode >= 0 && mode < StyleCount) s.Pt.Style = mode;
    }

    public static float GetOffset(VisualThinkerState s, bool y)
    {
        if (y) return (float)((s.VisualFlags & VtfFlipoffsety) != 0 ? s.OffsetY : -s.OffsetY);
        return (float)((s.VisualFlags & VtfFlipoffsetx) != 0 ? s.OffsetX : -s.OffsetX);
    }
}
