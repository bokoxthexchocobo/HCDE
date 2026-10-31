namespace HCDE.Playsim;
/// <summary>
/// Billboard projection, water clipping, sprite lights, and the portal column
/// clip from <c>r_sprite.cpp</c> (1-298). Chunk 1318.
/// </summary>
public sealed class VmSpriteProject1318
{
    public const int XFlip = 0x0001;
    public const int YFlip = 0x0002;
    public const int FullBright = 0x0010;
    public const int SpriteFlip = 0x08000000;
    public const int ZDoomTrans = 0x10000000;
    public const int Bright = 0x08000000;
    public const int FakeFloorOnly = 2;
    public const int InvertSource = 16;
    public const int InvertOverlay = 32;
    public const int FadeToBlack = 64;
    public const int FlipSpriteOffsetX = 0x0010;
    public const int InterpolateAlpha = 0x2000;
    public const int HqSprites = 2;
    public const float TooClose = 32f / 4096f;
    public enum WaterSide
    {
        Center,
        BelowFloor,
        AboveCeiling
    }
    public readonly record struct Style(int Kind, int Flags)
    {
        public static Style Normal { get; } = new(0, 0);
        public static Style Add { get; } = new(1, 0);
        public static Style Stencil { get; } = new(2, 0);
    }
    public int HqResizeMult { get; set; } = 1;
    public int HqResizeMode { get; set; }
    public int HqResizeTargets { get; set; }
    public bool FullbrightOverrides { get; set; }
    public int VanillaTrans { get; set; }
    public bool DynLights { get; set; }
    public bool LightSprites { get; set; } = true;
    public double ShadowAlpha { get; set; } = 0.5;
    public double ShadowFadeHeight { get; set; }
    public HashSet<string> AutoTrans { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<Sprite> Sprites { get; } = new();
    public List<Draw> Draws { get; } = new();
    public Func<Sprite, bool>? AcceptStyle { get; set; }
    public Func<int, Sprite, bool>? ClipColumn { get; set; }
    public Sprite? Project(
        Actor actor,
        View view,
        Texture tex,
        double scaleX,
        double scaleY,
        int renderFlags,
        WaterSide side,
        object? fakeFloor,
        object? fakeCeiling,
        int lightLevel,
        bool foggy,
        Colormap colormap,
        bool shadow)
    {
        float spriteOff = -SpriteOffset(actor);
        double dirX = view.Sin;
        double dirY = -view.Cos;
        double trsX = actor.X - view.X + spriteOff * dirX;
        double trsY = actor.Y - view.Y + spriteOff * dirY;
        double thingScale = scaleX / tex.ScaleX;
        double left = (renderFlags & XFlip) != 0
            ? tex.Width - tex.LeftOffset - 1
            : tex.LeftOffset;
        double p1X = trsX - dirX * (left * thingScale);
        double p1Y = trsY - dirY * (left * thingScale);
        double p2X = p1X + dirX * (tex.Width * thingScale);
        double p2Y = p1Y + dirY * (tex.Width * thingScale);
        var wall = new Wall();
        if (wall.Init(view, p1X, p1Y, p2X, p2Y))
            return null;
        double scaledTop = tex.ScaledTopOffset;
        double scaledBottom = scaledTop - tex.ScaledHeight;
        double gzt = actor.Z + scaleY * scaledTop;
        double gzb = actor.Z + scaleY * scaledBottom;
        Sector? height = actor.Sector.HeightSec;
        if (height != null && ClippedByWater(side, height, gzt, gzb))
            return null;
        renderFlags ^= view.MirrorFlags & XFlip;
        if ((actor.ActorFlags & SpriteFlip) != 0)
            renderFlags ^= XFlip;
        int resize = HqResizeMult;
        double origY = scaleY / tex.ScaleY;
        if (HqResizeMode == 0 || HqResizeMult < 1 || (HqResizeTargets & HqSprites) == 0)
            resize = 1;
        double yscale = origY / resize;
        var vis = new Sprite
        {
            Wall = wall,
            SpriteScale = yscale,
            PortalUniq = view.PortalUniq,
            ScreenScale = (float)(view.InvZtoScale * yscale / wall.Sz1),
            Idepth = 1f / wall.Sz1,
            FloorClip = actor.FloorClip / origY,
            TextureMid = tex.TopOffset - (view.Z - actor.Z + actor.FloorClip / resize) / yscale,
            X1 = (short)(wall.Sx1 < view.WindowLeft ? view.WindowLeft : wall.Sx1),
            X2 = (short)(wall.Sx2 > view.WindowRight ? view.WindowRight : wall.Sx2),
            HeightSec = height,
            Sector = actor.Sector,
            Section = actor.Section,
            Depth = wall.Sz1,
            Gx = (float)actor.X,
            Gy = (float)actor.Y,
            Gz = (float)actor.Z,
            Gzb = (float)gzb,
            Gzt = (float)gzt,
            DeltaX = (float)(actor.X - view.X),
            DeltaY = (float)(actor.Y - view.Y),
            RenderFlags = (short)renderFlags,
            RenderStyle = actor.RenderStyle,
            FillColor = actor.FillColor,
            Translation = actor.Translation,
            Side = side,
            Alpha = (float)InterpolatedAlpha(actor, view.TicFrac),
            FakeFloor = fakeFloor,
            FakeCeiling = fakeCeiling,
            Pic = tex,
            Foggy = foggy
        };
        if ((actor.Flags5 & Bright) != 0)
            vis.RenderFlags = (short)(vis.RenderFlags | FullBright);
        if (VanillaTrans != 0 && (actor.ActorFlags & ZDoomTrans) != 0
            && (VanillaTrans == 1 || AutoTrans.Contains(actor.TypeName)))
        {
            vis.RenderStyle = Style.Normal;
        }
        bool invert = (vis.RenderStyle.Flags & InvertOverlay) != 0;
        if ((vis.RenderStyle.Flags & InvertSource) != 0)
            invert = !invert;
        Colormap map = colormap;
        if (vis.RenderStyle == Style.Add && colormap.Fade != 0)
            map = new Colormap { Color = colormap.Color, Fade = 0, Desaturate = colormap.Desaturate };
        bool disables = tex.HasGameTexture && tex.FullbrightDisabled;
        bool forces = FullbrightOverrides && tex.HasGameTexture && tex.Fullbright;
        bool fullbright = !foggy && ((actor.Flags5 & Bright) != 0
            || (!disables && ((renderFlags & FullBright) != 0 || forces)));
        bool fadeToBlack = (vis.RenderStyle.Flags & FadeToBlack) != 0;
        if (shadow)
        {
            vis.RenderStyle = Style.Stencil;
            vis.FillColor = 0;
            vis.Alpha = ShadowAlphaOf(vis.Alpha, actor.Z - actor.FloorZ);
        }
        vis.DynLight = DynLights && LightSprites
            ? AccumulateLights(actor)
            : 0;
        vis.Light = new LightCall
        {
            Depth = wall.Sz1,
            LightLevel = lightLevel,
            Foggy = foggy,
            Map = map,
            Fullbright = fullbright,
            Invert = invert,
            FadeToBlack = fadeToBlack
        };
        Sprites.Add(vis);
        return vis;
    }
    public void Render(Sprite spr, short[] floorClip, short[] ceilingClip)
    {
        if (!(AcceptStyle?.Invoke(spr) ?? spr.Alpha > 0))
            return;
        var portalFloor = new short[spr.X2];
        for (int x = spr.X1; x < spr.X2; x++)
        {
            bool blocked = ClipColumn != null && ClipColumn(x, spr);
            portalFloor[x] = blocked ? ceilingClip[x] : floorClip[x];
        }
        Draws.Add(new Draw
        {
            Y = spr.Gzt - spr.FloorClip,
            SpriteScale = spr.SpriteScale,
            XFlip = (spr.RenderFlags & XFlip) != 0,
            YFlip = (spr.RenderFlags & YFlip) != 0,
            Wall = spr.Wall,
            X1 = spr.X1,
            X2 = spr.X2,
            DynLight = spr.DynLight,
            Pic = spr.Pic,
            PortalFloor = portalFloor,
            Ceiling = ceilingClip,
            Style = spr.RenderStyle,
            Alpha = spr.Alpha,
            Translation = spr.Translation,
            FillColor = spr.FillColor
        });
    }
    public static bool ClippedByWater(WaterSide side, Sector height, double gzt, double gzb)
    {
        if (side == WaterSide.AboveCeiling)
            return gzt < height.CeilingAt;
        if (side == WaterSide.BelowFloor)
            return gzb >= height.FloorAt;
        if (gzt < height.FloorAt)
            return true;
        return (height.MoreFlags & FakeFloorOnly) == 0 && gzb >= height.CeilingAt;
    }
    public float ShadowAlphaOf(float baseAlpha, double heightAboveFloor)
    {
        float shadow = (float)ShadowAlpha;
        if (ShadowFadeHeight > 0)
        {
            float faded = (float)(ShadowAlpha - heightAboveFloor * (1.0 / ShadowFadeHeight));
            shadow = MathF.Max(MathF.Min(0f, (float)ShadowAlpha), faded);
        }
        return baseAlpha * shadow;
    }
    private uint AccumulateLights(Actor actor)
    {
        float red = 0, green = 0, blue = 0;
        var level = actor.Sector.Level;
        var section = actor.Section;
        if (level != null && section != null && level.FlatLights.Count > section.Index)
        {
            foreach (DynLight? node in level.FlatLights[section.Index])
            {
                if (node == null || !node.LightsActor)
                    continue;
                float lx = (float)(node.X - actor.X);
                float ly = (float)(node.Y - actor.Y);
                float lz = (float)(node.Z - actor.CenterZ);
                float reach = lx * lx + ly * ly + lz * lz;
                float radius = node.Radius;
                if (radius * radius < reach)
                    continue;
                float attenuation = 1f - MathF.Sqrt(reach) / radius;
                if (attenuation <= 0)
                    continue;
                red += node.Red * (1f / 255f) * attenuation;
                green += node.Green * (1f / 255f) * attenuation;
                blue += node.Blue * (1f / 255f) * attenuation;
            }
        }
        red = Math.Clamp(red * 255f, 0f, 255f);
        green = Math.Clamp(green * 255f, 0f, 255f);
        blue = Math.Clamp(blue * 255f, 0f, 255f);
        return ((uint)red << 16) | ((uint)green << 8) | (uint)blue;
    }
    private static float SpriteOffset(Actor actor)
    {
        float raw = (actor.RenderFlags2 & FlipSpriteOffsetX) != 0
            ? (float)actor.SpriteOffsetX
            : (float)-actor.SpriteOffsetX;
        return raw;
    }
    private static double InterpolatedAlpha(Actor actor, double ticFrac)
    {
        if ((actor.RenderFlags2 & InterpolateAlpha) == 0)
            return actor.Alpha;
        return actor.PrevAlpha * (1.0 - ticFrac) + actor.Alpha * ticFrac;
    }
    public static int RoundHalfUp(float value)
    {
        return (int)Math.Floor(value + 0.5);
    }
    public sealed class Wall
    {
        public float TLeftX, TLeftY, TRightX, TRightY;
        public float Tx1, Tx2, Sz1, Sz2;
        public int Sx1, Sx2;
        public bool Init(View view, double p1X, double p1Y, double p2X, double p2Y)
        {
            TLeftX = (float)(p1X * view.Sin - p1Y * view.Cos);
            TRightX = (float)(p2X * view.Sin - p2Y * view.Cos);
            TLeftY = (float)(p1X * view.TanCos + p1Y * view.TanSin);
            TRightY = (float)(p2X * view.TanCos + p2Y * view.TanSin);
            if ((view.MirrorFlags & XFlip) != 0)
            {
                float swapped = -TLeftX;
                TLeftX = -TRightX;
                TRightX = swapped;
                (TLeftY, TRightY) = (TRightY, TLeftY);
            }
            float sx1, sz1, sx2, sz2;
            if (TLeftX >= -TLeftY)
            {
                if (TLeftX > TLeftY || TLeftY == 0)
                    return true;
                sx1 = (float)view.CenterX + TLeftX * (float)view.CenterX / TLeftY;
                sz1 = TLeftY;
                Tx1 = 0;
            }
            else
            {
                if (TRightX < -TRightY)
                    return true;
                float den = TLeftX - TRightX - TRightY + TLeftY;
                if (den == 0)
                    return true;
                sx1 = 0;
                Tx1 = (TLeftX + TLeftY) / den;
                sz1 = TLeftY + (TRightY - TLeftY) * Tx1;
            }
            if (sz1 < TooClose)
                return true;
            if (TRightX <= TRightY)
            {
                if (TRightX < -TRightY || TRightY == 0)
                    return true;
                sx2 = (float)view.CenterX + TRightX * (float)view.CenterX / TRightY;
                sz2 = TRightY;
                Tx2 = 1;
            }
            else
            {
                if (TLeftX > TLeftY)
                    return true;
                float den = TRightY - TLeftY - TRightX + TLeftX;
                if (den == 0)
                    return true;
                sx2 = view.ViewWidth;
                Tx2 = (TLeftX - TLeftY) / den;
                sz2 = TLeftY + (TRightY - TLeftY) * Tx2;
            }
            if (sz2 < TooClose)
                return true;
            Sx1 = RoundHalfUp(sx1);
            Sx2 = RoundHalfUp(sx2);
            if (Sx2 <= Sx1)
                return true;
            float delta = sx2 - sx1;
            float t1 = (Sx1 + 0.5f - sx1) / delta;
            float t2 = (Sx2 + 0.5f - sx1) / delta;
            float inv1 = 1f / sz1;
            float inv2 = 1f / sz2;
            Sz1 = 1f / (inv1 * (1f - t1) + inv2 * t1);
            Sz2 = 1f / (inv1 * (1f - t2) + inv2 * t2);
            float ftx1 = Tx1 * inv1;
            float ftx2 = Tx2 * inv2;
            Tx1 = (ftx1 * (1f - t1) + ftx2 * t1) * Sz1;
            Tx2 = (ftx1 * (1f - t2) + ftx2 * t2) * Sz2;
            return false;
        }
    }
    public sealed class View
    {
        public double X, Y, Z;
        public double Sin, Cos = 1, TanCos = 1, TanSin;
        public double CenterX = 160, CenterY = 100, InvZtoScale = 1;
        public float ViewWidth = 320;
        public int MirrorFlags, PortalUniq;
        public short WindowLeft, WindowRight = 320;
        public double TicFrac;
    }
    public sealed class Actor
    {
        public double X, Y, Z, FloorZ, CenterZ, Alpha = 1, PrevAlpha = 1;
        public double SpriteOffsetX, FloorClip;
        public int ActorFlags, RenderFlags2, Flags5, Translation;
        public uint FillColor;
        public string TypeName = "";
        public Style RenderStyle;
        public Sector Sector { get; set; } = new();
        public Section? Section { get; set; }
    }
    public sealed class Sector
    {
        public Sector? HeightSec { get; set; }
        public double FloorAt, CeilingAt;
        public int MoreFlags;
        public Level? Level { get; set; }
    }
    public sealed class Section
    {
        public int Index { get; set; }
    }
    public sealed class Level
    {
        public List<List<DynLight?>> FlatLights { get; } = new();
    }
    public sealed class DynLight
    {
        public double X, Y, Z;
        public float Radius;
        public int Red, Green, Blue;
        public bool LightsActor { get; set; } = true;
    }
    public sealed class Texture
    {
        public double ScaleX = 1, ScaleY = 1;
        public double Width = 32, Height = 64;
        public double LeftOffset, TopOffset = 64, ScaledTopOffset = 64, ScaledHeight = 64;
        public bool HasGameTexture { get; set; } = true;
        public bool FullbrightDisabled, Fullbright;
    }
    public sealed class Colormap
    {
        public int Color, Fade, Desaturate;
    }
    public sealed class Sprite
    {
        public Wall Wall = new();
        public double SpriteScale, FloorClip, TextureMid;
        public int PortalUniq;
        public float ScreenScale, Idepth, Depth, Gx, Gy, Gz, Gzb, Gzt, DeltaX, DeltaY, Alpha;
        public short X1, X2, RenderFlags;
        public Sector? HeightSec, Sector;
        public Section? Section;
        public Style RenderStyle;
        public uint FillColor, DynLight;
        public int Translation;
        public WaterSide Side;
        public object? FakeFloor, FakeCeiling;
        public Texture? Pic;
        public bool Foggy;
        public LightCall Light = new();
    }
    public sealed class LightCall
    {
        public float Depth;
        public int LightLevel;
        public bool Foggy, Fullbright, Invert, FadeToBlack;
        public Colormap? Map;
    }
    public sealed class Draw
    {
        public double Y, SpriteScale;
        public bool XFlip, YFlip;
        public Wall? Wall;
        public int X1, X2, Translation;
        public uint DynLight, FillColor;
        public Texture? Pic;
        public short[] PortalFloor = [];
        public short[] Ceiling = [];
        public Style Style;
        public float Alpha;
    }
}
