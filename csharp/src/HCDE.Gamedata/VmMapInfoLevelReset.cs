namespace HCDE.Gamedata;

/// <summary><c>level_info_t::Reset</c> defaults from <c>g_mapinfo.cpp</c> (243–334).</summary>
public static class VmMapInfoLevelReset
{
    public const string NoFlatSky = "-NOFLAT-";
    public const string DefaultSkyMist = "SKYMIST1";

    public static VmMapInfoLevelResetDraft CreateDefaults(bool hexenGame)
    {
        return new VmMapInfoLevelResetDraft
        {
            SkyPic1 = NoFlatSky,
            SkyPic2 = NoFlatSky,
            SkyMistPic = DefaultSkyMist,
            Flags2 = hexenGame ? 0 : 0x00000001,
            LightningSound = "world/thunder",
            FadeTable = "COLORMAP",
            CustomColorMap = "COLORMAP",
            WallHorizLight = -8,
            WallVertLight = 8,
            AirSupply = 20,
            PixelStretch = 1.2f,
            ThickFogDistance = -1.0f,
            ThickFogMultiplier = 30.0f,
        };
    }
}

public sealed class VmMapInfoLevelResetDraft
{
    public string SkyPic1 { get; set; } = VmMapInfoLevelReset.NoFlatSky;
    public string SkyPic2 { get; set; } = VmMapInfoLevelReset.NoFlatSky;
    public string SkyMistPic { get; set; } = VmMapInfoLevelReset.DefaultSkyMist;
    public int Flags2 { get; set; }
    public string LightningSound { get; set; } = "world/thunder";
    public string FadeTable { get; set; } = "COLORMAP";
    public string CustomColorMap { get; set; } = "COLORMAP";
    public int WallHorizLight { get; set; }
    public int WallVertLight { get; set; }
    public int AirSupply { get; set; }
    public float PixelStretch { get; set; }
    public float ThickFogDistance { get; set; }
    public float ThickFogMultiplier { get; set; }
}
