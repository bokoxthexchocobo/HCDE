using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Camera colormap, visibility distances, and the sprite shade from <c>r_light.cpp</c> (1-218).
/// Chunk 1267. A missing actor faults. The camera player wins. A colormap in range forces the
/// real map. A light level in range is level times 256. Minimum extralight selects the inverse
/// map and clears that extralight. A zero focal term returns after the clamp. Particles halve
/// visibility. Weapon sprites zero it and subtract 24 from the shade.
/// </summary>
public sealed class VmLight1267
{
    public const int NumColorMaps = 32;
    public const int FracUnit = 1 << 16;
    public const int NoLightFadeFlag = 0x8;
    public const double MinZ = (2048 * 4) / (double)(1 << 20);

    public static readonly VmSwColormap1267 RealColormaps = new("real");
    public static readonly VmSwColormap1267 FullNormal = new("full");
    public static readonly VmSwColormap1267 InverseMap = new("inverse");
    public static readonly VmLight1267 Camera = new();
    public static readonly List<string> Trace = new();

    static int _dynSeq;

    public int FixedLightLev, ExtraCvar, ColormapNum;
    public bool FullbrightIgnore, NoLightFade;
    public VmSwColormap1267? FixedColormap, RealFixedColormap, BaseColormap;
    public readonly List<VmSwColormap1267> Specials = new();
    public double CurrentVisibility = 8, BaseVisibility, WallVisibility, FloorVisibility, MaxVisForWall, MaxVisForFloor;
    public float TiltVisibility;

    public static void ResetHost()
    {
        Camera.FixedLightLev = 0;
        Camera.FixedColormap = null;
        Camera.RealFixedColormap = null;
        Camera.Specials.Clear();
        Camera.ExtraCvar = 0;
        Camera.FullbrightIgnore = false;
        _dynSeq = 0;
        Trace.Clear();
    }

    public void SetCamera(VmLightActor1267? actor, VmLightViewpoint1267 viewpoint)
    {
        if (actor == null)
            throw new NullReferenceException();

        VmLightActor1267? camera = viewpoint.Camera;
        VmLightPlayer1267? player = actor.Player;
        string source = "actor";
        if (camera != null && camera.Player != null)
        {
            player = camera.Player;
            source = "camera";
        }

        if (player == null)
            source = "none";

        RealFixedColormap = null;
        FixedColormap = null;
        FixedLightLev = -1;

        if (player != null && ReferenceEquals(camera, player.Mo))
        {
            if (player.FixedColormap >= 0 && player.FixedColormap < Specials.Count)
            {
                RealFixedColormap = Specials[player.FixedColormap];
                FixedColormap = RealColormaps;
            }
            else if (player.FixedLightLevel >= 0 && player.FixedLightLevel < NumColorMaps)
            {
                FixedLightLev = player.FixedLightLevel * 256;
                if (FullbrightIgnore && FixedLightLev >= 0)
                    FixedColormap = FullNormal;
            }
        }

        if (FixedColormap == null && viewpoint.ExtraLight == int.MinValue)
        {
            FixedColormap = InverseMap;
            viewpoint.ExtraLight = 0;
        }

        Trace.Add($"cam {source} lev={FixedLightLev} fixed={MapName(FixedColormap)} real={MapName(RealFixedColormap)} extra={viewpoint.ExtraLight}");
    }

    public void SetVisibility(VmLightView1267 view, double vis, bool noLightFade)
    {
        vis = Math.Clamp(vis, -204.7, 204.7);
        CurrentVisibility = vis;
        if (view.FocalTangent == 0 || view.FocalLengthY == 0)
        {
            Trace.Add("early cur=" + Fmt(CurrentVisibility) + " wall=" + Fmt(WallVisibility) + " fade=" + (NoLightFade ? "1" : "0"));
            return;
        }

        BaseVisibility = vis;
        double maxWall = view.InvZtoScale * (view.ScreenWidth * 200.0) / (view.ViewWidth * view.ScreenHeight * view.FocalTangent);
        maxWall = 32767.0 / maxWall;
        MaxVisForWall = maxWall;
        MaxVisForFloor = 32767.0 / (view.ViewHeight >> 2) * view.FocalLengthY / 160;

        double wall = BaseVisibility;
        if (BaseVisibility < 0 && BaseVisibility < -MaxVisForWall)
            wall = -MaxVisForWall;
        else if (BaseVisibility > 0 && BaseVisibility > MaxVisForWall)
            wall = MaxVisForWall;

        int aspect = AspectBaseHeight(view.WidescreenRatio);
        int denom = view.ViewWidth * view.ScreenHeight * 3;
        WallVisibility = view.InvZtoScale * view.ScreenWidth * aspect / denom * (wall * view.FocalTangent);

        double floor = BaseVisibility;
        if (BaseVisibility < 0 && BaseVisibility < -MaxVisForFloor)
            floor = -MaxVisForFloor;
        else if (BaseVisibility > 0 && BaseVisibility > MaxVisForFloor)
            floor = MaxVisForFloor;

        FloorVisibility = 160.0 * floor / view.FocalLengthY;
        TiltVisibility = (float)(vis * view.FocalTangent * (16f * 320f) / view.ViewWidth);
        NoLightFade = noLightFade;
        Trace.Add($"vis cur={Fmt(CurrentVisibility)} base={Fmt(BaseVisibility)} wall={Fmt(WallVisibility)} floor={Fmt(FloorVisibility)} tilt={Fmt(TiltVisibility)} maxw={Fmt(MaxVisForWall)} maxf={Fmt(MaxVisForFloor)} fade={(NoLightFade ? "1" : "0")} aspect={aspect}");
    }

    public int ShadeImpl(int lightlevel, bool foggy, int levelFlags)
    {
        lightlevel = RescaleFalse(lightlevel, Camera.ExtraCvar);
        bool noFade = !foggy && (levelFlags & NoLightFadeFlag) != 0;
        if (noFade)
            return (Math.Max(255 - lightlevel, 0) * NumColorMaps) << 8;
        return (NumColorMaps * 2 * FracUnit) - ((lightlevel + 12) * (FracUnit * NumColorMaps / 128));
    }

    public void SetColormap(
        VmLight1267 visibility,
        VmLightView1267 view,
        double z,
        int lightlevel,
        bool foggy,
        VmSwColormap1267? baseColormap,
        bool fullbright,
        bool invertColormap,
        bool fadeToBlack,
        bool psprite,
        bool particle)
    {
        if (fadeToBlack)
        {
            if (baseColormap == null)
                throw new NullReferenceException();
            if (invertColormap)
            {
                baseColormap = SpecialLights(baseColormap.Color, 0x00FFFFFFu, baseColormap.Desaturate);
                invertColormap = false;
            }
            else
            {
                baseColormap = SpecialLights(baseColormap.Color, 0, baseColormap.Desaturate);
            }
        }

        if (invertColormap)
        {
            if (baseColormap == null)
                throw new NullReferenceException();
            baseColormap = SpecialLights(baseColormap.Color, InversePacked(baseColormap.Fade), baseColormap.Desaturate);
        }

        VmLight1267 camera = Camera;
        if (camera.FixedLightLev >= 0)
        {
            BaseColormap = camera.FullbrightIgnore ? FullNormal : baseColormap;
            ColormapNum = camera.FixedLightLev >> 8;
            Trace.Add("cmap fixedlev base=" + MapName(BaseColormap) + " num=" + ColormapNum.ToString(CultureInfo.InvariantCulture));
            return;
        }

        if (camera.FixedColormap != null)
        {
            BaseColormap = camera.FixedColormap;
            ColormapNum = 0;
            Trace.Add("cmap map base=" + MapName(BaseColormap) + " num=0");
            return;
        }

        if (fullbright)
        {
            BaseColormap = camera.FullbrightIgnore ? FullNormal : baseColormap;
            ColormapNum = 0;
            Trace.Add("cmap full base=" + MapName(BaseColormap) + " num=0");
            return;
        }

        double spriteVis = visibility.SpriteVis(z, foggy);
        if (particle)
            spriteVis *= 0.5;

        int shade = ShadeImpl(lightlevel + (foggy ? 0 : view.ExtraLight << 4), foggy, view.LevelFlags3);
        if (psprite)
        {
            spriteVis = 0;
            shade -= 24 * FracUnit;
        }

        BaseColormap = baseColormap;
        ColormapNum = Palookup(spriteVis, shade);
        Trace.Add($"cmap sprite base={MapName(BaseColormap)} num={ColormapNum} vis={Fmt(spriteVis)} shade={shade}");
    }

    public double SpriteVis(double screenZ, bool foggy)
    {
        double glob = NoLightFade && !foggy ? 0.0 : WallVisibility;
        return glob / Math.Max(screenZ, MinZ);
    }

    VmSwColormap1267 SpecialLights(uint color, uint fade, int desaturate)
    {
        var map = new VmSwColormap1267("dyn" + (++_dynSeq).ToString(CultureInfo.InvariantCulture))
        {
            Color = color,
            Fade = fade,
            Desaturate = desaturate,
            Id = _dynSeq,
        };
        Trace.Add($"lights {color:x8} {fade:x8} {desaturate} {map.Id}");
        return map;
    }

    public static int RescaleFalse(int lightlevel, int extra)
    {
        int scaled = (int)((Math.Max(lightlevel, 0) / 255.0) * (255.0 - extra));
        return Math.Max(scaled + extra, 0);
    }

    public static int Palookup(double vis, int shade)
    {
        int fixedVis = (int)Math.Round(Math.Min(24.0, vis) * 65536.0, MidpointRounding.ToEven);
        int index = (shade - fixedVis) >> 16;
        if (index < 0)
            return 0;
        if (index > NumColorMaps - 1)
            return NumColorMaps - 1;
        return index;
    }

    public static int AspectBaseHeight(float aspect)
    {
        if (!(aspect < 1.333f))
        {
            float width = 240f * aspect * 3f;
            int baseWidth = (int)Math.Round(width, MidpointRounding.AwayFromZero);
            float height = 200f * (320f / (baseWidth / 3f)) * 3f;
            return (int)Math.Round(height, MidpointRounding.AwayFromZero);
        }

        float tall = (200f * (4f / 3f)) / aspect * 3f;
        return (int)Math.Round(tall, MidpointRounding.AwayFromZero);
    }

    public static uint InversePacked(uint packed)
    {
        uint a = (packed >> 24) & 0xff;
        uint r = (packed >> 16) & 0xff;
        uint g = (packed >> 8) & 0xff;
        uint b = packed & 0xff;
        return (a << 24) | ((255 - r) << 16) | ((255 - g) << 8) | (255 - b);
    }

    static string MapName(VmSwColormap1267? map) => map == null ? "null" : map.Name;

    static string Fmt(double value) => value.ToString("F6", CultureInfo.InvariantCulture);

    static string Fmt(float value) => ((double)value).ToString("F6", CultureInfo.InvariantCulture);
}

public sealed class VmSwColormap1267
{
    public VmSwColormap1267(string name) => Name = name;
    public string Name;
    public uint Color;
    public uint Fade;
    public int Desaturate;
    public int Id;
}

public sealed class VmLightActor1267
{
    public VmLightPlayer1267? Player;
}

public sealed class VmLightPlayer1267
{
    public VmLightActor1267? Mo;
    public int FixedColormap;
    public int FixedLightLevel;
}

public sealed class VmLightViewpoint1267
{
    public VmLightActor1267? Camera;
    public int ExtraLight;
}

public sealed class VmLightView1267
{
    public int ScreenWidth, ScreenHeight, ViewWidth, ViewHeight, LevelFlags3, ExtraLight;
    public double InvZtoScale, FocalTangent, FocalLengthY;
    public float WidescreenRatio;
}
