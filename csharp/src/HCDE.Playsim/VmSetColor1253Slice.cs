namespace HCDE.Playsim;

/// <summary>
/// Hardware light color, shader light distance, and fog setup from <c>hw_setcolor.cpp</c> (<b>1–155</b>).
/// Chunk <b>1253</b>. Light level, light color, and fog density are host seams. The 0..255 clamp and the
/// software-mode predicate are the header inlines those calls use.
/// </summary>
public sealed class VmSetColor1253
{
    public const int LinearStandard = 0;
    public const int DoomBright = 1;
    public const int Doom = 2;
    public const int DoomDark = 3;
    public const int DoomLegacy = 4;
    public const int Build = 5;
    public const int ZDoomSoftware = 8;
    public const int DoomSoftware = 16;

    public readonly List<string> Log = new();
    public float Visibility = 8f;
    public int FogMode = 2;
    public bool HasFadeTable;
    public bool BrightFog;
    public bool InSkybox;
    public Func<int, int, int, bool, int, int>? CalcLevel;
    public Func<int, int, uint, int, uint>? CalcColor;
    public Func<int, int, uint, int, int, float>? FogDensity;

    public void SetColor(int mode, int sector, int rel, bool fullBright, Colormap cm, float alpha, bool weapon)
    {
        if (fullBright)
        {
            Log.Add($"color {Hex(0xffffff)} {Fmt(alpha)} desat=0");
            Log.Add(IsSoftware(mode) ? "soft 255" : "nosoft");
            return;
        }
        int hw = CalcLevel!(mode, sector, rel, weapon, cm.BlendFactor);
        uint pe = CalcColor!(mode, hw, cm.LightColor, cm.BlendFactor);
        Log.Add($"calclevel mode={mode} light={sector} rel={rel} weapon={(weapon ? 1 : 0)} blend={cm.BlendFactor} -> {hw}");
        Log.Add($"calccolor mode={mode} light={hw} pe={Hex(cm.LightColor)} blend={cm.BlendFactor} -> {Hex(pe)}");
        Log.Add($"color {Hex(pe)} {Fmt(alpha)} desat={cm.Desaturation}");
        if (IsSoftware(mode))
            Log.Add($"soft {ClampLight(sector + rel)} blend={cm.BlendFactor}");
        else
            Log.Add("nosoft");
    }

    public void SetShaderLight(float level, float olight)
    {
        const float threshold = 96f;
        const float factor = 0.75f;
        float maxDist = 256f * (Visibility / 8f);
        if (level > 0f)
        {
            float lightDist;
            if (olight < threshold)
            {
                lightDist = (maxDist / 2f) + (olight * maxDist / threshold / 2f);
                olight = threshold;
            }
            else
            {
                lightDist = maxDist;
            }
            float lightFactor = 1f + ((olight / level) - 1f) * factor;
            if (lightFactor == 1f)
                lightDist = 0f;
            Parms(lightFactor, lightDist);
        }
        else
        {
            Parms(1f, 0f);
        }
    }

    public void SetFog(int mode, int lightLevel, int relLight, bool fullBright, Colormap? cmap, bool additive)
    {
        uint fogColor;
        float fogDensity;
        if (HasFadeTable)
        {
            fogDensity = 70f;
            fogColor = 0x808080;
        }
        else if (cmap != null && !fullBright)
        {
            fogColor = cmap.FadeColor;
            fogDensity = FogDensity!(mode, lightLevel, fogColor, cmap.FogDensity, cmap.BlendFactor);
            Log.Add($"fogdensity mode={mode} light={lightLevel} color={Hex(fogColor)} sector={cmap.FogDensity} blend={cmap.BlendFactor} -> {Fmt(fogDensity)}");
            fogColor &= 0x00ffffff;
        }
        else
        {
            fogColor = 0;
            fogDensity = 0f;
        }
        if (InSkybox)
            fogDensity += fogDensity / 2f;
        Log.Add($"fogprep dens={Fmt(fogDensity)} color={Hex(fogColor)} sky={(InSkybox ? 1 : 0)} fogmode={FogMode}");
        if (fogDensity == 0f || FogMode == 0)
        {
            Log.Add("fog off");
            Log.Add($"fog {Hex(0)} {Fmt(0)}");
            return;
        }
        bool softwareBlend = IsSoftware(mode) && cmap != null && cmap.BlendFactor > 0;
        if ((mode == Doom || softwareBlend) && fogColor == 0)
        {
            int blend = cmap!.BlendFactor;
            int light = CalcLevel!(mode, lightLevel, relLight, false, blend);
            Log.Add($"calclevel mode={mode} light={lightLevel} rel={relLight} weapon=0 blend={blend} -> {light}");
            SetShaderLight(light, lightLevel);
        }
        else if (mode == Build)
        {
            Parms(0.2f * fogDensity, 1f / 31f);
        }
        else
        {
            Parms(1f, 0f);
        }
        if (additive)
            fogColor = 0;
        Log.Add("fog on");
        Log.Add($"fog {Hex(fogColor)} {Fmt(fogDensity)}");
        if (IsSoftware(mode) && cmap != null && cmap.BlendFactor == 0 && BrightFog && fogDensity != 0f && fogColor != 0)
            Log.Add("soft 255");
    }

    public static bool IsSoftware(int mode)
    {
        return mode == ZDoomSoftware || mode == DoomSoftware || mode == Build;
    }

    public static int ClampLight(int light)
    {
        if (light < 0)
            return 0;
        if (light > 255)
            return 255;
        return light;
    }

    void Parms(float factor, float dist)
    {
        Log.Add($"parms {Fmt(factor)} {Fmt(dist)}");
    }

    public static string Fmt(float value)
    {
        return value.ToString("F6", System.Globalization.CultureInfo.InvariantCulture);
    }

    public static string Hex(uint value)
    {
        return value.ToString("X8");
    }

    public sealed class Colormap
    {
        public uint LightColor = 0xffffff;
        public uint FadeColor;
        public byte Desaturation;
        public byte BlendFactor;
        public ushort FogDensity;
    }
}
